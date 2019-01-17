using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;
using Microsoft.Extensions.Logging;

namespace OpenSoftware.WebApiGenerator
{
    /// <summary>
    /// Based on https://www.codeproject.com/Articles/1194332/Resolving-Assemblies-in-NET-Core
    /// </summary>
    internal sealed class AssemblyResolver : IDisposable
    {
        private readonly ILogger _logger;
        private readonly ICompilationAssemblyResolver _assemblyResolver;
        private readonly DependencyContext _dependencyContext;
        private readonly AssemblyLoadContext _loadContext;

        public AssemblyResolver(ILogger logger, string path)
        {
            _logger = logger;
            Assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            _dependencyContext = DependencyContext.Load(Assembly);

            _assemblyResolver = new CompositeCompilationAssemblyResolver
            (new ICompilationAssemblyResolver[]
            {
                new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(path)),
                new ReferenceAssemblyPathResolver(),
                new PackageCompilationAssemblyResolver()
            });

            _loadContext = AssemblyLoadContext.GetLoadContext(Assembly);
            _loadContext.Resolving += OnResolving;
        }

        public Assembly Assembly { get; }

        public void Dispose()
        {
            _loadContext.Resolving -= OnResolving;
        }

        private Assembly OnResolving(AssemblyLoadContext context, AssemblyName name)
        {
            bool NamesMatch(RuntimeLibrary runtime)
            {
                return string.Equals(runtime.Name, name.Name, StringComparison.OrdinalIgnoreCase);
            }

            _logger.LogInformation("Loading " + name.FullName);

            var library =
                _dependencyContext.RuntimeLibraries.FirstOrDefault(NamesMatch);
            if (library != null)
            {
                var wrapper = new CompilationLibrary(
                    library.Type,
                    library.Name,
                    library.Version,
                    library.Hash,
                    library.RuntimeAssemblyGroups.SelectMany(g => g.AssetPaths),
                    library.Dependencies,
                    library.Serviceable);

                var assemblies = new List<string>();
                _assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies);
                if (assemblies.Count > 0)
                {
                    return _loadContext.LoadFromAssemblyPath(assemblies[0]);
                }
            }
            _logger.LogWarning("Could not load assembly " + name.FullName);

            return null;
        }
    }
}