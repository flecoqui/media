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
#if WINDOWS_PHONE_APP
using Windows.Graphics.Display;
#endif
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
                if (IsHLSUrl(url))
                {
                    url = UpdateHLSUrl(url);
                }
                Player.RealTimePlayback = true;
                Player.Source = new Uri(url);                
            }
            catch (Exception ex)
            {
                LogMessage("Exception while playing media: " + ex.Message.ToString());
            }
        }





        #region Plugins
        // HLS Component
        public MediaExtensionManager MediaManager;        
        Microsoft.HLSClient.HLSControllerFactory ControllerFactory;
        Microsoft.HLSClient.IHLSController Controller;

        /// <summary>
        /// Check whether it's a HLS Url
        /// </summary>
        /// <param name="url">Url </param>
        bool IsHLSUrl(string url)
        {
            url = url.ToLower();
            if (url.Contains(".m3u8") == true)
                return true;
            else if (url.IndexOf("manifest(format=m3u8") > 0)
                return true;
            return false;
        }

        /// <summary>
        /// Update HLS url and return an url associated with the HLS Plugin
        /// </summary>
        /// <param name="url">HLS Url </param>
        /// <return >New HLS Url </param>
        string UpdateHLSUrl(string url)
        {
            if (url.StartsWith("http:"))
                url = url.Replace("http:", "ms-hls:");
            else if (url.StartsWith("https:"))
                url = url.Replace("https:", "ms-hls-s:");
            return url;
        }
        void RegisterPlugins()
        {
            LogMessage("Register plugins");
            
            // HLS registration
            MediaManager = new Windows.Media.MediaExtensionManager();
            
            if (ControllerFactory != null)
            {
                ControllerFactory.HLSControllerReady -= ControllerFactory_HLSControllerReady;
                ControllerFactory = null;
            }
            ControllerFactory = new Microsoft.HLSClient.HLSControllerFactory();
            ControllerFactory.HLSControllerReady += ControllerFactory_HLSControllerReady;
            ControllerFactory.PrepareResourceRequest += ControllerFactory_PrepareResourceRequest;

            PropertySet hlsps = new PropertySet();
            hlsps.Add("MimeType", "application/x-mpegurl");
            hlsps.Add("ControllerFactory", ControllerFactory);

            MediaManager.RegisterSchemeHandler("Microsoft.HLSClient.HLSPlaylistHandler", "ms-hls:", hlsps);
            MediaManager.RegisterSchemeHandler("Microsoft.HLSClient.HLSPlaylistHandler", "ms-hls-s:", hlsps);
            MediaManager.RegisterByteStreamHandler("Microsoft.HLSClient.HLSPlaylistHandler", ".m3u8", "application/x-mpegurl", hlsps);
            MediaManager.RegisterByteStreamHandler("Microsoft.HLSClient.HLSPlaylistHandler", ".ism/manifest(format=m3u8-aapl)", "application/x-mpegurl", hlsps);
        }

        void ControllerFactory_PrepareResourceRequest(Microsoft.HLSClient.IHLSControllerFactory sender, Microsoft.HLSClient.IHLSResourceRequestEventArgs args)
        {
            var Headers = args.GetHeaders();
            args.SetHeader("User-Agent", "NSPlayer/12.00.9600.17415 WMFSDK/12.00.9600.17415");
            args.Submit();
        }


        void UnregisterPlugins()
        {
            LogMessage("Unregister plugins");

            if (ControllerFactory != null)
            {
                ControllerFactory.HLSControllerReady -= ControllerFactory_HLSControllerReady;
                ControllerFactory = null;
            }
        }
        void Playlist_StreamSelectionChanged(Microsoft.HLSClient.IHLSPlaylist sender, Microsoft.HLSClient.IHLSStreamSelectionChangedEventArgs args)
        {
            LogMessage("Stream Selection changed for uri: " + sender.Url.ToString());
        }
        uint Lastbitrate = 0;
        void Playlist_BitrateSwitchCompleted(Microsoft.HLSClient.IHLSPlaylist sender, Microsoft.HLSClient.IHLSBitrateSwitchEventArgs args)
        {
            if ((args != null) && (args.ForTrackType == Microsoft.HLSClient.TrackType.VIDEO))
            {
                if (Lastbitrate != args.ToBitrate)
                {
                    Lastbitrate = args.ToBitrate;
                    LogMessage("Bitrate changed for uri: " + sender.Url.ToString());
                    LogMessage(" New bitrate: " + args.ToBitrate.ToString());
                }
            }
        }
        void ControllerFactory_HLSControllerReady(Microsoft.HLSClient.IHLSControllerFactory sender, Microsoft.HLSClient.IHLSController args)
        {

            Controller = args;
            Controller.EnableAdaptiveBitrateMonitor = true;
            Lastbitrate = 0;
            if ((Controller != null) && (Controller.Playlist != null))
            {
                Controller.Playlist.StreamSelectionChanged += Playlist_StreamSelectionChanged;
                Controller.Playlist.BitrateSwitchCompleted += Playlist_BitrateSwitchCompleted;
                Controller.PrepareResourceRequest += Controller_PrepareResourceRequest;
                LogMessage("HLS PlayList Ready for uri: " + Controller.Playlist.Url.ToString());

                var Streams = Controller.Playlist.GetVariantStreams();
                if (Streams != null)
                {
                    foreach (var track in Streams)
                    {
                        if (track.HasResolution == true)
                            LogMessage("  Bitrate: " + track.Bitrate.ToString() + " Width: " + track.HorizontalResolution.ToString() + " Height: " + track.VerticalResolution.ToString());
                        else
                            LogMessage("  Bitrate: " + track.Bitrate.ToString());
                    }
                }
            }
/*
            var settings = App.ViewModel.hlsSettings;
            //Controller.MinimumBufferLength = settings.MinimumBufferLength;
            Controller.EnableAdaptiveBitrateMonitor = settings.EnableAdaptiveBitrateMonitor;
            //Controller.MinimumBufferLength = settings.MinimumBufferLength;
            Controller.UseTimeAveragedNetworkMeasure = settings.UseTimeAveragedNetworkMeasure;
            //          Controller.BitrateChangeNotificationInterval = settings.BitrateChangeNotificationInterval;
            //Controller.KeyFrameMatchTryLimitOnBitrateSwitch = settings.KeyFrameMatchTryLimitOnBitrateSwitch;
            Controller.SegmentTryLimitOnBitrateSwitch = settings.SegmentTryLimitOnBitrateSwitch;
            //Controller.BitrateSwitchOnSegmentBoundaryOnly = settings.BitrateSwitchOnSegmentBoundaryOnly;
            //Controller.AllowParallelDownloadsForBitrateSwitch = settings.AllowParallelDownloadsForBitrateSwitch;
            Controller.Playlist.StreamSelectionChanged += Playlist_StreamSelectionChanged;
            Controller.Playlist.SegmentSwitched += Playlist_SegmentSwitched;
            Controller.Playlist.SegmentDataLoaded += Playlist_SegmentDataLoaded;
            Controller.ForceKeyFrameMatchOnSeek = settings.ForceKeyframeMatchOnSeek;
            Controller.AllowSegmentSkipOnSegmentFailure = settings.AllowSegmentSkipOnSegmentFailure;
            //Controller.BitrateToleranceMarginInPercentage = settings.BitrateToleranceMarginInPercentage;
            Controller.UseTimeAveragedNetworkMeasure = settings.UseTimeAveragedNetworkMeasure;
            if (Controller.Playlist.IsMaster)
            {
                if (settings.StartBitrate != 0 && settings.StartBitrate * 1024 <= Controller.Playlist.GetVariantStreams().Last().Bitrate && settings.StartBitrate * 1024 >= Controller.Playlist.GetVariantStreams().First().Bitrate)
                    Controller.Playlist.StartBitrate = settings.StartBitrate * 1024;
                if (settings.MaximumBitrate != 0 && settings.MaximumBitrate * 1024 >= Controller.Playlist.GetVariantStreams().First().Bitrate)
                    Controller.Playlist.MaximumAllowedBitrate = settings.MaximumBitrate * 1024;
                if (settings.MinimumBitrate != 0 && settings.MinimumBitrate * 1024 <= Controller.Playlist.GetVariantStreams().Last().Bitrate)
                    Controller.Playlist.MinimumAllowedBitrate = settings.MinimumBitrate * 1024;
            }
*/
        }

        void Controller_PrepareResourceRequest(Microsoft.HLSClient.IHLSController sender, Microsoft.HLSClient.IHLSResourceRequestEventArgs args)
        {
            var Headers =  args.GetHeaders();
            args.SetHeader("User-Agent","NSPlayer/12.00.9600.17415 WMFSDK/12.00.9600.17415");
            args.Submit();
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
