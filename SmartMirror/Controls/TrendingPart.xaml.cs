using SmartMirror.Models;
using System;
using System.Collections.Generic;
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
    public sealed partial class TrendingPart : MirrorPartBase
    {
        public TrendingPart()
        {
            this.InitializeComponent();
        }

        public async override void Initialize(User user, bool isEditMode = false)
        {
            base.Initialize(user, isEditMode);

            // Query trending information for the user
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + user.AuthResults.access_token);
            using (var resp = await client.GetAsync($"https://graph.microsoft.com/beta/me/insights/trending"))
            {
            }
        }
    }
}
