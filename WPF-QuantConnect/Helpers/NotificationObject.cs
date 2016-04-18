using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;

namespace WPF_QuantConnect.Helpers
{
    public abstract class NotificationObject : DependencyObject, INotifyPropertyChanged
    {
        #region Events

        internal event Action InitialisationCompleted;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region All Other Members

        protected internal void RaiseInitialisationCompleted()
        {
            if (InitialisationCompleted != null) InitialisationCompleted();
        }

        protected internal void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected internal void RaisePropertyChanged<T>(T oldValue, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new OldValueNewValuePropertyChangedEventArgs<T>(oldValue, newValue, propertyName));
        }

        protected internal static void SubscribeToCollectionChanged(INotifyCollectionChanged eventSource, NotifyCollectionChangedEventHandler collectionChangedEventHandler)
        {
            eventSource.CollectionChanged -= collectionChangedEventHandler;
            eventSource.CollectionChanged += collectionChangedEventHandler;
        }

        protected internal static void SubscribeToInitialisationCompleted(NotificationObject eventSource, Action initialisationCompletedEventHandler)
        {
            eventSource.InitialisationCompleted -= initialisationCompletedEventHandler;
            eventSource.InitialisationCompleted += initialisationCompletedEventHandler;
        }

        protected internal static void SubscribeToPropertyChanged(INotifyPropertyChanged eventSource, PropertyChangedEventHandler propertyChangedEventHandler)
        {
            eventSource.PropertyChanged -= propertyChangedEventHandler;
            eventSource.PropertyChanged += propertyChangedEventHandler;
        }

        protected internal static void UnSubscribeToCollectionChanged(INotifyCollectionChanged eventSource, NotifyCollectionChangedEventHandler collectionChangedEventHandler)
        {
            eventSource.CollectionChanged -= collectionChangedEventHandler;
        }

        protected internal static void UnSubscribeToInitialisationCompleted(NotificationObject eventSource, Action initialisationCompletedEventHandler)
        {
            eventSource.InitialisationCompleted -= initialisationCompletedEventHandler;
        }

        protected internal static void UnSubscribeToPropertyChanged(INotifyPropertyChanged eventSource, PropertyChangedEventHandler propertyChangedEventHandler)
        {
            eventSource.PropertyChanged -= propertyChangedEventHandler;
        }

        protected internal static PropertyInfo GetProperty<T>(Expression<Func<T>> expr)
        {
            var member = expr.Body as MemberExpression;
            if (member == null)
                throw new InvalidOperationException("Expression is not a member access expression.");
            var property = member.Member as PropertyInfo;
            if (property == null)
                throw new InvalidOperationException("Member in expression is not a property.");
            return property;
        }

        protected internal static string GetPropertyName<T>(Expression<Func<T>> expr)
        {
            var property = GetProperty(expr);
            return property.Name;
        }

        #endregion
    }
    internal class OldValueNewValuePropertyChangedEventArgs<T> : PropertyChangedEventArgs
    {
        internal OldValueNewValuePropertyChangedEventArgs(T oldValue, T newValue, string propertyName) : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        #region Properties

        internal T NewValue { get; set; }
        internal T OldValue { get; set; }

        #endregion
    }
}