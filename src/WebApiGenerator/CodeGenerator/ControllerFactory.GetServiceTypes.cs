using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenSoftware.WebApiClient;

namespace OpenSoftware.WebApiGenerator.CodeGenerator
{
    public static partial class ControllerFactory
    {
        public static IEnumerable<Type> GetServiceTypes(params Assembly[] assemblies)
        {
            return assemblies.SelectMany(GetServiceTypesFromAssembly);
        }

        private static IEnumerable<Type> GetServiceTypesFromAssembly(Assembly assembly)
        {
            var allTypes = assembly.GetExportedTypes();
            return allTypes.Where(IsServiceClass);
        }

        private static bool IsServiceClass(Type type)
        {
            var allMethods = type.GetMethods();
            return allMethods.Any(IsServiceMethod);
        }

        private static bool IsServiceMethod(MethodInfo method)
        {
            return method.GetCustomAttributes<FromGetMethodAttribute>().Any() ||
                   method.GetCustomAttributes<FromPostMethodAttribute>().Any();
        }
    }
}
