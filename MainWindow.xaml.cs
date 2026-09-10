using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FluffiesEngine.ViewModels;
using Microsoft.Win32;

namespace FluffiesEngine;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    // --- Window chrome controls ------------------------------------------
    private void OnMinimize(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void OnMaximizeRestore(object sender, RoutedEventArgs e)
        => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void OnClose(object sender, RoutedEventArgs e) => Close();

    // --- Export the current view to a PNG --------------------------------
    private void OnExportPng(object sender, RoutedEventArgs e)
    {
        if (Viewport.ActualWidth < 1 || Viewport.ActualHeight < 1)
            return;

        var dlg = new SaveFileDialog
        {
            Title = "Export render",
            Filter = "PNG image (*.png)|*.png",
            FileName = "fluffy-render.png",
        };
        if (dlg.ShowDialog() != true)
            return;

        // 2x super-sampling for a crisp export.
        const double scale = 2.0;
        var bmp = new RenderTargetBitmap(
            (int)(Viewport.ActualWidth * scale),
            (int)(Viewport.ActualHeight * scale),
            96 * scale, 96 * scale, PixelFormats.Pbgra32);
        bmp.Render(Viewport);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bmp));
        using var stream = File.Create(dlg.FileName);
        encoder.Save(stream);
    }
}
