using SpellGallery.Enums;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace SpellGallery.Windows
{
    public class LightDarkWindow : Window, INotifyPropertyChanged
    {
        // The custom background brush property
        private static readonly DependencyProperty MyBackgroundProperty = DependencyProperty.Register("MyBackground", typeof(Brush), typeof(MainWindow), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(255, 255, 255))));
        private static readonly DependencyProperty MyForegroundProperty = DependencyProperty.Register("MyForeground", typeof(Brush), typeof(MainWindow), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(0, 0, 0))));
        private static readonly DependencyProperty MyBorderProperty = DependencyProperty.Register("MyBorder", typeof(Brush), typeof(MainWindow), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(211, 211, 211))));

        private BrightnessMode brightnessMode;

        public BrightnessMode BrightnessMode
        {
            get => brightnessMode;
            set
            {
                bool changed = brightnessMode != value;
                brightnessMode = value;
                if (changed)
                    OnBrightnessChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Brush MyBackground
        {
            get => (Brush)GetValue(MyBackgroundProperty);
            set
            {
                SetValue(MyBackgroundProperty, value);
                RaisePropertyChanged();
            }
        }

        public Brush MyForeground
        {
            get => (Brush) GetValue(MyForegroundProperty);
            set
            {
                SetValue(MyForegroundProperty, value);
                RaisePropertyChanged();
            }
        }

        public Brush MyBorder
        {
            get => (Brush) GetValue(MyBorderProperty);
            set
            {
                SetValue(MyBorderProperty, value);
                RaisePropertyChanged();
            }
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnBrightnessChanged()
        {
            MyBackground = new SolidColorBrush(BrightnessMode == BrightnessMode.Light ? Color.FromRgb(255, 255, 255) : Color.FromRgb(0, 0, 0));
            MyForeground = new SolidColorBrush(BrightnessMode == BrightnessMode.Light ? Color.FromRgb(0, 0, 0) : Color.FromRgb(255, 255, 255));
            MyBorder = new SolidColorBrush(BrightnessMode == BrightnessMode.Light ? Color.FromRgb(211, 211, 211) : Color.FromRgb(100, 100, 100));
        }
    }
}
