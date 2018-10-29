using System;

namespace OpenSoftware.WebApiClient
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromHttpHeaderAttribute : Attribute
    {
        public FromHttpHeaderAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
    }
}