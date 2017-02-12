using System.Web.Http;
using Backend;
using Swashbuckle.Application;
using WebActivatorEx;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace Backend
{
    /// <summary>
    /// The config for the Swagger UI.
    /// </summary>
    public class SwaggerConfig
    {
        /// <summary>
        /// Registers the settings for Swagger.
        /// </summary>
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;

            GlobalConfiguration.Configuration
                .EnableSwagger(c =>
                    {
                        c.SingleApiVersion("v1", "Mobile Rounds Backend");
                        c.IncludeXmlComments(System.AppDomain.CurrentDomain.BaseDirectory + ".\\bin\\Backend.XML");
                        c.DescribeAllEnumsAsStrings();
                    })
                .EnableSwaggerUi();
        }
    }
}
