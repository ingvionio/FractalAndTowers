using System;
using System.Collections.Generic;
using System.Linq;
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
        private bool _isStepByStep = false;
        private TaskCompletionSource<bool> _nextStep;
        private MainWindow _mainWindow;
        private Stack<Move> _history;
        private Stack<Move> _redoStack; // Для хранения шагов после отката

        public HanoiWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            InitializeTowers();
            _AnimationSpeed = 1050 - 500;
            _history = new Stack<Move>();
            _redoStack = new Stack<Move>();
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
            _AnimationSpeed = 1050 - (int)AnimationSpeedSlider.Value;
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            NextStepButton.IsEnabled = false;
            PreviousStepButton.IsEnabled = false;

            // Сбрасываем состояние кнопки переключения режима на автоматический
            _isStepByStep = false;
            ToggleStepByStepModeButton.Content = "Включить пошаговый режим";

            // Очищаем историю шагов и стеки для отмены и повторения
            _history.Clear();
            _redoStack.Clear();

            try
            {
                _diskCount = Convert.ToInt32(DiscCount.Text);

                if (_diskCount < 1)
                {
                    MessageBox.Show("Количество дисков должно быть положительным числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                InitializeDisks();
                await Task.Run(() => Hanoi(_diskCount, 0, 2, 1));
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
            finally
            {
                // Активируем кнопку "Начать" после завершения задачи
                StartButton.IsEnabled = true;

                // Включаем кнопки "Следующий шаг" и "Шаг назад" только если активен пошаговый режим
                if (_isStepByStep)
                {
                    NextStepButton.IsEnabled = true;
                    PreviousStepButton.IsEnabled = true;
                }
            }
        }

        private async Task Hanoi(int n, int from, int to, int aux)
        {
            if (n > 0)
            {
                await Hanoi(n - 1, from, aux, to);

                _history.Push(new Move(from, to));
                _redoStack.Clear(); // Очищаем стек шагов вперед при любом новом действии

                await Task.Delay(_AnimationSpeed / 100);
                await Task.Run(() => MoveDisk(from, to));

                if (_isStepByStep) await WaitForNextStep();

                await Hanoi(n - 1, aux, to, from);
            }
        }

        private async Task MoveDisk(int from, int to)
        {

            await Dispatcher.InvokeAsync(() =>
            {

                if (_towers[from].Children.Count > 1)
                {
                    
                    
                    var disk = _towers[from].Children.OfType<Rectangle>().First();
                    TranslateTransform transform = new TranslateTransform();
                    disk.RenderTransform = transform;

                    // Получаем текущее положение диска относительно окна
                    Point startPoint = disk.TranslatePoint(new Point(0, 0), this);

                    // Удаляем диск из его текущего контейнера
                    _towers[from].Children.Remove(disk);
                    _towers[to].Children.Insert(1, disk);

                    Rectangle newDisk = _towers[to].Children.OfType<Rectangle>().First();
                    Point endPoint = newDisk.TranslatePoint(new Point(0, 0), _mainWindow);

                    _towers[to].Children.Remove(disk);
                    (this.Content as Grid).Children.Add(disk);

                    Canvas.SetLeft(disk, startPoint.X);
                    Canvas.SetBottom(disk, startPoint.Y);

                    Point targetPosition = _towers[to].TranslatePoint(new Point(0, 0), _mainWindow);

                    // Создаем Storyboard для анимации
                    Storyboard storyboard = new Storyboard();

                    // Анимация по X (горизонтальное перемещение)
                    DoubleAnimation animX = new DoubleAnimation
                    {
                        From = Canvas.GetLeft(disk) - 75,
                        To = targetPosition.X + 30,
                        Duration = TimeSpan.FromSeconds(_AnimationSpeed / 250)
                    };
                    Storyboard.SetTarget(animX, disk);
                    Storyboard.SetTargetProperty(animX, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
                    storyboard.Children.Add(animX);

                    // Анимация по Y (подъем и опускание диска)
                    DoubleAnimation animY = new DoubleAnimation
                    {
                        From = Canvas.GetBottom(disk) - 160,
                        To = endPoint.Y - 15,
                        Duration = TimeSpan.FromSeconds(_AnimationSpeed / 250)
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
                        _towers[to].Children.Insert(1, disk);

                        // Сбрасываем трансформации после перемещения
                        disk.RenderTransform = null;

                       
                    };

                    // Запускаем анимацию
                    storyboard.Begin();
                }
            });

            await Task.Delay(_AnimationSpeed * 4);
        }


        private async void PreviousStepButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isStepByStep)
            {
                if (_history.Count > 0)
                {
                    var lastMove = _history.Pop();
                    _redoStack.Push(lastMove);

                    // Выполняем обратное движение
                    await Task.Run(() => MoveDisk(lastMove.To, lastMove.From));
                }
                else
                {
                    MessageBox.Show("Невозможно вернуться назад, нет предыдущих шагов!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Шаг назад доступен только в пошаговом режиме!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void NextStepButton_Click(object sender, RoutedEventArgs e)
        {
            if (_redoStack.Count > 0)
            {
                var nextMove = _redoStack.Pop();
                _history.Push(nextMove);

                Task.Run(() => MoveDisk(nextMove.From, nextMove.To));
            }
            else if (_isStepByStep && _nextStep != null && !_nextStep.Task.IsCompleted)
            {
                _nextStep.SetResult(true);
            }
            else
            {
                MessageBox.Show("Нет доступных шагов для выполнения вперед!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void ToggleStepByStepMode_Click(object sender, RoutedEventArgs e)
        {
            _isStepByStep = !_isStepByStep;
            var button = sender as Button;
            button.Content = _isStepByStep ? "Переключиться на автоматический режим" : "Включить пошаговый режим";
            if (_isStepByStep)
            {
                NextStepButton.IsEnabled = true;
                PreviousStepButton.IsEnabled = true;
            }
            else
            {
                NextStepButton.IsEnabled = false;
                PreviousStepButton.IsEnabled = false;
            }
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

        private class Move
        {
            public int From { get; }
            public int To { get; }

            public Move(int from, int to)
            {
                From = from;
                To = to;
            }
        }
    }
}
