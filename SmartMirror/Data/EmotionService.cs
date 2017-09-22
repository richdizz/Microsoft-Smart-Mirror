using Newtonsoft.Json;
using SmartMirror.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SmartMirror.Data
{
    public class EmotionService
    {
        public static async Task<string> GetPrimaryEmotion(byte[] userphoto)
        {
            var resources = new Windows.ApplicationModel.Resources.ResourceLoader("Keys");
            var emotionApiKey = resources.GetString("emotion_api_key");
            string responseContent;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", emotionApiKey);
                var uri = "https://westus.api.cognitive.microsoft.com/emotion/v1.0/recognize?";

                using (var content = new ByteArrayContent(userphoto))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    var response = await client.PostAsync(uri, content);
                    responseContent = response.Content.ReadAsStringAsync().Result;
                }

                var emotions = JsonConvert.DeserializeObject<List<Emotion>>(responseContent);
                var firstFace = emotions.FirstOrDefault();

                var emotionscores = new Dictionary<string, double>();
                emotionscores.Add("anger", firstFace.scores.anger);
                emotionscores.Add("contempt", firstFace.scores.contempt);
                emotionscores.Add("disgust", firstFace.scores.disgust);
                emotionscores.Add("fear", firstFace.scores.fear);
                emotionscores.Add("happiness", firstFace.scores.happiness);

                var maxScore = emotionscores.Values.Max();
                var primaryEmotion = emotionscores.Where(e => e.Value == maxScore).Select(e => e.Key);
                return primaryEmotion.FirstOrDefault().ToString();
            }
        }
    }
}
