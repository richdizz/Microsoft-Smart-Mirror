using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SmartMirror.Models
{
    [DataContract]
    public class Weather
    {
        [DataMember]
        public string station_id { get; set; }
        [DataMember]
        public string observation_time { get; set; }
        [DataMember]
        public string observation_time_rfc822 { get; set; }
        [DataMember]
        public string observation_epoch { get; set; }
        [DataMember]
        public string local_time_rfc822 { get; set; }
        [DataMember]
        public string local_epoch { get; set; }
        [DataMember]
        public string local_tz_short { get; set; }
        [DataMember]
        public string local_tz_long { get; set; }
        [DataMember]
        public string local_tz_offset { get; set; }
        [DataMember]
        public string weather { get; set; }
        [DataMember]
        public string temperature_string { get; set; }
        [DataMember]
        public double temp_f { get; set; }
        [DataMember]
        public double temp_c { get; set; }
        [DataMember]
        public string relative_humidity { get; set; }
        [DataMember]
        public string wind_string { get; set; }
        [DataMember]
        public string wind_dir { get; set; }
        [DataMember]
        public int wind_degrees { get; set; }
        [DataMember]
        public double wind_mph { get; set; }
        [DataMember]
        public double wind_gust_mph { get; set; }
        [DataMember]
        public double wind_kph { get; set; }
        [DataMember]
        public double wind_gust_kph { get; set; }
        [DataMember]
        public string pressure_mb { get; set; }
        [DataMember]
        public string pressure_in { get; set; }
        [DataMember]
        public string pressure_trend { get; set; }
        [DataMember]
        public string dewpoint_string { get; set; }
        [DataMember]
        public int dewpoint_f { get; set; }
        [DataMember]
        public int dewpoint_c { get; set; }
        [DataMember]
        public string heat_index_string { get; set; }
        [DataMember]
        public string heat_index_f { get; set; }
        [DataMember]
        public string heat_index_c { get; set; }
        [DataMember]
        public string windchill_string { get; set; }
        [DataMember]
        public string windchill_f { get; set; }
        [DataMember]
        public string windchill_c { get; set; }
        [DataMember]
        public string feelslike_string { get; set; }
        [DataMember]
        public string feelslike_f { get; set; }
        [DataMember]
        public string feelslike_c { get; set; }
        [DataMember]
        public string visibility_mi { get; set; }
        [DataMember]
        public string visibility_km { get; set; }
        [DataMember]
        public string solarradiation { get; set; }
        [DataMember]
        public string UV { get; set; }
        [DataMember]
        public string precip_1hr_string { get; set; }
        [DataMember]
        public string precip_1hr_in { get; set; }
        [DataMember]
        public string precip_1hr_metric { get; set; }
        [DataMember]
        public string precip_today_string { get; set; }
        [DataMember]
        public string precip_today_in { get; set; }
        [DataMember]
        public string precip_today_metric { get; set; }
        [DataMember]
        public string icon { get; set; }
        [DataMember]
        public string icon_url { get; set; }
        [DataMember]
        public string forecast_url { get; set; }
        [DataMember]
        public string history_url { get; set; }
        [DataMember]
        public string ob_url { get; set; }
        [DataMember]
        public string nowcast { get; set; }
        [DataMember]
        public Forecast Forecast { get; set; }
    }
}
