#region Using Directives
using Microsoft.Win32;
using SpellGallery.Enums;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
#endregion

namespace SpellGallery.Windows
{
    public class LightDarkWindow : Window, INotifyPropertyChanged
    {
        #region Private Data Members
        // The custom background brush dependency property
        private static readonly DependencyProperty MyBackgroundProperty = DependencyProperty.Register("MyBackground", typeof(Brush), typeof(MainWindow), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(255, 255, 255))));
        
        // The custom foreground brush dependency property
        private static readonly DependencyProperty MyForegroundProperty = DependencyProperty.Register("MyForeground", typeof(Brush), typeof(MainWindow), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(0, 0, 0))));
        
        // The custom border brush dependency property
        private static readonly DependencyProperty MyBorderProperty = DependencyProperty.Register("MyBorder", typeof(Brush), typeof(MainWindow), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(211, 211, 211))));

        // The current appearance of the window
        private Appearance appearance;

        // The last rendered appearance of the window
        private Appearance lastRenderedAppearance;
        #endregion

        #region Public Properties
        /// <summary>
        /// True if the window should minimize then normalize its display in order to refresh the title bar
        /// </summary>
        public bool MinimizeToRefresh { get; set; } = true;
        
        /// <summary>
        /// The current appearance of the window
        /// </summary>
        public Appearance Appearance
        {
            get => appearance;
            set
            {
                bool changed = appearance != value;
                appearance = value;
                if (changed)
                    OnAppearanceChanged();
            }
        }

        /// <summary>
        /// The event handler for when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The custom background brush property
        /// </summary>
        public Brush MyBackground
        {
            get => (Brush)GetValue(MyBackgroundProperty);
            set
            {
                SetValue(MyBackgroundProperty, value);
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// The custom foreground brush property
        /// </summary>
        public Brush MyForeground
        {
            get => (Brush) GetValue(MyForegroundProperty);
            set
            {
                SetValue(MyForegroundProperty, value);
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// The custom border brush property
        /// </summary>
        public Brush MyBorder
        {
            get => (Brush) GetValue(MyBorderProperty);
            set
            {
                SetValue(MyBorderProperty, value);
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// The default constructor
        /// </summary>
        public LightDarkWindow()
        {
            SystemEvents.UserPreferenceChanged += (sender, args) =>
            {
                if (args.Category == UserPreferenceCategory.General)
                   OnAppearanceChanged();
            };
        }
        #endregion

        #region Protected Methods
        // Helper function to invoke the property changed event
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            catch
            {
                // Ignored
            }
        }
        #endregion

        #region Private Methods
        // Called when the appearance changes and re-paints the window if necesaary
        public void OnAppearanceChanged()
        {
            try
            {
                Appearance renderAppearance;

                switch (Appearance)
                {
                    case Appearance.System:
                        renderAppearance = IsSystemDarkMode() ? Appearance.Dark : Appearance.Light;
                        break;
                    case Appearance.Light:
                    case Appearance.Dark:
                        renderAppearance = Appearance;
                        break;
                    default:
                        throw new InvalidOperationException("Appearance setting not initialized");
                }
                
                // Only execute the re-rendering steps if the appearance has changed
                if (renderAppearance == lastRenderedAppearance)
                    return;

                lastRenderedAppearance = renderAppearance;

                MyBackground = new SolidColorBrush(renderAppearance == Appearance.Light ? Color.FromRgb(255, 255, 255) : Color.FromRgb(0, 0, 0));
                MyForeground = new SolidColorBrush(renderAppearance == Appearance.Light ? Color.FromRgb(0, 0, 0) : Color.FromRgb(255, 255, 255));
                MyBorder = new SolidColorBrush(renderAppearance == Appearance.Light ? Color.FromRgb(211, 211, 211) : Color.FromRgb(100, 100, 100));
                SetDarkSystemMenu(renderAppearance == Appearance.Dark);
                SetDarkTitleBar(renderAppearance == Appearance.Dark);
            }
            catch
            {
                // Ignored
            }
        }

        // External method for setting light/dark mode
        [DllImport("uxtheme.dll", EntryPoint = "#135", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int SetPreferredAppMode(int preferredAppMode);

        // External method for flushing menu themes (refreshing app menu)
        [DllImport("uxtheme.dll", EntryPoint = "#136", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern void FlushMenuThemes();

        // Sets the app menu to dark or light
        private static void SetDarkSystemMenu(bool dark)
        {
            SetPreferredAppMode(dark ? 2 : 1);
            FlushMenuThemes();
        }

        // Returns true if the system setting is dark mode
        private static bool IsSystemDarkMode()
        {
            int res = (int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", -1);
            return res == 0;
        }

        // An external method to set dark/light mode
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        // An external constant to indicate Use Immersive Dark Mode
        private const int DwmwaUseImmersiveDarkMode = 20;

        // Sets the title bar to dark/light mode, minimizing/normalizing if needed
        private void SetDarkTitleBar(bool dark)
        {
            try
            {
                IntPtr handle = new WindowInteropHelper(this).Handle;
                int useImmersiveDarkMode = dark ? 1 : 0;
                DwmSetWindowAttribute(handle, DwmwaUseImmersiveDarkMode, ref useImmersiveDarkMode, sizeof(int));

                if (!MinimizeToRefresh)
                    return;
                WindowState = WindowState.Minimized;
                WindowState = WindowState.Normal;
            }
            catch
            {
                // Ignored
            }
        }
        #endregion
    }
}
