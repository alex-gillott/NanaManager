using System;

namespace NanaManagerAPI.Attributes
{
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
    /// <summary>
    /// Defines a class and method as the entry point of the plugin. Use on a contained method to specify as the entry point
    /// </summary>
    public sealed class EntryPointAttribute : Attribute { }
}