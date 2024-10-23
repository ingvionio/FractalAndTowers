using System.Windows;

namespace Launcher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFractalPage_Click(object sender, RoutedEventArgs e)
        {
            FractalWindow fractalWindow = new FractalWindow(this);
            fractalWindow.Show();
            this.Hide();
            fractalWindow.Closed += (s, args) => this.Show(); // Добавляем обработчик события Closed
        }

        private void OpenHanoiPage_Click(object sender, RoutedEventArgs e)
        {
            HanoiWindow hanoiWindow = new HanoiWindow(this);
            hanoiWindow.Show();
            this.Hide();
            hanoiWindow.Closed += (s, args) => this.Show(); // Добавляем обработчик события Closed
        }

        private void OpenGraphPage_Click(object sender, RoutedEventArgs e)
        {
            HanoiGraphWindow graphWindow = new HanoiGraphWindow();
            graphWindow.Show();
        }
    }
}