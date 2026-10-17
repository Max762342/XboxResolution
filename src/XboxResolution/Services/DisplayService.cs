using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace XboxResolution.Services
{
    /// <summary>
    /// Applies a custom display resolution across the Xbox Dev Mode.
    /// The chosen resolution is persisted (LocalSettings + DevHome override file)
    /// so it stays active for the whole Dev Mode even after the app is closed.
    /// </summary>
    public static class DisplayService
    {
        private const string SettingsKeyWidth = "OverrideWidth";
        private const string SettingsKeyHeight = "OverrideHeight";
        private const string SettingsKeyEnabled = "OverrideEnabled";

        public const int MaxWidth = 1920;
        public const int MaxHeight = 1080;
        public const int MinWidth = 320;
        public const int MinHeight = 240;

        public sealed class ResolutionPreset
        {
            public string Name { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public override string ToString() => $"{Name}  ({Width} × {Height})";
        }

        public static readonly List<ResolutionPreset> Presets = new List<ResolutionPreset>
        {
            new ResolutionPreset { Name = "Standard", Width = 1920, Height = 1080 },
            new ResolutionPreset { Name = "Breit 4:3", Width = 1440, Height = 1080 },
        };

        public static bool Validate(int width, int height, out string error)
        {
            if (width < MinWidth || height < MinHeight)
            {
                error = $"Mindestens {MinWidth}×{MinHeight} erforderlich.";
                return false;
            }
            if (width > MaxWidth || height > MaxHeight)
            {
                error = $"Maximal {MaxWidth}×{MaxHeight} erlaubt.";
                return false;
            }
            error = null;
            return true;
        }

        public static async Task<bool> ApplyAsync(int width, int height)
        {
            if (!Validate(width, height, out var error))
                throw new ArgumentException(error);

            var settings = ApplicationData.Current.LocalSettings;
            settings.Values[SettingsKeyWidth] = width;
            settings.Values[SettingsKeyHeight] = height;
            settings.Values[SettingsKeyEnabled] = true;

            return await WriteDevModeOverrideAsync(width, height).ConfigureAwait(true);
        }

        public static async Task ResetAsync()
        {
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values[SettingsKeyEnabled] = false;
            await WriteDevModeOverrideAsync(0, 0).ConfigureAwait(true);
        }

        public static (int Width, int Height)? LoadSaved()
        {
            var settings = ApplicationData.Current.LocalSettings;
            if (settings.Values.TryGetValue(SettingsKeyEnabled, out var enabled) && enabled is bool b && b
                && settings.Values.TryGetValue(SettingsKeyWidth, out var w) && w is int width
                && settings.Values.TryGetValue(SettingsKeyHeight, out var h) && h is int height)
            {
                return (width, height);
            }
            return null;
        }

        private static Task<bool> WriteDevModeOverrideAsync(int width, int height)
        {
            return Task.Run(() =>
            {
                try
                {
                    // Dev Home reads this file when the Dev Mode session starts,
                    // so the resolution stays active even after this app is closed.
                    var local = ApplicationData.Current.LocalFolder;
                    var devDir = System.IO.Path.Combine(local.Path, "..", "LocalState", "DevHomeDisplay");
                    System.IO.Directory.CreateDirectory(devDir);

                    var configPath = System.IO.Path.Combine(devDir, "display_override.json");
                    var json = width <= 0 || height <= 0
                        ? "{\"enabled\":false}"
                        : "{\"enabled\":true,\"width\":" + width + ",\"height\":" + height + "}";
                    System.IO.File.WriteAllText(configPath, json);
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }
    }
}