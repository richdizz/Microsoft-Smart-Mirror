using Newtonsoft.Json.Linq;
using SmartMirror.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SmartMirror.Controls
{
    public sealed partial class InboxPart : MirrorPartBase
    {
        public InboxPart()
        {
            this.InitializeComponent();
        }

        public async override void Initialize(User user, bool isEditMode = false)
        {
            base.Initialize(user, isEditMode);
            
            var jsonMessage = await GetCurrentUserInboxMessagesAsync(user.AuthResults.access_token);
            JArray messages = JObject.Parse(jsonMessage).Value<JArray>("value");

            if (messages.Count > 0)
            {
                messageBox.Visibility = Visibility.Visible;

                // loop through messages
                foreach (var msg in messages)
                {
                    string from = msg.SelectToken("from.emailAddress.name").Value<string>();
                    string subject = msg.SelectToken("subject").Value<string>();
                    //string bodyPreview = msg.SelectToken("bodyPreview").Value<string>();
                 
                    Grid grid = new Grid();
                    grid.Margin = new Thickness(0, 10, 0, 10);
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { MaxWidth = 40, MinWidth = 40 });
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                    StackPanel sp = new StackPanel();
                    sp.Orientation = Orientation.Vertical;
                    grid.Children.Add(sp);
                    Grid.SetColumn(sp, 1);

                    TextBlock tbFrom = new TextBlock();
                    tbFrom.Width = this.ActualWidth - 40;
                    tbFrom.Text = from;
                    tbFrom.FontSize = 18;
                    tbFrom.TextWrapping = TextWrapping.WrapWholeWords;
                    sp.Children.Add(tbFrom);

                    TextBlock tbSubject = new TextBlock();
                    tbSubject.Width = this.ActualWidth - 40;
                    tbSubject.Text = subject;
                    tbSubject.FontSize = 14;
                    tbSubject.TextWrapping = TextWrapping.WrapWholeWords;
                    sp.Children.Add(tbSubject);

                    //TextBlock tbbody = new TextBlock();
                    //tbbody.Width = this.ActualWidth - 60;
                    //tbbody.Text = bodyPreview;
                    //tbbody.FontSize = 12;
                    //tbbody.TextWrapping = TextWrapping.WrapWholeWords;
                    //sp.Children.Add(tbbody);

                    Line line = new Line();
                    line.Y1 = 15;
                    line.Y2 = 15;
                    line.X2 = this.ActualWidth - 40;
                    line.StrokeThickness = 1;
                    line.Stroke = new SolidColorBrush(Windows.UI.Colors.White);
                    line.StrokeDashArray = new DoubleCollection(){1};
                    sp.Children.Add(line);

                    inboxPanel.Children.Add(grid);
                }

            }
        }

        public async Task<string> GetCurrentUserInboxMessagesAsync(string accessToken)

        {
            // get the user's messages from inbox
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            using (var response = await client.GetAsync($"https://graph.microsoft.com/v1.0/me/messages?$top=5&$filter=isRead eq false")) //&$filter=inferenceClassification eq 'Focused'")
            {
                if (response.IsSuccessStatusCode)
                {
                    var jsonMessage = await response.Content.ReadAsStringAsync();
                    return jsonMessage;
                }
            }
            return null;
        }
    }
}

