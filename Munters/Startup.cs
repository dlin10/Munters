using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Munters.Engines;
using Munters.ResourceAccess;

namespace Munters
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Add response caching services.
            services.AddResponseCaching(); 
            
            services.AddControllers();
            services.AddSwaggerDocument();

            services.AddOptions<GifDownloadOptions>()
                    .Configure(options =>
                               {
                                   options.BaseAddress = Configuration["Giphy:BaseAddress"];
                                   options.TrendingUrl = Configuration["Giphy:Trending"];
                                   options.SearchUrl = Configuration["Giphy:Search"];
                                   options.ApiKey = Configuration["Giphy:ApiKey"];
                               });

            services.AddTransient<IGiphyResourceAccess, GiphyHttpExecutor>();
            services.AddTransient<IGifDownloadEngine, GifDownloadEngine>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting()
               .UseOpenApi()
               .UseSwaggerUi3();

            app.UseAuthorization();

            //Instead of implementing caching, we can use implicitly available Response Caching Middleware
            app.UseResponseCaching();

            //Middleware to add headers to control caching on subsequent requests
            app.Use(async (context, next) =>
                    {
                        context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
                        {
                            Public = true,
                            MaxAge = TimeSpan.FromSeconds(10)
                        };
                        context.Response.Headers[HeaderNames.Vary] = new[] { "Accept-Encoding" };
                        await next();
                    });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
