using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace XamlPearls.ValueConverters
{
    // 只用两三次的转换器,建议采用markup extension的方式,不需要单独创建一个实例,也不需要在资源中声明
    // 要用到多次的转换器,建议采用静态的方式,在资源中声明,或者直接在代码中使用静态属性,不需要每次都创建一个实例
    // 静态方式不满足,需要在资源中声明,或者直接在代码中使用静态属性,不需要每次都创建一个实例(建议前者)
    // 静态的方式只是为了写着爽,没考虑内存问题,能不能用静态的方式,主要看转换器的实现,如果转换器的实现是无状态的,或者说状态不重要,那么就可以用静态的方式,如果转换器的实现是有状态的,或者说状态很重要,那么就不能用静态的方式,需要在资源中声明,或者直接在代码中使用静态属性,不需要每次都创建一个实例
    // 这个类的设计是为了让转换器的实现者只需要关注转换的逻辑,而不需要关心实例的创建和管理,同时也提供了一个静态的实例属性,方便在代码中使用,如果需要在资源中声明,也可以直接使用这个类,不需要单独创建一个实例
    // 静态方式的心态就是,如果要用很多次转换器(比如列表元素)就直接用,浪费点内存无所谓,不能用的话,再考虑在资源中声明.
    // 在资源中声明是最省内存的方式,但是写扩展和静态不是为了省内存,纯粹是为了写着爽,如果要省内存,直接在代码中使用静态属性就好了,不需要在资源中声明,但是在资源中声明也是很方便的,不需要每次都创建一个实例,也不需要在代码中使用静态属性,直接在资源中声明就好了.
    public abstract class ValueConverterBase<TSource, TDependency> : MarkupExtension, IValueConverter
    {
        private static T ConvertObjectTo<T>(object @object, CultureInfo culture)
        {
            // 提前处理掉null,后续的转换逻辑就不需要再考虑引用类型和可空值类型是NULL的情况了
            if (@object == null || @object == DBNull.Value || @object == DependencyProperty.UnsetValue) return default;
            // 如果已经是目标类型了,直接返回. 注意:string在这一步已经被处理掉了
            if (@object is T matched) return matched;
            // 1. 处理基本类型的转换 (int, double, bool, DateTime, etc.) 和可空值类型 (Nullable<T>)
            var targetType = typeof(T);
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (@object is IConvertible)
            {
                // @object一定不是null,因为前面已经处理掉了null和DBNull.Value,所以这里直接转换就好了,不需要再考虑null的情况了
                return (T)System.Convert.ChangeType(@object, underlyingType, culture);
            }
            // 2. 处理枚举 (支持 Nullable Enum)
            if (underlyingType.IsEnum)
            {
                if (@object is string str) return (T)Enum.Parse(underlyingType, str, true);
                return (T)Enum.ToObject(underlyingType, @object);
            }

            // 3. 最后的杀手锏：使用 TypeDescriptor (支持自定义类型转换器) // 开销太大,建议全部当字符串处理,然后再手动处理
            //var converter = TypeDescriptor.GetConverter(targetType);
            //if (converter != null && converter.CanConvertFrom(@object.GetType()))
            //{
            //    return (T)converter.ConvertFrom(null, culture, @object);
            //}
            // 4. 这一步可以处理(可空)自定义的值类型
            return (T)@object;
        }

        // sealed可以中断继承链,让子类无法重写这个方法,从而保证这个方法的行为不被改变,同时也可以提高性能,因为编译器可以对sealed方法进行一些优化,比如内联.
        // 在这里并不是为了提升性能,而是为了保证这个方法的行为不被改变,因为这个方法已经是最终实现,不应当被派生类做任何修改.
        public override sealed object ProvideValue(IServiceProvider serviceProvider) => this;

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return Convert(ConvertObjectTo<TSource>(value, culture), targetType, parameter, culture)
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }

        protected abstract TDependency Convert(TSource value, Type targetType, object parameter, CultureInfo culture);

        protected virtual TSource ConvertBack(TDependency value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return ConvertBack(ConvertObjectTo<TDependency>(value, culture), targetType, parameter, culture);
            }
            catch (NotImplementedException)
            {
                throw;
            }
            catch
            {
                return Binding.DoNothing;
            }
        }
    }
}