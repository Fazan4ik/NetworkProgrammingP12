using Azure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NetworkProgrammingP12
{
    /// <summary>
    /// Interaction logic for CryptoWindow.xaml
    /// </summary>
    public partial class CryptoWindow : Window
    {
        private readonly HttpClient _httpClient;
        public ObservableCollection<CoinData> CoinsData { get; set; }
        private ListViewItem? previousSelectedItem;
            
        public CryptoWindow()
        {
            InitializeComponent();
            CoinsData = new();
            this.DataContext = this;
            LabelCourse.Content = "Графіки курсів криптовалют";
            _httpClient = new()
            {
                BaseAddress = new Uri("https://api.coincap.io/")
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAssetsAsync();
        }

        private async Task LoadAssetsAsync()
        {
            var response = JsonSerializer.Deserialize<CoincapResponse>(
                await _httpClient.GetStringAsync("/v2/assets?limit=10")
            );
            if (response == null)
            {
                MessageBox.Show("Deserialization error");
                return;
            }
            CoinsData.Clear();
            foreach (var coinData in response.data)
            {
                CoinsData.Add(coinData);
            }
        }
        private void FrameworkElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item)
            {
                if (item.Content is CoinData coinData)
                {
                    if (previousSelectedItem is not null)
                    {
                        previousSelectedItem.Background = Brushes.White;  
                    }
                    // MessageBox.Show(coinData.symbol);
                    ShowHistory(coinData);
                    LabelCourse.Content = $"Графік курса криптовалюти {coinData.name}";
                    item.Background = Brushes.Aqua;
                    previousSelectedItem = item;
                    MessageBox.Show($"Asset id -> {coinData.id}");

                }
                // DrawLine(10, 10, 100, 100);
            }
        }

        private async Task ShowHistory(CoinData coinData)
        {
            String body = await _httpClient.GetStringAsync(
                $"/v2/assets/{coinData.id}/history?interval=d1");
            var response = JsonSerializer.Deserialize<HistoryResponse>(body);
            
            if(response == null || response.data == null)
            {
                MessageBox.Show("Помилка завантаження данних");
                return;
            }
            double yOffset = 30;
            double graphH = Graph.ActualHeight - yOffset;

            long minTime, maxTime;
            double minPrice, maxPrice;
            minTime = maxTime = response.data[0].time;
            minPrice = maxPrice = response.data[0].price;
            foreach(HistoryItem item in response.data)
            {
                if (item.time < minTime) { minTime = item.time; }
                if (item.time > maxTime) { maxTime = item.time; }

                if (item.price < minPrice) { minPrice = item.price; }
                if (item.price > maxPrice) { maxPrice = item.price; }                
            }
            double x0 = (response.data[0].time - minTime) * Graph.ActualWidth / (maxTime - minTime);
            double y0 = graphH - (response.data[0].price - minPrice) * graphH / (maxPrice - minPrice);
            Graph.Children.Clear(); 
            
            foreach (HistoryItem item in response.data)
            {
                double x = (item.time - minTime) * Graph.ActualWidth / (maxTime - minTime);
                double y = graphH - (item.price - minPrice) * graphH / (maxPrice - minPrice);
                if (y < y0)
                {
                    DrawLine(x0, y0, x, y, Brushes.Green);
                }
                else
                {
                    DrawLine(x0, y0, x, y, Brushes.Red);
                }
                
                x0 = x;
                y0 = y;
            }
            DrawLine(0, graphH, Graph.ActualWidth, graphH);
        }

        private void DrawLine(double x1,double y1,double x2,double y2, Brush brush = null)
        {
            brush ??= new SolidColorBrush(Colors.Black);
            Graph.Children.Add(new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = brush,
                StrokeThickness = 2
            });
        }
    }

    ///////////////// ORM ////////////////////////
    
    public class HistoryResponse
    {
        public List<HistoryItem> data { get; set; }
        public long timestamp { get; set; }
    }
    public class HistoryItem
    {
        public String priceUsd { get; set; }
        public long time { get; set; }
        public double price => Double.Parse(priceUsd, CultureInfo.InvariantCulture);
    }

    public class CoincapResponse
    {
        public List<CoinData> data { get; set; }
        public long timestamp { get; set; }
    }
    public class CoinData
    {
        public string id { get; set; }
        public string rank { get; set; }
        public string symbol { get; set; }
        public string name { get; set; }
        public string supply { get; set; }
        public string maxSupply { get; set; }
        public string marketCapUsd { get; set; }
        public string volumeUsd24Hr { get; set; }
        public string priceUsd { get; set; }
        public string changePercent24Hr { get; set; }
        public string vwap24Hr { get; set; }
        public string explorer { get; set; }
    }

}