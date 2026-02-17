using System;
using System.Windows;
using System.Windows.Data;

namespace XamlPearls.StaticConverters
{
    internal abstract class Converter
    {
        protected Converter(
            ConverterErrorStrategy errorStrategy,
            object defaultInputTypeValue,
            object defaultOutputTypeValue,
            Type inputType,
            Type outputType,
            bool isConvertFunctionAvailable,
            bool isConvertBackFunctionAvailable)
        {
            ErrorStrategy = errorStrategy;
            DefaultInputTypeValue = defaultInputTypeValue;
            DefaultOutputTypeValue = defaultOutputTypeValue;
            InputType = inputType;
            OutputType = outputType;
            IsConvertFunctionAvailable = isConvertFunctionAvailable;
            IsConvertBackFunctionAvailable = isConvertBackFunctionAvailable;
        }

        internal ConverterErrorStrategy ErrorStrategy { get; }

        internal object DefaultInputTypeValue { get; }

        internal object DefaultOutputTypeValue { get; }

        internal Type InputType { get; }

        internal Type OutputType { get; }

        internal bool IsConvertFunctionAvailable { get; }

        internal bool IsConvertBackFunctionAvailable { get; }

        internal object GetErrorValue(object defaultValue)
        {
            switch (ErrorStrategy)
            {
                case ConverterErrorStrategy.ReturnDefaultValue:
                    return defaultValue;
                case ConverterErrorStrategy.UseFallbackOrDefaultValue:
                    return DependencyProperty.UnsetValue;
                case ConverterErrorStrategy.DoNothing:
                    return Binding.DoNothing;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}