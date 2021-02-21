using System.ComponentModel.DataAnnotations;
using Uno.Validation.Shared.Validation;
using Windows.UI.Xaml.Data;

namespace Uno.Validation.Shared.ViewModels
{
    [Bindable]
    public class MainViewModel : ValidatingBase
    {
        private string _firstName;
        private string _lastName;
        private bool _isRequired;

        [MinLength(3), Required, MustBeTrue]
        public string FirstName
        {
            get { return _firstName; }
            set
            {
                Set(ref _firstName, value);
                ValidateProperty(value);
            }
        }

        [Required, MinLength(6)]
        public string LastName
        {
            get { return _lastName; }
            set
            {
                Set(ref _lastName, value);
                ValidateProperty(value);
            }
        }

        [MustBeTrue]
        public bool IsRequired
        {
            get { return _isRequired; }
            set
            {
                Set(ref _isRequired, value);
                ValidateProperty(value);
            }
        }

        public void Validate()
        {
            ValidateObject();
        }
    }
}
