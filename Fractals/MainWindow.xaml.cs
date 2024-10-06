using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HanoiTowers
{
    public partial class MainWindow : Window
    {
        private const int PegCount = 3; // Количество столбов (A, B, C)

        private List<Stack<Rectangle>> pegs; // Список для хранения дисков на каждом столбе
        private List<Tuple<int, int>> moves; // Список ходов для пошаговой анимации
        private int currentMoveIndex; // Текущий ход в анимации

        public MainWindow()
        {
            InitializeComponent();
            InitializePegs((int)DiskSlider.Value); // Инициализация башен с выбранным количеством дисков
        }

        private void InitializePegs(int numDisks)
        {
            HanoiCanvas.Children.Clear();
            pegs = new List<Stack<Rectangle>>(PegCount);
            moves = new List<Tuple<int, int>>();
            currentMoveIndex = 0;

            // Инициализируем столбы
            for (int i = 0; i < PegCount; i++)
            {
                pegs.Add(new Stack<Rectangle>());
            }

            // Добавляем диски на первый столб (A)
            for (int i = numDisks; i > 0; i--)
            {
                var disk = new Rectangle
                {
                    Width = 20 + i * 20,
                    Height = 20,
                    Fill = Brushes.LightBlue,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                pegs[0].Push(disk);
            }

            DrawTowers();
        }

        private void GenerateMoves(int n, int fromPeg, int toPeg, int auxPeg)
        {
            if (n > 0)
            {
                GenerateMoves(n - 1, fromPeg, auxPeg, toPeg);
                moves.Add(new Tuple<int, int>(fromPeg, toPeg)); // Сохраняем ход
                GenerateMoves(n - 1, auxPeg, toPeg, fromPeg);
            }
        }

        private async Task MoveDisk(int fromPeg, int toPeg)
        {
            if (pegs[fromPeg].Count > 0)
            {
                var disk = pegs[fromPeg].Pop();
                pegs[toPeg].Push(disk);
                DrawTowers();
                await Task.Delay(500); // Задержка для визуализации (не обязательно)
            }
        }

        private void DrawTowers()
        {
            HanoiCanvas.Children.Clear(); // Очищаем канвас

            double canvasWidth = HanoiCanvas.ActualWidth;
            double pegSpacing = canvasWidth / PegCount;

            // Рисуем столбы
            for (int i = 0; i < PegCount; i++)
            {
                double pegX = pegSpacing / 2 + i * pegSpacing;
                var pegLine = new Line
                {
                    X1 = pegX,
                    Y1 = 50,
                    X2 = pegX,
                    Y2 = HanoiCanvas.ActualHeight - 50,
                    Stroke = Brushes.Black,
                    StrokeThickness = 5
                };
                HanoiCanvas.Children.Add(pegLine);

                // Рисуем диски на текущем столбе, начиная снизу
                int offset = 0;
                foreach (var disk in pegs[i])
                {
                    Canvas.SetLeft(disk, pegX - disk.Width / 2);
                    Canvas.SetTop(disk, HanoiCanvas.ActualHeight - 50 - (offset + 1) * 20);
                    HanoiCanvas.Children.Add(disk);
                    offset++;
                }
            }
        }

        // Начать решение
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            int numDisks = (int)DiskSlider.Value;
            InitializePegs(numDisks);
            moves.Clear();
            GenerateMoves(numDisks, 0, 2, 1); // Генерируем список шагов
        }

        // Шаг вперед
        private async void StepForward_Click(object sender, RoutedEventArgs e)
        {
            if (currentMoveIndex < moves.Count)
            {
                var move = moves[currentMoveIndex];
                await MoveDisk(move.Item1, move.Item2);
                currentMoveIndex++;
            }
        }

        // Сброс
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            InitializePegs((int)DiskSlider.Value); // Сброс башен
            currentMoveIndex = 0; // Сброс шагов
        }
    }
}
