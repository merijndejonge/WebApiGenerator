using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace OpenSoftware.WebApiGenerator.ControllerInstaller
{
    public class GenericControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        private readonly IEnumerable<Type> _controllerTypes;

        public GenericControllerFeatureProvider(IEnumerable<Type> controllerTypes)
        {
            _controllerTypes = controllerTypes;
        }
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (var controllerType in _controllerTypes)
            {
                feature.Controllers.Add(controllerType.GetTypeInfo());
            }
        }
    }
}