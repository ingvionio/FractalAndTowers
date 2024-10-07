using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HanoiTowers
{
    public partial class MainWindow : Window
    {
        private StackPanel[] _towers; // Массив башен
        private int _diskCount; // Количество дисков
        private int _AnimationSpeed; // Скорость анимации в миллисекундах

        public MainWindow()
        {
            InitializeComponent();
            InitializeTowers();
            _AnimationSpeed = 500; // Устанавливаем начальную скорость анимации
        }

        private void InitializeTowers()
        {
            _towers = new StackPanel[] { Tower1, Tower2, Tower3 };
        }

        private void InitializeDisks()
        {
            // Очищаем башни от предыдущих дисков
            foreach (var tower in _towers)
            {
                tower.Children.Clear();
                tower.Children.Add(new Border { Width = 10, Height = 300, Background = Brushes.Black });
            }

            Random random = new Random();
            for (int i = _diskCount; i > 0; i--)
            {
                var disk = new Rectangle
                {
                    Width = 30 + 20 * i, // Ширина диска зависит от его номера
                    Height = 20,
                    Fill = new SolidColorBrush(Color.FromRgb(
                        (byte)random.Next(100, 255),
                        (byte)random.Next(100, 255),
                        (byte)random.Next(100, 255))),
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };

                // Добавляем диски в начало башни (0 позиция), чтобы большие были внизу
                _towers[0].Children.Insert(1, disk); // Делаем вставку после основания (Border) башни
            }
        }

        // Метод для обновления значения скорости анимации
        private void AnimationSpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Инвертируем значение слайдера: 2000 - текущая позиция
            _AnimationSpeed = 2100 - (int)AnimationSpeedSlider.Value;
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _diskCount = Convert.ToInt32(DiscCount.Text); // Пробуем преобразовать текст в число

                if (_diskCount < 1) // Проверка на допустимость значения
                {
                    MessageBox.Show("Количество дисков должно быть положительным числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                InitializeDisks(); // Инициализация башен с дисками
                await Task.Run(() => Hanoi(_diskCount, 0, 2, 1)); // Запуск решения задачи Ханойских башен
            }
            catch (FormatException)
            {
                MessageBox.Show("Пожалуйста, введите корректное числовое значение!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (OverflowException)
            {
                MessageBox.Show("Число слишком велико!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task Hanoi(int n, int from, int to, int aux)
        {
            if (n > 0)
            {
                await Hanoi(n - 1, from, aux, to);
                await MoveDisk(from, to);
                await Hanoi(n - 1, aux, to, from);
            }
        }

        private async Task MoveDisk(int from, int to)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                if (_towers[from].Children.Count > 1) // Проверяем, что есть диски для перемещения
                {
                    var disk = _towers[from].Children.OfType<Rectangle>().First(); // Выбираем первый (верхний) диск
                    _towers[from].Children.Remove(disk); // Убираем его с текущей башни
                    _towers[to].Children.Insert(1, disk); // Добавляем его на целевую башню
                }
            });

            // Используем скорость анимации, заданную пользователем
            await Task.Delay(_AnimationSpeed); // Задержка для визуализации перемещения
        }
    }
}
