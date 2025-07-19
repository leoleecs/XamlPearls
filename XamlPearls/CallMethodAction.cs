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

namespace XamlPearls
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

    #region Help class

    /// <summary>
    /// Extension methods for <see cref="IEnumerable&lt;T&gt;"/>
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Applies the action to each element in the list.
        /// </summary>
        /// <typeparam name="T">The enumerable item's type.</typeparam>
        /// <param name="enumerable">The elements to enumerate.</param>
        /// <param name="action">The action to apply to each item in the list.</param>
        public static void Apply<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }
    }

    /// <summary>
    /// A collection that can exist as part of a behavior.
    /// </summary>
    /// <typeparam name="T">The type of item in the attached collection.</typeparam>
    public class AttachedCollection<T> : FreezableCollection<T>, IAttachedObject
        where T : DependencyObject, IAttachedObject
    {
        private DependencyObject associatedObject;

        /// <summary>
        /// Creates an instance of <see cref="AttachedCollection{T}"/>
        /// </summary>
        public AttachedCollection()
        {
            ((INotifyCollectionChanged)this).CollectionChanged += OnCollectionChanged;
        }

        DependencyObject IAttachedObject.AssociatedObject
        {
            get { return associatedObject; }
        }

        /// <summary>
        /// Attached the collection.
        /// </summary>
        /// <param name="dependencyObject">The dependency object to attach the collection to.</param>
        public void Attach(DependencyObject dependencyObject)
        {
            WritePreamble();
            associatedObject = dependencyObject;
            WritePostscript();

            this.Apply(x => x.Attach(associatedObject));
        }

        /// <summary>
        /// Detaches the collection.
        /// </summary>
        public void Detach()
        {
            this.Apply(x => x.Detach());
            WritePreamble();
            associatedObject = null;
            WritePostscript();
        }

        /// <summary>
        /// Called when an item is added from the collection.
        /// </summary>
        /// <param name="item">The item that was added.</param>
        protected virtual void OnItemAdded(T item)
        {
            if (associatedObject != null)
                item.Attach(associatedObject);
        }

        /// <summary>
        /// Called when an item is removed from the collection.
        /// </summary>
        /// <param name="item">The item that was removed.</param>
        protected virtual void OnItemRemoved(T item)
        {
            if (item.AssociatedObject != null)
                item.Detach();
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    e.NewItems.OfType<T>().Where(x => !Contains(x)).Apply(OnItemAdded);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    e.OldItems.OfType<T>().Apply(OnItemRemoved);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    e.OldItems.OfType<T>().Apply(OnItemRemoved);
                    e.NewItems.OfType<T>().Where(x => !Contains(x)).Apply(OnItemAdded);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    this.Apply(OnItemRemoved);
                    this.Apply(OnItemAdded);
                    break;
            }
        }
    }

    /// <summary>
    /// Represents a parameter of an <see cref="ActionMessage"/>.
    /// </summary>
    public class Parameter : Freezable, IAttachedObject
    {
        /// <summary>
        /// A dependency property representing the parameter's value.
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(object),
                typeof(Parameter)
            );

        private DependencyObject associatedObject;

        DependencyObject IAttachedObject.AssociatedObject
        {
            get
            {
                ReadPreamble();
                return associatedObject;
            }
        }

        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        /// <value>The value.</value>
        [Category("Common Properties")]
        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        void IAttachedObject.Attach(DependencyObject dependencyObject)
        {
            WritePreamble();
            associatedObject = dependencyObject;
            WritePostscript();
        }

        void IAttachedObject.Detach()
        {
            WritePreamble();
            associatedObject = null;
            WritePostscript();
        }

        /// <summary>
        /// When implemented in a derived class, creates a new instance of the <see cref="T:System.Windows.Freezable"/> derived class.
        /// </summary>
        /// <returns>The new instance.</returns>
        protected override Freezable CreateInstanceCore()
        {
            return new Parameter();
        }
    }

    #endregion Help class
}