using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.FaceAnalysis;

namespace SmartMirror.Models
{
    public class ImageWithFaceDetection
    {
        public ImageWithFaceDetection(byte[] imageBytes, IList<DetectedFace> faces)
        {
            ImageBytes = imageBytes;
            Faces = faces;
        }

        public byte[] ImageBytes { get; set; }
        public IList<DetectedFace> Faces { get; set; }
    }
}
