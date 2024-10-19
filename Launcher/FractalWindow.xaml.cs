using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Launcher
{
    public partial class FractalWindow : Window
    {
        private Point _lastMousePosition;
        private bool _isDragging;
        private MainWindow _mainWindow;
        private const double MaxZoom = 5.0;
        private const double MinZoom = 0.2;

        public FractalWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.Show();
            this.Close();
        }

        private void DrawFractalButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(RecursionDepthTextBox.Text, out int recursionDepth) && recursionDepth >= 0)
            {
                DrawDragonCurve(recursionDepth, SizeSlider.Value);
            }
            else
            {
                MessageBox.Show("Введите корректную глубину рекурсии (неотрицательное число).");
            }
        }

        private void DrawDragonCurve(int depth, double size)
        {
            FractalCanvas.Children.Clear();

            double x1 = FractalCanvas.ActualWidth / 2 - size / 2;
            double y1 = FractalCanvas.ActualHeight / 2;
            double x2 = FractalCanvas.ActualWidth / 2 + size / 2;
            double y2 = FractalCanvas.ActualHeight / 2;

            DrawDragonCurveRecursive(x1, y1, x2, y2, depth, true);
        }

        private void DrawDragonCurveRecursive(double x1, double y1, double x2, double y2, int depth, bool turnRight)
        {
            if (depth == 0)
            {
                DrawLine(x1, y1, x2, y2);
            }
            else
            {
                double midX = (x1 + x2) / 2 + (turnRight ? (y2 - y1) / 2 : -(y2 - y1) / 2);
                double midY = (y1 + y2) / 2 + (turnRight ? -(x2 - x1) / 2 : (x2 - x1) / 2);

                DrawDragonCurveRecursive(x1, y1, midX, midY, depth - 1, true);
                DrawDragonCurveRecursive(midX, midY, x2, y2, depth - 1, false);
            }
        }

        private void DrawLine(double x1, double y1, double x2, double y2)
        {
            Line line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            FractalCanvas.Children.Add(line);
        }

        private void FractalCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scaleFactor = e.Delta > 0 ? 1.1 : 0.9;
            double newScaleX = scaleTransform.ScaleX * scaleFactor;
            double newScaleY = scaleTransform.ScaleY * scaleFactor;

            // Ограничиваем зум
            //if (newScaleX >= MinZoom && newScaleX <= MaxZoom && newScaleY >= MinZoom && newScaleY <= MaxZoom)
           // {
                scaleTransform.ScaleX = newScaleX;
                scaleTransform.ScaleY = newScaleY;
           // }
        }

        private void FractalCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isDragging = true;
                _lastMousePosition = e.GetPosition(this);
            }
        }

        private void FractalCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                var currentPosition = e.GetPosition(this);
                var deltaX = currentPosition.X - _lastMousePosition.X;
                var deltaY = currentPosition.Y - _lastMousePosition.Y;
                _lastMousePosition = currentPosition;

                translateTransform.X += deltaX;
                translateTransform.Y += deltaY;
            }
        }

        private void FractalCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
        }
    }
}
