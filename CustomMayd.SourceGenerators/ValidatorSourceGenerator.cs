using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using Uno.RoslynHelpers;
using System.Diagnostics;

namespace CustomMayd.SourceGenerators
{
    [Generator]
    public class ValidatorSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // Search for the GeneratedPropertyAttribute symbol
            var _generatedPropertyAttributeSymbol =
                context.Compilation.GetTypeByMetadataName("CustomMayd.SourceGenerators.NotifyPropertyChangedAttribute");

            if (_generatedPropertyAttributeSymbol != null)
            {
                // Search in all types defined in the current compilation (not in the dependents)
                var query = from typeSymbol in context.Compilation.SourceModule.GlobalNamespace.GetNamespaceTypes()
                            from property in typeSymbol.GetFields()

                                // Find the attribute on the field
                            let info = property.FindAttributeFlattened(_generatedPropertyAttributeSymbol)
                            where info != null

                            // Group properties by type
                            group property by typeSymbol into g
                            select g;

                foreach (var type in query)
                {
                  
                    //Debug.WriteLine($"SourceGen: Type={type}");

                    // Let's generate the needed class
                    var builder = new IndentedStringBuilder();

                    builder.AppendLineInvariant("using System;");
                    builder.AppendLineInvariant("using System.Collections;");
                    builder.AppendLineInvariant("using System.Collections.Generic;");
                    builder.AppendLineInvariant("using System.ComponentModel;");
                    builder.AppendLineInvariant("using System.ComponentModel.DataAnnotations;");
                    builder.AppendLineInvariant("using System.Linq;");
                    builder.AppendLineInvariant("using System.Runtime.CompilerServices;");
                    builder.AppendLineInvariant("using System.Threading.Tasks;");
                    builder.AppendLineInvariant("using Windows.ApplicationModel.Core;");
                    builder.AppendLineInvariant("using Windows.UI.Core;");
                    builder.AppendLineInvariant("using Uno.Extensions;");
                    builder.AppendLineInvariant("using Uno.Logging;");

                    using (builder.BlockInvariant($"namespace {type.Key.ContainingNamespace}"))
                    {
                        using (builder.BlockInvariant($"partial class {type.Key.Name} : INotifyPropertyChanged, INotifyDataErrorInfo"))
                        {
                            builder.AppendLineInvariant($"private Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();");
                            builder.AppendLineInvariant($"protected CoreDispatcher Dispatcher => CoreApplication.MainView.Dispatcher;");
                            builder.AppendLineInvariant($"public event PropertyChangedEventHandler PropertyChanged;");
                            builder.AppendLineInvariant($"public bool HasErrors => _errors.Any();");
                            builder.AppendLineInvariant($"public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;");

                            foreach (var fieldInfo in type)
                            {
                                var propertyName = fieldInfo.Name.TrimStart('_');

#if DEBUG
                                if (!Debugger.IsAttached)
                                {
                                    Debugger.Launch();
                                }
#endif                                 
                                var attrs = fieldInfo.GetAllAttributes();

                                foreach (var attr in attrs)
                                {
                                    if (attr.AttributeClass.Name == "NotifyPropertyChangedAttribute")
                                    {
                                        continue;
                                    }

                                    if (attr.ConstructorArguments.Any())
                                    {
                                        builder.AppendLineInvariant($"[{attr.AttributeClass}({attr.ConstructorArguments.First().Value})]");
                                    }
                                    else
                                    {
                                        builder.AppendLineInvariant($"[{attr.AttributeClass}]");
                                    }
                                    //var a = attrs.Skip(1).First().ConstructorArguments;
                                    //var na = attrs.First().NamedArguments;
                                    //var f = na.First();

                                }


                                // Uppercase name for camel case
                                propertyName = propertyName[0].ToString().ToUpperInvariant() + propertyName.Substring(1);

                                using (builder.BlockInvariant($"public {fieldInfo.Type} {propertyName}"))
                                {
                                    builder.AppendLineInvariant($"get => {fieldInfo.Name};");

                                    using (builder.BlockInvariant($"set"))
                                    {
                                        builder.AppendLineInvariant($"var previous = {fieldInfo.Name};");
                                        builder.AppendLineInvariant($"SetProperty(ref {fieldInfo.Name}, value);");
                                        builder.AppendLineInvariant($"On{propertyName}Changed(previous, value);");
                                        builder.AppendLineInvariant($"ValidateProperty(value);");
                                    }
                                }

                                builder.AppendLineInvariant($"partial void On{propertyName}Changed({fieldInfo.Type} previous, {fieldInfo.Type} value);");
                            }

                            using (builder.BlockInvariant($"public IEnumerable GetErrors([CallerMemberName] string propertyName = null)"))
                            {
                                using (builder.BlockInvariant($"if (_errors.ContainsKey(propertyName))"))
                                {
                                    builder.AppendLineInvariant($"return _errors[propertyName];");
                                }

                                builder.AppendLineInvariant($"return Enumerable.Empty<string>();");
                            }

                            using (builder.BlockInvariant($"private void ValidateObject()"))
                            {
                                builder.AppendLineInvariant($"var results = new List<ValidationResult>();");
                                builder.AppendLineInvariant($"var context = new ValidationContext(this);");
                                builder.AppendLineInvariant($"var isValid = Validator.TryValidateObject(this, context, results, true);");
                                builder.AppendLineInvariant($"ClearAllErrors();");
                                builder.AppendLineInvariant($"ProcessResults(results);");
                            }

                            using (builder.BlockInvariant($"private void ValidateProperty(object value, [CallerMemberName] string propertyName = null)"))
                            {
                                builder.AppendLineInvariant($"var results = new List<ValidationResult>();");
                                builder.AppendLineInvariant($"var context = new ValidationContext(this);");
                                builder.AppendLineInvariant($"context.MemberName = propertyName;");
                                builder.AppendLineInvariant($"var isValid = Validator.TryValidateProperty(value, context, results);");
                                builder.AppendLineInvariant("this.Log().Debug($\"{{ propertyName}} - Valid? {{isValid}} - results: {{results.Count}}\");");
                                builder.AppendLineInvariant($"ClearAllErrors();");
                                builder.AppendLineInvariant($"ProcessResults(results);");
                            }

                            using (builder.BlockInvariant($"private void ProcessResults(List<ValidationResult> results)"))
                            {
                                using (builder.BlockInvariant($"foreach (var result in results)"))
                                {
                                    using (builder.BlockInvariant($"foreach (var memberName in result.MemberNames)"))
                                    {
                                        builder.AppendLineInvariant($"AddError(result.ErrorMessage, memberName);");
                                    }
                                }
                            }

                            using (builder.BlockInvariant($"private void AddError(string error, [CallerMemberName] string propertyName = null)"))
                            {
                                using (builder.BlockInvariant($"if (_errors.ContainsKey(propertyName))"))
                                {
                                    builder.AppendLineInvariant($"_errors[propertyName].Add(error);");
                                }
                                using (builder.BlockInvariant($"else"))
                                {
                                    builder.AppendLineInvariant("_errors.Add(propertyName, new List<string> {{ error }});");
                                }
                                builder.AppendLineInvariant($"ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));");
                            }

                            using (builder.BlockInvariant($"private void ClearErrors([CallerMemberName] string propertyName = null)"))
                            {
                                using (builder.BlockInvariant($"if (_errors.ContainsKey(propertyName))"))
                                {
                                    builder.AppendLineInvariant($"_errors.Remove(propertyName);");
                                }
                                builder.AppendLineInvariant($"ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));");
                            }

                            using (builder.BlockInvariant($"private void ClearAllErrors()"))
                            {
                                builder.AppendLineInvariant($"_errors.Clear();");
                                builder.AppendLineInvariant($"ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(string.Empty));");
                            }

                            using (builder.BlockInvariant($"private bool SetProperty<T>(ref T backingVariable, T value, [CallerMemberName] string propertyName = null)"))
                            {
                                builder.AppendLineInvariant($"if (EqualityComparer<T>.Default.Equals(backingVariable, value)) return false;");
                                builder.AppendLineInvariant($"backingVariable = value;");
                                builder.AppendLineInvariant($"RaisePropertyChanged(propertyName);");
                                builder.AppendLineInvariant($"return true;");
                            }

                            using (builder.BlockInvariant($"private void RaisePropertyChanged([CallerMemberName] string propertyName = null)"))
                            {
                                builder.AppendLineInvariant($"#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed");
                                builder.AppendLineInvariant($"DispatchAsync(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));");
                                builder.AppendLineInvariant($"#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed");
                            }

                            using (builder.BlockInvariant($"private async Task DispatchAsync(DispatchedHandler callback)"))
                            {
                                builder.AppendLineInvariant($"var hasThreadAccess =");
                                builder.AppendLineInvariant($"#if __WASM__");
                                builder.AppendLineInvariant($"true;");
                                builder.AppendLineInvariant($"#else");
                                builder.AppendLineInvariant($"Dispatcher.HasThreadAccess;");
                                builder.AppendLineInvariant($"#endif");
                                using (builder.BlockInvariant($"if (hasThreadAccess)"))
                                {
                                    builder.AppendLineInvariant($"callback.Invoke();");
                                }
                                using (builder.BlockInvariant($"else"))
                                {
                                    builder.AppendLineInvariant($"await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, callback);");
                                }
                            }

                        }
                    }

                    var sanitizedName = type.Key.ToDisplayString().Replace(".", "_").Replace("+", "_");
                    context.AddSource(sanitizedName, SourceText.From(builder.ToString(), Encoding.UTF8));
                }
            }
        }
    }
}
