using Yu.Image.Desktop.ViewModels.Windows;

namespace Yu.Image.Desktop.Views.Windows;

public partial class LabCv2Window
{
    public LabCv2WindowViewModel ViewModel { get; }
    public LabCv2Window(LabCv2WindowViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}
