using Mehedi.Patterns.Observer.Asynchronous;
using System.Collections.ObjectModel;
using System.Windows;

namespace ObserverExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<StockData> _stocks = new();
        private readonly Random _random = new();
        private bool _isUpdating = false;
        private const string StockUpdateKey = "StockUpdates";

        public MainWindow()
        {
            InitializeComponent();
            StockDataGrid.ItemsSource = _stocks;
        }

        private void Subscribe_Click(object sender, RoutedEventArgs e)
        {
            var symbol = StockSymbolTextBox.Text.Trim().ToUpper();
            if (string.IsNullOrEmpty(symbol))
            {
                MessageBox.Show("Please enter a stock symbol");
                return;
            }

            // Register async handler
            AsyncObserverFactory.Instance.RegisterHandler<StockData>(
                StockUpdateKey,
                this,
                async data => await UpdateStockDataAsync(data));

            UpdateStatus($"Subscribed to {symbol} updates");
        }

        private async Task UpdateStockDataAsync(StockData data)
        {
            // This runs on background thread - must dispatch to UI thread
            await Dispatcher.InvokeAsync(() =>
            {
                var existing = _stocks.FirstOrDefault(s => s.Symbol == data.Symbol);
                if (existing != null)
                {
                    _stocks.Remove(existing);
                }
                _stocks.Add(data);
            });
        }

        private void Unsubscribe_Click(object sender, RoutedEventArgs e)
        {
            AsyncObserverFactory.Instance.UnregisterHandler(StockUpdateKey, this);
            UpdateStatus("Unsubscribed from updates");
        }

        private async void StartUpdates_Click(object sender, RoutedEventArgs e)
        {
            if (_isUpdating) return;

            _isUpdating = true;
            UpdateStatus("Starting price updates...");

            // Start background updates
            await Task.Run(async () =>
            {
                while (_isUpdating)
                {
                    var symbol = StockSymbolTextBox.Text.Trim().ToUpper();
                    if (!string.IsNullOrEmpty(symbol))
                    {
                        var newPrice = Math.Round(100 + (_random.NextDouble() * 50), 2);
                        var change = Math.Round((_random.NextDouble() * 4) - 2); // -2 to +2

                        var stockData = new StockData
                        {
                            Symbol = symbol,
                            Price = newPrice,
                            Change = change,
                            Timestamp = DateTime.Now
                        };

                        // Notify all observers asynchronously
                        await AsyncObserverFactory.Instance.NotifyAsync(StockUpdateKey, stockData);
                    }

                    await Task.Delay(1000); // Update every second
                }
            });
        }

        private void StopUpdates_Click(object sender, RoutedEventArgs e)
        {
            _isUpdating = false;
            UpdateStatus("Stopped price updates");
        }

        private void UpdateStatus(string message)
        {
            StatusText.Text = $"{DateTime.Now:T} - {message}";
        }

        protected override void OnClosed(EventArgs e)
        {
            // Clean up when window closes
            AsyncObserverFactory.ShutdownAsync().GetAwaiter().GetResult();
            base.OnClosed(e);
        }
    }

    public class StockData
    {
        public string? Symbol { get; set; }
        public double Price { get; set; }
        public double Change { get; set; }
        public DateTime Timestamp { get; set; }
    }
}