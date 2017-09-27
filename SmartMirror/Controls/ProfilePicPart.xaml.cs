using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using SmartMirror.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Net.Http;
using System.Text;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SmartMirror.Controls
{
    public sealed partial class ProfilePicPart : MirrorPartBase
    {
        public ProfilePicPart()
        {
            this.InitializeComponent();
        }

        public async override void Initialize(User user, bool isEditMode = false)
        {
            base.Initialize(user, isEditMode);

            // get the user's profile picture
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + user.AuthResults.access_token);
            using (var resp = await client.GetAsync($"https://graph.microsoft.com/beta/users/{user.Id}/photo/$value"))
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
                        imgEllipse.Fill = fill;
                        imgEllipse.Visibility = Visibility.Visible;
                        imgEllipse.Width = this.ActualWidth / 2;
                        imgEllipse.Height = this.ActualWidth / 2;
                        imgEllipse.Margin = new Thickness(imgEllipse.Width / 2, 0, 0, 0);
                    }
                }
            }
        }
    }
}
