using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ToDo.Client.ViewModels;

namespace ToDo.Client
{
    /// <summary>
    /// Interaction logic for TasksTree.xaml
    /// </summary>
    public partial class TasksTree : UserControl
    {
        #region Dependency Properties
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.RegisterAttached("ItemsSource", typeof(ObservableCollection<TaskItemViewModel>), typeof(TasksTree), new PropertyMetadata(ItemSourcePropertyChanged));
        public static TaskItemViewModel GetItemsSource(DependencyObject obj)
        {
            return (TaskItemViewModel)obj.GetValue(SelectedItemProperty);
        }
        public static void SetItemsSource(DependencyObject obj, ObservableCollection<TaskItemViewModel> item)
        {
            obj.SetValue(SelectedItemProperty, item);
        }

        private static void ItemSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TasksTree tree = sender as TasksTree;
            tree.UpdateItemsSource();
        }

        private void UpdateItemsSource()
        {
            vm.UpdateBindings();
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.RegisterAttached("SelectedItem", typeof(TaskItemViewModel), typeof(TasksTree));
        public static TaskItemViewModel GetSelectedItem(DependencyObject obj)
        {
            return (TaskItemViewModel)obj.GetValue(SelectedItemProperty);
        }
        public static void SetSelectedItem(DependencyObject obj, TaskItemViewModel item)
        {
            obj.SetValue(SelectedItemProperty, item);
        }

        #endregion

        private readonly TasksTreeViewModel vm;

        public TasksTree()
        {
            vm = new TasksTreeViewModel(this);

            InitializeComponent();
            this.DataContext = vm;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.SetValue(SelectedItemProperty, e.NewValue);
        }
    }

    public class TasksTreeViewModel : ViewModel
    {
        private ObservableCollection<TasksTreeViewModel> items;
        private TaskItemViewModel selectedItem;

        private TasksTree tree;
        public TasksTreeViewModel(TasksTree tree)
        {
            this.tree = tree;
            UpdateBindings();
        }
        
        public void UpdateBindings()
        {
            this.selectedItem = tree.GetValue(TasksTree.SelectedItemProperty) as TaskItemViewModel;
            this.items = tree.GetValue(TasksTree.ItemsSourceProperty) as ObservableCollection<TasksTreeViewModel>;


        }

        public ObservableCollection<TasksTreeViewModel> ItemsSource
        {
            get
            {
                return items;
            }
        }


    }
}
