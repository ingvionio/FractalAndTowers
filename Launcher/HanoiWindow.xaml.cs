using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Launcher
{
    public partial class HanoiWindow : Window
    {
        private StackPanel[] _towers;
        private int _diskCount;
        private int _AnimationSpeed;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isStepByStep = false;
        private TaskCompletionSource<bool> _nextStep;
        private MainWindow _mainWindow;

        public HanoiWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            InitializeTowers();
            _AnimationSpeed = 500;

        }

        private void InitializeTowers()
        {
            _towers = new StackPanel[] { Tower1, Tower2, Tower3 };
        }

        private void InitializeDisks()
        {
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
                    Width = 30 + 20 * i,
                    Height = 20,
                    Fill = new SolidColorBrush(Color.FromRgb(
                        (byte)random.Next(100, 255),
                        (byte)random.Next(100, 255),
                        (byte)random.Next(100, 255))),
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };

                _towers[0].Children.Insert(1, disk);
            }
        }

        private void AnimationSpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _AnimationSpeed = 2500 - (int)AnimationSpeedSlider.Value;
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _isStepByStep = false;

            try
            {
                _diskCount = Convert.ToInt32(DiscCount.Text);

                if (_diskCount < 1)
                {
                    MessageBox.Show("Количество дисков должно быть положительным числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                InitializeDisks();
                await Task.Run(() => Hanoi(_diskCount, 0, 2, 1, _cancellationTokenSource.Token));
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

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        private void NextStepButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isStepByStep && _nextStep != null && !_nextStep.Task.IsCompleted)
            {
                _nextStep.SetResult(true);
            }
        }

        private async Task WaitForNextStep()
        {
            if (_isStepByStep)
            {
                _nextStep = new TaskCompletionSource<bool>();
                await _nextStep.Task;
            }
        }

        private async Task Hanoi(int n, int from, int to, int aux, CancellationToken token)
        {
            if (n > 0)
            {
                await Hanoi(n - 1, from, aux, to, token);

                if (token.IsCancellationRequested) return;

                await MoveDisk(from, to, token);
                await Task.Delay(_AnimationSpeed*5);
                if (_isStepByStep) await WaitForNextStep();

                if (token.IsCancellationRequested) return;

                await Hanoi(n - 1, aux, to, from, token);
            }
        }

        private async Task MoveDisk(int from, int to, CancellationToken token)
        {
            await Dispatcher.InvokeAsync(() =>
            {

                if (_towers[from].Children.Count > 1)
                {
                    var disk = _towers[from].Children.OfType<Rectangle>().First();
                    //_towers[from].Children.Remove(disk);

                    //_towers[to].Children.Insert(1, disk);
                    //var newDisk = _towers[to].Children.OfType<Rectangle>().First();
                    //_towers[to].Children.Remove(disk);
                    
                    DiskAnimation(disk, _towers[from], _towers[to], token);
                    
                }
            });
            await Task.Delay(_AnimationSpeed, token);
        }

        private async Task DiskAnimation(Rectangle disk, StackPanel currentTower, StackPanel targetTower, CancellationToken token)
        {

            // Создаем TranslateTransform для диска
            TranslateTransform transform = new TranslateTransform();
            disk.RenderTransform = transform;

            // Получаем текущее положение диска относительно окна
            Point startPoint = disk.TranslatePoint(new Point(0, 0), this);



            // Удаляем диск из его текущего контейнера
            currentTower.Children.Remove(disk);
            targetTower.Children.Insert(1, disk);

            Rectangle newDisk = targetTower.Children.OfType<Rectangle>().First();

            Point endPoint = newDisk.TranslatePoint(new Point(0, 0), this);

            targetTower.Children.Remove(disk);
            // Добавляем диск в общий Grid (предположим, что ваш главный контейнер - это Grid)
            (this.Content as Grid).Children.Add(disk);

            // Устанавливаем абсолютные координаты диска в Grid
            Canvas.SetLeft(disk, startPoint.X);
            Canvas.SetBottom(disk, startPoint.Y);

            // Получаем координаты целевой башни относительно окна
            Point targetPosition = targetTower.TranslatePoint(new Point(0, 0), this);

            // Создаем Storyboard для анимации
            Storyboard storyboard = new Storyboard();

            // Анимация по X (горизонтальное перемещение)
            DoubleAnimation animX = new DoubleAnimation
            {
                From = Canvas.GetLeft(disk)-75,
                To = targetPosition.X-10,
                Duration = TimeSpan.FromSeconds(_AnimationSpeed/250)
            };
            Storyboard.SetTarget(animX, disk);
            Storyboard.SetTargetProperty(animX, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
            storyboard.Children.Add(animX);

            // Анимация по Y (подъем и опускание диска)
            DoubleAnimation animY = new DoubleAnimation
            {
                From = Canvas.GetBottom(disk)-220,
                To = endPoint.Y-241, // Подъем на 100 пикселей вверх
                Duration = TimeSpan.FromSeconds(_AnimationSpeed/250)
            };
            Storyboard.SetTarget(animY, disk);
            Storyboard.SetTargetProperty(animY, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(animY);

            // После завершения анимации, перемещаем диск в целевую башню
            storyboard.Completed += (s, e) =>
            {
                // Удаляем диск из Grid
                (this.Content as Grid).Children.Remove(disk);

                // Добавляем диск обратно в целевую башню (StackPanel)
                targetTower.Children.Insert(1, disk);

                // Сбрасываем трансформации после перемещения
                disk.RenderTransform = null;
            };
            
            // Запускаем анимацию
            storyboard.Begin();
            
        }




        private void ToggleStepByStepMode_Click(object sender, RoutedEventArgs e)
        {
            _isStepByStep = !_isStepByStep;
            var button = sender as Button;
            button.Content = _isStepByStep ? "Переключиться на автоматический режим" : "Включить пошаговый режим";

            if (!_isStepByStep && _nextStep != null && !_nextStep.Task.IsCompleted)
            {
                _nextStep.SetResult(true);
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.Show();
            this.Close();
        }
    }
}