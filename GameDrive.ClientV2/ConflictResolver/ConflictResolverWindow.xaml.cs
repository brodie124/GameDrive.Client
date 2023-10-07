using System.Windows;

namespace GameDrive.ClientV2.ConflictResolver;

public partial class ConflictResolverWindow : Window
{
    public ConflictResolverWindow(ConflictResolverViewModel conflictResolverViewModel)
    {
        this.DataContext = conflictResolverViewModel;
        InitializeComponent();
    }
}