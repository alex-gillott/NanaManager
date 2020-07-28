using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanaManagerAPI.Attributes
{
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
    /// <summary>
    /// Defines a class and method as the entry point of the plugin. Use the method to initialise your plugin
    /// </summary>
    public sealed class EntryPointAttribute : Attribute { }
}