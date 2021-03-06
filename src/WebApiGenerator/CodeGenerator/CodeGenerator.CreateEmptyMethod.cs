﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenSoftware.WebApiClient;

namespace OpenSoftware.WebApiGenerator.CodeGenerator
{
    public partial class CodeGenerator
    {
        private static MethodDeclarationSyntax CreateEmptyMethod(MethodInfo methodInfo, IList<Type> attributeTypes)
        {
            var returnType = MakeReturnType(methodInfo);

            var claimParameters = GetClaimsParameters(methodInfo);
            if (claimParameters.Any())
            {
                attributeTypes = attributeTypes.ToList();
                attributeTypes.Add(typeof(AuthorizeAttribute));
            }
            var attributeList = new SeparatedSyntaxList<AttributeSyntax>();
            foreach (var attribute in attributeTypes)
            {
                attributeList = attributeList.Add(SyntaxFactory.Attribute(SyntaxFactory.ParseName(attribute.FullName)));
            }

            var routeAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName(typeof(RouteAttribute).FullName),
                SyntaxFactory.ParseAttributeArgumentList(@"(""[action]"")")
                );
            attributeList = attributeList.Add(routeAttribute);

            var methodDeclaration = SyntaxFactory
                .MethodDeclaration(SyntaxFactory.ParseTypeName(Type2String(returnType)), methodInfo.Name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            if (IsAsyncMethod(methodInfo))
            {
                methodDeclaration = methodDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.AsyncKeyword));
            }
            var attributes = SyntaxFactory.AttributeList(attributeList);
            var attributesList = methodDeclaration.AttributeLists.Add(attributes);
            methodDeclaration = methodDeclaration.WithAttributeLists(attributesList);

            var payloadParameters = GetPayLoadParameters(methodInfo);
            if (payloadParameters.Any())
            {

                var payloadClassName = methodInfo.Name + "Payload";
                var parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier("payload"))
                    .WithType(SyntaxFactory.ParseTypeName(payloadClassName));
                var parameters =
                    SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList<ParameterSyntax>().Add(parameter));
                methodDeclaration = methodDeclaration.WithParameterList(parameters);
                return methodDeclaration;
            }

            var normalParameters = GetNormalParameters(methodInfo);
            foreach (var normalParameter in normalParameters)
            {
                var parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(normalParameter.Name))
                    .WithType(SyntaxFactory.ParseTypeName(normalParameter.ParameterType.FullName));
                var parameters =
                    SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList<ParameterSyntax>().Add(parameter));
                methodDeclaration = methodDeclaration.WithParameterList(parameters);
            }

            return methodDeclaration;
        }

        private static IEnumerable<ParameterInfo> GetNormalParameters(MethodInfo methodInfo)
        {
            return methodInfo.GetParameters()
                .Where(x => x.GetCustomAttributes().Any() == false);
        }

        private static IEnumerable<ParameterInfo> GetClaimsParameters(MethodInfo methodInfo)
        {
            return methodInfo.GetParameters().Where(x => x.GetCustomAttributes(typeof(FromClaimAttribute)).Any());
        }
        private static bool IsAsyncMethod(MethodInfo method)
        {
            var attType = typeof(AsyncStateMachineAttribute);

            // Obtain the custom attribute for the method. 
            // The value returned contains the StateMachineType property. 
            // Null is returned if the attribute isn't present for the method. 
            var attrib = (AsyncStateMachineAttribute)method.GetCustomAttribute(attType);

            return (attrib != null);
        }

        private static Type MakeReturnType(MethodInfo method)
        {
            return IsAsyncMethod(method) ? typeof(Task<IActionResult>) : typeof(IActionResult);
        }

        public static string Type2String(Type type)
        {
            if (!type.IsGenericType) return $"{type.Namespace}.{type.Name}";
            
            // Get the C# representation of the generic type minus its type arguments.
            var name = type.Name.Substring(0, type.Name.IndexOf("`", StringComparison.Ordinal));
            var arguments = type.GetGenericArguments().Select(Type2String);
            return $"{type.Namespace}.{name}<{string.Join(",", arguments)}>";
        }
    }
}