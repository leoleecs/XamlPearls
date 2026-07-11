using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;

namespace XamlPearls.Miscellaneous
{
    public class RoutedEventTrigger : EventTriggerBase<DependencyObject>
    {
        public RoutedEventTrigger()
        {
        }

        public RoutedEvent RoutedEvent { get; set; }

        protected override string GetEventName() => RoutedEvent.Name;

        protected override void OnAttached()
        {
            Behavior behavior = AssociatedObject as Behavior;
            FrameworkElement associatedElement = AssociatedObject as FrameworkElement;
            if (behavior != null)
            {
                associatedElement = ((IAttachedObject)behavior).AssociatedObject as FrameworkElement;
            }

            if (associatedElement == null)
            {
                throw new ArgumentException("Routed Event trigger can only be associated to framework elements");
            }

            if (RoutedEvent != null)
            {
                associatedElement.AddHandler(RoutedEvent, new RoutedEventHandler(this.OnRoutedEvent));
            }
        }

        //protected override void OnDetaching()
        //{
        //    (AssociatedObject as FrameworkElement).RemoveHandler(RoutedEvent, new RoutedEventHandler(this.OnRoutedEvent));

        //}

        private void OnRoutedEvent(object sender, RoutedEventArgs args) => OnEvent(args);
    }
}