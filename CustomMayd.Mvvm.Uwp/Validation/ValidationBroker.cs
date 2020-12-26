using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#if NETFX_CORE
using Popup = Windows.UI.Xaml.Controls.Primitives.Popup;
#else
using Popup = Windows.UI.Xaml.Controls.Popup;
#endif
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Uno.Disposables;
using Uno.Extensions;
using Uno.Logging;

namespace CustomMayd.Mvvm.Validation
{
    public class ValidationBroker : IDisposable
    {
        private static SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);

        private Panel _parentPanel;
        private bool _disposedValue;
        private FrameworkElement _control;
        private string _propertyName;
        private Popup _errorPopup;
        private INotifyDataErrorInfo _viewModel;

        public ValidationBroker(FrameworkElement frameworkElement, string propertyName)
        {
            this.Log().Debug("Created ValidationBroker");
            _control = frameworkElement;
            _propertyName = propertyName;
            _control.DataContextChanged += Control_DataContextChanged;

            _errorPopup = new Popup();
            var r = new Rectangle();
            r.Width = 8;
            r.Fill = redBrush;
            _errorPopup.Child = r;
        }

        private void Control_DataContextChanged(DependencyObject sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue is INotifyDataErrorInfo notifyDataErrorInfo)
            {
                if (_viewModel != null)
                {
                    _viewModel.ErrorsChanged -= NotifyDataErrorInfo_ErrorsChanged;
                }

                _viewModel = notifyDataErrorInfo;
                notifyDataErrorInfo.ErrorsChanged += NotifyDataErrorInfo_ErrorsChanged;
            }
        }

        private static DependencyObject GetParent(DependencyObject fe)
        {
            return VisualTreeHelper.GetParent(fe);
        }

        private void NotifyDataErrorInfo_ErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        {
            if (e.PropertyName != _propertyName)
            {
                return;
            }

            this.Log().Debug($"Errors changed for {_propertyName}");

            if (sender is INotifyDataErrorInfo notifyDataErrorInfo)
            {
                var errors = notifyDataErrorInfo.GetErrors(e.PropertyName) as List<string>;

                var t = sender.GetType();
                this.Log().Debug($"Type: {t.FullName}");
                foreach (var p in t.GetProperties())
                {
                    this.Log().Debug($"Property: {p.Name}");
                    var c = p.CustomAttributes;

                    this.Log().Debug($"Attributes: {c.Count()}");
                    foreach (var attr in c)
                    {
                        this.Log().Debug($"Name: {attr.AttributeType}");
                    }
                }

                System.Attribute[] attrs = System.Attribute.GetCustomAttributes(sender.GetType());  // Reflection.
                this.Log().Debug($"Number of attrs: {attrs.Length}");

                if (errors != null && errors.Any())
                {
                    this.Log().Debug($"Number of Errors: {errors.Count}");

                    var sb = new StringBuilder();
                    foreach (var error in errors)
                    {
                        sb.AppendLine(error);
                    }

                    if (_parentPanel == null)
                    {
                        FindParentPanel();
                    }

                    if (_parentPanel == null)
                    {
                        throw new Exception($"No parent panel found for {_control.Name}");
                    }

                    var transform = _control.TransformToVisual(_parentPanel);
                    var point = transform.TransformPoint(new Windows.Foundation.Point(0, 0));

                    if (_errorPopup.Parent == null)
                    {
                        _parentPanel?.Children.Add(_errorPopup);
                        this.Log().Debug($"Popup added to parent");
                    }

                    var rectangle = _errorPopup.Child as Rectangle;
                    rectangle.Height = _control.ActualHeight;
                    rectangle.SetValue(ToolTipService.ToolTipProperty, sb.ToString());
#if NETFX_CORE
                    this.Log().Debug($"NETFX code for Popup");
                    _errorPopup.Translation = new System.Numerics.Vector3((float)point.X - 10, (float)point.Y, 0);
#else
                    this.Log().Debug($"Wasm code for Popup");

                    _errorPopup.IsLightDismissEnabled = false;
                    _errorPopup.HorizontalOffset = point.X - 10;
                    _errorPopup.VerticalOffset = point.Y;
#endif
                    _errorPopup.IsOpen = true;
                }
                else
                {
                    this.Log().Debug($"No errors found");

                    if (_errorPopup.Parent != null)
                    {
                        this.Log().Debug($"Hiding Popup");
                        _errorPopup.IsOpen = false;
                        _parentPanel?.Children.Remove(_errorPopup);
                    }
                }
            }
        }

        private void FindParentPanel()
        {
            var dependencyObject = GetParent(_control);
            while (dependencyObject != null)
            {
                if (dependencyObject is Panel p)
                {
                    _parentPanel = p;
                }
                dependencyObject = GetParent(dependencyObject);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    if (_control != null)
                    {
                        _control.DataContextChanged -= Control_DataContextChanged;
                        _control.TryDispose();
                        _control = null;
                    }

                    if (_viewModel != null)
                    {
                        _viewModel.ErrorsChanged -= NotifyDataErrorInfo_ErrorsChanged;
                        _viewModel = null;
                    }

                    if (_errorPopup != null)
                    {
                        _errorPopup.TryDispose();
                        _errorPopup = null;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ValidationBroker()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
