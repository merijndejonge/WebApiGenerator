using System;

namespace OpenSoftware.WebApiClient
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromClaimAttribute : Attribute
    {
        public FromClaimAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
