using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SmartMirror.Models
{
    [DataContract]
    public class FaceRectangle
    {
        [DataMember]
        public int height { get; set; }
        [DataMember]
        public int left { get; set; }
        [DataMember]
        public int top { get; set; }
        [DataMember]
        public int width { get; set; }
    }

    [DataContract]
    public class Scores
    {
        [DataMember]
        public double anger { get; set; }
        [DataMember]
        public double contempt { get; set; }
        [DataMember]
        public double disgust { get; set; }
        [DataMember]
        public double fear { get; set; }
        [DataMember]
        public double happiness { get; set; }
        [DataMember]
        public double neutral { get; set; }
        [DataMember]
        public double sadness { get; set; }
        [DataMember]
        public double surprise { get; set; }
    }

    [DataContract]
    public class Emotion
    {
        [DataMember]
        public FaceRectangle faceRectangle { get; set; }
        [DataMember]
        public Scores scores { get; set; }
    }
}