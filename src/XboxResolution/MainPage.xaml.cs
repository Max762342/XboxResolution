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
            var saved = DisplayService.LoadSaved();
            if (saved != null)
                StatusText.Text = "Aktive Auflösung: " + saved[0] + " x " + saved[1];
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
            int w, h;
            if (!int.TryParse(WidthBox.Text.Trim(), out w) || !int.TryParse(HeightBox.Text.Trim(), out h))
            {
                StatusText.Text = "Bitte gültige Zahlen eingeben.";
                return;
            }
            string error;
            if (!DisplayService.Validate(w, h, out error))
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
                StatusText.Text = "Wende " + w + " x " + h + " an...";
                var ok = await DisplayService.ApplyAsync(w, h);
                StatusText.Text = ok
                    ? w + " x " + h + " wurde für den GESAMTEN Dev Mode übernommen (Dev Home + alle Apps)."
                    : w + " x " + h + " gespeichert.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Fehler: " + ex.Message;
            }
        }
    }
}