#region Using Directives
using CockatriceArtFinder.Scryfall;
using CockatriceArtFinder.Scryfall.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endregion

namespace CockatriceArtFinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        // TODO: Documentation throughout

        #region Private Data Members
        private const string ScryfallApiUrl = "https://api.scryfall.com";

        // C:\Users\ARUTLEDGE\AppData\Local\Cockatrice\Cockatrice\pics\CUSTOM
        private readonly string customArtDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Cockatrice", "Cockatrice", "pics", "CUSTOM");

        private readonly HttpClient httpClient = new HttpClient();

        private readonly Dictionary<string, BitmapImage> urlBitmaps = new Dictionary<string, BitmapImage>();
        #endregion

        #region Constructors
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                CardNameTextBox.Focus();

                // TODO: Auto-complete on card name - see https://www.nuget.org/packages/AutoCompleteTextBox

                if (!Directory.Exists(customArtDirectory))
                    throw new DirectoryNotFoundException($"Cockatrice custom pics folder not found: {customArtDirectory}");
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        #endregion

        #region Private Event Handlers
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
                var artPath = Path.Combine(customArtDirectory, $"{card.Name}.jpg");
                
                File.WriteAllBytes(artPath, imageBytes);

                // TODO: Animation after successful selection
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void CardOnMouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                PreviewImage.Source = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void CardOnMouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                var image = (Image)sender;
                var card = (Card)image.Tag;

                if (card == null)
                    return;

                BitmapImage bitmap;

                if (urlBitmaps.ContainsKey(card.Image_Uris.Large))
                {
                    bitmap = urlBitmaps[card.Image_Uris.Large];
                }
                else
                {
                    bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(card.Image_Uris.Large, UriKind.Absolute);
                    bitmap.EndInit();

                    urlBitmaps[card.Image_Uris.Large] = bitmap;
                }

                PreviewImage.Source = bitmap;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        #endregion

        #region Private Methods
        private void Go()
        {
            string cardName = CardNameTextBox.Text;
            if (string.IsNullOrEmpty(cardName))
                return;
            
            var scryfallMethods = new ScryfallMethods(ScryfallApiUrl);
            var cards = scryfallMethods.GetCardsByName(cardName);

            ThumbnailGrid.Children.Clear();
            ThumbnailGrid.RowDefinitions.Clear();

            int col = 0;
            int row = 0;

            foreach (var card in cards)
            {
                if (col == 0)
                    ThumbnailGrid.RowDefinitions.Add(new RowDefinition());

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
                image.MouseEnter += CardOnMouseEnter;
                image.MouseLeave += CardOnMouseLeave;
                image.Tag = card;

                image.SetValue(Grid.ColumnProperty, col);
                image.SetValue(Grid.RowProperty, row);

                ThumbnailGrid.Children.Add(image);

                col++;

                if (col != ThumbnailGrid.ColumnDefinitions.Count)
                    continue;
                
                col = 0;
                row++;
            }
        }

        private void HandleException(Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", Title);
        }
        #endregion
    }
}