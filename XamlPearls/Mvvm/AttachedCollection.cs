using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;

namespace XamlPearls.Wpf.Mvvm
{
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
}