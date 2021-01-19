using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace dotnetcore_aspnetcore_odata
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
            services.AddControllers();
            services.AddOData();
            services.AddSingleton(sp => new ODataUriResolver() { EnableCaseInsensitive = true });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.EnableDependencyInjection();
                endpoints.Select().Filter().OrderBy().Count().MaxTop(500);
                endpoints.MapODataRoute("odata", null, container =>
                {
                    container.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => GetEdmModel());
                    container.AddService(Microsoft.OData.ServiceLifetime.Singleton, _ => app.ApplicationServices.GetRequiredService<ODataUriResolver>());
                });
                endpoints.MapControllers();
            });
        }

        private static IEdmModel GetEdmModel()
        {
            var odataBuilder = new ODataConventionModelBuilder();
            odataBuilder.Namespace = "Default";
            odataBuilder.EnableLowerCamelCase();
            odataBuilder.EntitySet<WeatherForecast>("Weather");
            var action = odataBuilder
                .EntityType<WeatherForecast>()
                .Collection
                .Function("LessThan");
            action.Parameter<int>("TemperatureC");
            action.ReturnsCollectionFromEntitySet<WeatherForecast>("Weather");
            var model = odataBuilder.GetEdmModel();
            return model;
        }
    }
}
