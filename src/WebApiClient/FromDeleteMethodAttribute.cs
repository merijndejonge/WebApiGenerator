using System;

namespace OpenSoftware.WebApiClient
{
    /// <summary>
    /// Indicates that a method is to be called from an HTTP DELETE verb in a generated controller class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class FromDeleteMethodAttribute : Attribute
    {
    }
}