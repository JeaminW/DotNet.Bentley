using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Interactivity;

namespace AddinManager.MVVM
{
    public class SelectedItemBehavior : Behavior<System.Windows.Controls.TreeView>
    {
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public static readonly System.Windows.DependencyProperty SelectedItemProperty =
        System.Windows.DependencyProperty.Register("SelectedItem", typeof(object), typeof(SelectedItemBehavior), new System.Windows.UIPropertyMetadata(null, OnSelectedItemChanged));

        private static void OnSelectedItemChanged(System.Windows.DependencyObject sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is System.Windows.Controls.TreeViewItem)
            {
                var item = e.NewValue as System.Windows.Controls.TreeViewItem;

                item.SetValue(System.Windows.Controls.TreeViewItem.IsSelectedProperty, true);
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }
        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
            }
        }
        private void OnTreeViewSelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            this.SelectedItem = e.NewValue;
        }
    }
}
