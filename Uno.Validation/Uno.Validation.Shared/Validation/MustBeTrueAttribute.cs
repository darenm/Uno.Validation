using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;

namespace Uno.Validation.Shared.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MustBeTrueAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            bool result = true;
            if (value is bool b)
            {
                result = b;
            }
            return result;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(CultureInfo.CurrentCulture,
              ErrorMessageString, name);
        }
    }
}
