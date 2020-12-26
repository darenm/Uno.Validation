using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using Uno.Extensions;
using Uno.Logging;

namespace CustomMayd.Mvvm.Validation
{
    public class ValidatingBase : DispatchedBindableBase, INotifyDataErrorInfo
    {
        private Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

        public bool HasErrors => _errors.Any();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors([CallerMemberName] string propertyName = "")
        {
            if (_errors.ContainsKey(propertyName))
            {
                return _errors[propertyName];
            }

            return Enumerable.Empty<string>();
        }

        protected void ValidateObject()
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(this);
            var isValid = Validator.TryValidateObject(this, context, results, true); // true also validates properties
            ClearAllErrors();
            ProcessResults(results);
        }

        protected void ValidateProperty(object value, [CallerMemberName] string propertyName = "")
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(this);
            context.MemberName = propertyName;
            var isValid = Validator.TryValidateProperty(value, context, results);
            this.Log().Debug($"{propertyName} - Valid? {isValid} - results: {results.Count}");

            ClearErrors(propertyName);
            ProcessResults(results);
        }

        private void ProcessResults(List<ValidationResult> results)
        {
            foreach (var result in results)
            {
                foreach (var memberName in result.MemberNames)
                {
                    AddError(result.ErrorMessage, memberName);
                }
            }
        }

        protected void AddError(string error, [CallerMemberName] string propertyName = "")
        {
            if (_errors.ContainsKey(propertyName))
            {
                _errors[propertyName].Add(error);
            }
            else
            {
                _errors.Add(propertyName, new List<string> { error });
            }

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        protected void ClearErrors([CallerMemberName] string propertyName = "")
        {
            if (_errors.ContainsKey(propertyName))
            {
                _errors.Remove(propertyName);
            }

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        protected void ClearAllErrors()
        {
            _errors.Clear();

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(string.Empty));
        }

    }
}
