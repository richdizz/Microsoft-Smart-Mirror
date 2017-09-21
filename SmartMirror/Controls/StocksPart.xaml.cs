using Newtonsoft.Json.Linq;
using SmartMirror.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SmartMirror.Controls
{
    public sealed partial class StocksPart : MirrorPartBase
    {
        public StocksPart()
        {
            this.InitializeComponent();
        }
        public async override void Initialize(User user, bool isEditMode = false)
        {
            base.Initialize(user, isEditMode);

            var jsonMessage = await GetStockIndexAsync();
            //JArray metadata = JObject.Parse(jsonMessage).Value<JArray>("Meta Data");
            //JArray timeseries = JObject.Parse(jsonMessage).Value<JArray>("Time Series (1min)");


            //if (messages.Count > 0)
            //{

            //    // loop through messages
            //    foreach (var msg in messages)
            //    {
            //        string from = msg.SelectToken("from.emailAddress.name").Value<string>();
            //        string subject = msg.SelectToken("subject").Value<string>();
            //        //string bodyPreview = msg.SelectToken("bodyPreview").Value<string>();

            //        Grid grid = new Grid();
            //        grid.Margin = new Thickness(0, 10, 0, 10);
            //        grid.ColumnDefinitions.Add(new ColumnDefinition() { MaxWidth = 40, MinWidth = 40 });
            //        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            //        SymbolIcon icon = new SymbolIcon(Symbol.Mail);
            //        icon.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
            //        icon.VerticalAlignment = VerticalAlignment.Top;
            //        grid.Children.Add(icon);
            //        Grid.SetColumn(icon, 0);

            //        StackPanel sp = new StackPanel();
            //        sp.Orientation = Orientation.Vertical;
            //        grid.Children.Add(sp);
            //        Grid.SetColumn(sp, 1);


            //        TextBlock tbFrom = new TextBlock();
            //        tbFrom.Width = this.ActualWidth - 40;
            //        tbFrom.Text = from;
            //        tbFrom.FontSize = 18;
            //        tbFrom.TextWrapping = TextWrapping.Wrap;
            //        tbFrom.VerticalAlignment = VerticalAlignment.Top;
            //        sp.Children.Add(tbFrom);

            //        TextBlock tbSubject = new TextBlock();
            //        tbSubject.Width = this.ActualWidth - 40;
            //        tbSubject.Text = subject;
            //        tbSubject.FontSize = 12;
            //        tbSubject.TextWrapping = TextWrapping.Wrap;
            //        sp.Children.Add(tbSubject);

            //        //TextBlock tbbody = new TextBlock();
            //        //tbbody.Width = this.ActualWidth - 60;
            //        //tbbody.Text = bodyPreview;
            //        //tbbody.FontSize = 12;
            //        //tbbody.TextWrapping = TextWrapping.WrapWholeWords;
            //        //sp.Children.Add(tbbody);

            //        Line line = new Line();
            //        line.Y1 = 10;
            //        line.Y2 = 10;
            //        line.X2 = this.ActualWidth;
            //        line.StrokeThickness = 1;
            //        line.Stroke = new SolidColorBrush(Windows.UI.Colors.White);
            //        line.StrokeDashArray = new DoubleCollection() { 1 };

            //        stockPanel.Children.Add(grid);
            //        //inboxPanel.Children.Add(line);
            //    }

            //}
        }

        public async Task<string> GetStockIndexAsync()

        {
            // get nasdaq index from Alpha Vantage! API key is: 9VPD7KMTO9QX3ACK
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            using (var response = await client.GetAsync($"https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol=IXIC&interval=1min&apikey=9VPD7KMTO9QX3ACK"))
            {
                if (response.IsSuccessStatusCode)
                {
                    var jsonMessage = await response.Content.ReadAsStringAsync();
                    return jsonMessage;
                }
            }
            return null;
        }

    }
}
