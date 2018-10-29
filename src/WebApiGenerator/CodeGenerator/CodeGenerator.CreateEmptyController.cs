using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenSoftware.WebApiGenerator.ControllerBase;

namespace OpenSoftware.WebApiGenerator.CodeGenerator
{
    public partial class CodeGenerator

    {
        private static ClassDeclarationSyntax CreateEmptyController(Type service)
        {
            var serviceName = service.Name;
            var controllerName = CreateControllerName(serviceName);
            var classDeclaration = SyntaxFactory.ClassDeclaration(controllerName);

            // Add the public modifier: (public class Order)
            classDeclaration = classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            classDeclaration = classDeclaration.AddBaseListTypes(
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(typeof(ServiceControllerBase).FullName)));

            var serviceField = CreateField(service);
            var constructor = CreateConstructor(service);
            classDeclaration = classDeclaration.AddMembers(serviceField, constructor);
            return classDeclaration;
        }
        private static FieldDeclarationSyntax CreateField(Type service)
        {
            // Create a string variable: (bool canceled;)
            var variableDeclaration = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(service.FullName))
                .AddVariables(SyntaxFactory.VariableDeclarator("_service"));

            // Create a field declaration: (private bool canceled;)
            var fieldDeclaration = SyntaxFactory.FieldDeclaration(variableDeclaration)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));
            return fieldDeclaration;
        }
        private static ConstructorDeclarationSyntax CreateConstructor(Type service)
        {
            var serviceName = service.Name;
            var controllerName = CreateControllerName(serviceName);
            var constructor = SyntaxFactory.ConstructorDeclaration(controllerName)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithBody(SyntaxFactory.Block());

            var parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier("service"))
                .WithType(SyntaxFactory.ParseTypeName(service.FullName));
            var parameters = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList<ParameterSyntax>().Add(parameter));
            constructor = constructor.WithParameterList(parameters);

            var assignment = SyntaxFactory.ParseStatement("_service = service;");
            constructor = constructor.WithBody(SyntaxFactory.Block(assignment));

            return constructor;
        }

        private static string CreateControllerName(string serviceName)
        {
            if (serviceName.EndsWith("Service"))
            {
                serviceName = serviceName.Substring(0, serviceName.Length - "Service".Length);
            }
            return serviceName + "Controller";
        }
    }
}