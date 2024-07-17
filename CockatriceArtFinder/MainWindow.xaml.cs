using CockatriceArtFinder.Scryfall;
using System;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CockatriceArtFinder.Configuration;
using System.Net.Http;
using System.Windows.Media;
using CockatriceArtFinder.Scryfall.Models;

namespace CockatriceArtFinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ArtFinderSettings settings;

        private HttpClient httpClient = new HttpClient();

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                settings = (ArtFinderSettings) ConfigurationManager.GetSection(ArtFinderSettings.SectionName)
                           ?? throw new ConfigurationErrorsException($"Configuration section [{ArtFinderSettings.SectionName}] not found");
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        #region Event Handlers
        private void CardNameTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key != Key.Enter)
                    return;

                Go();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Go();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void CardOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var image = (Image)sender;
                var card = (Card)image.Tag;

                var imageBytes = httpClient.GetByteArrayAsync(card.Image_Uris.Large).Result;
                var artPath = Path.Combine(settings.CustomArtDirectory, $"{card.Name}.jpg");
                
                File.WriteAllBytes(artPath, imageBytes);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        #endregion

        private void Go()
        {
            string cardName = CardNameTextBox.Text;
            if (string.IsNullOrEmpty(cardName))
                return;
            
            var scryfallMethods = new ScryfallMethods(settings.ScryfallApiUrl);
            var cards = scryfallMethods.GetCardsByName(cardName);

            ImageListView.Items.Clear();
            
            foreach (var card in cards)
            {
                var image = new Image();

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(card.Image_Uris.Small, UriKind.Absolute);
                bitmap.EndInit();

                image.Source = bitmap;
                image.Height = 200;
                image.Width = 143;
                image.Stretch = Stretch.UniformToFill;
                image.StretchDirection = StretchDirection.Both;
                image.MouseUp += CardOnMouseUp;
                image.Tag = card;

                ImageListView.Items.Add(image);
            }
        }

        private void HandleException(Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", Title);
        }
    }
}
