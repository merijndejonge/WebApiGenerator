using System;

namespace OpenSoftware.WebApiClient
{
    /// <summary>
    /// Indicates that a method is to be called from an HTTP POST verb in a generated controller class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class FromPostMethodAttribute : Attribute
    {
    }
}