using Microsoft.Xaml.Behaviors;
using System.ComponentModel;
using System.Windows;

namespace XamlPearls.Wpf.Mvvm
{
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
}