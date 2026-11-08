
using System.Numerics;
using System.Windows;
using System.Windows.Media;

namespace CG
{
    public partial class RgbaColorWindow : Window
    {
        public byte Red { get; private set; }
        public byte Green { get; private set; }
        public byte Blue { get; private set; }

        public byte Alpha { get; private set; }
        public RgbaColorWindow()
        {
            InitializeComponent();
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (!byte.TryParse(RedBox.Text, out byte r) ||
                !byte.TryParse(GreenBox.Text, out byte g) ||
                !byte.TryParse(BlueBox.Text, out byte b) ||
                !byte.TryParse(AlphaBox.Text,out byte a))
            {
                MessageBox.Show(
                    "Введите целые числа от 0 до 255.",
                    "Ошибка ввода",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            Red = r;
            Green = g;
            Blue = b;
            Alpha = a;
            DialogResult = true;
        }

        public Vector4 SelectedColor =>
            new Vector4(Red / 255f, Green/ 255f, Blue / 255f, Alpha / 255f);
    }
}