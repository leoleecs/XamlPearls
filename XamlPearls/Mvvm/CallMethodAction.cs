using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace XamlPearls.Wpf.Mvvm
{
    [ContentProperty(nameof(Parameters))]
    public class CallMethodAction : TriggerAction<DependencyObject>
    {
        public static readonly DependencyProperty MethodNameProperty = DependencyProperty.Register(nameof(MethodName), typeof(string), typeof(CallMethodAction), new PropertyMetadata(null));
        public static readonly DependencyProperty ParametersProperty = DependencyProperty.Register(nameof(Parameters), typeof(AttachedCollection<Parameter>), typeof(CallMethodAction), new PropertyMetadata(null));
        public static readonly DependencyProperty PassTriggerArgsToMethodProperty = DependencyProperty.Register(nameof(PassTriggerArgsToMethod), typeof(bool), typeof(CallMethodAction), new PropertyMetadata(false));
        public static readonly DependencyProperty TargetObjectProperty = DependencyProperty.Register(nameof(TargetObject), typeof(object), typeof(CallMethodAction), new PropertyMetadata(null));
        public static readonly DependencyProperty TriggerArgsConverterParameterProperty = DependencyProperty.Register(nameof(TriggerArgsConverterParameter), typeof(object), typeof(CallMethodAction), new PropertyMetadata(null));
        public static readonly DependencyProperty TriggerArgsConverterProperty = DependencyProperty.Register(nameof(TriggerArgsConverter), typeof(IValueConverter), typeof(CallMethodAction), new PropertyMetadata(null));

        public CallMethodAction()
        {
            SetValue(ParametersProperty, new AttachedCollection<Parameter>());
        }

        public string MethodName
        {
            get => (string)this.GetValue(MethodNameProperty);
            set => this.SetValue(MethodNameProperty, value);
        }

        public AttachedCollection<Parameter> Parameters => (AttachedCollection<Parameter>)GetValue(ParametersProperty);

        public bool PassTriggerArgsToMethod
        {
            get { return (bool)GetValue(PassTriggerArgsToMethodProperty); }
            set { SetValue(PassTriggerArgsToMethodProperty, value); }
        }

        public object TargetObject
        {
            get => (object)this.GetValue(TargetObjectProperty);
            set => this.SetValue(TargetObjectProperty, value);
        }

        public IValueConverter TriggerArgsConverter
        {
            get => (IValueConverter)GetValue(TriggerArgsConverterProperty);
            set => SetValue(TriggerArgsConverterProperty, value);
        }

        public object TriggerArgsConverterParameter
        {
            get => (object)GetValue(TriggerArgsConverterParameterProperty);
            set => SetValue(TriggerArgsConverterParameterProperty, value);
        }

        private object Target => this.TargetObject ?? AssociatedObject;

        protected override void Invoke(object parameter)
        {
            if (PassTriggerArgsToMethod)
            {
                object parameter2 = parameter;
                if (TriggerArgsConverter != null)
                {
                    parameter2 = TriggerArgsConverter.Convert(parameter, typeof(object), TriggerArgsConverterParameter, CultureInfo.CurrentCulture);
                }
                var methodInfos = this.Target.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (var methodInfo in methodInfos)
                {
                    if (string.Equals(methodInfo.Name, MethodName, StringComparison.Ordinal))
                    {
                        var parameterInfos = methodInfo.GetParameters();
                        if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType.IsInstanceOfType(parameter2))
                        {
                            methodInfo.Invoke(Target, new object[] { parameter2 });
                            return;
                        }
                    }
                }
                throw new MissingMethodException($"Missed method {MethodName}.");
            }
            else
            {
                Type targetType = this.Target.GetType();
                MethodInfo[] methods = targetType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (var methodInfo in methods)
                {
                    if (string.Equals(methodInfo.Name, MethodName, StringComparison.Ordinal))
                    {
                        var parameterInfos = methodInfo.GetParameters();
                        if (parameterInfos.Length == Parameters.Count)
                        {
                            bool match = true;
                            for (int i = 0; i < parameterInfos.Length; i++)
                            {
                                if (Parameters[i].Value == null)
                                {
                                    if (!parameterInfos[i].ParameterType.IsClass && Nullable.GetUnderlyingType(parameterInfos[i].ParameterType) == null)
                                    {
                                        match = false;
                                        break;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                else if (!parameterInfos[i].ParameterType.IsAssignableFrom(Parameters[i].Value.GetType()))
                                {
                                    match = false;
                                    break;
                                }
                            }

                            if (match)
                            {
                                methodInfo.Invoke(Target, Parameters.Select(item => item.Value).ToArray());
                                return;
                            }
                        }
                    }
                }
                throw new MissingMethodException($"Missed method {MethodName}.");
            }
        }
    }


}