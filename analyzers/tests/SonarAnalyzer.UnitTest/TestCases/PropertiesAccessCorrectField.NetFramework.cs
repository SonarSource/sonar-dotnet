// https://github.com/SonarSource/sonar-dotnet/issues/3442
// Also see "Add WPF support in unit tests when targeting .Net Core" https://github.com/SonarSource/sonar-dotnet/issues/4883
namespace TestCases
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class CustomControl : UserControl
    {
        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor", typeof(Color), typeof(CustomControl), new PropertyMetadata(Colors.Red, ColorPropertyChanged));

        public static readonly DependencyProperty ForegroundColorProperty =
            DependencyProperty.Register("ForegroundColor", typeof(Color), typeof(CustomControl), new PropertyMetadata(Colors.Red, ColorPropertyChanged));

        private Color backgroundColor;
        private Color foregroundColor;

        public Color BackgroundColor
        {
            get => (Color)GetValue(BackgroundColorProperty);
            set => this.SetValue(BackgroundColorProperty, value);
        }

        public Color ForegroundColor
        {
            get
            {
                return (Color) GetValue(ForegroundColorProperty);
            }

            set
            {
                this.SetValue(ForegroundColorProperty, value);
            }
        }

        private static void ColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}
