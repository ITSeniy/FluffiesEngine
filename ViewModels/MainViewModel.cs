using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using FluffiesEngine.Infrastructure;
using FluffiesEngine.Models;
using FluffiesEngine.Rendering;
using Microsoft.Win32;

namespace FluffiesEngine.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private static readonly Random Rng = new();

    public CharacterParameters Parameters { get; } = new();
    public ObservableCollection<SpeciesPreset> Presets { get; } = new(SpeciesPreset.All);

    private Model3D? _characterModel;
    public Model3D? CharacterModel { get => _characterModel; private set => Set(ref _characterModel, value); }

    private int _triangleCount;
    public int TriangleCount { get => _triangleCount; private set => Set(ref _triangleCount, value); }

    public ICommand ApplyPresetCommand { get; }
    public ICommand RandomizeCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand LoadCommand { get; }

    public MainViewModel()
    {
        ApplyPresetCommand = new RelayCommand(o => ApplyPreset(o as SpeciesPreset));
        RandomizeCommand = new RelayCommand(Randomize);
        ResetCommand = new RelayCommand(() => ApplyPreset(SpeciesPreset.All[0]));
        SaveCommand = new RelayCommand(Save);
        LoadCommand = new RelayCommand(Load);

        Parameters.PropertyChanged += (_, e) =>
        {
            // Renaming the character does not change geometry.
            if (e.PropertyName != nameof(CharacterParameters.Name))
                Rebuild();
        };

        ApplyPreset(SpeciesPreset.All[0]); // Wolf, triggers the first build.
    }

    private void Rebuild()
    {
        var result = Parameters.ModelKind == ModelKind.Vark
            ? VarkCharacterBuilder.Build(Parameters)
            : FurryCharacterBuilder.Build(Parameters);
        CharacterModel = result.Model;
        TriangleCount = result.TriangleCount;
    }

    private void ApplyPreset(SpeciesPreset? preset)
    {
        if (preset is null) return;
        Parameters.Batch(p =>
        {
            p.ModelKind = ModelKind.Procedural; // default; a preset (e.g. Vark) may override it
            preset.Apply(p);
        });
    }

    private void Randomize()
    {
        Parameters.Batch(p =>
        {
            p.ModelKind = ModelKind.Procedural;
            var pool = SpeciesPreset.All.Where(x => x.Name != "Vark").ToList();
            var preset = pool[Rng.Next(pool.Count)];
            preset.Apply(p);
            p.Name = "Random " + preset.Name;
            p.FurPrimary = RandomColor(0.55, 0.85);
            p.FurSecondary = RandomColor(0.7, 0.97);
            p.EyeColor = RandomColor(0.6, 0.95);
            p.Height = Lerp(0.85, 1.2);
            p.Build = Lerp(0.7, 1.4);
            p.HeadSize = Lerp(0.85, 1.25);
            p.LimbLength = Lerp(0.85, 1.2);
            p.SnoutLength = Lerp(0.1, 1.4);
            p.EarSize = Lerp(0.7, 1.5);
            p.EarShape = (EarShape)Rng.Next(3);
            p.Horns = Rng.NextDouble() > 0.65;
            p.TailLength = Lerp(0.5, 1.6);
            p.TailFluff = Lerp(0.5, 1.7);
            p.Glossiness = Lerp(0.1, 0.7);
        });
    }

    private static double Lerp(double a, double b) => a + (b - a) * Rng.NextDouble();

    private static Color RandomColor(double minV, double maxV)
    {
        double h = Rng.NextDouble() * 360.0;
        double s = 0.35 + Rng.NextDouble() * 0.5;
        double v = minV + (maxV - minV) * Rng.NextDouble();
        return FromHsv(h, s, v);
    }

    private static Color FromHsv(double h, double s, double v)
    {
        int i = (int)Math.Floor(h / 60.0) % 6;
        double f = h / 60.0 - Math.Floor(h / 60.0);
        double p = v * (1 - s);
        double q = v * (1 - f * s);
        double t = v * (1 - (1 - f) * s);
        (double r, double g, double b) = i switch
        {
            0 => (v, t, p),
            1 => (q, v, p),
            2 => (p, v, t),
            3 => (p, q, v),
            4 => (t, p, v),
            _ => (v, p, q),
        };
        return Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
    }

    // --- Save / Load (JSON) ----------------------------------------------

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private void Save()
    {
        var dlg = new SaveFileDialog
        {
            Title = "Save character",
            Filter = "Fluffy character (*.fluffy)|*.fluffy|JSON (*.json)|*.json",
            FileName = SanitizeFileName(Parameters.Name) + ".fluffy",
        };
        if (dlg.ShowDialog() != true) return;

        var dto = CharacterDto.From(Parameters);
        File.WriteAllText(dlg.FileName, JsonSerializer.Serialize(dto, JsonOptions));
    }

    private void Load()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Load character",
            Filter = "Fluffy character (*.fluffy;*.json)|*.fluffy;*.json|All files (*.*)|*.*",
        };
        if (dlg.ShowDialog() != true) return;

        var dto = JsonSerializer.Deserialize<CharacterDto>(File.ReadAllText(dlg.FileName), JsonOptions);
        dto?.ApplyTo(Parameters);
    }

    private static string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return string.IsNullOrWhiteSpace(name) ? "character" : name;
    }

    /// <summary>Plain serialisable mirror of <see cref="CharacterParameters"/> (colors as hex).</summary>
    private sealed class CharacterDto
    {
        public string Name { get; set; } = "Unnamed Fluff";
        public ModelKind ModelKind { get; set; } = ModelKind.Procedural;
        public string FurPrimary { get; set; } = "#8A8F9C";
        public string FurSecondary { get; set; } = "#EFECE3";
        public string EyeColor { get; set; } = "#F2B13C";
        public double Height { get; set; } = 1;
        public double Build { get; set; } = 1;
        public double HeadSize { get; set; } = 1;
        public double LimbLength { get; set; } = 1;
        public double SnoutLength { get; set; } = 0.7;
        public double EarSize { get; set; } = 1;
        public EarShape EarShape { get; set; } = EarShape.Pointed;
        public bool Horns { get; set; }
        public double TailLength { get; set; } = 1;
        public double TailFluff { get; set; } = 1;
        public double Glossiness { get; set; } = 0.25;

        public static CharacterDto From(CharacterParameters p) => new()
        {
            Name = p.Name,
            ModelKind = p.ModelKind,
            FurPrimary = Hex(p.FurPrimary),
            FurSecondary = Hex(p.FurSecondary),
            EyeColor = Hex(p.EyeColor),
            Height = p.Height,
            Build = p.Build,
            HeadSize = p.HeadSize,
            LimbLength = p.LimbLength,
            SnoutLength = p.SnoutLength,
            EarSize = p.EarSize,
            EarShape = p.EarShape,
            Horns = p.Horns,
            TailLength = p.TailLength,
            TailFluff = p.TailFluff,
            Glossiness = p.Glossiness,
        };

        public void ApplyTo(CharacterParameters p) => p.Batch(t =>
        {
            t.Name = Name;
            t.ModelKind = ModelKind;
            t.FurPrimary = FromHex(FurPrimary);
            t.FurSecondary = FromHex(FurSecondary);
            t.EyeColor = FromHex(EyeColor);
            t.Height = Height;
            t.Build = Build;
            t.HeadSize = HeadSize;
            t.LimbLength = LimbLength;
            t.SnoutLength = SnoutLength;
            t.EarSize = EarSize;
            t.EarShape = EarShape;
            t.Horns = Horns;
            t.TailLength = TailLength;
            t.TailFluff = TailFluff;
            t.Glossiness = Glossiness;
        });

        private static string Hex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        private static Color FromHex(string hex)
        {
            try { return (Color)ColorConverter.ConvertFromString(hex); }
            catch { return Colors.Gray; }
        }
    }
}
