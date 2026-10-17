using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace XboxResolution.Services
{
    /// <summary>
    /// Applies a custom display resolution across the WHOLE Xbox Dev Mode
    /// (Dev Home lobby + all apps) via the local Device Portal display API.
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

        private const string DevicePortalBase = "https://localhost:11443";
        private const string DisplaySettingsPath = "/api/os/resolution";

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
            new ResolutionPreset { Name = "Stretch 4:3", Width = 1440, Height = 1080 },
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

            return await SetSystemResolutionAsync(width, height).ConfigureAwait(true);
        }

        public static async Task ResetAsync()
        {
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values[SettingsKeyEnabled] = false;
            await SetSystemResolutionAsync(1920, 1080).ConfigureAwait(true);
        }

        public static (int Width, int Height)? LoadSaved()
        {
            var settings = ApplicationData.Current.LocalSettings;
            if (settings.Values.TryGetValue(SettingsKeyEnabled, out var enabled) && enabled is bool b && b
                && settings.Values.TryGetValue(SettingsKeyWidth, out var w) && w is int width
                && settings.Values.TryGetValue(SettingsKeyHeight, out var h) && h is int height)
            {
                return (width, height);

        /// <summary>
        /// Talks to the local Device Portal display endpoint - this is what makes
        /// the resolution apply to the WHOLE Dev Mode (Dev Home lobby included).
        /// </summary>
        private static async Task<bool> SetSystemResolutionAsync(int width, int height)
        {
            try
            {
                using (var http = new HttpClient(new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (a, b, c, d) => true
                })
                {
                    Timeout = TimeSpan.FromSeconds(10)
                })
                {
                    var payload = "{\"width\":" + width + ",\"height\":" + height + "}";
                    using (var content = new StringContent(payload, Encoding.UTF8, "application/json"))
                    {
                        var resp = await http.PostAsync(DevicePortalBase + DisplaySettingsPath, content)
                            .ConfigureAwait(false);
                        if (resp.IsSuccessStatusCode)
                            return true;
                    }
                }
            }
            catch
            {
                // fall through to the Dev Home override file
            }

            return WriteDevHomeOverride(width, height);
        }

        private static bool WriteDevHomeOverride(int width, int height)
        {
            try
            {
                var local = ApplicationData.Current.LocalFolder;
                var devDir = System.IO.Path.Combine(local.Path, "..", "LocalState", "DevHomeDisplay");
                System.IO.Directory.CreateDirectory(devDir);
                var configPath = System.IO.Path.Combine(devDir, "display_override.json");
                var json = "{\"enabled\":true,\"width\":" + width + ",\"height\":" + height + "}";
                System.IO.File.WriteAllText(configPath, json);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
            }
            return null;
        }