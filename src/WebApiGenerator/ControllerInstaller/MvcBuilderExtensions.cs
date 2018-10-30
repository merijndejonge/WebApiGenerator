using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenSoftware.WebApiGenerator.CodeGenerator;

namespace OpenSoftware.WebApiGenerator.ControllerInstaller
{
    public static class MvcBuilderExtensions
    {
        public static IWebHostBuilder AddServiceGenerator(this IWebHostBuilder builder, ILoggerFactory logFactory,
            params Assembly[] assemblies) =>
            builder.ConfigureServices(services => services.AddMvc().AddControllers(logFactory, assemblies));

        public static IMvcBuilder AddControllers(this IMvcBuilder builder, ILoggerFactory logFactory,
            params Assembly[] assemblies)
        {
            // Generate controllers for types registered in the assemblies
            var controllers = ControllerFactory.Create(logFactory, assemblies).ToList();

            var controllerTypes = controllers.SelectMany(x => x.controllerType);
            var serviceTypes = ControllerFactory.GetServiceTypes(assemblies).ToList();

            // Enable the services to configure themselves by registering requires service dependencies
            controllers.ForEach(x => x.serviceMetadata?.ConfigureServices(builder.Services));

            // Register services to DI container
            serviceTypes.ForEach(x => builder.Services.AddTransient(x));

            builder.ConfigureApplicationPartManager(apm =>
                apm.FeatureProviders.Add(new GenericControllerFeatureProvider(controllerTypes)));
            return builder;
        }
    }
}