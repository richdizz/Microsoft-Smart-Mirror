using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartMirror.Models;
using Windows.Devices.Geolocation;
using SmartMirror.Data;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SmartMirror.Controls
{
    public sealed partial class WeatherPart : MirrorPartBase
    {
        public WeatherPart()
        {
            this.InitializeComponent();
            this.Loaded += WeatherPart_Loaded;
        }
        private async void WeatherPart_Loaded(object sender, RoutedEventArgs e)
        {
            var location = await LocationService.GetLocationAsync();
            var weather = await WeatherService.GetWeather(location);

            Temperature.Text = $"{weather.temp_f.ToString()} °F";
            City.Text = $"{location.city}, {location.state}";
            //FeelsLike.Text = $"Feels like: {weather.feelslike_f.ToString()} °F";
            //Humidity.Text = $"Humidity: {weather.relative_humidity}";
            Wind.Text = $"Wind: {weather.wind_mph} mph";
            High.Text = $"High: {weather.Forecast.high.fahrenheit} °F";
            Low.Text = $"Low: {weather.Forecast.low.fahrenheit} °F";
            WeatherIcon.Source = new BitmapImage(new Uri($"ms-appx:///Assets/Weather/{weather.icon}.png"));
        }
    }
}
