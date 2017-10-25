using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartMirror.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SmartMirror.Data
{
    public class WeatherService
    {
        public static async Task<Weather> GetWeather(Location location)
        {
            var resources = new Windows.ApplicationModel.Resources.ResourceLoader("Keys");
            var wundergroundApiKey = resources.GetString("wunderground_api_key");

            using (var client = new HttpClient())
            {
                var weatherResponse = await client.GetStringAsync($"http://api.wunderground.com/api/{wundergroundApiKey}/conditions/q/{location.state}/{location.city}.json");
                var weatherJObject = JObject.Parse(weatherResponse);
                var weatherData = weatherJObject["current_observation"];
                var weather = weatherData.ToObject<Weather>();

                var forecastResponse = await client.GetStringAsync($"http://api.wunderground.com/api/{wundergroundApiKey}/forecast/q/{location.state}/{location.city}.json");
                var forecastJObject = JObject.Parse(forecastResponse);
                var forecastData = forecastJObject.SelectToken("$.forecast.simpleforecast.forecastday[0]");
                var forecast = forecastData.ToObject<Forecast>();

                weather.Forecast = forecast;
                return weather;
            }
        }
    }
}
