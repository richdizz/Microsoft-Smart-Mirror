using Microsoft.Bot.Connector.DirectLine;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.ProjectOxford.Face;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartMirror.Controls;
using SmartMirror.Data;
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
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.FaceAnalysis;
using Windows.Media.MediaProperties;
using Windows.Media.SpeechSynthesis;
using Windows.Networking.Sockets;
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
using Microsoft.Cognitive.LUIS;
using Windows.Media.SpeechRecognition;
using Windows.ApplicationModel;
using Windows.UI.Core;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System.Diagnostics;
using System.Text;
using Windows.Devices.Enumeration;


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
        private Queue<string> statementQueue = new Queue<string>();
        private int timeoutTicks = 100000; // ticks between checks if the signed in user is still in front of mirror
        private int timeoutCountdown = 1000; // seconds before we automatically sign the user out after we determine him away from mirror
        private static string FACE_COGSVC_KEY = "a1be835a55e64469a5d150bff962f15f"; // subscription key for Microsoft Face Cognitive Service
        private static string FACE_RECO_GROUP_NAME = "smartmirrorrecogroup_950c414a-af4e-4b5f-b32c-be191bc867d5";
        private static string LUIS_COGSVC_KEY = "6879f9dd46e3476fbf0f95e9c118fe29"; // subscription key for LUIS model
        private ResourceLoader loader = new ResourceLoader();
        private SpeechRecognizer speechRecognizer;
        private static string directLineSecret = "sbnacTqQoeM.cwA.Cgo.fXlrtx5GtXTKcGY50wTiGRG5mNzCj8-grm4MsdcG8GE";
        private static string botId = "MsftSmartMirror";
        private bool inEditMode = false;
        private FaceDetector faceDetector;
        private MediaCapture capture;

        // TODO: should these be in a json file instead???
        private Dictionary<char, WidgetOption> availableWidgets = new Dictionary<char, WidgetOption>()
        {
            { 'A', new WidgetOption("Empty", "SmartMirror.Controls.EmptyPart") },
            { 'B', new WidgetOption("Profile picture", "SmartMirror.Controls.ProfilePicPart") },
            { 'C', new WidgetOption("Daily agenda", "SmartMirror.Controls.ProfilePicPart") },
            { 'D', new WidgetOption("Weather", "SmartMirror.Controls.ProfilePicPart") },
            { 'E', new WidgetOption("Inbox", "SmartMirror.Controls.ProfilePicPart") },
            { 'F', new WidgetOption("Trending info", "SmartMirror.Controls.ProfilePicPart") },
            { 'G', new WidgetOption("Stocks", "SmartMirror.Controls.ProfilePicPart") },
            { 'H', new WidgetOption("News", "SmartMirror.Controls.ProfilePicPart") },
            { 'I', new WidgetOption("Clock", "SmartMirror.Controls.ClockPart") }
        };
        private Dictionary<string, int> numMapping = new Dictionary<string, int>()
        {
            { "one", 1 },
            { "two", 2 },
            { "three", 3 },
            { "four", 4 },
            { "five", 5 },
            { "six", 6 },
            { "seven", 7 },
            { "eight", 8 },
            { "nine", 9 },
            { "ten", 10 },
            { "eleven", 11 },
            { "twleve", 12 },
            { "thirteen", 13 },
            { "fourteen", 14 },
            { "fifteen", 15 }
        };


        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
            this.mediaElement.MediaEnded += MediaElement_MediaEnded;

            // Setup default view of fullscreen
            //TODO: undo the comment-out
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // wait for a user in front of the mirror
            tbMessage.Text = "Stand in front of the Microsoft Smart Mirror to sign-in";
            await startListening();

            // Initialize MediaCapture and FaceDetector
            if (this.capture == null)
            {
                // Choose the first externally plugged in camera found 
                var cameras = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
                var preferredCamera = cameras.FirstOrDefault(deviceInfo => deviceInfo.EnclosureLocation == null);

                // If no external camera, choose the front facing camera ELSE choose the first available camera found 
                if (preferredCamera == null)
                    preferredCamera = cameras.FirstOrDefault(deviceInfo => deviceInfo.EnclosureLocation?.Panel == Windows.Devices.Enumeration.Panel.Front) ?? cameras.FirstOrDefault();

                capture = new MediaCapture();
                await capture.InitializeAsync(new MediaCaptureInitializationSettings()
                {
                    VideoDeviceId = preferredCamera.Id
                });
            }

            if (this.faceDetector == null)
            {
                this.faceDetector = await FaceDetector.CreateAsync();
            }

            await waitForUser();
        }

        /// <summary>
        /// Fires when speech has ended and used to clear text dictation and process next item in statement queue
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">RoutedEventArgs</param>
        private async void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            // clear the text and continue processing statement queue
            tbMessage.Text = "";
            await processStatementQueue();
        }

        /// <summary>
        /// Configures and starts the speech recognizer
        /// </summary>
        /// <returns>Task</returns>
        private async Task startListening()
        {
            // Create an instance of SpeechRecognizer.
            speechRecognizer = new Windows.Media.SpeechRecognition.SpeechRecognizer();

            // Load the SRGS grammar file
            var grammerStorageFile = await Package.Current.InstalledLocation.GetFileAsync(@"MirrorCommands.xml");
            var grammerConstraint = new SpeechRecognitionGrammarFileConstraint(grammerStorageFile);
            speechRecognizer.Constraints.Add(grammerConstraint);

            // Compile the constraints and check for success
            SpeechRecognitionCompilationResult result = await speechRecognizer.CompileConstraintsAsync();
            if (result.Status != SpeechRecognitionResultStatus.Success)
            {
                //TODO
            }
            speechRecognizer.ContinuousRecognitionSession.Completed += (SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args) =>
            {
                //TODO
            };
            speechRecognizer.ContinuousRecognitionSession.ResultGenerated += async (SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args) =>
            {
                // Get the specific command recognized and the text
                string command = args.Result.RulePath[1];
                string text = args.Result.Text;
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    // Process each command type differently
                    if (command == "clearUsersCommand")
                    {
                        await StorageHelper.ResetUsers();
                        activeUser = null;
                        repaint(this.RenderSize);
                        await waitForUser();
                    }
                    else if (command == "saveLayoutCommand")
                    {
                        toggleEditMode(false);
                        await StorageHelper.SaveUserAsync(activeUser);
                    }
                    else if (command == "editLayoutCommand")
                    {
                        toggleEditMode(true);
                        string[] statements = { "To place a widget, use the add command with the widget letter and area number.", "For example, \"yo mirror add B to area seven\"" };
                        await speak(statements);
                    }
                    else if (command == "addWidgetCommand")
                    {
                        text = text.Substring(text.IndexOf("add") + 4).Trim();
                        var parts = text.Split(' ');
                        activeUser.Preferences[numMapping[parts[parts.Length - 1]]] = availableWidgets[Convert.ToChar(parts[0].ToUpper())].ClassName;
                        repaint(this.RenderSize);
                    }
                });
            };
            speechRecognizer.StateChanged += (SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args) =>
            {
                //TODO
            };

            // Start the speechRecognizer with continuous recognition
            await speechRecognizer.ContinuousRecognitionSession.StartAsync();
        }

        /// <summary>
        /// Waits for a user to stand in front of the mirror
        /// </summary>
        /// <returns></returns>
        private async Task waitForUser()
        {
            try
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
                            tbMessage.Text = codeResult.Message;
#if DEBUG
                            // Convenience for debugging
                            //                        await Windows.System.Launcher.LaunchUriAsync(new Uri("https://aka.ms/devicelogin"));
#endif
                            var result = await AuthHelper.AcquireTokenByDeviceCodeAsync(codeResult);
                            if (result == null)
                                await waitForUser();
                            else
                            {
                                // Provision User in local storage
                                activeUser = new User(result);
                                activeUser.Photo = results.ImageBytes;

#if !FACE_COMPARISON
                                // Upload the user image to the Face Reco group and (re)train the model
                                if (!await AddUserToFaceRecoGroup(activeUser))
                                {
                                    // TODO: photo rejected by Face Reco - need to prompt them to try again
                                    await speak(new string[] { "Warning: more or less than one face is detected - try again. NOT YET IMPLEMENTED" });
                                    await waitForUser();
                                }
#endif

                                // Save the user in storage
                                await StorageHelper.SaveUserAsync(activeUser);
                                await speak(new string[] { String.Format(loader.GetString("Welcome"), activeUser.GivenName), loader.GetString("GetStarted"), loader.GetString("ConfigInstructions") });
                                repaint(this.RenderSize);
                                await waitForUserExit(timeoutTicks);
                            }
                        }
                        else
                        {
                            // Try to get new token for the user
                            var token = await AuthHelper.AcquireTokenWithRefreshTokenAsync(activeUser.AuthResults.refresh_token, AuthHelper.GRAPH_RESOURCE);
                            if (token == null)
                            {
                                // This is an existing user, but their token is expired
                                var codeResult = await ctx.AcquireDeviceCodeAsync(AuthHelper.GRAPH_RESOURCE, AuthHelper.CLIENT_ID);
                                tbMessage.Text = codeResult.Message;
                                var result = await AuthHelper.AcquireTokenByDeviceCodeAsync(codeResult);
                                if (result == null)
                                    await waitForUser();
                                else
                                {
                                    // update the user's tokens
                                    activeUser.AuthResults = result;

                                    // Save the user in storage
                                    await StorageHelper.SaveUserAsync(activeUser);
                                    Random rand = new Random();
                                    var statement = String.Format(loader.GetString("WelcomeBack" + rand.Next(6)), activeUser.GivenName);
                                    await speak(statement);
                                    repaint(this.RenderSize);
                                    await waitForUserExit(timeoutTicks);
                                }
                            }
                            else
                            {
                                // This is an existing user and their token is good...update it in storage
                                activeUser.AuthResults = token;
                                await StorageHelper.SaveUserAsync(activeUser);

                                // Welcome the user
                                Random rand = new Random();
                                var statement = String.Format(loader.GetString("WelcomeBack" + rand.Next(6)), activeUser.GivenName);
                                await speak(statement);
                                repaint(this.RenderSize);
                                await waitForUserExit(timeoutTicks);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        /// <summary>
        /// Processes the statement queue
        /// </summary>
        /// <returns></returns>
        private async Task processStatementQueue()
        {
            // check if the statement queue is empty before processing
            if (statementQueue.Count > 0)
            {
                // dequeue the next statement and speak it
                var statement = statementQueue.Dequeue();
                await speak(statement);
            }
        }

        /// <summary>
        /// Converts text to speech for an array of statements
        /// </summary>
        /// <param name="statements">string array</param>
        /// <returns></returns>
        private async Task speak(string[] statements)
        {
            // add each statement into the statement Queue and then process
            foreach (var statement in statements)
                statementQueue.Enqueue(statement);
            await processStatementQueue();
        }
        
        /// <summary>
        /// Speaks a string of text to the user
        /// </summary>
        /// <param name="statement">text to be synthesized</param>
        /// <returns></returns>
        private async Task speak(string statement)
        {
            // display the text
            tbMessage.Text = statement;

            // The object for controlling the speech synthesis engine (voice).
            var synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
            VoiceInformation voiceInfo = (
                from voice in SpeechSynthesizer.AllVoices
                where voice.Gender == VoiceGender.Female
                select voice).FirstOrDefault() ?? SpeechSynthesizer.DefaultVoice;
            synth.Voice = voiceInfo;

            // Generate the audio stream from plain text.
            var wrapper = "<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"en-US\"><p><s><prosody rate=\"medium\">{0}</prosody></s></p></speak>";
            SpeechSynthesisStream stream = await synth.SynthesizeSsmlToStreamAsync(String.Format(wrapper, statement));

            // Send the stream to the media object.
            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.Play();
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
                        tbMessage.Text = "Stand in front of the Microsoft Smart Mirror to sign-in";
                        await waitForUser();
                    }
                    else
                    {
                        // update the sign-out countdown
                        tbMessage.Text = $"Where did you go {activeUser.DisplayName}? We will automatically sign you out in {timeoutCountdown--.ToString()} sec.";
                        await waitForUserExit(1000);
                    }
                }
                else
                {
                    // the current user matches the active user...reset the countdown
                    timeoutCountdown = 15;
                    tbMessage.Text = "";
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
                    tbMessage.Text = "Stand in front of the Microsoft Smart Mirror to sign-in";
                    await waitForUser();
                }
                else
                {
                    // update the sign-out countdown
                    tbMessage.Text = $"Where did you go {activeUser.DisplayName}? We will automatically sign you out in {timeoutCountdown--.ToString()} sec.";
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
            byte[] imageBytes = null;
            IList<DetectedFace> faces = null;

            try
            {
                var lowLagCapture = await capture.PrepareLowLagPhotoCaptureAsync(ImageEncodingProperties.CreateUncompressed(MediaPixelFormat.Bgra8));

                // Capture a frame and check for faces
                var photo = await lowLagCapture.CaptureAsync();
                var bmp = SoftwareBitmap.Convert(photo.Frame.SoftwareBitmap, BitmapPixelFormat.Nv12);

                // Get the jpeg image bytes for this image
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
                faces = await faceDetector.DetectFacesAsync(bmp);
            }
            catch (Exception)
            {
                throw;
            }

            // TODO: crop captured picture to only look at user in the center;
            return new ImageWithFaceDetection(imageBytes, faces);
        }

#if FACE_COMPARISON
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
            catch (Exception)
            {
                return 0;
            }
        }

