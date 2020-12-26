using System;

namespace CustomMayd.SourceGenerators
{
    [AttributeUsage(AttributeTargets.Field)]
    public class NotifyPropertyChangedAttribute : Attribute
    {
    }
}
