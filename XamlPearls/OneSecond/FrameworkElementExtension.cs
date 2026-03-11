using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace XamlPearls.OneSecond
{
    public static partial class FrameworkElementExtension
    {
        #region IsLoading

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.RegisterAttached("IsLoading", typeof(bool), typeof(FrameworkElementExtension), new PropertyMetadata(IsLoadingPropertyChangedCallback));

        public static bool GetIsLoading(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsLoadingProperty);
        }

        public static void SetIsLoading(DependencyObject obj, bool value)
        {
            obj.SetValue(IsLoadingProperty, value);
        }

        private static void Control_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement control)
                control.Loading(GetIsLoading(control));
        }

        private static void IsLoadingPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement control)
            {
                if (!control.IsLoaded)
                {
                    control.Loaded -= Control_Loaded;
                    control.Loaded += Control_Loaded;
                }
                else
                {
                    control.Loading((bool)e.NewValue);
                }
            }
        }

        #endregion IsLoading

        #region MaskContent

        public static readonly DependencyProperty MaskContentProperty =
            DependencyProperty.RegisterAttached("MaskContent", typeof(object), typeof(FrameworkElementExtension));

        public static object GetMaskContent(DependencyObject obj)
        {
            return obj.GetValue(MaskContentProperty);
        }

        public static void SetMaskContent(DependencyObject obj, object value)
        {
            obj.SetValue(MaskContentProperty, value);
        }

        #endregion MaskContent

        public static void Loading(this FrameworkElement element, bool isOpen = true)
        {
            if (element == null) return;
            //移除遮罩
            if (!isOpen)
            {
                ClearAdorners(element);
                return;
            }

            //获取遮罩元素
            var maskContent = GetMaskContent(element);
            if (maskContent == null)
            {
                maskContent = element.TryFindResource("MaskContent");
                if (maskContent == null)
                {
                    maskContent = new DefaultLoading()
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                }
            }

            //获取装饰层
            var adornerLayer = AdornerLayer.GetAdornerLayer(element);
            if (adornerLayer == null)
            {
                throw new Exception("未找到装饰层。");
            }

            //添加遮罩层
            if (maskContent is FrameworkElement maskElement)
            {
                if (maskElement.Parent != null)
                {
                    var type = maskContent.GetType();
                    var properties = type.GetProperties().Where(p => p.CanWrite).ToArray();
                    var obj = Activator.CreateInstance(type);
                    foreach (var property in properties)
                    {
                        if (property.Name == "Content" || property.Name == "Child" || property.Name == "Children")
                            continue;
                        property.SetValue(obj, property.GetValue(maskContent));
                    }
                    maskContent = obj;
                }
            }
            adornerLayer.Add(new MaskAdorner(element, maskContent));
        }

        private static void ClearAdorners(FrameworkElement element)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(element);
            if (adornerLayer != null)
            {
                Adorner[] adorners = adornerLayer.GetAdorners(element);
                if (adorners != null)
                {
                    for (int i = 0; i < adorners.Length; i++)
                    {
                        if (adorners[i] is IDisposable disposable)
                            disposable.Dispose();
                        adornerLayer.Remove(adorners[i]);
                    }
                }
            }
        }
    }
}