using System.Windows;
using System.Windows.Controls;

namespace GameDrive.ClientV2.Dashboard.Controls.AppStatus;

public partial class AppStatusControl : UserControl
{
    public AppStatusControl()
    {
        InitializeComponent();
    }

    private void OnCloseStatusClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not AppStatusViewModel appStatusViewModel)
            return;

        appStatusViewModel.Close();
    }
}