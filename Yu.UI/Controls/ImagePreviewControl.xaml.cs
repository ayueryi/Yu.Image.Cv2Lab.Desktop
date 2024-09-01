using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Color = System.Drawing.Color;
using Point = System.Windows.Point;

namespace Yu.UI.Controls;

public partial class ImagePreviewControl : UserControl
{
    #region 字段

    // 用于截图以获取特定像素颜色的 Bitmap 对象，初始大小为 1x1。
    private readonly Bitmap bmp = new(1, 1);

    // 记录上一次图像中心位置（在目标上的位置）。
    private Point? lastCenterPositionOnTarget;

    // 记录上一次拖动操作的鼠标位置。
    private Point? lastDragPoint;

    // 记录鼠标在目标上的最后位置，用于缩放时保持中心位置。
    private Point? lastMousePositionOnTarget;

    #endregion

    #region 依赖属性

    // ImageSource 的 DependencyProperty，用于绑定图像源。
    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(ImagePreviewControl),
            new PropertyMetadata(default(ImageSource), OnImageSourceChanged));

    // Scale 的 DependencyProperty，用于绑定图像的缩放比例。
    public static readonly DependencyProperty ScaleProperty =
        DependencyProperty.Register("Scale", typeof(double), typeof(ImagePreviewControl),
            new PropertyMetadata(1.0, OnScaleChanged));

    /// <summary>
    /// 获取或设置 ImageSource 属性，该属性绑定到要显示的图像。
    /// </summary>
    public ImageSource ImageSource
    {
        get => (ImageSource)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    /// <summary>
    /// 获取或设置 Scale 属性，该属性控制图像的缩放比例。
    /// </summary>
    public double Scale
    {
        get => (double)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }

    #region 依赖属性更新事件

    /// <summary>
    /// 当 ImageSource 属性发生更改时的回调方法，用于更新显示的图像。
    /// </summary>
    /// <param name="d">引发更改的依赖对象。</param>
    /// <param name="e">包含事件数据的参数。</param>
    private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = d as ImagePreviewControl;
        var image = e.NewValue as ImageSource;

        if (control?.DrawingImage != null) control.DrawingImage.Source = image;

        control.DrawingImage.Width = image.Width;
        control.DrawingImage.Height = image.Height;
    }

    /// <summary>
    /// 当 Scale 属性发生更改时的回调方法，用于更新滑块的值。
    /// </summary>
    /// <param name="d">引发更改的依赖对象。</param>
    /// <param name="e">包含事件数据的参数。</param>
    private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = d as ImagePreviewControl;
        if (control?.Slider != null && e.NewValue is double) control.Slider.Value = (double)e.NewValue;
    }

    #endregion

    #endregion

    public ImagePreviewControl()
    {
        InitializeComponent();
    }

    #region 辅助方法

    /// <summary>
    /// 获取当前鼠标在屏幕上的位置。
    /// </summary>
    /// <returns>鼠标的屏幕坐标。</returns>
    private Point GetMousePos() => PointToScreen(Mouse.GetPosition(this));

    /// <summary>
    /// 获取指定坐标位置的颜色值。
    /// </summary>
    /// <param name="x">X 坐标。</param>
    /// <param name="y">Y 坐标。</param>
    /// <returns>指定坐标位置的颜色。</returns>
    public Color GetColorAt(int x, int y)
    {
        var bounds = new Rectangle(x, y, 1, 1);
        using (var g = Graphics.FromImage(bmp))
        {
            g.CopyFromScreen(bounds.Location, System.Drawing.Point.Empty, bounds.Size);
        }

        return bmp.GetPixel(0, 0);
    }

    /// <summary>
    /// 判断 Ctrl 键是否按下。
    /// </summary>
    /// <returns>如果 Ctrl 键按下，返回 true；否则返回 false。</returns>
    public bool IsControlKeyDown() => Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

    #endregion

    #region 画布事件

    /// <summary>
    /// 当滑块值更改时触发，用于调整图像的缩放比例。
    /// </summary>
    /// <param name="sender">事件的发起者。</param>
    /// <param name="e">事件参数，包含旧值和新值。</param>
    private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ScaleTransform == null) return;
        ScaleTransform.ScaleX = e.NewValue;
        ScaleTransform.ScaleY = e.NewValue;

        // 记录视口中心点在目标中的位置
        var centerOfViewport = new Point(ScrollViewer.ViewportWidth / 2, ScrollViewer.ViewportHeight / 2);
        lastCenterPositionOnTarget = ScrollViewer.TranslatePoint(centerOfViewport, Scrollgrid);
    }

    /// <summary>
    /// 处理鼠标移动事件，用于拖动图像或显示鼠标悬停位置的颜色信息。
    /// </summary>
    /// <param name="sender">事件的发起者。</param>
    /// <param name="e">事件参数，包含鼠标位置等信息。</param>
    private void ScrollViewer_MouseMove(object sender, MouseEventArgs e)
    {
        var p = Mouse.GetPosition(DrawingImage);
        if (p.X >= 0 && p.Y >= 0 && p.X <= DrawingImage.ActualWidth && p.Y <= DrawingImage.ActualHeight)
        {
            var r1 = Math.Max(DrawingImage.Source.Width, DrawingImage.Source.Height) /
                     Math.Max(DrawingImage.ActualWidth, DrawingImage.ActualHeight);

            var pos = GetMousePos();
            var color = GetColorAt((int)pos.X, (int)pos.Y);
            var ori = Slider.Value;
            MouseXy.Text = $"X:{(int)(p.X * r1)} Y:{(int)(p.Y * r1)} R:{color.R} G:{color.G} B:{color.B}";
        }

        if (IsControlKeyDown() && lastDragPoint.HasValue)
        {
            var posNow = e.GetPosition(ScrollViewer);
            var dX = posNow.X - lastDragPoint.Value.X;
            var dY = posNow.Y - lastDragPoint.Value.Y;
            lastDragPoint = posNow;
            ScrollViewer.ScrollToHorizontalOffset(ScrollViewer.HorizontalOffset - dX);
            ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset - dY);
            return;
        }

        if (e.LeftButton == MouseButtonState.Pressed)
        {
            var posNow = e.GetPosition(ScrollViewer);
            if (!lastDragPoint.HasValue)
            {
                lastDragPoint = posNow;
                return;
            }

            var dX = posNow.X - lastDragPoint.Value.X;
            var dY = posNow.Y - lastDragPoint.Value.Y;
            lastDragPoint = posNow;
            ScrollViewer.ScrollToHorizontalOffset(ScrollViewer.HorizontalOffset - dX);
            ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset - dY);
            return;
        }

        if (e.LeftButton == MouseButtonState.Released && lastDragPoint != null) lastDragPoint = null;
    }

    /// <summary>
    /// 处理鼠标滚轮预览事件，用于在按下 Ctrl 键时缩放图像。
    /// </summary>
    /// <param name="sender">事件的发起者。</param>
    /// <param name="e">事件参数，包含鼠标滚轮增量等信息。</param>
    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!IsControlKeyDown()) return;
        lastMousePositionOnTarget = Mouse.GetPosition(Scrollgrid);

        if (e.Delta > 0) Slider.Value += 0.1;
        if (e.Delta < 0) Slider.Value -= 0.1;
        if (Slider.Value > Slider.Maximum) Slider.Value = Slider.Maximum;
        if (Slider.Value < Slider.Minimum) Slider.Value = Slider.Minimum;

        e.Handled = true;
    }

    /// <summary>
    /// 处理 ScrollViewer 滚动更改事件，用于在内容尺寸变化时保持图像的中心位置。
    /// </summary>
    /// <param name="sender">事件的发起者。</param>
    /// <param name="e">事件参数，包含滚动和内容尺寸变化信息。</param>
    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
        {
            Point? targetBefore = null;
            Point? targetNow = null;

            if (!lastMousePositionOnTarget.HasValue)
            {
                if (lastCenterPositionOnTarget.HasValue)
                {
                    var centerOfViewport = new Point(ScrollViewer.ViewportWidth / 2, ScrollViewer.ViewportHeight / 2);
                    var centerOfTargetNow = ScrollViewer.TranslatePoint(centerOfViewport, Scrollgrid);

                    targetBefore = lastCenterPositionOnTarget;
                    targetNow = centerOfTargetNow;
                }
            }
            else
            {
                targetBefore = lastMousePositionOnTarget;
                targetNow = Mouse.GetPosition(Scrollgrid);

                lastMousePositionOnTarget = null;
            }

            if (targetBefore.HasValue)
            {
                var dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                var dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                var multiplicatorX = e.ExtentWidth / Scrollgrid.Width;
                var multiplicatorY = e.ExtentHeight / Scrollgrid.Height;

                var newOffsetX = ScrollViewer.HorizontalOffset - dXInTargetPixels * multiplicatorX;
                var newOffsetY = ScrollViewer.VerticalOffset - dYInTargetPixels * multiplicatorY;

                if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY)) return;

                ScrollViewer.ScrollToHorizontalOffset(newOffsetX);
                ScrollViewer.ScrollToVerticalOffset(newOffsetY);
            }
        }
    }

    #endregion
}
