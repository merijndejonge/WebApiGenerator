using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using OpenSoftware.WebApiGenerator.CodeGenerator;
using OpenSoftware.WebApiGenerator.ControllerInstaller;

namespace OpenSoftware.WebApiGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logFactory = new LoggerFactory()
                .AddConsole(LogLevel.Debug)
                .AddDebug();
            AppDomain.CurrentDomain.UnhandledException += (s, o) =>
            {
                var errorLogger = logFactory.CreateLogger<Program>();
                errorLogger.LogError(((Exception)o.ExceptionObject).Message);
                logFactory.Dispose();
                Environment.Exit(1);
            };

            var logger = logFactory.CreateLogger<Program>();
            var options = new ServiceGeneratorOptions();

            options.Parse(args);
            if (options.Usage.IsDefined)
            {
                options.DisplayUsage(Console.Error);
                Environment.Exit(0);
            }

            var serviceAssembly = LoadAssemblyFromName(options.ServiceAssembly.Value);
            logger.LogInformation("Using service assembly " + serviceAssembly.Location);
            var startupType = typeof(DefaultGeneratorStartup);

            if (options.DryRun.IsDefined)
            {
                var serviceTypes = ControllerFactory.GetServiceTypes(serviceAssembly);
                var code = ControllerFactory.CreateControllerCode(serviceTypes).ToList();
                if (code.Any() == false)
                {
                    throw new Exception("No controller methods found in service assembly.");
                }
                code.ForEach(x => Console.WriteLine(x.NormalizeWhitespace().ToFullString()));
                logFactory.Dispose();
                return;
            }
            if (options.StartupAssembly.IsDefined)
            {
                var startupAssembly = LoadAssemblyFromName(options.StartupAssembly.Value);
                startupType = GetStartupClassFromAssembly(startupAssembly);
                if (startupType == null)
                {
                    throw new Exception("Startup type specified with `--startup` switch bug assembly doesn't contain startup class.");
                }
            }
            logger.LogInformation("Using startup class " + startupType.Name);

            var serviceArgs = options.Arguments;
            if (options.Urls.IsDefined)
            {
                serviceArgs.Add(options.Urls.Name + "=" + options.Urls.Value);
            }

            CreateWebHostBuilder(logFactory, serviceArgs.ToArray(), serviceAssembly, startupType).Build().Run();
        }
        private static IWebHostBuilder CreateWebHostBuilder(ILoggerFactory logFactory, string[] args,
            Assembly serviceAssembly, Type startupType)
            => WebHost.CreateDefaultBuilder(args)
                .UseStartup(startupType)
                .AddServiceGenerator(logFactory, serviceAssembly);
        private static Type GetStartupClassFromAssembly(Assembly startupAssembly)
        {
            return startupAssembly.GetTypes().SingleOrDefault(x =>
                x.GetMethods().Any(m => m.Name == nameof(DefaultGeneratorStartup.Configure)));
        }

        /// <summary>
        /// https://github.com/Particular/Workshop/issues/64
        /// </summary>
        /// <param name="assemblyPath"></param>
        /// <returns></returns>
        private static Assembly LoadAssemblyFromName(string assemblyPath)
        {
            var assemblyFullPath = Path.GetFullPath(assemblyPath);
            var fileNameWithOutExtension = Path.GetFileNameWithoutExtension(assemblyFullPath);

            var inCompileLibraries = DependencyContext.Default.CompileLibraries.Any(l => l.Name.Equals(fileNameWithOutExtension, StringComparison.OrdinalIgnoreCase));
            var inRuntimeLibraries = DependencyContext.Default.RuntimeLibraries.Any(l => l.Name.Equals(fileNameWithOutExtension, StringComparison.OrdinalIgnoreCase));

            var assembly = (inCompileLibraries || inRuntimeLibraries)
                ? Assembly.Load(new AssemblyName(fileNameWithOutExtension))
                : AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFullPath);

            return assembly;
        }
    }
}
