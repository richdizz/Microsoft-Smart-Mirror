using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.ProjectOxford.Face;
using Newtonsoft.Json.Linq;
using SmartMirror.Models;
using SmartMirror.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.FaceAnalysis;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SmartMirror
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private User activeUser;
        private AuthenticationContext ctx = new AuthenticationContext(AuthHelper.AUTHORITY, false, new TokenCache());
        private int timeoutTicks = 10000; // ticks between checks if the signed in user is still in front of mirror
        private int timeoutCountdown = 15; // seconds before we automatically sign the user out after we determine him away from mirror
        private static string FACE_COGSVC_KEY = "c7b03b0b8fe748bcb090475af14ed338"; // subscription key for Microsoft Face Cognitive Service

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;

            // Setup default view of fullscreen
            //TODO: undo the comment-out
            //ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
            //ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // wait for a user in front of the mirror
            tbDeviceCodePrompt.Text = "Stand in front of the Microsoft Smart Mirror to sign-in";
            await waitForUser();
        }

        /// <summary>
        /// Waits for a user to stand in front of the mirror
        /// </summary>
        /// <returns></returns>
        private async Task waitForUser()
        {
            // Go into loop looking for faces
            IList<DetectedFace> faces = new List<DetectedFace>();
            while (faces.Count == 0)
            {
                var results = await detectFaces();
                faces = results.Faces;
                if (faces.Count == 0)
                {
                    // wait one second and then look again
                    var timer = Task.Delay(1000);
                    await timer;
                }
                else
                {
                    // get user match from picture
                    activeUser = await getUserMatch(results.ImageBytes);

                    // See if we found a match
                    if (activeUser == null)
                    {
                        // This is a new user...prompt them to sign-in using device code flow
                        var codeResult = await ctx.AcquireDeviceCodeAsync(AuthHelper.GRAPH_RESOURCE, AuthHelper.CLIENT_ID);
                        tbDeviceCodePrompt.Text = codeResult.Message;
                        var result = await AuthHelper.AcquireTokenByDeviceCodeAsync(codeResult);
                        if (result == null)
                            await waitForUser();
                        else
                        {
                            // Provision User in local storage
                            activeUser = new User(result);
                            activeUser.Photo = results.ImageBytes;

                            // Save the user in storage
                            await StorageHelper.SaveUserAsync(activeUser);
                            tbDeviceCodePrompt.Text = $"Welcome {activeUser.DisplayName}...you are now registered to use the Microsoft Smart Mirror";
                            await waitForUserExit(timeoutTicks);
                        }
                    }
                    else
                    {
                        // Try to get new token for the user
                        var token = await AuthHelper.AcquireTokenWithRefreshTokenAsync(activeUser.AuthResults.refresh_token, AuthHelper.GRAPH_RESOURCE);
                        if (token == null)
                        {
                            // This is a new user...prompt them to sign-in using device code flow
                            var codeResult = await ctx.AcquireDeviceCodeAsync(AuthHelper.GRAPH_RESOURCE, AuthHelper.CLIENT_ID);
                            tbDeviceCodePrompt.Text = codeResult.Message;
                            var result = await AuthHelper.AcquireTokenByDeviceCodeAsync(codeResult);
                            if (result == null)
                                await waitForUser();
                            else
                            {
                                // update the user's tokens
                                activeUser.AuthResults = result;

                                // Save the user in storage
                                await StorageHelper.SaveUserAsync(activeUser);
                                tbDeviceCodePrompt.Text = $"Welcome back {activeUser.DisplayName}";
                                await waitForUserExit(timeoutTicks);
                            }
                        }
                        else
                        {
                            tbDeviceCodePrompt.Text = $"Welcome back {activeUser.DisplayName}";
                            await waitForUserExit(timeoutTicks);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Waits for a signed in user to step away from the mirror
        /// </summary>
        /// <param name="ticks"></param>
        /// <returns></returns>
        private async Task waitForUserExit(int ticks)
        {
            // delay by timeout ticks
            var timer = Task.Delay(ticks);
            await timer;

            // look for the user in the frame
            var results = await detectFaces();

            // check for match
            if (results.Faces.Count != 0)
            {
                // Look for matches
                var user = await getUserMatch(results.ImageBytes);
                if (user.Id != activeUser.Id)
                {
                    // the current user does not match the activeuser
                    if (timeoutCountdown < 0)
                    {
                        // the sign-out countdown has expired
                        timeoutCountdown = 15;
                        activeUser = null;
                        tbDeviceCodePrompt.Text = "Stand in front of the Microsoft Smart Mirror to sign-in";
                        await waitForUser();
                    }
                    else
                    {
                        // update the sign-out countdown
                        tbDeviceCodePrompt.Text = $"Where did you go {activeUser.DisplayName}? We will automatically sign you out in {timeoutCountdown--.ToString()} sec.";
                        await waitForUserExit(1000);
                    }
                }
                else
                {
                    // the current user matches the active user...reset the countdown
                    timeoutCountdown = 15;
                    tbDeviceCodePrompt.Text = "";
                    await waitForUserExit(timeoutTicks);
                }
            }
            else
            {
                // no faces detected...start the countdown
                if (timeoutCountdown < 0)
                {
                    // the sign-out countdown has expired
                    timeoutCountdown = 15;
                    activeUser = null;
                    tbDeviceCodePrompt.Text = "Stand in front of the Microsoft Smart Mirror to sign-in";
                    await waitForUser();
                }
                else
                {
                    // update the sign-out countdown
                    tbDeviceCodePrompt.Text = $"Where did you go {activeUser.DisplayName}? We will automatically sign you out in {timeoutCountdown--.ToString()} sec.";
                    await waitForUserExit(1000);
                }
            }
        }

        /// <summary>
        /// Detects faces
        /// </summary>
        /// <returns>complex object containing the detected faces and the image captured from camera</returns>
        private async Task<ImageWithFaceDetection> detectFaces()
        {
            // Initialize MediaCapture and FaceDetector
            MediaCapture capture = new MediaCapture();
            await capture.InitializeAsync();
            var lowLagCapture = await capture.PrepareLowLagPhotoCaptureAsync(ImageEncodingProperties.CreateUncompressed(MediaPixelFormat.Bgra8));
            FaceDetector faceDetector = await FaceDetector.CreateAsync();

            // Capture a frame and check for faces
            var photo = await lowLagCapture.CaptureAsync();
            var bmp = SoftwareBitmap.Convert(photo.Frame.SoftwareBitmap, BitmapPixelFormat.Nv12);

            // Get the jpeg image bytes for this image
            byte[] imageBytes = null;
            using (var ms = new InMemoryRandomAccessStream())
            {
                var jpeg = SoftwareBitmap.Convert(photo.Frame.SoftwareBitmap, BitmapPixelFormat.Rgba16);
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, ms);
                encoder.SetSoftwareBitmap(jpeg);
                await encoder.FlushAsync();
                imageBytes = new byte[ms.Size];
                await ms.ReadAsync(imageBytes.AsBuffer(), (uint)ms.Size, InputStreamOptions.None);
            }

            // Detect faces
            IList<DetectedFace> faces = await faceDetector.DetectFacesAsync(bmp);

            // TODO: crop captured picture to only look at user in the center;
            return new ImageWithFaceDetection(imageBytes, faces);
        }

        /// <summary>
        /// Finds an existing registered user that matches the picture passed in (using msft face cog svc)
        /// </summary>
        /// <param name="capturedPictureBytes"></param>
        /// <returns>Matching User</returns>
        private async Task<User> getUserMatch(byte[] capturedPictureBytes)
        {
            // get all registered mirror users
            var users = await StorageHelper.GetUsersAsync();

            //var stream = photo.Frame.AsStreamForRead();
            for (var i = 0; i < users.Count; i++)
            {
                // check for match (score >= 95 from face cog svc)
                var score = await getScoreForTwoFaces(new MemoryStream(capturedPictureBytes), new MemoryStream(users[i].Photo));
                if (score >= 95)
                {
                    return users[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Gets match score for two photos
        /// </summary>
        /// <param name="img1">the first photo</param>
        /// <param name="img2">the second photo</param>
        /// <returns>match score between 0 and 100</returns>
        private async Task<double> getScoreForTwoFaces(Stream img1, Stream img2)
        {
            // TODO: update to handle multiple faces???
            try
            {
                FaceServiceClient client = new FaceServiceClient(FACE_COGSVC_KEY);
                var faces1 = await client.DetectAsync(img1);
                var faces2 = await client.DetectAsync(img2);

                if (faces1 == null || faces2 == null)
                {
                    var x = 1;
                    //return Json(new { error = "Error: It looks like we can't detect faces in one of these photos..." });
                }
                if (faces1.Count() == 0 || faces2.Count() == 0)
                {
                    var x = 1;
                    //return Json(new { error = "Error: It looks like we can't detect faces in one of these photos..." });
                }
                if (faces1.Count() > 1 || faces2.Count() > 1)
                {
                    var x = 1;
                    //return Json(new { error = "Error: Each photo must have only one face. Nothing more, nothing less..." });
                }
                var res = await client.VerifyAsync(faces1[0].FaceId, faces2[0].FaceId);
                double score = 0;
                if (res.IsIdentical)
                    score = 100;
                else
                {
                    score = Math.Round((res.Confidence / 0.5) * 100);
                }

                return score;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}
