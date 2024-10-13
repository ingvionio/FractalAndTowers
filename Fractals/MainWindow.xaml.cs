// MainWindow.xaml.cs
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DragonFractal
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Привязываем обработчики к слайдерам
            DepthSlider.ValueChanged += DepthSlider_ValueChanged;
            SizeSlider.ValueChanged += SizeSlider_ValueChanged;

            // Устанавливаем начальные значения текстовых блоков
            UpdateDepthText();
            UpdateSizeText();
        }

        // Метод для построения фрактала дракона
        private void DrawDragonCurve(double x1, double y1, double x2, double y2, int depth, bool turnRight)
        {
            if (depth == 0)
            {
                // Рисуем прямую линию на базовом уровне
                DrawLine(x1, y1, x2, y2);
            }
            else
            {
                // Вычисляем поворот в зависимости от того, направо ли нужно повернуть
                double midX = (x1 + x2) / 2 + (turnRight ? (y2 - y1) / 2 : -(y2 - y1) / 2);
                double midY = (y1 + y2) / 2 + (turnRight ? -(x2 - x1) / 2 : (x2 - x1) / 2);

                // Рекурсивно строим обе части фрактала
                DrawDragonCurve(x1, y1, midX, midY, depth - 1, true);
                DrawDragonCurve(midX, midY, x2, y2, depth - 1, false);
            }
        }

        // Метод для отрисовки линии на Canvas
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
            MyCanvas.Children.Add(line);
        }

        // Метод для очистки Canvas
        private void ClearCanvas()
        {
            MyCanvas.Children.Clear();
        }

        // Обработчик кнопки для перерисовки фрактала
        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            ClearCanvas();

            int depth = (int)DepthSlider.Value; // Получаем значение глубины из слайдера
            double size = SizeSlider.Value;     // Получаем значение размера из слайдера

            // Начальные точки
            double x1 = MyCanvas.ActualWidth / 2 - size / 2;
            double y1 = MyCanvas.ActualHeight / 2;
            double x2 = MyCanvas.ActualWidth / 2 + size / 2;
            double y2 = MyCanvas.ActualHeight / 2;

            // Рисуем фрактал кривой дракона
            DrawDragonCurve(x1, y1, x2, y2, depth, true);
        }

        // Обработчики событий изменения значений слайдеров
        private void DepthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateDepthText();
        }

        private void SizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateSizeText();
        }

        // Методы для обновления значений текстовых блоков
        private void UpdateDepthText()
        {
            DepthTextBlock.Text = $"Depth: {DepthSlider.Value:F0}";
        }

        private void UpdateSizeText()
        {
            SizeTextBlock.Text = $"Size: {SizeSlider.Value:F0}";
        }
    }
}
