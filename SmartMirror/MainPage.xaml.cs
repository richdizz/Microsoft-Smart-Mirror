using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SmartMirror
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await tryLogin();
        }

        private static string CLIENT_ID = "53280cb3-3587-43fe-a264-234ce1d22524";
        private static string REDIRECT = "https://localhost:44300";
        private static string AUTHORITY = "https://login.microsoftonline.com/common";
        private static string GRAPH_RESOURCE = "https://graph.microsoft.com";

        private async Task tryLogin()
        {
            /* TODO:
             * if (facialRecognitionMatch) {
             *     if (tokenStillGood) {
             *         signUserIntoMirror
             *     }
             *     else {
             *         displayDeviceCodeToSignin
             *     }
             * }
             * else {
             *    displayDeviceCodeToSignin
             * }
             */

            AuthenticationContext ctx = new AuthenticationContext(AUTHORITY);
            var codeResult = await ctx.AcquireDeviceCodeAsync(GRAPH_RESOURCE, CLIENT_ID);
            tbDeviceCodePrompt.Text = codeResult.Message;
            var result = await ctx.AcquireTokenByDeviceCodeAsync(codeResult);
            // can now use result.AccessToken with graph...we would also want to store a token and take pictures of the user here
            tbDeviceCodePrompt.Text = $"Hello {result.UserInfo.GivenName} {result.UserInfo.FamilyName}! Your customized mirror will now load!";
        }
    }
}
