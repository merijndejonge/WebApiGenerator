﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using OpenSoftware.WebApiGenerator.CodeGenerator;
using OpenSoftware.WebApiGenerator.ControllerInstaller;

namespace OpenSoftware.WebApiGenerator
{
    public class Program
    {
        private static ILoggerFactory _loggerFactory;

        public static void Main(string[] args)
        {
            _loggerFactory = new LoggerFactory()
                .AddConsole(LogLevel.Debug)
                .AddDebug();

            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;

            var logger = _loggerFactory.CreateLogger<Program>();
            var options = new ServiceGeneratorOptions();

            options.Parse(args);
            if (options.Usage.IsDefined)
            {
                options.DisplayUsage(Console.Error);
                Environment.Exit(0);
            }

            var assemblyFolder = Path.GetDirectoryName(options.ServiceAssembly.Value);
            if (string.IsNullOrEmpty(assemblyFolder)) assemblyFolder = ".";
            assemblyFolder = Path.GetFullPath(assemblyFolder);
            var assemblyFileName = Path.GetFileName(options.ServiceAssembly.Value);
            var assemblyFolders = new List<string> {assemblyFolder};

            AssemblyLoadContext.Default.Resolving += (a1,a2) => Default_Resolving(a2, assemblyFolders);


            var serviceAssembly = LoadAssemblyFromName(assemblyFileName, assemblyFolders);
            if (serviceAssembly == null)
            {
                throw new Exception("Could not load assembly " + options.ServiceAssembly.Value);
            }
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
                _loggerFactory.Dispose();
                return;
            }
            if (options.StartupAssembly.IsDefined)
            {
                assemblyFolder = Path.GetDirectoryName(options.StartupAssembly.Value);
                if (string.IsNullOrEmpty(assemblyFolder)) assemblyFolder = ".";
                assemblyFolder = Path.GetFullPath(assemblyFolder);
                assemblyFileName = Path.GetFileName(options.ServiceAssembly.Value);
                assemblyFolders.Add(assemblyFolder);
                var startupAssembly = LoadAssemblyFromName(assemblyFileName, assemblyFolders);
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

            CreateWebHostBuilder(_loggerFactory, serviceArgs.ToArray(), serviceAssembly, startupType).Build().Run();
        }

        private static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var errorLogger = _loggerFactory.CreateLogger<Program>();
            errorLogger.LogError(((Exception) e.ExceptionObject).Message);
            _loggerFactory.Dispose();
            Environment.Exit(1);
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
        /// <param name="assemblyFileName"></param>
        /// <param name="assemblyFolders"></param>
        /// <returns></returns>
        private static Assembly LoadAssemblyFromName(string assemblyFileName, List<string> assemblyFolders)
        {
            foreach (var assemblyFolder in assemblyFolders)
            {
                var assemblyPath = $@"{assemblyFolder}\{assemblyFileName}";
                if (Path.HasExtension(assemblyPath) == false)
                {
                    assemblyPath += ".dll";
                }

                if (File.Exists(assemblyPath) == false) continue;

                // Requires nuget - System.Runtime.Loader
                var assembly = AssemblyLoadContext.Default
                    .LoadFromAssemblyPath(assemblyPath);
                return assembly;
            }

            return null;
        }

        private static Assembly Default_Resolving(AssemblyName assemblyToLoad, List<string> assembliesFolder)
        {
            return LoadAssemblyFromName(assemblyToLoad.Name, assembliesFolder);
        }
    }
}
