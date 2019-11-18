using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CoderJesus.SortableGridView
{
    /// <summary>
    /// A MVVM friendly GridViewColumnHeader that supports sorting.
    /// </summary>
    public class SortableGridViewColumnHeader : GridViewColumnHeader
    {
        /// <summary>
        /// The column sort direction, this property is read-only.
        /// </summary>
        public ListSortDirection? SortDirection
        {
            get => (ListSortDirection?)GetValue(SortDirectionProperty);
            protected set => SetValue(SortDirectionPropertyKey, value);
        }


        // Register the Dependency Property as read-only.
        private static readonly DependencyPropertyKey SortDirectionPropertyKey = 
            DependencyProperty.RegisterReadOnly("SortDirection",
                                                typeof(ListSortDirection?),
                                                typeof(SortableGridViewColumnHeader),
                                                new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for SortDirection. This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SortDirectionProperty = SortDirectionPropertyKey.DependencyProperty;

        /// <summary>
        /// The property name to sort.
        /// </summary>
        public string SortPropertyName
        {
            get => (string)GetValue(SortPropertyNameProperty);
            set => SetValue(SortPropertyNameProperty, value);
        }

        // Using a DependencyProperty as the backing store for SortPropertyName. This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SortPropertyNameProperty =
            DependencyProperty.Register("SortPropertyName",
                                        typeof(string),
                                        typeof(SortableGridViewColumnHeader),
                                        new PropertyMetadata(null));

        /// <summary>
        /// Create a new Instance of SortableGridViewColumnHeader.
        /// </summary>
        public SortableGridViewColumnHeader()
        {
            AddHandler(ClickEvent, new RoutedEventHandler(SortableGridViewColumnHeader_Click));
        }

        /// <summary>
        /// Attach an event handler to monitor when the SortDescriptionCollection changes.
        /// </summary>
        /// <param name="oldParent"></param>
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            if (GetAncestor<ListView>(this) is ListView listView)
            {
                ((INotifyCollectionChanged)listView.Items.SortDescriptions).CollectionChanged += SortDescriptions_CollectionChanged;
            }
        }

        /// <summary>
        /// Update SortDirection based on the addition or removal its SortDescription.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SortDescriptions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (SortDescription description in e.NewItems)
                    {
                        if (description.PropertyName == SortPropertyName)
                        {
                            SortDirection = description.Direction;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (SortDescription description in e.OldItems)
                    {
                        if (description.PropertyName == SortPropertyName)
                        {
                            SortDirection = null;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    SortDirection = null;
                    break;
            }
        }

        /// <summary>
        /// Sort the collection when the ColumnViewHeader is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SortableGridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (GetAncestor<ListView>(this) is ListView listView &&
                !string.IsNullOrWhiteSpace(SortPropertyName))
            {
                // Enable multi-sorting if Shift + Click
                bool append = Keyboard.Modifiers == ModifierKeys.Shift;
                Sort(listView.Items, append);
            }
        }

        /// <summary>
        /// Sort the collection view using SortDescriptions.
        /// </summary>
        /// <param name="view">ICollectionView to Sort</param>
        /// <param name="append">If true, the SortDescriptions collection will not be cleared.</param>
        private void Sort(ICollectionView view, bool append)
        {
            ListSortDirection direction = ListSortDirection.Ascending;
            List<SortDescription> removeList = new List<SortDescription>();
            bool sortExists = false;

            for (int i = 0; i < view.SortDescriptions.Count; i++)
            {
                SortDescription description = view.SortDescriptions[i];
                if (description.PropertyName == SortPropertyName)
                {
                    direction = (description.Direction == ListSortDirection.Ascending) ?
                        ListSortDirection.Descending : ListSortDirection.Ascending;

                    // The following replace statement does *not* raise NotifyCollectionChangedAction.Replace.
                    // When using MaterialDesign ListSortDirectionIndicator, the sort arrow will disappear/reappear 
                    // instead of transitioning to the opposite arrow.
                    // A workaround for this is to detach the event handler and to reattach after replacing the element.
                    ((INotifyCollectionChanged)view.SortDescriptions).CollectionChanged -= SortDescriptions_CollectionChanged;
                    view.SortDescriptions[i] = new SortDescription(SortPropertyName, direction);
                    SortDirection = direction;
                    ((INotifyCollectionChanged)view.SortDescriptions).CollectionChanged += SortDescriptions_CollectionChanged;

                    sortExists = true;
                }
                else
                {
                    if (!append)
                    {
                        removeList.Add(description);
                    }
                }
            }

            foreach (SortDescription desc in removeList)
            {
                view.SortDescriptions.Remove(desc);
            }

            if (!sortExists)
            {
                view.SortDescriptions.Add(new SortDescription(SortPropertyName, direction));
            }
        }

        private static T GetAncestor<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj == null)
            {
                return null;
            }

            DependencyObject parent = VisualTreeHelper.GetParent(obj);

            while (!(parent == null || parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return (T)parent ?? null;
        }
    }
}
