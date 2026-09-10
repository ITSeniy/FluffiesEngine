using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FluffiesEngine.Controls;

public partial class ColorEditor : UserControl, INotifyPropertyChanged
{
    private static readonly string[] Palette =
    {
        "#F5F2EC", "#C9C3CF", "#8A8F9C", "#4A4F5C", "#23212B",
        "#E2622B", "#D6A24A", "#E2A52B", "#7FB23A", "#2EA17C",
        "#49C7F2", "#A06BFF", "#FF6FB5", "#D96B86", "#7A4B22",
    };

    private bool _sync;

    public ColorEditor()
    {
        InitializeComponent();
        BuildSwatches();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void Raise([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(ColorEditor), new PropertyMetadata("COLOR"));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
        nameof(Color), typeof(Color), typeof(ColorEditor),
        new FrameworkPropertyMetadata(Colors.Gray, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            (d, _) => ((ColorEditor)d).OnColorChanged()));

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    private void OnColorChanged()
    {
        if (_sync) return;
        _sync = true;
        Raise(nameof(R));
        Raise(nameof(G));
        Raise(nameof(B));
        _sync = false;
    }

    public double R
    {
        get => Color.R;
        set { if (!_sync) Color = Color.FromRgb(Clamp(value), Color.G, Color.B); }
    }

    public double G
    {
        get => Color.G;
        set { if (!_sync) Color = Color.FromRgb(Color.R, Clamp(value), Color.B); }
    }

    public double B
    {
        get => Color.B;
        set { if (!_sync) Color = Color.FromRgb(Color.R, Color.G, Clamp(value)); }
    }

    private static byte Clamp(double v) => (byte)Math.Max(0, Math.Min(255, Math.Round(v)));

    private void BuildSwatches()
    {
        foreach (var hex in Palette)
        {
            var color = (Color)ColorConverter.ConvertFromString(hex);
            var fill = new SolidColorBrush(color);
            fill.Freeze();
            var swatch = new Border
            {
                Width = 18,
                Height = 18,
                CornerRadius = new CornerRadius(5),
                Margin = new Thickness(0, 0, 5, 5),
                Background = fill,
                BorderBrush = new SolidColorBrush(Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF)),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand,
                Tag = color,
            };
            swatch.MouseLeftButtonUp += (s, _) => Color = (Color)((Border)s).Tag;
            SwatchPanel.Children.Add(swatch);
        }
    }
}