#else

        /// <summary>
        /// Finds an existing registered user that matches the picture passed in (using msft face cog svc Face Recognition)
        /// </summary>
        /// <param name="capturedPictureBytes"></param>
        /// <returns>Matching User</returns>
        private async Task<User> getUserMatch(byte[] capturedPictureBytes)
        {
            // Ensure person group exists
            await EnsurePersonGroupExists();

            // get all registered mirror users
            var users = await StorageHelper.GetUsersAsync();

            // List of  objects to record matches
            var facesFound = new List<dynamic>();

            var faceServiceClient = new FaceServiceClient(FACE_COGSVC_KEY);

            try
            {
                // Check for reco against the existing group
                var faces = await faceServiceClient.DetectAsync(new MemoryStream(capturedPictureBytes));


                // Call identify REST API, the result contains identified person information
                var identifyResult = await faceServiceClient.IdentifyAsync(FACE_RECO_GROUP_NAME, faces.Select(ff => ff.FaceId).ToArray());
                for (int idx = 0; idx < faces.Length; idx++)
                {
                    // Look at the identified faces and try to match against our known Smart Mirror users
                    var res = identifyResult[idx];
                    if (res.Candidates.Length > 0 && users.Any(u => u.PersonId == res.Candidates[0].PersonId))
                    {
                        var user = users.Where(p => p.PersonId == res.Candidates[0].PersonId).First();
                        facesFound.Add(new { Index = idx, Name = user.DisplayName, User = user });
                    }
                    else
                    {
                        facesFound.Add(new { Index = idx, Name = "Unknown" });
                    }
                }
            }
            catch (FaceAPIException ex)
            {
                if (ex.ErrorCode != "PersonGroupNotTrained")
                {
                    throw;
                }
                else
                {
                    Debug.WriteLine($"Response: Group {FACE_RECO_GROUP_NAME} not trained.");
                    return null;
                }
            }

            var outString = new StringBuilder();
            foreach (var face in facesFound)
            {
                outString.AppendFormat($"Face {face.Index} is identified as {face.Name}. ");
            }
            Debug.WriteLine($"Response: Success. {outString}");

            // Return the identified user - 
            // TODO: Currently returning the first identified face, what to do if there was more than one face found in the captured image?
            foreach (var face in facesFound)
            {
                if (face.Name != "Unknown")
                {
                    return face.User;
                }
            }

            return null;
        }

        private async Task EnsurePersonGroupExists()
        {
            bool groupExists = false;

            var faceServiceClient = new FaceServiceClient(FACE_COGSVC_KEY);

            // Test whether the group already exists
            try
            {
                Debug.WriteLine($"Request: Group {FACE_RECO_GROUP_NAME} will be used to build a person database. Checking whether the group exists.");

                await faceServiceClient.GetPersonGroupAsync(FACE_RECO_GROUP_NAME);
                groupExists = true;
                Debug.WriteLine($"Response: Group {FACE_RECO_GROUP_NAME} exists.");
            }
            catch (FaceAPIException ex)
            {
                if (ex.ErrorCode != "PersonGroupNotFound")
                {
                    throw;
                }
                else
                {
                    Debug.WriteLine($"Response: Group {FACE_RECO_GROUP_NAME} did not exist previously.");
                }
            }

            if (!groupExists)
            {
                Debug.WriteLine($"Request: Creating group \"{FACE_RECO_GROUP_NAME}\"");
                try
                {
                    // Create person group API call will failed if group with the same name already exists
                    await faceServiceClient.CreatePersonGroupAsync(FACE_RECO_GROUP_NAME, FACE_RECO_GROUP_NAME);
                    Debug.WriteLine($"Response: Success. Group \"{FACE_RECO_GROUP_NAME}\" created");
                }
                catch (FaceAPIException ex)
                {
                    Debug.WriteLine($"Response: {ex.ErrorCode}. {ex.ErrorMessage}");
                    throw;
                }
            }
        }

        private async Task<bool> AddUserToFaceRecoGroup(User user)
        {
            var faceServiceClient = new FaceServiceClient(FACE_COGSVC_KEY);

            // First ensure we do not inadvertently create any dups
            var persons = await faceServiceClient.ListPersonsAsync(FACE_RECO_GROUP_NAME);
            foreach (var person in (persons.Where(p => p.PersonId == user.Id)))
            {
                await faceServiceClient.DeletePersonAsync(FACE_RECO_GROUP_NAME, person.PersonId);
                Debug.WriteLine($"Response: Success. Person \"{user.DisplayName}\" (PersonID:{person.PersonId.ToString()}) removed");
            }
            foreach (var person in (persons.Where(p => p.Name== user.DisplayName)))
            {
                await faceServiceClient.DeletePersonAsync(FACE_RECO_GROUP_NAME, person.PersonId);
                Debug.WriteLine($"Response: Success. Person \"{user.DisplayName}\" (PersonID:{person.PersonId.ToString()}) removed");
            }

            // Call create person REST API, the new create person id will be returned
            Debug.WriteLine($"Request: Creating person \"{user.DisplayName}\"");
            user.PersonId = (await faceServiceClient.CreatePersonAsync(FACE_RECO_GROUP_NAME, user.DisplayName)).PersonId;
            Debug.WriteLine($"Response: Success. Person \"{user.DisplayName}\" (PersonID:{user.PersonId}) created");

            bool tooManyFacesInImage = false;
            try
            {
                // Update person faces on server side
                var persistFace = await faceServiceClient.AddPersonFaceAsync(FACE_RECO_GROUP_NAME, user.PersonId, new MemoryStream(user.Photo), user.Id.ToString());
            }
            catch (FaceAPIException ex)
            {
                if (ex.ErrorMessage.Contains("more than 1 face in the image."))
                {
                    // Need to prompt user to retry and make sure they are the only one in front of the mirror
                    Debug.WriteLine($"Response: Failure. more than 1 face in the image");
                    tooManyFacesInImage = true;
                }
            }

            if (tooManyFacesInImage)
            {
                Debug.WriteLine("Warning: more or less than one face is detected in the image, can not add to face list.");
                return false;
            }
            Debug.WriteLine("Response: Success. Face image registered.");

            try
            {
                // Start train person group
                Debug.WriteLine($"Request: Training group \"{FACE_RECO_GROUP_NAME}\"");
                await faceServiceClient.TrainPersonGroupAsync(FACE_RECO_GROUP_NAME);

                // Wait until train completed
                while (true)
                {
                    await Task.Delay(1000);
                    var status = await faceServiceClient.GetPersonGroupTrainingStatusAsync(FACE_RECO_GROUP_NAME);
                    Debug.WriteLine($"Response: Success. Group \"{FACE_RECO_GROUP_NAME}\" training process is {status.Status}" );
                    if (status.Status != Microsoft.ProjectOxford.Face.Contract.Status.Running)
                    {
                        break;
                    }
                }
            }
            catch (FaceAPIException ex)
            {
                Debug.WriteLine("Response: {0}. {1}", ex.ErrorCode, ex.ErrorMessage);
                throw;
            }

            return true;
        }

