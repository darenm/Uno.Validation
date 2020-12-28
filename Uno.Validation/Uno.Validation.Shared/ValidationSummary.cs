using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Uno.Extensions.Specialized;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace Uno.Validation
{
    public sealed class ValidationSummary : Control
    {
        public ValidationSummary()
        {
            this.DefaultStyleKey = typeof(ValidationSummary);
        }

        public INotifyDataErrorInfo ViewModel
        {
            get { return (INotifyDataErrorInfo)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(INotifyDataErrorInfo), typeof(ValidationSummary), new PropertyMetadata(null, ViewModelChanged));
        private Grid _errorsGrid;
        private ItemsControl _errorList;
        private TextBlock _title;

        private static void ViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = d as ValidationSummary;
            if (e.OldValue is INotifyDataErrorInfo oldViewModel)
            {
                oldViewModel.ErrorsChanged -= instance.ViewModel_ErrorsChanged;
                instance?.ClearErrors();
            }

            if (e.NewValue is INotifyDataErrorInfo newViewModel)
            {
                newViewModel.ErrorsChanged += instance.ViewModel_ErrorsChanged;
                instance?.UpdateErrors(newViewModel);
            }
        }

        private void ClearErrors()
        {
            _errorsGrid.Visibility = Visibility.Collapsed;
        }

        private void UpdateErrors(INotifyDataErrorInfo newViewModel)
        {
            if (_errorsGrid == null || _errorList == null || _title == null)
            {
                return;
            }

            if (newViewModel == null)
            {
                return;
            }

            var allErrors = newViewModel.GetErrors("__AllErrors__").Cast<string>();
            if (allErrors.Any())
            {
                _errorsGrid.Visibility = Visibility.Visible;
                _title.Text = $"{allErrors.Count()} error{(allErrors.Count() > 1 ? "s" : "")}";
                _errorList.Items.Clear();
                foreach (var error in allErrors)
                {
                    _errorList.Items.Add(error);
                }
            }
            else
            {
                _errorsGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void ViewModel_ErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        {
            if (sender is INotifyDataErrorInfo newViewModel)
            {
                UpdateErrors(newViewModel);
            }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("Title") is TextBlock title)
            {
                _title = title;
            }

            if (GetTemplateChild("ErrorList") is ItemsControl errorList)
            {
                _errorList = errorList;
            }

            if (GetTemplateChild("ErrorsGrid") is Grid errorsGrid)
            {
                _errorsGrid = errorsGrid;
                if (ViewModel != null)
                {
                    UpdateErrors(ViewModel);
                }
            }
        }
    }
}
