using System;
using System.Collections.Generic;
using System.Text;

namespace CustomMayd.SourceGenerators
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ValidateClassAttribute : Attribute
    {
    }
}
