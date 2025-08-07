using System.Windows;

namespace ObserverExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize the ViewModel and set it as DataContext`
            viewModel = new MainViewModel();
            this.DataContext = viewModel;
        }

        private void Subscribe_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SubscribeAsync();
        }

        private void Unsubscribe_Click(object sender, RoutedEventArgs e)
        {
            viewModel.UnsubscribeAsync();
        }

        private async void StartUpdates_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.StartUpdateAsync().ConfigureAwait(false);
        }

        private void StopUpdates_Click(object sender, RoutedEventArgs e)
        {
            viewModel.StopUpdates();
        }

        protected override async void OnClosed(EventArgs e)
        {
            await viewModel.ShutdownAsync().ConfigureAwait(false);

            base.OnClosed(e);
        }
    }
}