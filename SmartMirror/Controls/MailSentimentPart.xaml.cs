using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartMirror.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SmartMirror.Controls
{
    public sealed partial class MailSentimentPart : MirrorPartBase
    {
        public MailSentimentPart()
        {
            this.InitializeComponent();
        }

        public async override void Initialize(User user, bool isEditMode = false)
        {
            base.Initialize(user, isEditMode);

            // First check if we have previous day sentiment score in schema extension
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + user.AuthResults.access_token);
            using (var resp = await client.GetAsync($"https://graph.microsoft.com/beta/me/extensions/MSMPrevDaySentiment"))
            {
                if (resp.IsSuccessStatusCode)
                {
                    // HAPPY PATH
                }
                else
                {
                    // first get the user's sent items mailFolder
                    client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + user.AuthResults.access_token);
                    using (var mfResp = await client.GetAsync($"https://graph.microsoft.com/v1.0/me/mailFolders"))
                    {
                        if (mfResp.IsSuccessStatusCode)
                        {
                            var mfJson = await mfResp.Content.ReadAsStringAsync();
                            JArray mfResults = JObject.Parse(mfJson).SelectToken("value").Value<JArray>();
                            var sentFolderId = "";
                            for (var i = 0; i < mfResults.Count; i++)
                            {
                                if (mfResults[i].SelectToken("displayName").Value<string>() == "Sent Items")
                                {
                                    sentFolderId = mfResults[i].SelectToken("id").Value<string>();
                                    break;
                                }
                            }

                            // Process mail for a specific day
                            var now = DateTime.Now;
                            var d1 = $"{now.AddDays(-2).ToString("yyyy-MM-dd")}T00:00:00Z";
                            var d2 = $"{now.AddDays(-2).ToString("yyyy-MM-dd")}T23:59:59Z";
                            var d3 = $"{now.AddDays(-1).ToString("yyyy-MM-dd")}T00:00:00Z";
                            var d4 = $"{now.AddDays(-1).ToString("yyyy-MM-dd")}T23:59:59Z";
                            var yesterdayEndpoint = $"https://graph.microsoft.com/v1.0/me/mailfolders/{sentFolderId}/messages?$select=subject,sentDateTime,body&$orderBy=sentDateTime%20desc&$filter=sentDateTime ge {d1} and sentDateTime le {d2}";
                            var prev = await getItemsRecursive(user.AuthResults.access_token, yesterdayEndpoint);
                            var yesterdayEndPoint = $"https://graph.microsoft.com/v1.0/me/mailfolders/{sentFolderId}/messages?$select=subject,sentDateTime,body&$orderBy=sentDateTime%20desc&$filter=sentDateTime ge {d3} and sentDateTime le {d4}";
                            var yesterday = await getItemsRecursive(user.AuthResults.access_token, yesterdayEndPoint);

                            var prevSentiment = await getAvgSentiment(prev);
                            var yesterdaySentiment = await getAvgSentiment(yesterday);
                            tbPrev.Text = $"2 days ago: {prevSentiment * 100}";
                            tbYesterday.Text = $"Yesterday: {yesterdaySentiment * 100}";
                        }
                    }
                }
            }
        }

        private async Task<Decimal> getAvgSentiment(JArray items)
        {
            Decimal sentiment = 0.0M;

            // Build up a payload of all messages so we can make just one call into the text analytics service
            var payload = new Documents();
            for (var i = 0; i < items.Count; i++)
            {
                payload.documents.Add(new Document() { id = i, language = "en", text = items[i].SelectToken("body.content").Value<string>() });
            }

            // Only try to get sentiment if we are processing one or more messages
            if (payload.documents.Count > 0)
            {
                // Get sentiment for all the messages
                HttpClient sentimentClient = new HttpClient();
                sentimentClient.DefaultRequestHeaders.Add("Accept", "application/json");
                sentimentClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "03c751a65dcd4263a8cec60be5fdcfa5");
                StringContent content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                using (var resp = await sentimentClient.PostAsync("https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment", content))
                {
                    if (resp.IsSuccessStatusCode)
                    {
                        // Add the sentiment as an open extension on the message
                        var sentimentResponse = await resp.Content.ReadAsStringAsync();
                        var docs = (JArray)JObject.Parse(sentimentResponse)["documents"];
                        for (var j = 0; j < docs.Count; j++)
                        {
                            sentiment += docs[j].SelectToken("score").Value<Decimal>();
                        }

                        sentiment = sentiment / docs.Count;
                        return sentiment;
                    }
                    else
                        return sentiment;
                }
            }
            else
                return sentiment;
        }

        private async Task<JArray> getItemsRecursive(string accessToken, string endpoint)
        {
            // First check if we have previous day sentiment score in schema extension
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            client.DefaultRequestHeaders.Add("Prefer", "outlook.body-content-type=\"text\"");
            using (var resp = await client.GetAsync(endpoint))
            {
                if (resp.IsSuccessStatusCode)
                {
                    var jsonStr = await resp.Content.ReadAsStringAsync();
                    var json = JObject.Parse(jsonStr);
                    var array = json.SelectToken("value").Value<JArray>();
                    //array.Concat()

                    // Check if there is a next link and keep processing
                    if (json["@odata.nextLink"] != null)
                    {
                        // call recursively to get net page
                        array.Concat(await getItemsRecursive(accessToken, json["@odata.nextLink"].Value<string>()));
                    }

                    return array;
                }
                else
                    return null;
            }
        }
    }

    public class Documents
    {
        public Documents() { documents = new List<Document>(); }
        public List<Document> documents { get; set; }
    }
    public class Document
    {
        public string language { get; set; }
        public int id { get; set; }
        public string text { get; set; }
    }
}
