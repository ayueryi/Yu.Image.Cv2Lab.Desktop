using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Wpf.Ui;
using Wpf.Ui.Controls;

using Yu.Image.Desktop.ViewModels.Windows;

namespace Yu.Image.Desktop.Views.Windows;

[ObservableObject]
public partial class MainWindow
{
    public MainWindowViewModel ViewModel { get; }

    public ISnackbarService _snackbarService { get; }

    public MainWindow(MainWindowViewModel viewModel, ISnackbarService snackbarService)
    {
        ViewModel = viewModel;

        DataContext = this;
        InitializeComponent();

        _snackbarService = snackbarService;
        _snackbarService.SetSnackbarPresenter(SnackbarPresenter);
    }

    #region Style

    [RelayCommand]
    private void StyleChangeMouseEnterImageAddRegion()
    {
        ImageAddIcon.Foreground = Brushes.LightSkyBlue;
        ImageAddOutlineBorder.Stroke = Brushes.LightSkyBlue;
        ImageAddButton.Foreground = Brushes.LightSkyBlue;
        ImageAddText.Foreground = Brushes.LightSkyBlue;
    }

    [RelayCommand]
    private void StyleChangeMouseLeaveImageAddRegion()
    {
        ImageAddIcon.Foreground = Brushes.White;
        ImageAddOutlineBorder.Stroke = Brushes.White;
        ImageAddButton.Foreground = Brushes.White;
        ImageAddText.Foreground = Brushes.White;
    }

    #endregion

    private void Grid_Drop(object sender, System.Windows.DragEventArgs e)
    {
        StyleChangeMouseEnterImageAddRegionCommand.Execute(null);

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string? file = files?.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(file))
            {
                Trace.WriteLine($"drop file：{file}");

                string fileExtension = Path.GetExtension(file);
                if (fileExtension is not ".jpg" or ".png" or ".bmp")
                {
                    _snackbarService.Show(
                        "Opps.",
                        "拖入的文件似乎不是图像文件呢?",
                        ControlAppearance.Danger,
                        new SymbolIcon(SymbolRegular.Warning24),
                        TimeSpan.FromSeconds(1.5)
                    );
                    return;
                }

                LabCv2Window labWindow = App.GetService<LabCv2Window>();
                labWindow.ViewModel.ImgPath = file;
                labWindow.Show();

                this.Close();
            }
        }
    }

    private void Grid_DragEnter(object sender, System.Windows.DragEventArgs e)
    {
        StyleChangeMouseEnterImageAddRegionCommand.Execute(null);
    }

    private void File_Leave(object sender, MouseEventArgs e)
        => StyleChangeMouseLeaveImageAddRegionCommand.Execute(null);
}