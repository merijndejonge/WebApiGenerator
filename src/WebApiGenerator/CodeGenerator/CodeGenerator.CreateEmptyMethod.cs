using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenSoftware.WebApiGenerator.CodeGenerator
{
    public partial class CodeGenerator
    {
        private static MethodDeclarationSyntax CreateEmptyMethod(MethodInfo methodInfo, IList<Type> attributeTypes)
        {
            var returnType = typeof(IActionResult);


            var attributeList = new SeparatedSyntaxList<AttributeSyntax>();
            foreach (var attribute in attributeTypes)
            {
                attributeList = attributeList.Add(SyntaxFactory.Attribute(SyntaxFactory.ParseName(attribute.FullName)));
            }

            var methodDeclaration = SyntaxFactory
                .MethodDeclaration(SyntaxFactory.ParseTypeName(returnType.FullName), methodInfo.Name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            var attributes = SyntaxFactory.AttributeList(attributeList);
            var attributesList = methodDeclaration.AttributeLists.Add(attributes);
            methodDeclaration = methodDeclaration.WithAttributeLists(attributesList);

            var payloadParameters = GetPayLoadParameters(methodInfo);
            if (payloadParameters.Any() == false) return methodDeclaration;

            var payloadClassName = methodInfo.Name + "Payload";
            var parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier("payload"))
                .WithType(SyntaxFactory.ParseTypeName(payloadClassName));
            var parameters = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList<ParameterSyntax>().Add(parameter));

            methodDeclaration = methodDeclaration.WithParameterList(parameters);

            return methodDeclaration;
        }
    }
}