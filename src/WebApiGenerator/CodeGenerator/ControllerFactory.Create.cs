using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using OpenSoftware.WebApiClient;

namespace OpenSoftware.WebApiGenerator.CodeGenerator
{
    public static partial class ControllerFactory
    {
        public static IEnumerable<(IEnumerable<Type> controllerType, IServiceMetadata serviceMetadata)> Create(
            ILoggerFactory logFactory, params Assembly[] assemblies)
        {
            return assemblies.Select(x => CreateSingle(logFactory, x));
        }
        public static (IEnumerable<Type> controllerType, IServiceMetadata serviceMetadata) CreateSingle(
            ILoggerFactory logFactory, Assembly assembly)
        {
            var logger = logFactory.CreateLogger(nameof(ControllerFactory));

            IServiceMetadata serviceMetadata = null;
            var serviceMetadataType = assembly.GetTypes().SingleOrDefault(x => typeof(IServiceMetadata).IsAssignableFrom(x));
            if (serviceMetadataType != null)
            {
                logger.LogInformation($"Using service meta data from {serviceMetadataType.FullName}.");
                serviceMetadata = (IServiceMetadata)Activator.CreateInstance(serviceMetadataType);
            }
            else
            {
                logger.LogInformation($"No service meta data found, using default.");
            }

            var serviceTypes = GetServiceTypes(assembly);
            var generatedCode = CreateControllerCode(serviceTypes);
            var generatedAssembly = CompileControllerCode(generatedCode, assembly);
            var controllerTypes = GetControllerTypes(generatedAssembly);
            return (controllerTypes, serviceMetadata);
        }

        private static Assembly CompileControllerCode(IEnumerable<CompilationUnitSyntax> code, Assembly assembly)
        {
            return Compiler.Compile(code, assembly);
        }

        private const string Namespace = "OpenSoftware.WebControllers";

        public static IEnumerable<CompilationUnitSyntax> CreateControllerCode(IEnumerable<Type> serviceTypes)
        {
            return serviceTypes.Select(serviceType => CodeGenerator.GenerateController(serviceType, Namespace));
        }
    }
}