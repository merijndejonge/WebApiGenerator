﻿using System;
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
        private static BlockSyntax CreateMethodBody(MethodInfo methodInfo)
        {
            var body = SyntaxFactory.Block();
            var assignmentStats = CreateAssignmentStatements(methodInfo).ToArray();
            var serviceCall = CreateServiceCall(methodInfo).ToArray();

            body = body.AddStatements(assignmentStats);
            body = body.AddStatements(serviceCall);
            return body;
        }
        private static IEnumerable<StatementSyntax> CreateAssignmentStatements(MethodInfo methodInfo)
        {
            var varCount = 0;
            foreach (var parameterInfo in methodInfo.GetParameters())
            {
                var attribute = GetCustomAttribute(parameterInfo);
                if (attribute is FromHttpHeaderAttribute)
                {
                    yield return HeaderAssignmentStatement(parameterInfo, varCount);
                    yield return MakeValueCheck(varCount);
                }
                else if (attribute is FromClaimAttribute)
                {
                    yield return ClaimAssignmentStatement(parameterInfo, varCount);
                }
                else
                {
                    continue;
                }
                varCount++;
            }
        }

        private static IEnumerable<StatementSyntax> CreateServiceCall(MethodInfo methodInfo)
        {
            var parameterList = new List<string>();
            var argumentCount = 0;
            foreach (var parameterInfo in methodInfo.GetParameters())
            {
                var attribute = GetCustomAttribute(parameterInfo);
                switch (attribute)
                {
                    case FromHttpHeaderAttribute _:
                    case FromClaimAttribute _:
                        parameterList.Add("value" + argumentCount++);
                        break;
                    case FromPayloadAttribute payloadAttribute:
                    {
                        var propName = payloadAttribute.Name;
                        parameterList.Add("payload." + propName);
                        break;
                    }
                    default:
                        parameterList.Add(parameterInfo.Name);
                        break;
                }
            }

            var callText = $"var result = _service.{methodInfo.Name}({string.Join(',', parameterList)});";
            yield return SyntaxFactory.ParseStatement(callText);

            var returnText = "return Ok(result);";
            yield return SyntaxFactory.ParseStatement(returnText);
        }

        private static Attribute GetCustomAttribute(ParameterInfo parameterInfo)
        {
            Attribute attribute = parameterInfo.GetCustomAttributes(typeof(FromHttpHeaderAttribute)).SingleOrDefault() as
                FromHttpHeaderAttribute;
            if (attribute != null) return attribute;
            attribute =
                parameterInfo.GetCustomAttributes(typeof(FromClaimAttribute)).SingleOrDefault() as FromClaimAttribute;
            if (attribute != null) return attribute;
            attribute =
                parameterInfo.GetCustomAttributes(typeof(FromPayloadAttribute)).SingleOrDefault() as
                    FromPayloadAttribute;
            return attribute;
        }

        private static StatementSyntax HeaderAssignmentStatement(ParameterInfo parameterInfo, int varCount)
        {
            var attribute =
                (FromHttpHeaderAttribute) parameterInfo.GetCustomAttributes(typeof(FromHttpHeaderAttribute)).Single();
            var type = parameterInfo.ParameterType;

            var statement =
                SyntaxFactory.ParseStatement(
                    $@"var value{varCount} = GetFromHttpHeader<{type.FullName}>(""{attribute.Name}"");");
            return statement;
        }

        private static StatementSyntax ClaimAssignmentStatement(ParameterInfo parameterInfo, int varCount)
        {
            var claim = (FromClaimAttribute) parameterInfo.GetCustomAttributes(typeof(FromClaimAttribute)).Single();
            var type = parameterInfo.ParameterType;

            var statement =
                SyntaxFactory.ParseStatement(
                    $@"var value{varCount} = GetFromClaim<{type.FullName}>(""{claim.Name}"");");
            return statement;
        }

        private static StatementSyntax MakeValueCheck(int varCount)
        {
            return SyntaxFactory.ParseStatement(
                $@"if(string.IsNullOrEmpty(value{varCount})){{ return BadRequest(); }} ");
        }
    }
}