using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Munters.ResourceAccess
{
    /// <summary>
    /// Wrapper on <see cref="HttpClient"/> (for HTTP GET only) that handles boilerplate code.
    /// In real application it should be replaced by automatic code generation, for ex. by NSwagStudio
    /// </summary>
    public abstract class HttpExecutor
    {
        private readonly Lazy<JsonSerializerSettings> _settings;

        protected HttpExecutor()
        {
            _settings = new Lazy<JsonSerializerSettings>(CreateSerializerSettings);
        }

        protected async Task<T> Get<T>(string relativeUrl, params KeyValuePair<string, string>[] queryParams)
        {
            var url = ConstructUrl(relativeUrl, queryParams);
            using var httpClient = new HttpClient{BaseAddress = new Uri(BaseAddress)};
            using var request = await CreateHttpRequestMessageAsync();
            request.Method = new HttpMethod("GET");
            request.RequestUri = new Uri(url, UriKind.Relative);
            
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return await ReadObjectResponseAsync<T>(response).ConfigureAwait(false);
            }

            var responseData = response.Content == null ? null : await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            throw new ApiException("The HTTP status code of the response was not expected",
                                   (int) response.StatusCode,
                                   responseData);
        }

        protected abstract string BaseAddress { get; }

        /// <summary>
        /// Override this to add custom request headers
        /// </summary>
        /// <returns></returns>
        protected virtual Task<HttpRequestMessage> CreateHttpRequestMessageAsync()
        {
            var request = new HttpRequestMessage();
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            return Task.FromResult(request);
        }

        protected virtual JsonSerializerSettings CreateSerializerSettings()
        {
            return new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All };
        }

        private static string ConstructUrl(string relativeUrl, params KeyValuePair<string, string>[] queryParams)
        {
            return queryParams == null || queryParams.Length == 0
                       ? relativeUrl
                       : $"{relativeUrl}?{string.Join("&", queryParams.Select(FormatQueryParam))}";

            static string FormatQueryParam(KeyValuePair<string, string> pair)
            {
                return $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(ConvertToString(pair.Value, CultureInfo.InvariantCulture))}";
            }
        }

        private static string ConvertToString(object value, IFormatProvider cultureInfo)
        {
            if (value == null)
            {
                return "";
            }

            if (value is Enum)
            {
                var name = Enum.GetName(value.GetType(), value);
                if (name != null)
                {
                    var field = value.GetType().GetTypeInfo().GetDeclaredField(name);
                    if (field != null)
                    {
                        if (field.GetCustomAttribute(typeof(EnumMemberAttribute)) is EnumMemberAttribute attribute)
                        {
                            return attribute.Value ?? name;
                        }
                    }

                    var converted = Convert.ToString(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()), cultureInfo));
                    return converted ?? string.Empty;
                }
            }
            else if (value is bool b)
            {
                return Convert.ToString(b, cultureInfo).ToLowerInvariant();
            }
            else if (value is byte[] bytes)
            {
                return Convert.ToBase64String(bytes);
            }
            else if (value.GetType().IsArray)
            {
                var array = ((Array)value).OfType<object>();
                return string.Join(",", array.Select(o => ConvertToString(o, cultureInfo)));
            }

            var result = Convert.ToString(value, cultureInfo);
            return result ?? "";
        }

        private async Task<T> ReadObjectResponseAsync<T>(HttpResponseMessage response)
        {
            if (response?.Content == null)
            {
                return default;
            }

            try
            {
                await using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using var streamReader = new System.IO.StreamReader(responseStream);
                using var jsonTextReader = new JsonTextReader(streamReader);
                var serializer = JsonSerializer.Create(_settings.Value);
                return serializer.Deserialize<T>(jsonTextReader);
            }
            catch (JsonException exception)
            {
                var message = "Could not deserialize the response body stream as " + typeof(T).FullName + ".";
                throw new ApiException(message, (int)response.StatusCode, string.Empty, exception);
            }
        }
    }
}
