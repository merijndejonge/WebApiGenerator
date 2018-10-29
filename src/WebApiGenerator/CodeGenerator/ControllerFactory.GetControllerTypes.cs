using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenSoftware.WebApiGenerator.ControllerBase;

namespace OpenSoftware.WebApiGenerator.CodeGenerator
{
    public static partial class ControllerFactory

    {
        public static IEnumerable<Type> GetControllerTypes(params Assembly[] assemblies)
        {
            return assemblies.SelectMany(GetControllerTypesFromAssembly);
        }

        private static IEnumerable<Type> GetControllerTypesFromAssembly(Assembly assembly)
        {
            var allTypes = assembly.GetExportedTypes();
            return allTypes.Where(IsControllerClass);
        }

        private static bool IsControllerClass(Type type)
        {
            return typeof(ServiceControllerBase).IsAssignableFrom(type);
        }
    }
}