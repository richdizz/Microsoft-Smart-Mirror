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

        // Using a DependencyProperty as the backing store for Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(String), typeof(EmailStat), new PropertyMetadata(String.Empty));


        public string Email { get; set; }



        public string ProfileImage
        {
            get { return (string)GetValue(ProfileImageProperty); }
            set { SetValue(ProfileImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ProfileImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProfileImageProperty =
            DependencyProperty.Register("ProfileImage", typeof(string), typeof(EmailStat), new PropertyMetadata(String.Empty));



        public int Count { get; set; }
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
            
            this.DataContext = ViewModel;
        }

        private User user;

        public async override void Initialize(User user, bool isEditMode = false)
        {
            base.Initialize(user, isEditMode);

            this.user = user;

            // First check if we have previous day sentiment score in schema extension
            HttpClient client = CreateClient();

            using (var resp = await client.GetAsync($"https://graph.microsoft.com/v1.0/me/mailFolders"))
            {
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var folders = JsonConvert.DeserializeObject<MailFoldersResponse.RootObject>(json).value;
                    var sentItems = folders.Where(f => f.displayName == "Sent Items").First();

                    using (var resp2 = await client.GetAsync($"https://graph.microsoft.com/v1.0/me/mailFolders/{sentItems.id}/messages?$top=100"))
                    {
                        var json2 = await resp2.Content.ReadAsStringAsync();
                        var messages = JsonConvert.DeserializeObject<MailFolderMessagesResponse.RootObject>(json2).value;

                        var last5days = messages.Where(m => m.sentDateTime > DateTime.Now.AddDays(-5)).ToList();

                        var recipients = new List<String>();

                        last5days.ForEach(m => m.toRecipients.ForEach(r => recipients.Add(r.emailAddress.name)));

                        var stats = recipients.GroupBy(r => r)
                            .Select(g => new EmailStat{ Name = g.Key, Count = g.Count() }) 
                            .OrderByDescending( s => s.Count).ToList();

                        stats.Take(4).ToList().ForEach(es =>
                        {
                            //LoadImage(es);
                            ViewModel.TopEmail.Add(es);
                        });

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

        // NOTE: This works with email being loaded, not name.  
        //public async void LoadImage(EmailStat stat)
        //{
        //    HttpClient client = CreateClient();
        //    using (var resp = await client.GetAsync($"https://graph.microsoft.com/v1.0/users?$filter=mail eq '{stat.Email}'"))
        //    {
        //        if (resp.IsSuccessStatusCode)
        //        {
        //            var json = resp.Content.ReadAsByteArrayAsync();
        //        }
        //    }
        //}
    }
}
