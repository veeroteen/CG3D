using SharpGL;
using SharpGL.WPF;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CG3D
{
    public partial class MainWindow : Window
    {
        private Handler handler;

        private long previousFrameTime;

        private Point lastMousePosition;

        public MainWindow()
        {
            handler = null;
            InitializeComponent();
            previousFrameTime = Environment.TickCount64;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            OpenGLControl.Focus();
        }

        private void OpenGLControl_Initialized(
            object sender, EventArgs e)
        {
            handler = new Handler(OpenGLControl);

        }

        private void OpenGLControl_Resized(
            object sender, EventArgs e)
        {
            handler.Resized((int)OpenGLControl.ActualWidth, (int)OpenGLControl.ActualHeight);
        }

        private void Window_KeyDown(
            object sender, KeyEventArgs e)
        {
            handler.AddKey(e.Key);
            
        }

        private void Window_KeyUp(
            object sender, KeyEventArgs e)
        {
            handler.RemoveKey(e.Key);
        }

        private void Window_Deactivated(
            object sender, EventArgs e)
        {
            handler.Deactovate();
            handler.CameraCapture(false);

            if (Mouse.Captured == OpenGLControl)
                Mouse.Capture(null);
        }

        private void OpenGLControl_MouseUp( object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Middle:
                    handler.CameraCapture(false);
                    if (Mouse.Captured == OpenGLControl)
                        Mouse.Capture(null);
                    e.Handled = true;
                    break;
            }
        }
        private void OpenGLControl_MouseDown(object sender, MouseButtonEventArgs e)
        {

            switch (e.ChangedButton)
            {
                case MouseButton.Middle:
                    handler.CameraCapture(true);
                    OpenGLControl.Focus();
                    Mouse.Capture(OpenGLControl, CaptureMode.Element);
                    e.Handled = true;
                    break;
            }
        }


        private void OpenGLControl_Draw(object sender, OpenGLRoutedEventArgs args)
        {
            Vector2 mousePosition = Mouse.GetPosition(this).ToVector2();
            long currentTimeStamp = Environment.TickCount64;
            float deltaTime = (currentTimeStamp - previousFrameTime)/1000f;
            previousFrameTime = currentTimeStamp;
            handler.Update(deltaTime, mousePosition);
        }


    }
}