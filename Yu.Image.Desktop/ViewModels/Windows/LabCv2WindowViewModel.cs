using System.IO;
using System.Windows.Media.Imaging;

using CommunityToolkit.Mvvm.ComponentModel;

using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace Yu.Image.Desktop.ViewModels.Windows;

public partial class LabCv2WindowViewModel : ObservableObject
{
    #region Mvvm

    [ObservableProperty] private string _imgPath = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ImgSource))]
    private Mat? _img;

    public BitmapSource? ImgSource
    {
        get
        {
            if (Img is null) return null;
            return Img.ToBitmapSource();
        }
    }

    #region Mvvm Properties Changed

    partial void OnImgPathChanged(string? oldValue, string newValue)
    {
        if (!File.Exists(newValue)) return;
        Img = Cv2.ImRead(newValue);
    }

    #endregion

    #endregion
}
