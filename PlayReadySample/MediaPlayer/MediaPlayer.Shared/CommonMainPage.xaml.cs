using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Media.AdaptiveStreaming;
#if WINDOWS_PHONE_APP
using Windows.Graphics.Display;
#endif
using Windows.Media.Protection;
using Microsoft.Media.PlayReadyClient;
using Windows.Media;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MediaPlayer
{
    /// <summary>
    /// Common (Windows Phone 8.1 and Windows 8.1) MainPage class source code 
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // Streams array used for the tests
        public MediaStream[] StreamArray = MediaStream.GetMediaStreamArray();

        // MainPage constructor
        public MainPage()
        {
            this.InitializeComponent();


            PlayButton.Click += PlayButton_Click;
            ComboStream.SelectionChanged += ComboStream_SelectionChanged;
            for (int i = 0; i < StreamArray.Length; i++)
                ComboStream.Items.Add(StreamArray[i]);
            ComboStream.SelectedIndex = 0;

            Logs.TextChanged  += Logs_TextChanged;

#if WINDOWS_PHONE_APP
            this.NavigationCacheMode = NavigationCacheMode.Required;
#endif
        }
        // ComboStream Selection Changed Event
        void ComboStream_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboStream.SelectedItem != null)
            {
                MediaStream ms = ComboStream.SelectedItem as MediaStream;
                MediaUri.Text = ms.MediaUri;
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LogMessage("OnNavigatedTo");
            base.OnNavigatedTo(e);
            Application.Current.Suspending += App_Suspending;
            Application.Current.Resuming += App_Resuming;
            Player.MediaEnded += Player_MediaEnded;
            Player.MediaOpened += Player_MediaOpened;
            Player.MediaFailed += Player_MediaFailed;
            Player.CurrentStateChanged += Player_CurrentStateChanged;
#if WINDOWS_PHONE_APP
            DisplayInformation.GetForCurrentView().OrientationChanged += OnOrientationChanged;
#endif
            RegisterPlugins();
        }

        void Player_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            LogMessage("Media CurrentStateChanged Event State: " + Player.CurrentState.ToString());      
        }


        void Player_MediaClosed(object sender, RoutedEventArgs e)
        {
            LogMessage("Media Closed Event");
        }

        void Player_MediaStarted(object sender, RoutedEventArgs e)
        {
            LogMessage("Media Started Event");
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            LogMessage("OnNavigatedFrom");
            Application.Current.Suspending -= App_Suspending;
            Application.Current.Resuming -= App_Resuming;
            Player.MediaEnded -= Player_MediaEnded;
            Player.MediaOpened -= Player_MediaOpened;
            Player.MediaFailed -= Player_MediaFailed;
#if WINDOWS_PHONE_APP
            DisplayInformation.GetForCurrentView().OrientationChanged -= OnOrientationChanged;
#endif

            UnregisterPlugins();
        }


        void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("Play Uri: " + MediaUri.Text);
            try
            {
                /// if MPEG DASH Uri Register Dash Plugin else Unregister Dash Plugin
                string url = MediaUri.Text;
                Player.RealTimePlayback = true;
                Player.Source = new Uri(url);                
            }
            catch (Exception ex)
            {
                LogMessage("Exception while playing media: " + ex.Message.ToString());
            }
        }





        #region Plugins
        public MediaExtensionManager MediaManager;
        public IAdaptiveSourceManager AdaptiveSrcManager { get; private set; }


        void RegisterPlugins()
        {
            LogMessage("Register plugins");
            
            // HLS registration
            MediaManager = new Windows.Media.MediaExtensionManager();
            if (MediaManager == null)
                MediaManager = new Windows.Media.MediaExtensionManager();
            if (AdaptiveSrcManager == null)
                AdaptiveSrcManager = AdaptiveSourceManager.GetDefault();
            PropertySet ssps = new PropertySet();
            ssps["{A5CE1DE8-1D00-427B-ACEF-FB9A3C93DE2D}"] = AdaptiveSrcManager;


            MediaManager.RegisterByteStreamHandler("Microsoft.Media.AdaptiveStreaming.SmoothByteStreamHandler", ".ism", "text/xml", ssps);
            MediaManager.RegisterByteStreamHandler("Microsoft.Media.AdaptiveStreaming.SmoothByteStreamHandler", ".ism", "application/vnd.ms-sstr+xml", ssps);
            MediaManager.RegisterByteStreamHandler("Microsoft.Media.AdaptiveStreaming.SmoothByteStreamHandler", ".isml", "text/xml", ssps);
            MediaManager.RegisterByteStreamHandler("Microsoft.Media.AdaptiveStreaming.SmoothByteStreamHandler", ".isml", "application/vnd.ms-sstr+xml", ssps);


            MediaManager.RegisterSchemeHandler("Microsoft.Media.AdaptiveStreaming.SmoothSchemeHandler", "ms-sstr:", ssps);
            if (AdaptiveSrcManager != null)
            {
                AdaptiveSrcManager.ManifestReadyEvent += AdaptiveSrcManager_ManifestReadyEvent;
                AdaptiveSrcManager.AdaptiveSourceStatusUpdatedEvent += AdaptiveSrcManager_AdaptiveSourceStatusUpdatedEvent;
            }

            // PlayReady
            // Init PlayReady Protection Manager
            var protectionManager = new MediaProtectionManager();
            Windows.Foundation.Collections.PropertySet cpSystems = new Windows.Foundation.Collections.PropertySet();
            cpSystems.Add("{F4637010-03C3-42CD-B932-B48ADF3A6A54}", "Microsoft.Media.PlayReadyClient.PlayReadyWinRTTrustedInput"); //Playready
            protectionManager.Properties.Add("Windows.Media.Protection.MediaProtectionSystemIdMapping", cpSystems);
            protectionManager.Properties.Add("Windows.Media.Protection.MediaProtectionSystemId", "{F4637010-03C3-42CD-B932-B48ADF3A6A54}");
            this.Player.ProtectionManager = protectionManager;

            // PlayReady Events registration
            Player.ProtectionManager.ComponentLoadFailed += ProtectionManager_ComponentLoadFailed;
            Player.ProtectionManager.ServiceRequested += ProtectionManager_ServiceRequested;


            
        }
        uint MinBitRate = 0;
        uint MaxBitRate = 10000000;
        void AdaptiveSrcManager_ManifestReadyEvent(AdaptiveSource sender, ManifestReadyEventArgs args)
        {


            LogMessage("Manifest Ready for uri: " + sender.Uri.ToString());
            foreach (var stream in args.AdaptiveSource.Manifest.SelectedStreams)
            {
                if (stream.Type == MediaStreamType.Video)
                {
                    foreach (var track in stream.SelectedTracks)
                    {
                        LogMessage("  Bitrate: " + track.Bitrate.ToString() + " Width: " + track.MaxWidth.ToString() + " Height: " + track.MaxHeight.ToString());

                    }

                    IReadOnlyList<IManifestTrack> list = null;
                    if ((MinBitRate > 0) && (MaxBitRate > 0))
                    {
                        list = stream.AvailableTracks.Where(t => (t.Bitrate > MinBitRate) && (t.Bitrate <= MaxBitRate)).ToList();
                        if ((list != null) && (list.Count > 0))
                            stream.RestrictTracks(list);
                    }
                    else if (MinBitRate > 0)
                    {
                        list = stream.AvailableTracks.Where(t => (t.Bitrate > MinBitRate)).ToList();
                        if ((list != null) && (list.Count > 0))
                            stream.RestrictTracks(list);
                    }
                    else if (MaxBitRate > 0)
                    {
                        list = stream.AvailableTracks.Where(t => (t.Bitrate < MaxBitRate)).ToList();
                        if ((list != null) && (list.Count > 0))
                            stream.RestrictTracks(list);
                    }
                    if ((list != null) && (list.Count > 0))
                    {
                        LogMessage("Select Bitrate between: " + MinBitRate.ToString() + " and " + MaxBitRate.ToString());
                        foreach (var track in stream.SelectedTracks)
                        {
                            LogMessage("  Bitrate: " + track.Bitrate.ToString() + " Width: " + track.MaxWidth.ToString() + " Height: " + track.MaxHeight.ToString());

                        }
                    }
                }
            }
        }
        void AdaptiveSrcManager_AdaptiveSourceStatusUpdatedEvent(AdaptiveSource sender, AdaptiveSourceStatusUpdatedEventArgs args)
        {
            if (args != null)
            {
                if (args.UpdateType == AdaptiveSourceStatusUpdateType.BitrateChanged)
                {

                    LogMessage("Bitrate changed for uri: " + sender.Uri.ToString());
                    foreach (var stream in args.AdaptiveSource.Manifest.SelectedStreams)
                    {
                        if (stream.Type == MediaStreamType.Video)
                        {
                            if (!string.IsNullOrEmpty(args.AdditionalInfo))
                            {
                                int pos = args.AdditionalInfo.IndexOf(';');
                                if (pos > 0)
                                {
                                    try
                                    {

                                        var newBitrate = uint.Parse(args.AdditionalInfo.Substring(0, pos));
                                        foreach (var track in stream.SelectedTracks)
                                        {
                                            if (track.Bitrate == newBitrate)
                                            {
                                                LogMessage("  Bitrate: " + track.Bitrate.ToString() + " Width: " + track.MaxWidth.ToString() + " Height: " + track.MaxHeight.ToString());
                                                break;
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {

                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        void UnregisterPlugins()
        {
            LogMessage("Unregister plugins");
            Player.ProtectionManager.ComponentLoadFailed -= ProtectionManager_ComponentLoadFailed;
            Player.ProtectionManager.ServiceRequested -= ProtectionManager_ServiceRequested;

            if (AdaptiveSrcManager != null)
            {
                AdaptiveSrcManager.ManifestReadyEvent -= AdaptiveSrcManager_ManifestReadyEvent;
                AdaptiveSrcManager.AdaptiveSourceStatusUpdatedEvent -= AdaptiveSrcManager_AdaptiveSourceStatusUpdatedEvent;
            }

        }
        private const int MSPR_E_CONTENT_ENABLING_ACTION_REQUIRED = -2147174251;
        private string PlayReadyLicenseUrl;
        private string PlayReadyChallengeCustomData;
        /// <summary>
        /// Invoked when the Protection Manager can't load some components
        /// </summary>
        void ProtectionManager_ComponentLoadFailed(MediaProtectionManager sender, ComponentLoadFailedEventArgs e)
        {
            LogMessage("ProtectionManager ComponentLoadFailed");
            e.Completion.Complete(false);
        }
        /// <summary>
        /// Invoked to acquire the PlayReady License
        /// </summary>
        async void LicenseAcquisitionRequest(PlayReadyLicenseAcquisitionServiceRequest licenseRequest, MediaProtectionServiceCompletion CompletionNotifier, string Url, string ChallengeCustomData)
        {
            bool bResult = false;
            string ExceptionMessage = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(Url))
                {
                    LogMessage("ProtectionManager PlayReady Manual License Acquisition Service Request in progress - URL: " + Url);

                    if (!string.IsNullOrEmpty(ChallengeCustomData))
                    {
                        System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                        byte[] b = encoding.GetBytes(ChallengeCustomData);
                        licenseRequest.ChallengeCustomData = Convert.ToBase64String(b, 0, b.Length);
                    }

                    PlayReadySoapMessage soapMessage = licenseRequest.GenerateManualEnablingChallenge();

                    byte[] messageBytes = soapMessage.GetMessageBody();
                    HttpContent httpContent = new ByteArrayContent(messageBytes);

                    IPropertySet propertySetHeaders = soapMessage.MessageHeaders;
                    foreach (string strHeaderName in propertySetHeaders.Keys)
                    {
                        string strHeaderValue = propertySetHeaders[strHeaderName].ToString();

                        // The Add method throws an ArgumentException try to set protected headers like "Content-Type"
                        // so set it via "ContentType" property
                        if (strHeaderName.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                            httpContent.Headers.ContentType = MediaTypeHeaderValue.Parse(strHeaderValue);
                        else
                            httpContent.Headers.Add(strHeaderName.ToString(), strHeaderValue);
                    }
                    CommonLicenseRequest licenseAcquision = new CommonLicenseRequest();
                    HttpContent responseHttpContent = await licenseAcquision.AcquireLicense(new Uri(Url), httpContent);
                    if (responseHttpContent != null)
                    {
                        Exception exResult = licenseRequest.ProcessManualEnablingResponse(await responseHttpContent.ReadAsByteArrayAsync());
                        if (exResult != null)
                        {
                            throw exResult;
                        }
                        bResult = true;
                    }
                    else
                        ExceptionMessage = licenseAcquision.GetLastErrorMessage();
                }
                else
                {
                    LogMessage("ProtectionManager PlayReady License Acquisition Service Request in progress - URL: " + licenseRequest.Uri.ToString());
                    await licenseRequest.BeginServiceRequest();
                    bResult = true;
                }
            }
            catch (Exception e)
            {
                ExceptionMessage = e.Message;
            }

            if (bResult == true)
                LogMessage(!string.IsNullOrEmpty(Url) ? "ProtectionManager Manual PlayReady License Acquisition Service Request successful" :
                    "ProtectionManager PlayReady License Acquisition Service Request successful");
            else
                LogMessage(!string.IsNullOrEmpty(Url) ? "ProtectionManager Manual PlayReady License Acquisition Service Request failed: " + ExceptionMessage :
                    "ProtectionManager PlayReady License Acquisition Service Request failed: " + ExceptionMessage);
            CompletionNotifier.Complete(bResult);
        }
        /// <summary>
        /// Proactive Individualization Request 
        /// </summary>
        async void ProActiveIndivRequest()
        {
            PlayReadyIndividualizationServiceRequest indivRequest = new PlayReadyIndividualizationServiceRequest();
            LogMessage("ProtectionManager PlayReady ProActive Individualization Service Request in progress...");
            bool bResultIndiv = await ReactiveIndivRequest(indivRequest, null);
            if (bResultIndiv == true)
                LogMessage("ProtectionManager PlayReady ProActive Individualization Service Request successful");
            else
                LogMessage("ProtectionManager PlayReady ProActive Individualization Service Request failed");

        }
        /// <summary>
        /// Invoked to send the Individualization Request 
        /// </summary>
        async Task<bool> ReactiveIndivRequest(PlayReadyIndividualizationServiceRequest IndivRequest, MediaProtectionServiceCompletion CompletionNotifier)
        {
            bool bResult = false;
            Exception exception = null;
            LogMessage("ProtectionManager PlayReady Individualization Service Request in progress...");
            try
            {
                await IndivRequest.BeginServiceRequest();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                if (exception == null)
                {
                    bResult = true;
                }
                else
                {
                    COMException comException = exception as COMException;
                    if (comException != null && comException.HResult == MSPR_E_CONTENT_ENABLING_ACTION_REQUIRED)
                    {
                        IndivRequest.NextServiceRequest();
                    }
                }
            }
            if (bResult == true)
                LogMessage("ProtectionManager PlayReady Individualization Service Request successful");
            else
                LogMessage("ProtectionManager PlayReady Individualization Service Request failed");
            if (CompletionNotifier != null) CompletionNotifier.Complete(bResult);
            return bResult;

        }
        /// <summary>
        /// Invoked to send a PlayReady request (Individualization or License request)
        /// </summary>
        async void ProtectionManager_ServiceRequested(MediaProtectionManager sender, ServiceRequestedEventArgs srEvent)
        {
            LogMessage("ProtectionManager ServiceRequested");
            if (srEvent.Request is PlayReadyIndividualizationServiceRequest)
            {
                PlayReadyIndividualizationServiceRequest IndivRequest = srEvent.Request as PlayReadyIndividualizationServiceRequest;
                bool bResultIndiv = await ReactiveIndivRequest(IndivRequest, srEvent.Completion);
            }
            else if (srEvent.Request is PlayReadyLicenseAcquisitionServiceRequest)
            {
                PlayReadyLicenseAcquisitionServiceRequest licenseRequest = srEvent.Request as PlayReadyLicenseAcquisitionServiceRequest;
                LicenseAcquisitionRequest(licenseRequest, srEvent.Completion, PlayReadyLicenseUrl, PlayReadyChallengeCustomData);
            }
        }

        #endregion



        #region Logs
        private int LogMaxSize = 10000;

        /// <summary>
        /// Display Message on the application page
        /// </summary>
        /// <param name="Message">String to display</param>
        async void LogMessage(string Message)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    DateTime Date = DateTime.Now;
                    string DateString = string.Format("{0:d/M/yyyy HH:mm:ss.fff}", Date);
                    string LocString = DateString + " " + Message + "\n";

                    if (Logs.Text.Length > LogMaxSize)
                    {
                        string LocalString = Logs.Text;
                        LocalString += LocString;
                        while (LocalString.Length > LogMaxSize)
                        {
                            int pos = LocalString.IndexOf('\n',LogMaxSize/4);
                            if ((pos > 0) && (pos < LocalString.Length))
                            {
                                LocalString = LocalString.Substring(pos + 1);
                            }
                        }
                        Logs.Text = LocalString;
                    }
                    else
                        Logs.Text += LocString;

                }
            );
        }
        void Logs_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Logs.Select(Logs.Text.Length, 0);
            
            var grid = (Grid)VisualTreeHelper.GetChild(Logs, 0);
            if (grid == null)
                return;
            for (var i = 0; i <= VisualTreeHelper.GetChildrenCount(grid) - 1; i++)
            {
                object obj = VisualTreeHelper.GetChild(grid, i);
                if (!(obj is ScrollViewer)) continue;
                ((ScrollViewer)obj).ChangeView(0, ((ScrollViewer)obj).ExtentHeight, 1);
                break;
            }
        }
        #endregion


        #region Suspending
        /// <summary>
        /// Invoked when Application is resuming
        /// </summary>
        void App_Resuming(object sender, object e)
        {
            LogMessage("App Resuming");
        }

        /// <summary>
        /// Invoked when Application is suspending
        /// </summary>
        void App_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            LogMessage("App Suspending");
        }
        #endregion


        #region MediaEvents


        void Player_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
#endif
            if ((Player != null) && (Player.Source != null))
                LogMessage("Media Failed for Uri: " + Player.Source.ToString() + " Exception: " + e.ErrorMessage.ToString());
            else
                LogMessage("Media Failed" + " Exception: " + e.ErrorMessage.ToString());
        }

        void Player_MediaOpened(object sender, RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
            initialHeight = Player.ActualHeight;
            initialWidth = Player.ActualWidth;
#endif
            if ((Player != null) && (Player.Source != null))
                LogMessage("Media Opened for Uri: " + Player.Source.ToString());
            else
                LogMessage("Media Opened");
        }
        void Player_MediaEnded(object sender, RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

#endif
            if ((Player != null) && (Player.Source != null))
                LogMessage("Media Ended for Uri: " + Player.Source.ToString());
            else
                LogMessage("Media Ended");
            Player.Source = Player.Source;
        }
        #endregion

    }
}
