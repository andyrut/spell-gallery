#region Using Directives

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
using log4net;
using log4net.Config;
using SpellGallery.Configuration;
using SpellGallery.Scryfall;
using SpellGallery.Scryfall.Models;
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
        private SpellGallerySettings settings;
        #endregion

        #region Constructors
        /// <summary>
        /// Main constructor
        /// </summary>
        public MainWindow()
        {
            try
            {
                XmlConfigurator.Configure();
                log.Info("Spell Gallery program started.");
                InitializeComponent();
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

                if (!Directory.Exists(settings.CustomPicsFolder))
                    throw new DirectoryNotFoundException($"Cockatrice custom pics folder not found: {settings.CustomPicsFolder}{Environment.NewLine}{Environment.NewLine}Please use the Settings to configure your custom pics folder.");
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

        // User selected an item from the auto-complete dropdown list
        private async void CardNameTextBoxOnItemSelected(object sender, EventArgs e)
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
                var artPath = Path.Combine(settings.CustomPicsFolder, $"{card.Name}.jpg");
                
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
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        // Form is unloaded
        private void Window_Unloaded(object sender, EventArgs e)
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
        #endregion

        #region Private Methods
        // Perform a card search and display the results as thumbnails
        private async Task GoAsync()
        {
            string cardName = CardNameTextBox.Text;
            if (string.IsNullOrEmpty(cardName))
                return;

            log.Debug($"Searching Scryfall for: {cardName}");

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

        // Given a card and location, creates a thumbnail image in the grid
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

        // Animate the image to indicate it's been selected/clicked
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
            log.Error("Encountered an error.", ex);
            MessageBox.Show($"Error: {ex.Message}", Title);
        }
        #endregion
    }
}