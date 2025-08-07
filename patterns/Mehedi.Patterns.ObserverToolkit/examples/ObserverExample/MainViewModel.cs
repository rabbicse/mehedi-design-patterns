using Mehedi.Patterns.Observer.Asynchronous;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace ObserverExample;

internal class MainViewModel : ObservableObject
{
    #region Declaration(s)   
    private readonly Random _random = new();
    private bool _isUpdating = false;
    private const string StockUpdateKey = "StockUpdates";
    #endregion

    #region Notification Properties
    private ObservableCollection<StockData> _stocks = new();
    public ObservableCollection<StockData> Stocks
    {
        get => _stocks;
        set
        {
            _stocks = value;
            OnPropertyChanged(() => Stocks);
        }
    }
    private string? _symbol;

    public string? Symbol
    {
        get => _symbol; set
        {
            _symbol = value;
            OnPropertyChanged(() => Symbol);
        }
    }

    public string? StatusMessage
    {
        get => _statusMessage; set
        {
            _statusMessage = value;
            OnPropertyChanged(() => StatusMessage);
        }
    }

    private string? _statusMessage;
    #endregion
    private readonly object _lock = new object();
    public MainViewModel() 
    {
        BindingOperations.EnableCollectionSynchronization(Stocks, _lock);
    }

    public async Task StartUpdateAsync()
    {
        if (_isUpdating) return;

        _isUpdating = true;
        UpdateStatus("Starting price updates...");

        // Start background updates
        await Task.Run(async () =>
        {
            while (_isUpdating)
            {
                var symbol = Symbol?.ToUpper();
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

    private async Task UpdateStockDataAsync(StockData data)
    {
        var existing = Stocks.FirstOrDefault(s => s.Symbol == data.Symbol);
        if (existing != null)
        {
            Stocks.Remove(existing);
        }
        Stocks.Add(data);
    }

    public void UpdateStatus(string message)
    {
        StatusMessage = $"{DateTime.Now:T} - {message}";
    }

    public void SubscribeAsync() 
    {
        var symbol = Symbol?.ToUpper();
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

    public void UnsubscribeAsync() 
    {
        AsyncObserverFactory.Instance.UnregisterHandler(StockUpdateKey, this);
        UpdateStatus("Unsubscribed from updates");
    }

    public void StopUpdates()
    {
        _isUpdating = false;
        UpdateStatus("Stopped price updates.");
    }

    public async Task ShutdownAsync()
    {
        // Clean up when window closes
        await AsyncObserverFactory.ShutdownAsync().ConfigureAwait(false);
    }
}

public class StockData
{
    public string? Symbol { get; set; }
    public double Price { get; set; }
    public double Change { get; set; }
    public DateTime Timestamp { get; set; }
}
