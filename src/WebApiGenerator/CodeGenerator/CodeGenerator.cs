using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenSoftware.WebApiClient;

namespace OpenSoftware.WebApiGenerator.CodeGenerator
{
    public partial class CodeGenerator
    {
        public static IEnumerable<MethodInfo> GetServiceMethods<T>(Type serviceType) where T : Attribute
        {
            return serviceType.GetMethods().Where(x => x.GetCustomAttributes<T>().Any());
        }

        public static CompilationUnitSyntax GenerateController(Type serviceType, string @namespace)
        {
            var ns = CreateNameSpace(@namespace, serviceType.Name);
            var controllerClass = CreateEmptyController(serviceType);

            var getMethodInfos = GetServiceMethods<FromGetMethodAttribute>(serviceType).ToArray();
            foreach (var methodInfo in getMethodInfos)
            {
                var (method, payloadClass) = CreateMethod<HttpGetAttribute>(methodInfo);
                controllerClass = controllerClass.AddMembers(method);
                if (payloadClass != null)
                {
                    ns = ns.AddMembers(payloadClass);
                }
            }

            var postMethodInfos = GetServiceMethods<FromPostMethodAttribute>(serviceType).ToArray();
            foreach (var methodInfo in postMethodInfos)
            {
                var (postMethod, payloadClass) = CreateMethod<HttpPostAttribute>(methodInfo);
                controllerClass = controllerClass.AddMembers(postMethod);
                if (payloadClass != null)
                {
                    ns = ns.AddMembers(payloadClass);
                }
            }

            var deleteMethodInfos = GetServiceMethods<FromDeleteMethodAttribute>(serviceType).ToArray();
            foreach (var methodInfo in deleteMethodInfos)
            {
                var (method, payloadClass) = CreateMethod<HttpDeleteAttribute>(methodInfo);
                controllerClass = controllerClass.AddMembers(method);
                if (payloadClass != null)
                {
                    ns = ns.AddMembers(payloadClass);
                }
            }

            ns = ns.AddMembers(controllerClass);

            var compilationUnit = SyntaxFactory.CompilationUnit();
            compilationUnit = compilationUnit.AddMembers(ns);
            return compilationUnit;
        }

        private static (MethodDeclarationSyntax, ClassDeclarationSyntax) CreateMethod<T>(MethodInfo methodInfo) where T : Attribute
        {
            var methodBody = CreateMethodBody(methodInfo);
            var method = CreateEmptyMethod(methodInfo, new[] { typeof(T) }).WithBody(methodBody);
            var payloadClass = CreatePayloadClass(methodInfo);
            return (method, payloadClass);
        }
    }
};