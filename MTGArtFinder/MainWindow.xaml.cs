﻿#region Using Directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using MTGArtFinder.Scryfall;
using MTGArtFinder.Scryfall.Models;
using Exception = System.Exception;
#endregion

namespace MTGArtFinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Private Data Members
        // Path to the Cockatrice custom art directory, e.g. C:\Users\Username\AppData\Local\Cockatrice\Cockatrice\pics\CUSTOM
        private readonly string customArtDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Cockatrice", "Cockatrice", "pics", "CUSTOM");

        // Reusable HTTP Client
        private readonly HttpClient httpClient = new HttpClient();

        // Catalog of large bitmap images by URL to quickly swap cache bitmaps on the image preview
        private readonly Dictionary<string, BitmapImage> urlBitmaps = new Dictionary<string, BitmapImage>();

        private Image mouseDownThumbnail;
        #endregion

        #region Constructors
        /// <summary>
        /// Main constructor
        /// </summary>
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                
                // TODO: Rename solution - come up with name?
                // TODO: README

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
        // Window loaded for the first time
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CardNameTextBox.Focus();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        // User taps a key, catches <ENTER>
        private async void CardNameTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key != Key.Enter)
                    return;

                await GoAsync();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        // User clicks the Go button
        private async void GoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await GoAsync();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        // User clicks down on a thumbnail card
        private void CardOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                mouseDownThumbnail = (Image)sender;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        // User clicks up a thumbnail card
        private void CardOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (mouseDownThumbnail == null)
                    return;

                var image = (Image)sender;

                if (!ReferenceEquals(image, mouseDownThumbnail))
                    return;

                var card = (Card)image.Tag;

                var imageBytes = httpClient.GetByteArrayAsync(card.ImageUris.Large).Result;
                var artPath = Path.Combine(customArtDirectory, $"{card.Name}.jpg");
                
                File.WriteAllBytes(artPath, imageBytes);

                Animate(image);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        // User's mouse leaves a thumbnail card
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

        // User's mouse enters a thumbnail card
        private void CardOnMouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                var image = (Image)sender;
                var card = (Card)image.Tag;

                if (card == null)
                    return;

                BitmapImage bitmap;

                if (urlBitmaps.ContainsKey(card.ImageUris.Large))
                {
                    bitmap = urlBitmaps[card.ImageUris.Large];
                }
                else
                {
                    bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(card.ImageUris.Large, UriKind.Absolute);
                    bitmap.EndInit();

                    urlBitmaps[card.ImageUris.Large] = bitmap;
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
        // Perform a card search and display the results as thumbnails
        private async Task GoAsync()
        {
            string cardName = CardNameTextBox.Text;
            if (string.IsNullOrEmpty(cardName))
                return;

            var cards = await ScryfallMethods.GetCardsByNameAsync(cardName);

            ThumbnailGrid.Children.Clear();
            ThumbnailGrid.RowDefinitions.Clear();

            int col = 0;
            int row = 0;

            foreach (var card in cards)
            {
                if (col == 0)
                    ThumbnailGrid.RowDefinitions.Add(new RowDefinition());

                var image = CreateThumbnailImage(card, col, row);
                if (image == null)
                    continue;

                ThumbnailGrid.Children.Add(image);

                col++;

                if (col != ThumbnailGrid.ColumnDefinitions.Count)
                    continue;
                
                col = 0;
                row++;
            }
        }

        private Image CreateThumbnailImage(Card card, int col, int row)
        {
            if (card.ImageUris?.Small == null)
                return null;

            var image = new Image();
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(card.ImageUris.Small, UriKind.Absolute);
            bitmap.EndInit();
            image.Source = bitmap;
            
            image.Height = 200;
            image.Width = 143;
            image.Stretch = Stretch.UniformToFill;
            image.StretchDirection = StretchDirection.Both;
            image.MouseDown += CardOnMouseDown;
            image.MouseUp += CardOnMouseUp;
            image.MouseEnter += CardOnMouseEnter;
            image.MouseLeave += CardOnMouseLeave;
            image.Tag = card;

            image.SetValue(Grid.ColumnProperty, col);
            image.SetValue(Grid.RowProperty, row);

            return image;
        }

        private void Animate(Image image)
        {
            image.RenderTransform = new ScaleTransform()
            {
                ScaleX = 1.0,
                ScaleY = 1.0,
                CenterX = image.ActualWidth / 2.0,
                CenterY = image.ActualHeight / 2.0
            };

            var embiggeningXanimation = new DoubleAnimation
            {
                From = 1.5,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromSeconds(0.2))
            };

            var embiggeningYanimation = new DoubleAnimation
            {
                From = 1.5,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromSeconds(0.2))
            };

            var opacityAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(3.5))
            };

            var storyboard = new Storyboard();

            Storyboard.SetTarget(embiggeningXanimation, image);
            Storyboard.SetTargetProperty(embiggeningXanimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
            storyboard.Children.Add(embiggeningXanimation);

            Storyboard.SetTarget(embiggeningYanimation, image);
            Storyboard.SetTargetProperty(embiggeningYanimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
            storyboard.Children.Add(embiggeningYanimation);

            Storyboard.SetTarget(opacityAnimation, SavedLabel);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));
            storyboard.Children.Add(opacityAnimation);

            storyboard.Begin();
        }

        // Show the error to the user
        private void HandleException(Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", Title);
        }
        #endregion
    }
}