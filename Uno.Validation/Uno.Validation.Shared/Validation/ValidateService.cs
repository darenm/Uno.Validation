using System.Collections.Generic;
using Uno.Disposables;
using Windows.UI.Xaml;

namespace Uno.Validation.Shared.Validation
{
    public partial class ValidationService : DependencyObject
    {
        private static readonly Dictionary<FrameworkElement, ValidationBroker> _validationBrokers = new Dictionary<FrameworkElement, ValidationBroker>();

        public static readonly DependencyProperty PropertyNameProperty =
        DependencyProperty.RegisterAttached(
          "PropertyName",
          typeof(string),
          typeof(ValidationService),
          new PropertyMetadata(string.Empty)
        );
        public static void SetPropertyName(UIElement element, string value)
        {
            element.SetValue(PropertyNameProperty, value);
            if (element is FrameworkElement fe)
            {
                if (!_validationBrokers.ContainsKey(fe))
                {
                    fe.Unloaded += Fe_Unloaded;
                    _validationBrokers.Add(fe, new ValidationBroker(fe, value));
                }
            }
        }

        private static void Fe_Unloaded(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            fe.Unloaded -= Fe_Unloaded;
            var broker = _validationBrokers[fe];
            broker.Dispose();
            _validationBrokers.Remove(fe);
        }

        public static string GetPropertyName(UIElement element)
        {
            return (string)element.GetValue(PropertyNameProperty);
        }
    }
}

