#region Using Directives
using System;
using System.IO;
using SpellGallery.Configuration;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Win32;
using MessageBox = System.Windows.MessageBox;

#endregion

namespace SpellGallery
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        #region Public Properties
        /// <summary>
        /// The program settings
        /// </summary>
        public SpellGallerySettings Settings { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor with program settings
        /// </summary>
        /// <param name="settings">The program's settings (object will not be modified)</param>
        public SettingsWindow(SpellGallerySettings settings)
        {
            InitializeComponent();

            Settings = (SpellGallerySettings)settings.Clone();
        }
        #endregion

        #region Private Event Handlers
        // Form is loaded for the first time
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Settings == null)
                    return;

                CustomPicsFolderTextBox.Text = Settings.CustomPicsFolder;
                CustomPicsFolderTextBox.Focus();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        // User clicked the Browse button for custom pics folder
        private void CustomPicsFolderBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new FolderBrowserDialog();
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    CustomPicsFolderTextBox.Text = dlg.SelectedPath;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        // User clicks the Save button
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Directory.Exists(CustomPicsFolderTextBox.Text))
                    throw new DirectoryNotFoundException($"Folder does not exist: {CustomPicsFolderTextBox.Text}");

                Settings.CustomPicsFolder = CustomPicsFolderTextBox.Text;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        // User clicks the Use Default button
        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CustomPicsFolderTextBox.Text = new SpellGallerySettings().CustomPicsFolder;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        #endregion

        #region Private Methods
        // Shows an error message to the user
        private void HandleException(Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", Title);
        }
        #endregion
    }
}