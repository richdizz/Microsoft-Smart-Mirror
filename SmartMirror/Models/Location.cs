using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SmartMirror.Models
{
    [DataContract(Name = "Location")]
    public class Location
    {
        [DataMember]
        public string type { get; set; }
        [DataMember]
        public string country { get; set; }
        [DataMember]
        public string country_iso3166 { get; set; }
        [DataMember]
        public string country_name { get; set; }
        [DataMember]
        public string state { get; set; }
        [DataMember]
        public string city { get; set; }
        [DataMember]
        public string tz_short { get; set; }
        [DataMember]
        public string tz_long { get; set; }
        [DataMember]
        public double lat { get; set; }
        [DataMember]
        public double lon { get; set; }
        [DataMember]
        public string zip { get; set; }
        [DataMember]
        public string magic { get; set; }
        [DataMember]
        public string wmo { get; set; }
        [DataMember]
        public string l { get; set; }
        [DataMember]
        public string requesturl { get; set; }
        [DataMember]
        public string wuiurl { get; set; }
    }
}
