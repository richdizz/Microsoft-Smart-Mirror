using Newtonsoft.Json.Linq;
using SmartMirror.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SmartMirror.Controls
{
    public sealed partial class AgendaPart : MirrorPartBase
    {
        public AgendaPart()
        {
            this.InitializeComponent();
        }

        public async override void Initialize(User user, bool isEditMode = false)
        {
            base.Initialize(user, isEditMode);

            // get the user's events from calendar
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + user.AuthResults.access_token);
            using (var resp = await client.GetAsync($"https://graph.microsoft.com/v1.0/me/calendarview?startdatetime={DateTime.Now.ToString("s")}&enddatetime={DateTime.Now.AddDays(1).ToString("s")}"))
            {
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    JArray events = JObject.Parse(json).Value<JArray>("value");

                    // get the date of the first item
                    if (events.Count > 0)
                    {
                        timeline.Visibility = Visibility.Visible;

                        // loop through events
                        var dateIndex = "";
                        foreach (var evt in events)
                        {
                            // check if we need to add a new date header
                            var thisDate = ParseDateWithUsCulture(evt.SelectToken("start.dateTime").Value<string>()).ToString("D");
                            if (thisDate != dateIndex)
                            {
                                // remove this condition to show more than one calendar date
                                if (dateIndex == "")
                                {
                                    dateIndex = thisDate;
                                    addDateHeader(dateIndex);
                                }
                                else
                                    break;
                            }

                            // parse the start and end date which are stored in UTC time
                            DateTime start = ParseDateWithUsCulture(evt.SelectToken("start.dateTime").Value<string>());
                            DateTime end = ParseDateWithUsCulture(evt.SelectToken("end.dateTime").Value<string>());
                            string subject = evt.SelectToken("subject").Value<string>();
                            string bodyPreview = evt.SelectToken("bodyPreview").Value<string>();

                            Grid grid = new Grid();
                            grid.Margin = new Thickness(0, 10, 0, 10);
                            grid.ColumnDefinitions.Add(new ColumnDefinition() { MaxWidth = 60, MinWidth = 60 });
                            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                            Ellipse ellipse = new Ellipse();
                            ellipse.HorizontalAlignment = HorizontalAlignment.Left;
                            ellipse.VerticalAlignment = VerticalAlignment.Top;
                            ellipse.Width = 15;
                            ellipse.Height = 15;
                            ellipse.Margin = new Thickness(34, 10, 0, 0);
                            ellipse.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));
                            grid.Children.Add(ellipse);
                            Grid.SetColumn(ellipse, 0);

                            StackPanel sp = new StackPanel();
                            sp.Orientation = Orientation.Vertical;
                            grid.Children.Add(sp);
                            Grid.SetColumn(sp, 1);

                            TextBlock tbTime = new TextBlock();
                            tbTime.Padding = new Thickness(20, 0, 0, 0);
                            tbTime.Width = this.ActualWidth - 60;
                            tbTime.Text = $"{start.ToString("h:mm tt")} - {end.ToString("h:mm tt")}";
                            tbTime.Style = (Style)App.Current.Resources["SubSectionHeader"];
                            tbTime.TextWrapping = TextWrapping.WrapWholeWords;
                            sp.Children.Add(tbTime);

                            TextBlock tbSubject = new TextBlock();
                            tbSubject.Padding = new Thickness(20, 0, 0, 0);
                            tbSubject.Width = this.ActualWidth - 60;
                            tbSubject.Text = subject;
                            tbSubject.Style = (Style)App.Current.Resources["Text"];
                            tbSubject.TextWrapping = TextWrapping.WrapWholeWords;
                            sp.Children.Add(tbSubject);

                            agendaPanel.Children.Add(grid);
                        }
                    }
                }
            }
        }

        private void addDateHeader(string date)
        {
            Grid grid = new Grid();
            grid.Margin = new Thickness(0, 10, 0, 10);
            grid.ColumnDefinitions.Add(new ColumnDefinition() { MaxWidth = 60, MinWidth = 60 });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            Ellipse ellipse = new Ellipse();
            ellipse.HorizontalAlignment = HorizontalAlignment.Left;
            ellipse.VerticalAlignment = VerticalAlignment.Top;
            ellipse.Width = 25;
            ellipse.Height = 25;
            ellipse.Margin = new Thickness(29, 15, 0, 0);
            ellipse.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));
            grid.Children.Add(ellipse);
            Grid.SetColumn(ellipse, 0);

            TextBlock tbDate = new TextBlock();
            tbDate.Width = this.ActualWidth - 60;
            tbDate.Text = date;
            tbDate.Style = (Style)App.Current.Resources["SectionHeader"];
            tbDate.TextWrapping = TextWrapping.WrapWholeWords;
            grid.Children.Add(tbDate);
            Grid.SetColumn(tbDate, 1);

            agendaPanel.Children.Add(grid);
        }

        private static DateTime ParseDateWithUsCulture(string date)
        {
            return DateTime.Parse(date + "+00", new CultureInfo("en-US"));
        }
    }
}
