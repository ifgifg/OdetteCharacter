using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace OdetteCharacter.Common.Drawing;

public sealed class AtlasFrame
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public float AnchorX { get; set; }
    public float AnchorY { get; set; }
    public float Scale { get; set; } = 1f;
    public float ShoulderX { get; set; }
    public float ShoulderY { get; set; }
    public float HandX { get; set; }
    public float HandY { get; set; }
    public bool ShowRestArm { get; set; } = true;
    public Rectangle Source => new(X, Y, Width, Height);
    public Vector2 Anchor => new(AnchorX, AnchorY);
}

public sealed class AtlasDefinition
{
    public AtlasFrame[] SeriousFrames { get; set; } = Array.Empty<AtlasFrame>();
    public AtlasFrame[] GentleFrames { get; set; } = Array.Empty<AtlasFrame>();
    public AtlasFrame PortraitSerious { get; set; } = new();
    public AtlasFrame PortraitGentle { get; set; } = new();
    public AtlasFrame[] ArmFrames { get; set; } = Array.Empty<AtlasFrame>();
}

public static class CharacterAtlas
{
    public static AtlasDefinition? Data { get; private set; }
    public static Asset<Texture2D>? Serious { get; private set; }
    public static Asset<Texture2D>? Gentle { get; private set; }
    public static Asset<Texture2D>? Arms { get; private set; }
    public static Asset<Texture2D>? PortraitSerious { get; private set; }
    public static Asset<Texture2D>? PortraitGentle { get; private set; }

    internal static void Load(Mod mod)
    {
        Data = JsonSerializer.Deserialize<AtlasDefinition>(mod.GetFileBytes("Assets/Character/atlas.json"),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new InvalidDataException("OdetteCharacter atlas.json is empty.");
        if (Data.SeriousFrames.Length != 12 || Data.GentleFrames.Length != 12) throw new InvalidDataException("OdetteCharacter needs exactly twelve frames for each expression.");
        if (Data.ArmFrames.Length != 2) throw new InvalidDataException("OdetteCharacter needs straight and bent foreground arm frames.");
        Serious = Request("Serious");
        Gentle = Request("Gentle");
        Arms = Request("Arms");
        PortraitSerious = Request("PortraitSerious");
        PortraitGentle = Request("PortraitGentle");
        foreach (AtlasFrame frame in Data.SeriousFrames) Validate(frame, Serious.Value, "serious");
        foreach (AtlasFrame frame in Data.GentleFrames) Validate(frame, Gentle.Value, "gentle");
        foreach (AtlasFrame frame in Data.ArmFrames) Validate(frame, Arms.Value, "arm");
        Validate(Data.PortraitSerious, PortraitSerious.Value, "serious portrait");
        Validate(Data.PortraitGentle, PortraitGentle.Value, "gentle portrait");
        // This tML build's rawimg converter preserves straight-alpha RGB, while
        // its PNG reader premultiplies. Native player drawing uses AlphaBlend.
        // Normalize only our rawimg-backed assets; PNG source files, icons and
        // other mods' textures are untouched. Reading raw bytes also avoids
        // Texture2D.GetData on the loader thread (not permitted by FNA).
        var normalized = new HashSet<Asset<Texture2D>>();
        NormalizeRawAlpha(mod, Serious, "Serious", normalized);
        NormalizeRawAlpha(mod, Gentle, "Gentle", normalized);
        NormalizeRawAlpha(mod, PortraitSerious, "PortraitSerious", normalized);
        NormalizeRawAlpha(mod, PortraitGentle, "PortraitGentle", normalized);
        NormalizeRawAlpha(mod, Arms, "Arms", normalized);
    }

    private static Asset<Texture2D> Request(string name)
        => ModContent.Request<Texture2D>("OdetteCharacter/Assets/Character/" + name, AssetRequestMode.ImmediateLoad);

    private static void NormalizeRawAlpha(Mod mod, Asset<Texture2D> asset, string name, HashSet<Asset<Texture2D>> normalized)
    {
        if (!normalized.Add(asset)) return;
        string path = "Assets/Character/" + name + ".rawimg";
        if (!mod.FileExists(path)) return; // PngReader has already normalized PNGs.
        using var reader = new BinaryReader(new MemoryStream(mod.GetFileBytes(path)));
        if (reader.ReadInt32() != 1) throw new InvalidDataException("Unsupported OdetteCharacter rawimg version.");
        int width = reader.ReadInt32();
        int height = reader.ReadInt32();
        Texture2D texture = asset.Value;
        if (width != texture.Width || height != texture.Height) throw new InvalidDataException("OdetteCharacter rawimg dimensions do not match the loaded texture.");
        int length = checked(width * height * 4);
        byte[] pixels = reader.ReadBytes(length);
        if (pixels.Length != length) throw new EndOfStreamException("OdetteCharacter rawimg pixels are truncated.");
        for (int i = 0; i < pixels.Length; i += 4) {
            int alpha = pixels[i + 3];
            pixels[i] = (byte)(pixels[i] * alpha / 255);
            pixels[i + 1] = (byte)(pixels[i + 1] * alpha / 255);
            pixels[i + 2] = (byte)(pixels[i + 2] * alpha / 255);
        }
        Main.RunOnMainThread(() => texture.SetData(pixels)).GetAwaiter().GetResult();
    }

    private static void Validate(AtlasFrame frame, Texture2D texture, string kind)
    {
        if (frame.Width <= 0 || frame.Height <= 0 || frame.X < 0 || frame.Y < 0 || frame.X + frame.Width > texture.Width || frame.Y + frame.Height > texture.Height
            || !float.IsFinite(frame.Scale) || frame.Scale <= 0 || frame.Scale > 2f
            || !float.IsFinite(frame.AnchorX) || !float.IsFinite(frame.AnchorY)
            || !float.IsFinite(frame.ShoulderX) || !float.IsFinite(frame.ShoulderY)
            || !float.IsFinite(frame.HandX) || !float.IsFinite(frame.HandY))
            throw new InvalidDataException($"Invalid OdetteCharacter {kind} source rectangle, anchor or scale.");
    }

    internal static void Unload() { Data = null; Serious = null; Gentle = null; Arms = null; PortraitSerious = null; PortraitGentle = null; }
}
