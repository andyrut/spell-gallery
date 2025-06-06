﻿#region Using Directives
using log4net;
using log4net.Config;
using SpellGallery.Configuration;
using SpellGallery.Scryfall;
using SpellGallery.Scryfall.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Exception = System.Exception;
#endregion

namespace SpellGallery
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Private Data Members
        private readonly ILog log = LogManager.GetLogger(typeof(MainWindow));

        // Reusable HTTP Client
        private readonly HttpClient httpClient = new HttpClient();

        // Catalog of large bitmap images by URL to quickly swap cache bitmaps on the image preview
        private readonly Dictionary<string, BitmapImage> urlBitmaps = new Dictionary<string, BitmapImage>();

        // The current thumbnail the user clicked down
        private Image mouseDownThumbnail;

        // The program's settings
        private static SpellGallerySettings settings;
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

                XmlConfigurator.Configure();
                log.InfoFormat("Spell Gallery v{0} program started.", Assembly.GetExecutingAssembly().GetName().Version);
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
                CardNameTextBox.ItemSelected += CardNameTextBoxOnItemSelected;
                settings = SpellGallerySettings.Load();

                Appearance = settings.Appearance;

                if (!Directory.Exists(settings.CustomPicsFolder))
                    throw new DirectoryNotFoundException($"Cockatrice custom pics folder not found: {settings.CustomPicsFolder}{Environment.NewLine}{Environment.NewLine}Please use the Settings to configure your custom pics folder.");
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        // Window is closing
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                log.Info("Spell Gallery program completed.");
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        // User taps a key (for catching <ENTER>)
        private async void CardNameTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key != Key.Enter)
                    return;

                if (CardNameTextBox.SelectedItem != null)
                    return;

                await SearchAsync();
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
                await SearchAsync();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        // User selected an item from the auto-complete dropdown list
        private async void CardNameTextBoxOnItemSelected(object sender, EventArgs e)
        {
            try
            {
                await SearchAsync();
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
        private async void CardOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (mouseDownThumbnail == null)
                    return;

                var image = (Image)sender;

                if (!ReferenceEquals(image, mouseDownThumbnail))
                    return;

                Animate(image);

                var card = (Card)image.Tag;
                await card.StoreAsync(httpClient, settings);
                log.DebugFormat("Stored print for: {0}", card.Name);
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
                PreviewBorder.BorderThickness = new Thickness(0);
                ClickLabel.Visibility = Visibility.Visible;
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
                PreviewBorder.BorderThickness = new Thickness(1);
                ClickLabel.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        // User clicked the settings buton
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settingsWindow = new SettingsWindow(settings);
                if (settingsWindow.ShowDialog() == true)
                {
                    settings = settingsWindow.Settings;
                    settings.Save();

                    Appearance = settings.Appearance;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        // The Window has been resized
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SaveSizeLocation();
        }

        // The Window's location has changed
        private void Window_LocationChanged(object sender, EventArgs e)
        {
            SaveSizeLocation();
        }
        #endregion

        #region Private Methods
        // Perform a card search and display the results as thumbnails
        private async Task SearchAsync()
        {
            string cardName = CardNameTextBox.Text;
            if (string.IsNullOrEmpty(cardName))
                return;

            log.DebugFormat("Searching Scryfall for: {0}", cardName);

            var cards = await ScryfallMethods.GetCardsByNameAsync(cardName);
            cards = cards.Select(x => (Card)x.Clone()).ToList();

            log.DebugFormat("Scryfall search for [{0}] returned {1} result(s)", cardName, cards.Count);

            ThumbnailWrapPanel.Children.Clear();
            
            foreach (var card in cards)
            {
                var image = CreateThumbnailImage(card);
                if (image == null)
                    continue;

                ThumbnailWrapPanel.Children.Add(image);
            }
        }

        // Given a card, creates a thumbnail image in the grid
        private Image CreateThumbnailImage(Card card)
        {
            if (card.ImageUris?.Small == null)
                return null;

            var image = new Image();
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.DownloadFailed += (sender, args) => log.Error($"Error downloading image: {card.ImageUris.Small}", args.ErrorException);
            bitmap.DecodeFailed += (sender, args) => log.Error($"Error decoding image: {card.ImageUris.Small}", args.ErrorException);
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

            return image;
        }

        // Animate the image to indicate it's been selected/clicked
        private void Animate(Image image)
        {
            image.RenderTransform = new ScaleTransform
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

        // Stores the current window size and location in Properties
        private void SaveSizeLocation()
        {
            try
            {
                Properties.Settings.Default.Save();
            }
            catch
            {
                // Ignored
            }
        }

        // Show the error to the user
        private void HandleException(Exception ex)
        {
            log.Error("Encountered an error.", ex);
            MessageBox.Show($"Error: {ex.Message}", Title);
        }
        #endregion
    }
}