using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XboxResolution.Services;

namespace XboxResolution
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Show the currently active resolution, if any.
            var saved = DisplayService.LoadSaved();
            if (saved.HasValue)
                StatusText.Text = $"Aktive Auflösung: {saved.Value.Width} × {saved.Value.Height}";
        }

        private async void BtnStandard_Click(object sender, RoutedEventArgs e)
        {
            await ApplyAsync(1920, 1080);
        }

        private async void Btn1440_Click(object sender, RoutedEventArgs e)
        {
            await ApplyAsync(1440, 1080);
        }

        private async void BtnApplyCustom_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(WidthBox.Text.Trim(), out var w) ||
                !int.TryParse(HeightBox.Text.Trim(), out var h))
            {
                StatusText.Text = "Bitte gültige Zahlen für Breite und Höhe eingeben.";
                return;
            }

            if (!DisplayService.Validate(w, h, out var error))
            {
                StatusText.Text = error;
                return;
            }

            await ApplyAsync(w, h);
        }

        private async void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await DisplayService.ResetAsync();
                StatusText.Text = "Auf Standard zurückgesetzt.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Fehler: " + ex.Message;
            }
        }

        private async System.Threading.Tasks.Task ApplyAsync(int w, int h)
        {
            try
            {
                StatusText.Text = $"Wende {w} × {h} an...";
                var ok = await DisplayService.ApplyAsync(w, h);
                StatusText.Text = ok
                    ? $"{w} × {h} wurde für den gesamten Dev Mode übernommen. Die Einstellung bleibt auch ohne App aktiv."
                    : $"{w} × {h} gespeichert. (Dev-Home-Override konnte nicht geschrieben werden.)";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Fehler: " + ex.Message;
            }
        }
    }
}