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

            while (true)
            {
                //show inbox
                for (int loop = 1; loop <= 3; loop++)
                {
                    var jsonMessage = await GetCurrentUserInboxMessagesAsync(user.AuthResults.access_token);
                    JArray messages = JObject.Parse(jsonMessage).Value<JArray>("value");

                    if (messages.Count > 0)
                    {
                        inboxPanel.Children.Clear();
                        // loop through messages
                        foreach (var msg in messages)
                        {
                            string from = msg.SelectToken("from.emailAddress.name").Value<string>();
                            string subject = msg.SelectToken("subject").Value<string>();
                            string importance = msg.SelectToken("importance").Value<string>();
                            //string bodyPreview = msg.SelectToken("bodyPreview").Value<string>();

                            Grid grid = new Grid();
                            grid.Margin = new Thickness(0, 10, 0, 10);
                            grid.ColumnDefinitions.Add(new ColumnDefinition() { MaxWidth = 40, MinWidth = 40 });
                            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                            SymbolIcon icon = new SymbolIcon(Symbol.Mail);
                            icon.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
                            if (importance == "high")
                                icon.Foreground = new SolidColorBrush(Windows.UI.Colors.OrangeRed);
                            icon.VerticalAlignment = VerticalAlignment.Top;
                            grid.Children.Add(icon);
                            Grid.SetColumn(icon, 0);

                            StackPanel sp = new StackPanel();
                            sp.Orientation = Orientation.Vertical;
                            grid.Children.Add(sp);
                            Grid.SetColumn(sp, 1);


                            TextBlock tbFrom = new TextBlock();
                            tbFrom.Width = this.ActualWidth - 40;
                            tbFrom.Text = from;
                            tbFrom.FontSize = 24;
                            tbFrom.TextWrapping = TextWrapping.Wrap;
                            tbFrom.VerticalAlignment = VerticalAlignment.Top;
                            sp.Children.Add(tbFrom);

                            TextBlock tbSubject = new TextBlock();
                            tbSubject.Width = this.ActualWidth - 40;
                            tbSubject.Text = subject;
                            tbSubject.FontSize = 18;
                            tbSubject.TextWrapping = TextWrapping.Wrap;
                            sp.Children.Add(tbSubject);

                            //TextBlock tbbody = new TextBlock();
                            //tbbody.Width = this.ActualWidth - 60;
                            //tbbody.Text = bodyPreview;
                            //tbbody.FontSize = 12;
                            //tbbody.TextWrapping = TextWrapping.WrapWholeWords;
                            //sp.Children.Add(tbbody);

                            Line line = new Line();
                            line.Y1 = 10;
                            line.Y2 = 10;
                            line.X2 = this.ActualWidth;
                            line.StrokeThickness = 1;
                            line.Stroke = new SolidColorBrush(Windows.UI.Colors.White);
                            line.StrokeDashArray = new DoubleCollection() { 1 };

                            inboxPanel.Children.Add(grid);
                            //inboxPanel.Children.Add(line);
                        }
                    }

                    await Task.Delay(5000);
                }

                //show tasks
                var jsonTask = await GetCurrentUserTasksAsync(user.AuthResults.access_token);
                if (jsonTask != null)
                {
                    JArray tasks = JObject.Parse(jsonTask).Value<JArray>("value");

                    if (tasks.Count > 0)
                    {
                        inboxPanel.Children.Clear();
                        // loop through tasks
                        foreach (var tsk in tasks)
                        {
                            string title = tsk.SelectToken("title").Value<string>();
                            string dueDateTime = tsk.SelectToken("dueDateTime").Value<string>();

                            Grid grid = new Grid();
                            grid.Margin = new Thickness(0, 10, 0, 10);
                            grid.ColumnDefinitions.Add(new ColumnDefinition() { MaxWidth = 40, MinWidth = 40 });
                            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                            SymbolIcon icon = new SymbolIcon(Symbol.Accept); //AllApps or Stop
                            icon.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
                            icon.VerticalAlignment = VerticalAlignment.Top;
                            grid.Children.Add(icon);
                            Grid.SetColumn(icon, 0);

                            StackPanel sp = new StackPanel();
                            sp.Orientation = Orientation.Vertical;
                            grid.Children.Add(sp);
                            Grid.SetColumn(sp, 1);


                            TextBlock tbTitle = new TextBlock();
                            tbTitle.Width = this.ActualWidth - 40;
                            tbTitle.Text = title;
                            tbTitle.FontSize = 24;
                            tbTitle.TextWrapping = TextWrapping.Wrap;
                            tbTitle.VerticalAlignment = VerticalAlignment.Top;
                            sp.Children.Add(tbTitle);

                            TextBlock tbDueDate = new TextBlock();
                            tbDueDate.Width = this.ActualWidth - 40;
                            tbDueDate.Text = dueDateTime;
                            tbDueDate.FontSize = 18;
                            tbDueDate.TextWrapping = TextWrapping.Wrap;
                            sp.Children.Add(tbDueDate);

                            inboxPanel.Children.Add(grid);

                        }
                    }
                }
                inboxPanel.Children.Clear();
                await Task.Delay(15000);
            }
        }

        public async Task<string> GetCurrentUserInboxMessagesAsync(string accessToken)

        {
            // get the user's messages from inbox
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            using (var response = await client.GetAsync($"https://graph.microsoft.com/v1.0/me/messages?$top=5&$filter=isRead eq false ")) //and importance eq 'high'
            {
                if (response.IsSuccessStatusCode)
                {
                    var jsonMessage = await response.Content.ReadAsStringAsync();
                    return jsonMessage;
                }
            }
            return null;
        }

        public async Task<string> GetCurrentUserTasksAsync(string accessToken)

        {
            // get the user's task
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            using (var response = await client.GetAsync($"https://graph.microsoft.com/v1.0/me/planner/tasks/"))
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

