using System.Windows;
using System.Windows.Controls;
using ConsoleBoost.UI.ViewModels;

namespace ConsoleBoost.UI.Views;

public partial class GameLibraryView : UserControl
{
    public GameLibraryView()
    {
        InitializeComponent();
    }

    private void UserControl_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private async void UserControl_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && DataContext is GameLibraryViewModel viewModel)
            {
                foreach (string file in files)
                {
                    await viewModel.ImportExecutableFileAsync(file);
                }
            }
        }
    }
}
