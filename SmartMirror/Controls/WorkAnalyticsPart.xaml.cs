using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartMirror.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SmartMirror.Controls
{
    public class MailFoldersResponse
    {
        public class Value
        {
            public string id { get; set; }
            public string displayName { get; set; }
            public string parentFolderId { get; set; }
            public int childFolderCount { get; set; }
            public int unreadItemCount { get; set; }
            public int totalItemCount { get; set; }
        }

        public class RootObject
        {
            public List<Value> value { get; set; }
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

    public class MailFolderMessagesResponse
    {
        public class Body
        {
            public string contentType { get; set; }
            public string content { get; set; }
        }

        public class EmailAddress
        {
            public string name { get; set; }
            public string address { get; set; }
        }

        public class Sender
        {
            public EmailAddress emailAddress { get; set; }
        }

        public class EmailAddress2
        {
            public string name { get; set; }
            public string address { get; set; }
        }

        public class From
        {
            public EmailAddress2 emailAddress { get; set; }
        }

        public class EmailAddress3
        {
            public string name { get; set; }
            public string address { get; set; }
        }

        public class ToRecipient
        {
            public EmailAddress3 emailAddress { get; set; }
        }

        public class Value
        {
            public string id { get; set; }
            public DateTime createdDateTime { get; set; }
            public DateTime lastModifiedDateTime { get; set; }
            public string changeKey { get; set; }
            public List<object> categories { get; set; }
            public DateTime receivedDateTime { get; set; }
            public DateTime sentDateTime { get; set; }
            public bool hasAttachments { get; set; }
            public string internetMessageId { get; set; }
            public string subject { get; set; }
            public string bodyPreview { get; set; }
            public string importance { get; set; }
            public string parentFolderId { get; set; }
            public string conversationId { get; set; }
            public bool? isDeliveryReceiptRequested { get; set; }
            public bool isReadReceiptRequested { get; set; }
            public bool isRead { get; set; }
            public bool isDraft { get; set; }
            public string webLink { get; set; }
            public string inferenceClassification { get; set; }
            public Body body { get; set; }
            public Sender sender { get; set; }
            public From from { get; set; }
            public List<ToRecipient> toRecipients { get; set; }
            public List<object> ccRecipients { get; set; }
            public List<object> bccRecipients { get; set; }
            public List<object> replyTo { get; set; }
            public string meetingMessageType { get; set; }
        }

        public class RootObject
        {
            public List<Value> value { get; set; }
        }
    }

    public class EmailStat : DependencyObject
    {


        public String Name
        {
            get { return (String)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(String), typeof(EmailStat), new PropertyMetadata(String.Empty));

        // Using a DependencyProperty as the backing store for Fill.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(EmailStat), new PropertyMetadata(String.Empty));


        public string Email { get; set; }
        public int Count { get; set; }
        public double Width { get; set; }
    }

    public class WorkAnalyticsPartViewModel : DependencyObject
    {
        public ObservableCollection<EmailStat> TopEmail { get; set; } = new ObservableCollection<EmailStat>();
    }

    public sealed partial class WorkAnalyticsPart : MirrorPartBase
    {
        public WorkAnalyticsPartViewModel ViewModel { get; set; } = new WorkAnalyticsPartViewModel();
        public WorkAnalyticsPart()
        {
            this.InitializeComponent();
            wrapper.Title = "Workplace Analytics";
            this.DataContext = ViewModel;
        }

        private User user;

        private void addUnique(Dictionary<string, EmailStat> collection, string email, string name)
        {
            if (collection.ContainsKey(email))
                collection[email].Count++;
            else
                collection.Add(email, new EmailStat()
                {
                    Count = 1,
                    Email = email,
                    Name = name,
                    Fill = new SolidColorBrush(Windows.UI.Colors.Black)
                });
        }

        public async override void Initialize(User user, bool isEditMode = false)
        {
            base.Initialize(user, isEditMode);

            this.user = user;

            // First check if we have previous day sentiment score in schema extension
            HttpClient client = CreateClient();

            using (var resp = await client.GetAsync($"https://graph.microsoft.com/v1.0/me/mailFolders?$filter=displayName eq 'Sent Items'"))
            {
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var folders = JsonConvert.DeserializeObject<MailFoldersResponse.RootObject>(json).value;
                    var sentItems = folders.First();

                    using (var resp2 = await client.GetAsync($"https://graph.microsoft.com/v1.0/me/mailFolders/{sentItems.id}/messages?$top=100"))
                    {
                        var json2 = await resp2.Content.ReadAsStringAsync();
                        var messages = JsonConvert.DeserializeObject<MailFolderMessagesResponse.RootObject>(json2).value;

                        var last5days = messages.Where(m => m.sentDateTime > DateTime.Now.AddDays(-5)).ToList();
                        var recipients = new Dictionary<String, EmailStat>();
                        last5days.ForEach(m => m.toRecipients.ForEach(r => addUnique(recipients, r.emailAddress.address, r.emailAddress.name)));
                        var stats = recipients.Values.OrderByDescending(s => s.Count).ToList();

                        // Calculate the top count and apply to widths
                        var max = stats.Max(m => m.Count);
                        var w = this.ActualWidth;
                        stats.Take(4).ToList().ForEach(async (es) =>
                        {
                            es.Width = w / max * es.Count;
                            es.Fill = await getProfilePhoto(es.Email);
                            ViewModel.TopEmail.Add(es);
                        });


                        // get sentiment 
                        await processSentiment(sentItems.id);

                        // setup animations
                        await scroll();
                    }
                }
            }
        }

        private int activePanel = 1;
        private async Task scroll()
        {
            await Task.Delay(10000);
            if (activePanel == 1)
            {
                sp1Out.Begin();
                sp2In.Begin();
                activePanel = 2;
            }
            else
            {
                sp2Out.Begin();
                sp1In.Begin();
                activePanel = 1;
            }
            await scroll();
        }

        private async Task<ImageBrush> getProfilePhoto(string email)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + this.user.AuthResults.access_token);
            using (var resp = await client.GetAsync($"https://graph.microsoft.com/beta/users/{email}/photo/$value"))
            {
                if (resp.IsSuccessStatusCode)
                {
                    var stream = await resp.Content.ReadAsStreamAsync();
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, (int)stream.Length);

                    using (var memStream = new MemoryStream(bytes))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        await bitmap.SetSourceAsync(memStream.AsRandomAccessStream());
                        ImageBrush fill = new ImageBrush();
                        fill.ImageSource = bitmap;
                        return fill;
                    }
                }
                else
                {
                    using (var file = File.OpenRead("assets\\profile.png"))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        await bitmap.SetSourceAsync(file.AsRandomAccessStream());
                        ImageBrush fill = new ImageBrush();
                        fill.ImageSource = bitmap;
                        return fill;
                    }
                }
            }
        }

        private HttpClient CreateClient()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + this.user.AuthResults.access_token);
            return client;
        }

        private void DoubleAnimation_Completed(object sender, object e)
        {
            if (((DoubleAnimation)sender) == a1)
            {
                panelTrans1.X = 500;
            }
            else
            {
                panelTrans2.X = 500;
            }
        }

        private async Task processSentiment(string sentFolderId)
        {
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
            // Sometimes there is no sentiment - need to check first
            // Neil Hutson
            if (prev != null)
            {
                var prevSentiment = await getAvgSentiment(prev);
                var yesterdaySentiment = await getAvgSentiment(yesterday);
                var change = yesterdaySentiment - prevSentiment;
                var trend = (change >= 0) ? "▲" : "▼";
                tbSent.Text = yesterday.Count.ToString();
                tbSentiment.Text = yesterdaySentiment.ToString("P");
                tbTrend.Text = $"{change.ToString("P")} {trend}";
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

    public class Clip
    {
        public static bool GetToBounds(DependencyObject depObj)
        {
            return (bool)depObj.GetValue(ToBoundsProperty);
        }

        public static void SetToBounds(DependencyObject depObj, bool clipToBounds)
        {
            depObj.SetValue(ToBoundsProperty, clipToBounds);
        }

        /// <summary>
        /// Identifies the ToBounds Dependency Property.
        /// <summary>
        public static readonly DependencyProperty ToBoundsProperty =
            DependencyProperty.RegisterAttached("ToBounds", typeof(bool),
            typeof(Clip), new PropertyMetadata(false, OnToBoundsPropertyChanged));

        private static void OnToBoundsPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement fe = d as FrameworkElement;
            if (fe != null)
            {
                ClipToBounds(fe);

                // whenever the element which this property is attached to is loaded
                // or re-sizes, we need to update its clipping geometry
                fe.Loaded += new RoutedEventHandler(fe_Loaded);
                fe.SizeChanged += new SizeChangedEventHandler(fe_SizeChanged);
            }
        }

        /// <summary>
        /// Creates a rectangular clipping geometry which matches the geometry of the
        /// passed element
        /// </summary>
        private static void ClipToBounds(FrameworkElement fe)
        {
            if (GetToBounds(fe))
            {
                fe.Clip = new RectangleGeometry()
                {
                    Rect = new Rect(0, 0, fe.ActualWidth, fe.ActualHeight)
                };
            }
            else
            {
                fe.Clip = null;
            }
        }

        static void fe_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ClipToBounds(sender as FrameworkElement);
        }

        static void fe_Loaded(object sender, RoutedEventArgs e)
        {
            ClipToBounds(sender as FrameworkElement);
        }
    }
}
