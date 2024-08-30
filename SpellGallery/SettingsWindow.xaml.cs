﻿#region Using Directives
using SpellGallery.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
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

        // The About Spell Gallery link was clicked
        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string exeLocation = Assembly.GetExecutingAssembly().Location;
                if (string.IsNullOrEmpty(exeLocation))
                    throw new InvalidOperationException("Could not determine the Spell Gallery EXE's location");

                string exeDir = Path.GetDirectoryName(exeLocation);
                if (string.IsNullOrEmpty(exeDir))
                    throw new DirectoryNotFoundException($"Could not find the directory for path [{exeLocation}]");

                string readmePath = Path.Combine(exeDir, "README.txt");
                Process.Start(readmePath);
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