#endif

        /// <summary>
        /// Repaints the mirror UI...usually after a resize
        /// </summary>
        /// <param name="size"></param>
        private void repaint(Size size)
        {
            // Reset the grid
            for (var i = mainView.Children.Count - 1; i >= 0; i--)
            {
                if (mainView.Children[i] is Controls.MirrorPartBase)
                    mainView.Children.RemoveAt(i);
            }
            mainView.ColumnDefinitions.Clear();
            mainView.RowDefinitions.Clear();
            int columns = 5, rows = 6; // Default to portrait mode
            if (size.Height < size.Width)
            {
                // Reverse because we are in landscape mode
                columns = 6;
                rows = 5;
            }

            // Add row definitions
            var index = 1;
            for (var row = 0; row < rows; row++)
            {
                mainView.RowDefinitions.Add(new RowDefinition() { MinHeight = size.Height / rows });
                for (var column = 0; column < columns; column++)
                {
                    if (row == 0)
                        mainView.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = size.Width / columns });

                    if (row == 0 || column == 0 || column == columns - 1)
                    {
                        if (activeUser != null)
                        {
                            // Add the part based on the user's profile
                            MirrorPartBase ctrl = (MirrorPartBase)Activator.CreateInstance(Type.GetType(activeUser.Preferences[index]));
                            ctrl.Index = index++;
                            ctrl.SetValue(Grid.ColumnProperty, column);
                            ctrl.SetValue(Grid.RowProperty, row);
                            mainView.Children.Add(ctrl);
                            ctrl.Initialize(this.activeUser, inEditMode);
                        }
                    }
                }
            }

            // update the tbMessage position
            tbMessage.SetValue(Grid.ColumnProperty, 1);
            tbMessage.SetValue(Grid.RowProperty, rows - 1);
            tbMessage.SetValue(Grid.ColumnSpanProperty, columns - 2);

            // add the options
            tbOptions.Padding = new Thickness(20);
            tbOptions.SetValue(Grid.ColumnProperty, 1);
            tbOptions.SetValue(Grid.RowProperty, 1);
            tbOptions.SetValue(Grid.ColumnSpanProperty, columns - 2);
            tbOptions.SetValue(Grid.RowSpanProperty, rows - 2);
            tbOptions.Text = "";
            foreach (var key in availableWidgets.Keys)
                tbOptions.Text += $"{key}. {availableWidgets[key].DisplayName}\r\n";
        }

        /// <summary>
        /// Event that fires when the page size changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // repaint the ui (aspect ratio could have changed
            repaint(e.NewSize);
        }

        /// <summary>
        /// Toggles the UI into an edit mode
        /// </summary>
        /// <param name="edit"></param>
        public void toggleEditMode(bool edit)
        {
            inEditMode = edit;
            foreach (var ctrl in mainView.Children)
            {
                if (ctrl is Controls.MirrorPartBase)
                    ((Controls.MirrorPartBase)ctrl).ToggleEditMode(edit);
            }
            tbOptions.Visibility = (edit) ? Visibility.Visible : Visibility.Collapsed;
        }



        //*************************************************
        // From here down is all experimental code for 
        // calling a bot via directline/websockets and LUIS 
        // directly. Not sure we will use this code, but 
        // helpful if we need it.
        //*************************************************
        private async Task getLuisResult()
        {
            LuisClient client = new LuisClient("141992cd-189a-415b-94ee-49a745b28271", LUIS_COGSVC_KEY);
            var resp = await client.Predict("Clear all users");
        }
        
        private async Task setupBot()
        {
            // Obtain a token using the Direct Line secret
            var tokenResponse = await new DirectLineClient(directLineSecret).Tokens.GenerateTokenForNewConversationAsync();

            // Use token to create conversation
            var directLineClient = new DirectLineClient(tokenResponse.Token);
            var conversation = await directLineClient.Conversations.StartConversationAsync();
            MessageWebSocket webSock = new MessageWebSocket();
            webSock.Control.MessageType = SocketMessageType.Utf8;
            webSock.MessageReceived += WebSock_MessageReceived;
            Uri serverUri = new Uri(conversation.StreamUrl);
            try
            {
                //Connect to the server.
                await webSock.ConnectAsync(serverUri);

                //Send a message to the server.
                Activity userMessage = new Activity
                {
                    From = new ChannelAccount("FOO"),
                    Text = "clear all users",
                    Type = ActivityTypes.Message
                };
                await directLineClient.Conversations.PostActivityAsync(conversation.ConversationId, userMessage);
            }
            catch (Exception ex)
            {
                //Add code here to handle any exceptions
            }
        }

        private void WebSock_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            DataReader messageReader = args.GetDataReader();
            messageReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            string messageString = messageReader.ReadString(messageReader.UnconsumedBufferLength);

            // Occasionally, the Direct Line service sends an empty message as a liveness ping. Ignore these messages.

            if (string.IsNullOrWhiteSpace(messageString))
                return;

            var activitySet = JsonConvert.DeserializeObject<ActivitySet>(messageString);
            var activities = from x in activitySet.Activities
                             where x.From.Id == botId
                             select x;

            foreach (Activity activity in activities)
            {
                var x = "";
            }
        }
    }
}