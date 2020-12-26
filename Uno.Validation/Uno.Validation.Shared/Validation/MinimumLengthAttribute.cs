using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;

namespace Uno.Validation.Shared.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MinimumLengthAttribute : ValidationAttribute
    {
        private int _minLength;

        public MinimumLengthAttribute(int minLength) : base("{0} cannot be less than " + minLength + " in length")
        {
            _minLength = minLength;
        }

        public MinimumLengthAttribute(int minLength, string errorMessage) : base(errorMessage)
        {
            _minLength = minLength;
        }

        public MinimumLengthAttribute(int minLength, string errorMessage, Func<string> errorMessageAccessor) : base(errorMessageAccessor)
        {
            _minLength = minLength;
        }

        public override bool IsValid(object value)
        {
            bool result = true;
            if (value is string s)
            {
                result = s.Length >= _minLength;
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
