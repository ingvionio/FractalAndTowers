using System;
using System.Diagnostics;
using System.Windows;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Launcher
{
    public partial class HanoiGraphWindow : Window
    {
        public HanoiGraphWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Создаем пустой график с заголовком и осями
            PlotModel model = new PlotModel { Title = "Зависимость времени от числа дисков (измеренная)" };
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Число дисков" });
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Время (с)" });
            PlotView.Model = model;
        }

        private void MeasurementsButton_Click(object sender, RoutedEventArgs e)
        {
            MeasurementsButton.IsEnabled = false;

            // Получаем максимальное количество дисков из TextBox
            if (!int.TryParse(MaxDisksTextBox.Text, out int maxDisks) || maxDisks <= 0)
            {
                MessageBox.Show("Введите корректное максимальное количество дисков (положительное число).");
                MeasurementsButton.IsEnabled = true;
                return;
            }

            // Очищаем предыдущие серии данных
            PlotView.Model.Series.Clear();

            LineSeries series = new LineSeries();

            for (int i = 1; i <= maxDisks; i++) // Используем maxDisks в цикле
            {
                double timeMs = MeasureHanoiTime(i)/1000;
                series.Points.Add(new DataPoint(i, timeMs));
            }

            // Добавляем новую серию данных на график
            PlotView.Model.Series.Add(series);

            // Обновляем график
            PlotView.InvalidatePlot(true);

            // Разблокируем кнопку после завершения измерений
            MeasurementsButton.IsEnabled = true;
        }


        private double MeasureHanoiTime(int discCount)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Hanoi(discCount, 0, 2, 1);
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        // Рекурсивный алгоритм Ханойских башен 
        private void Hanoi(int n, int from, int to, int aux)
        {
            if (n > 0)
            {
                Hanoi(n - 1, from, aux, to);
                Hanoi(n - 1, aux, to, from);
            }
        }
    }
}