using System.Windows;
using System.Windows.Controls;
using GameDrive.ClientV2.Domain.Models;

namespace GameDrive.ClientV2.Dashboard.Controls.ProfileQuickView;

public partial class ProfileQuickView : UserControl
{
    private ProfileQuickViewViewModel _viewModel;

    public GameObject? GameObject
    {
        get => (GameObject?) GetValue(GameObjectProperty);
        set => SetValue(GameObjectProperty, value);
    }
    
    public static readonly DependencyProperty GameObjectProperty = DependencyProperty.Register(
        nameof(GameObject),
        typeof(GameObject),
        typeof(ProfileQuickView),
                new FrameworkPropertyMetadata(
                    defaultValue: null,
                    propertyChangedCallback: OnGameObjectPropertyChanged, 
                    flags: FrameworkPropertyMetadataOptions.AffectsRender 
                )
    );
    
    public ProfileQuickView()
    {
        _viewModel = new ProfileQuickViewViewModel();
        DataContext = _viewModel;
        InitializeComponent();
    }

    public static void OnGameObjectPropertyChanged(
        DependencyObject dependencyObject, 
        DependencyPropertyChangedEventArgs eventArgs
    )
    {
        if (dependencyObject is not ProfileQuickView profileQuickView)
            return;

        profileQuickView._viewModel.GameObject = (GameObject) eventArgs.NewValue; ;
    }
    
    
}