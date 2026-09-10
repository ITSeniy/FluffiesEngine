using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace FluffiesEngine.Models;

/// <summary>A named starting point that fills in every parameter for a species archetype.</summary>
public sealed class SpeciesPreset
{
    public required string Name { get; init; }
    public required string Emoji { get; init; }
    public required Action<CharacterParameters> Apply { get; init; }

    private static Color Hex(string hex)
    {
        hex = hex.TrimStart('#');
        return Color.FromRgb(
            System.Convert.ToByte(hex.Substring(0, 2), 16),
            System.Convert.ToByte(hex.Substring(2, 2), 16),
            System.Convert.ToByte(hex.Substring(4, 2), 16));
    }

    /// <summary>The built-in roster shown in the species palette.</summary>
    public static readonly IReadOnlyList<SpeciesPreset> All = new[]
    {
        new SpeciesPreset
        {
            Name = "Wolf", Emoji = "🐺",
            Apply = p =>
            {
                p.Name = "Grey Wolf"; p.FurPrimary = Hex("#6E7484"); p.FurSecondary = Hex("#E7E4DC");
                p.EyeColor = Hex("#E2A52B"); p.Height = 1.05; p.Build = 1.05; p.HeadSize = 1.0;
                p.LimbLength = 1.05; p.SnoutLength = 0.85; p.EarSize = 1.0; p.EarShape = EarShape.Pointed;
                p.Horns = false; p.TailLength = 1.1; p.TailFluff = 1.15; p.Glossiness = 0.2;
            }
        },
        new SpeciesPreset
        {
            Name = "Fox", Emoji = "🦊",
            Apply = p =>
            {
                p.Name = "Red Fox"; p.FurPrimary = Hex("#E2622B"); p.FurSecondary = Hex("#F7F2EA");
                p.EyeColor = Hex("#7FB23A"); p.Height = 0.95; p.Build = 0.85; p.HeadSize = 1.0;
                p.LimbLength = 1.0; p.SnoutLength = 1.15; p.EarSize = 1.25; p.EarShape = EarShape.Pointed;
                p.Horns = false; p.TailLength = 1.35; p.TailFluff = 1.5; p.Glossiness = 0.22;
            }
        },
        new SpeciesPreset
        {
            Name = "Cat", Emoji = "🐱",
            Apply = p =>
            {
                p.Name = "House Cat"; p.FurPrimary = Hex("#3B3540"); p.FurSecondary = Hex("#C9C3CF");
                p.EyeColor = Hex("#56C26B"); p.Height = 0.9; p.Build = 0.85; p.HeadSize = 1.05;
                p.LimbLength = 0.95; p.SnoutLength = 0.3; p.EarSize = 1.05; p.EarShape = EarShape.Pointed;
                p.Horns = false; p.TailLength = 1.2; p.TailFluff = 0.7; p.Glossiness = 0.32;
            }
        },
        new SpeciesPreset
        {
            Name = "Husky", Emoji = "🐶",
            Apply = p =>
            {
                p.Name = "Snow Husky"; p.FurPrimary = Hex("#4A4F5C"); p.FurSecondary = Hex("#F4F5F7");
                p.EyeColor = Hex("#49C7F2"); p.Height = 1.05; p.Build = 1.15; p.HeadSize = 1.0;
                p.LimbLength = 1.0; p.SnoutLength = 0.8; p.EarSize = 0.95; p.EarShape = EarShape.Pointed;
                p.Horns = false; p.TailLength = 1.05; p.TailFluff = 1.35; p.Glossiness = 0.2;
            }
        },
        new SpeciesPreset
        {
            Name = "Bunny", Emoji = "🐰",
            Apply = p =>
            {
                p.Name = "Cottontail"; p.FurPrimary = Hex("#EDE6DC"); p.FurSecondary = Hex("#F7C6D2");
                p.EyeColor = Hex("#D96B86"); p.Height = 0.9; p.Build = 0.95; p.HeadSize = 1.1;
                p.LimbLength = 0.9; p.SnoutLength = 0.25; p.EarSize = 1.4; p.EarShape = EarShape.Long;
                p.Horns = false; p.TailLength = 0.35; p.TailFluff = 1.7; p.Glossiness = 0.15;
            }
        },
        new SpeciesPreset
        {
            Name = "Dragon", Emoji = "🐲",
            Apply = p =>
            {
                p.Name = "Scale Drake"; p.FurPrimary = Hex("#2EA17C"); p.FurSecondary = Hex("#F2D85C");
                p.EyeColor = Hex("#F2C027"); p.Height = 1.1; p.Build = 1.1; p.HeadSize = 1.0;
                p.LimbLength = 1.05; p.SnoutLength = 1.0; p.EarSize = 0.8; p.EarShape = EarShape.Pointed;
                p.Horns = true; p.TailLength = 1.55; p.TailFluff = 0.55; p.Glossiness = 0.78;
            }
        },
        new SpeciesPreset
        {
            Name = "Lion", Emoji = "🦁",
            Apply = p =>
            {
                p.Name = "Savanna Lion"; p.FurPrimary = Hex("#D6A24A"); p.FurSecondary = Hex("#7A4B22");
                p.EyeColor = Hex("#9A6B1E"); p.Height = 1.1; p.Build = 1.25; p.HeadSize = 1.1;
                p.LimbLength = 1.05; p.SnoutLength = 0.7; p.EarSize = 0.9; p.EarShape = EarShape.Round;
                p.Horns = false; p.TailLength = 1.25; p.TailFluff = 0.6; p.Glossiness = 0.22;
            }
        },
        new SpeciesPreset
        {
            Name = "Panther", Emoji = "🐆",
            Apply = p =>
            {
                p.Name = "Night Panther"; p.FurPrimary = Hex("#23212B"); p.FurSecondary = Hex("#4A4757");
                p.EyeColor = Hex("#F2D34A"); p.Height = 1.0; p.Build = 0.95; p.HeadSize = 1.0;
                p.LimbLength = 1.1; p.SnoutLength = 0.45; p.EarSize = 1.0; p.EarShape = EarShape.Round;
                p.Horns = false; p.TailLength = 1.35; p.TailFluff = 0.55; p.Glossiness = 0.45;
            }
        },
        new SpeciesPreset
        {
            // Bespoke, high-detail latex "Aeroket" — built by VarkCharacterBuilder.
            Name = "Vark", Emoji = "🚀",
            Apply = p =>
            {
                p.ModelKind = ModelKind.Vark;
                p.Name = "Vark — F-111 Aardvark";
                p.FurPrimary = Hex("#262A2E");   // latex body (charcoal)
                p.FurSecondary = Hex("#717C54"); // military olive green
                p.EyeColor = Hex("#F2A92E");     // glowing amber
                p.Height = 1.0; p.Build = 1.0; p.HeadSize = 1.0; p.LimbLength = 1.0;
                p.EarSize = 1.0; p.TailLength = 1.0; p.TailFluff = 1.0;
                p.Glossiness = 0.9;              // wet latex sheen
            }
        },
    };
}
