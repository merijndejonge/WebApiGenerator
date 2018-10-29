using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenSoftware.WebApiGenerator.CodeGenerator
{
    public class Compiler
    {
        public static Assembly Compile(IEnumerable<CompilationUnitSyntax> compilationUnits,
            params Assembly[] assemblies)
        {
            var syntaxTrees = compilationUnits.Select(x => x.SyntaxTree);
            var references = GetReferences(Assembly.GetEntryAssembly().GetReferencedAssemblies()).ToList();
            foreach (var assembly in assemblies)
            {
                references.AddRange(GetReferences(assembly.GetReferencedAssemblies()));
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }

            references = references.Distinct().ToList();

            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToString())
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(references)
                .AddSyntaxTrees(syntaxTrees);

            using (var outputStream = new MemoryStream())
            {
                var result = compilation.Emit(outputStream);
                if (result.Success)
                {
                    outputStream.Position = 0;
                    var assembly = AssemblyLoadContext.Default.LoadFromStream(outputStream);
                    return assembly;
                }

                throw new Exception("Compilation failed" + result.Diagnostics);
            }
        }

        private static IEnumerable<MetadataReference> GetReferences(params AssemblyName[] assemblyName)
        {
            var coreAssemblyLocation = typeof(object).GetTypeInfo().Assembly.Location;
            var coreDir = Directory.GetParent(coreAssemblyLocation);

            yield return MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().Location);

            yield return MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar +
                                                          "netstandard.dll");
            yield return MetadataReference.CreateFromFile(coreAssemblyLocation);

            foreach (var referencedAssembly in assemblyName)
            {
                var loadedAssembly = Assembly.Load(referencedAssembly);

                yield return MetadataReference.CreateFromFile(loadedAssembly.Location);
            }
        }
    }
}