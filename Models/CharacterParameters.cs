using System;
using System.Windows.Media;
using FluffiesEngine.Infrastructure;

namespace FluffiesEngine.Models;

/// <summary>
/// All the knobs that define a furry character. Raises <see cref="ObservableObject.PropertyChanged"/>
/// on every edit so the viewport can rebuild live. Use <see cref="Batch"/> to apply many changes
/// (e.g. a species preset) with a single notification.
/// </summary>
public sealed class CharacterParameters : ObservableObject
{
    private bool _suspend;

    protected override void OnPropertyChanged(string? name = null)
    {
        if (_suspend)
            return;
        base.OnPropertyChanged(name);
    }

    /// <summary>Applies several edits at once and notifies listeners exactly once afterwards.</summary>
    public void Batch(Action<CharacterParameters> apply)
    {
        _suspend = true;
        try { apply(this); }
        finally { _suspend = false; }
        base.OnPropertyChanged(string.Empty);
    }

    // --- Identity ---------------------------------------------------------
    private string _name = "Unnamed Fluff";
    public string Name { get => _name; set => Set(ref _name, value); }

    private ModelKind _modelKind = ModelKind.Procedural;
    public ModelKind ModelKind { get => _modelKind; set => Set(ref _modelKind, value); }

    // --- Colors -----------------------------------------------------------
    private Color _furPrimary = Color.FromRgb(0x8A, 0x8F, 0x9C);
    public Color FurPrimary { get => _furPrimary; set => Set(ref _furPrimary, value); }

    private Color _furSecondary = Color.FromRgb(0xEF, 0xEC, 0xE3);
    public Color FurSecondary { get => _furSecondary; set => Set(ref _furSecondary, value); }

    private Color _eyeColor = Color.FromRgb(0xF2, 0xB1, 0x3C);
    public Color EyeColor { get => _eyeColor; set => Set(ref _eyeColor, value); }

    // --- Body proportions -------------------------------------------------
    private double _height = 1.0;       // overall vertical scale
    public double Height { get => _height; set => Set(ref _height, value); }

    private double _build = 1.0;        // torso girth (slim .. chunky)
    public double Build { get => _build; set => Set(ref _build, value); }

    private double _headSize = 1.0;
    public double HeadSize { get => _headSize; set => Set(ref _headSize, value); }

    private double _limbLength = 1.0;
    public double LimbLength { get => _limbLength; set => Set(ref _limbLength, value); }

    // --- Face -------------------------------------------------------------
    private double _snoutLength = 0.7;  // 0 = flat (cat) .. long (wolf)
    public double SnoutLength { get => _snoutLength; set => Set(ref _snoutLength, value); }

    private double _earSize = 1.0;
    public double EarSize { get => _earSize; set => Set(ref _earSize, value); }

    private EarShape _earShape = EarShape.Pointed;
    public EarShape EarShape { get => _earShape; set => Set(ref _earShape, value); }

    private bool _horns;
    public bool Horns { get => _horns; set => Set(ref _horns, value); }

    // --- Tail -------------------------------------------------------------
    private double _tailLength = 1.0;
    public double TailLength { get => _tailLength; set => Set(ref _tailLength, value); }

    private double _tailFluff = 1.0;
    public double TailFluff { get => _tailFluff; set => Set(ref _tailFluff, value); }

    // --- Material ---------------------------------------------------------
    private double _glossiness = 0.25;  // 0 = matte fur .. 1 = wet/scaly sheen
    public double Glossiness { get => _glossiness; set => Set(ref _glossiness, value); }

    /// <summary>Copies every field from <paramref name="other"/> in a single batch.</summary>
    public void CopyFrom(CharacterParameters other) => Batch(p =>
    {
        p.Name = other.Name;
        p.ModelKind = other.ModelKind;
        p.FurPrimary = other.FurPrimary;
        p.FurSecondary = other.FurSecondary;
        p.EyeColor = other.EyeColor;
        p.Height = other.Height;
        p.Build = other.Build;
        p.HeadSize = other.HeadSize;
        p.LimbLength = other.LimbLength;
        p.SnoutLength = other.SnoutLength;
        p.EarSize = other.EarSize;
        p.EarShape = other.EarShape;
        p.Horns = other.Horns;
        p.TailLength = other.TailLength;
        p.TailFluff = other.TailFluff;
        p.Glossiness = other.Glossiness;
    });
}
