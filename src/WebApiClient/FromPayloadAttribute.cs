using System;

namespace OpenSoftware.WebApiClient
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromPayloadAttribute : Attribute
    {
        public FromPayloadAttribute()
        {
            
        }
        public FromPayloadAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
    }
}