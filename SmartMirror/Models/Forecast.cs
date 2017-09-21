using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SmartMirror.Models
{
    [DataContract]
    public class High
    {
        [DataMember]
        public string fahrenheit { get; set; }
        [DataMember]
        public string celsius { get; set; }
    }

    [DataContract]
    public class Low
    {
        [DataMember]
        public string fahrenheit { get; set; }
        [DataMember]
        public string celsius { get; set; }
    }

    [DataContract]
    public class QpfAllday
    {
        [DataMember]
        public int @in { get; set; }
        [DataMember]
        public int mm { get; set; }
    }

    [DataContract]
    public class QpfDay
    {
        [DataMember]
        public object @in { get; set; }
        [DataMember]
        public object mm { get; set; }
    }

    [DataContract]
    public class QpfNight
    {
        [DataMember]
        public int @in { get; set; }
        [DataMember]
        public int mm { get; set; }
    }

    [DataContract]
    public class SnowAllday
    {
        [DataMember]
        public int @in { get; set; }
        [DataMember]
        public int cm { get; set; }
    }

    [DataContract]
    public class SnowDay
    {
        [DataMember]
        public object @in { get; set; }
        [DataMember]
        public object cm { get; set; }
    }

    [DataContract]
    public class SnowNight
    {
        [DataMember]
        public int @in { get; set; }
        [DataMember]
        public int cm { get; set; }
    }

    [DataContract]
    public class Maxwind
    {
        [DataMember]
        public int mph { get; set; }
        [DataMember]
        public int kph { get; set; }
        [DataMember]
        public string dir { get; set; }
        [DataMember]
        public int degrees { get; set; }
    }

    [DataContract]
    public class Avewind
    {
        [DataMember]
        public int mph { get; set; }
        [DataMember]
        public int kph { get; set; }
        [DataMember]
        public string dir { get; set; }
        [DataMember]
        public int degrees { get; set; }
    }

    [DataContract]
    public class Forecast
    {
        [DataMember]
        public int period { get; set; }
        [DataMember]
        public High high { get; set; }
        [DataMember]
        public Low low { get; set; }
        [DataMember]
        public string conditions { get; set; }
        [DataMember]
        public string icon { get; set; }
        [DataMember]
        public string icon_url { get; set; }
        [DataMember]
        public string skyicon { get; set; }
        [DataMember]
        public int pop { get; set; }
        [DataMember]
        public QpfAllday qpf_allday { get; set; }
        [DataMember]
        public QpfDay qpf_day { get; set; }
        [DataMember]
        public QpfNight qpf_night { get; set; }
        [DataMember]
        public SnowAllday snow_allday { get; set; }
        [DataMember]
        public SnowDay snow_day { get; set; }
        [DataMember]
        public SnowNight snow_night { get; set; }
        [DataMember]
        public Maxwind maxwind { get; set; }
        [DataMember]
        public Avewind avewind { get; set; }
        [DataMember]
        public int avehumidity { get; set; }
        [DataMember]
        public int maxhumidity { get; set; }
        [DataMember]
        public int minhumidity { get; set; }
    }
}

