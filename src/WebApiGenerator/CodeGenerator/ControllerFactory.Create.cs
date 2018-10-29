using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenSoftware.WebApiClient;

namespace OpenSoftware.WebApiGenerator.CodeGenerator
{
    public static partial class ControllerFactory
    {
        public static IEnumerable<(IEnumerable<Type> controllerType, IServiceMetadata serviceMetadata)> Create(
            params Assembly[] assemblies)
        {
            return assemblies.Select(CreateSingle);
        }
        public static (IEnumerable<Type> controllerType, IServiceMetadata serviceMetadata) CreateSingle(Assembly assembly)
        {
            var serviceMetadataType = assembly.GetTypes().SingleOrDefault(x => typeof(IServiceMetadata).IsAssignableFrom(x));
            if (serviceMetadataType == null)
            {
                throw new InvalidEnumArgumentException("assembly doesn't contain type with ServiceMetadataAttribute attribute.");
            }

            var serviceMetadata = (IServiceMetadata)Activator.CreateInstance(serviceMetadataType);

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