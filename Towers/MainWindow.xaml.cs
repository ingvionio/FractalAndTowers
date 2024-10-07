using System;
using System.Linq;
using System.Threading;
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
        private CancellationTokenSource _cancellationTokenSource; // Для отмены анимации
        private bool _isStepByStep = false; // Флаг пошагового режима
        private TaskCompletionSource<bool> _nextStep; // Для управления шагами

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
            // Останавливаем предыдущую анимацию, если она была запущена
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }

            _cancellationTokenSource = new CancellationTokenSource(); // Новый токен для отмены
            _isStepByStep = false; // Обычный режим

            try
            {
                _diskCount = Convert.ToInt32(DiscCount.Text); // Пробуем преобразовать текст в число

                if (_diskCount < 1) // Проверка на допустимость значения
                {
                    MessageBox.Show("Количество дисков должно быть положительным числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                InitializeDisks(); // Инициализация башен с дисками
                await Task.Run(() => Hanoi(_diskCount, 0, 2, 1, _cancellationTokenSource.Token)); // Запуск решения задачи Ханойских башен
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

        // Кнопка для остановки анимации
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel(); // Останавливаем текущую задачу
            }
        }

        // Кнопка для пошагового режима
        private void NextStepButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isStepByStep && _nextStep != null && !_nextStep.Task.IsCompleted) // Проверяем, что задача ещё не завершена
            {
                _nextStep.SetResult(true); // Разрешаем выполнение следующего шага
            }
        }

        // Ожидание следующего шага в пошаговом режиме
        private async Task WaitForNextStep()
        {
            if (_isStepByStep) // Ожидаем шага только в пошаговом режиме
            {
                _nextStep = new TaskCompletionSource<bool>();
                await _nextStep.Task; // Ожидаем сигнала на следующий шаг
            }
        }


        private async Task Hanoi(int n, int from, int to, int aux, CancellationToken token)
        {
            if (n > 0)
            {
                await Hanoi(n - 1, from, aux, to, token);

                if (token.IsCancellationRequested) return; // Проверка на отмену

                await MoveDisk(from, to, token);

                if (_isStepByStep) await WaitForNextStep(); // Ожидаем сигнала на следующий шаг

                if (token.IsCancellationRequested) return; // Проверка на отмену

                await Hanoi(n - 1, aux, to, from, token);
            }
        }

        private async Task MoveDisk(int from, int to, CancellationToken token)
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
            await Task.Delay(_AnimationSpeed, token); // Задержка для визуализации перемещения с учетом отмены
        }

        // Включение пошагового режима и обратно в автоматический
        private void ToggleStepByStepMode_Click(object sender, RoutedEventArgs e)
        {
            _isStepByStep = !_isStepByStep; // Переключаем режим
            var button = sender as Button;
            button.Content = _isStepByStep ? "Переключиться на автоматический режим" : "Включить пошаговый режим";

            // Если переключаемся в автоматический режим, освобождаем текущий шаг
            if (!_isStepByStep && _nextStep != null && !_nextStep.Task.IsCompleted)
            {
                _nextStep.SetResult(true); // Освобождаем задачу для продолжения в автоматическом режиме
            }
        }
    }
}
