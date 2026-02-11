using System.Windows;

namespace PlcDiff.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is ViewModels.MainViewModel viewModel)
        {
            viewModel.SelectedTreeNode = e.NewValue as Models.TreeNodeViewModel;
        }
    }
}
