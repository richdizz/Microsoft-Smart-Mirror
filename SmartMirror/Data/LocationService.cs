using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartMirror.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace SmartMirror.Data
{
    public class LocationService
    {       
        public static async Task<Location> GetLocationAsync()
        {
            var resources = new Windows.ApplicationModel.Resources.ResourceLoader("Keys");
            var wundergroundApiKey = resources.GetString("wunderground_api_key");

            var accessStatus = await Geolocator.RequestAccessAsync();

            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    var geolocator = new Geolocator();
                    try
                    {
                        var pos = await geolocator.GetGeopositionAsync();

                        using (var client = new HttpClient())
                        {
                            var data = await client.GetStringAsync($"http://api.wunderground.com/api/{wundergroundApiKey}/geolookup/q/{pos.Coordinate.Latitude},{pos.Coordinate.Longitude}.json");
                            var locationResponse = JObject.Parse(data);
                            var locationJson = locationResponse["location"];
                            return locationJson.ToObject<Location>();
                        }
                    }
                    catch (Exception)
                    {
                        return new Location
                        {
                            city = "Seattle",
                            country = "USA",
                            state = "WA",
                            lat = 47.608013,
                            lon = -122.335167
                        };
                    }

                default:
                    return new Location
                    {
                        city = "Seattle",
                        country = "USA",
                        state = "WA",
                        lat = 47.608013,
                        lon = -122.335167
                    };
            }
        }
    }
}
