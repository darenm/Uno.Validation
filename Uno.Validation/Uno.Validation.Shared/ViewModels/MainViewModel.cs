using CustomMayd.SourceGenerators;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Uno.Validation.Shared.Validation;

namespace Uno.Validation.Shared.ViewModels
{
    public partial class MainViewModel
    {
        [NotifyPropertyChanged, MinLength(3), Required, MustBeTrue]
        private string _firstName;
        [NotifyPropertyChanged, Required, MinLength(6)]
        private string _lastName;
        [NotifyPropertyChanged, MustBeTrue]
        private bool _isRequired;

        [NotifyPropertyChanged]
        [MinLength(3), Required, MustBeTrue]
        private int _myProp;

        //[MinLength(3), Required, MustBeTrue]
        //public string FirstName
        //{
        //    get { return _firstName; }
        //    set
        //    {
        //        SetProperty(ref _firstName, value);
        //        ValidateProperty(value);
        //    }
        //}

        //[Required, MinLength(6)]
        //public string LastName
        //{
        //    get { return _lastName; }
        //    set
        //    {
        //        SetProperty(ref _lastName, value);
        //        ValidateProperty(value);
        //    }
        //}

        //[MustBeTrue]
        //public bool IsRequired
        //{
        //    get { return _isRequired; }
        //    set
        //    {
        //        SetProperty(ref _isRequired, value);
        //        ValidateProperty(value);
        //    }
        //}

        public void Validate()
        {
            ValidateObject();
            this.MyProp = 23;

        }
    }
}
