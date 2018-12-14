using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenSoftware.WebApiClient;

namespace OpenSoftware.WebApiGenerator.CodeGenerator
{
    public partial class CodeGenerator
    {
        private static ClassDeclarationSyntax CreatePayloadClass(MethodInfo methodInfo)
        {
            var payloadClassName = methodInfo.Name + "Payload";
            var payloadClassDeclaration = SyntaxFactory.ClassDeclaration(payloadClassName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            var payloadParameters = GetPayLoadParameters(methodInfo).ToArray();
            if(payloadParameters.Any() == false) return null;
            foreach (var payloadParameter in payloadParameters)
            {
                var payloadAttribute = payloadParameter.GetCustomAttributes(typeof(FromPayloadAttribute))
                    .OfType<FromPayloadAttribute>().Single();
                var propertyName = payloadAttribute.Name;
                if (string.IsNullOrEmpty(propertyName))
                {
                    propertyName = payloadParameter.Name;
                }
                var propertyDeclaration = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(payloadParameter.ParameterType.FullName), propertyName)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddAccessorListAccessors(
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

                payloadClassDeclaration = payloadClassDeclaration.AddMembers(propertyDeclaration);
            }
            return payloadClassDeclaration;
        }

        private static IEnumerable<ParameterInfo> GetPayLoadParameters(MethodInfo methodInfo)
        {
            return methodInfo.GetParameters().Where(x => x.GetCustomAttributes(typeof(FromPayloadAttribute)).Any());
        }
    }
}