using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenSoftware.WebApiGenerator.CodeGenerator
{
    public partial class CodeGenerator
    {
        private static NamespaceDeclarationSyntax CreateNameSpace(string baseNameSpaceName, string serviceNameSpaceName)
        {
            var @namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNameSpaceName}.{serviceNameSpaceName}")).NormalizeWhitespace();
            return @namespace;
        }
    }
}