using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace XboxResolution.Services
{
    public static class DisplayService
    {
        const string KeyW = "OverrideWidth";
        const string KeyH = "OverrideHeight";
        const string KeyOn = "OverrideEnabled";

        public const int MaxWidth = 1920;
        public const int MaxHeight = 1080;
        public const int MinWidth = 320;
        public const int MinHeight = 240;

        const string PortalBase = "https://localhost:11443";
        const string DisplayPath = "/api/os/resolution";

        public sealed class Preset
        {
            public string Name;
            public int Width;
            public int Height;
            public override string ToString() { return Name + "  (" + Width + " x " + Height + ")"; }
        }

        public static List<Preset> Presets = new List<Preset>
        {
            new Preset { Name = "Standard", Width = 1920, Height = 1080 },
            new Preset { Name = "Stretch 4:3", Width = 1440, Height = 1080 }
        };

        public static bool Validate(int w, int h, out string error)
        {
            if (w < MinWidth || h < MinHeight) { error = "Mindestens " + MinWidth + "x" + MinHeight + "."; return false; }
            if (w > MaxWidth || h > MaxHeight) { error = "Maximal " + MaxWidth + "x" + MaxHeight + "."; return false; }
            error = null;
            return true;
        }

        public static async Task<bool> ApplyAsync(int w, int h)
        {
            string err;
            if (!Validate(w, h, out err)) throw new ArgumentException(err);

            var s = ApplicationData.Current.LocalSettings;
            s.Values[KeyW] = w;
            s.Values[KeyH] = h;
            s.Values[KeyOn] = true;

            return await SetSystemAsync(w, h);
        }

        public static async Task ResetAsync()
        {
            var s = ApplicationData.Current.LocalSettings;
            s.Values[KeyOn] = false;
            await SetSystemAsync(1920, 1080);
        }

        public static int[] LoadSaved()
        {
            var s = ApplicationData.Current.LocalSettings;
            object on, w, h;
            if (s.Values.TryGetValue(KeyOn, out on) && on is bool b && b
                && s.Values.TryGetValue(KeyW, out w) && w is int iw
                && s.Values.TryGetValue(KeyH, out h) && h is int ih)
            {
                return new int[] { iw, ih };
            }
            return null;
        }

        static async Task<bool> SetSystemAsync(int w, int h)
        {
            try
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (a, b, c, d) => true;
                using (var http = new HttpClient(handler))
                {
                    http.Timeout = TimeSpan.FromSeconds(10);
                    var json = "{\"width\":" + w + ",\"height\":" + h + "}";
                    using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
                    {
                        var resp = await http.PostAsync(PortalBase + DisplayPath, content);
                        if (resp.IsSuccessStatusCode) return true;
                    }
                }
            }
            catch { }
            return WriteOverride(w, h);
        }

        static bool WriteOverride(int w, int h)
        {
            try
            {
                var local = ApplicationData.Current.LocalFolder;
                var dir = System.IO.Path.Combine(local.Path, "..", "LocalState", "DevHomeDisplay");
                System.IO.Directory.CreateDirectory(dir);
                var path = System.IO.Path.Combine(dir, "display_override.json");
                System.IO.File.WriteAllText(path, "{\"enabled\":true,\"width\":" + w + ",\"height\":" + h + "}");
                return true;
            }
            catch { return false; }
        }
    }
}