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
using Windows.Media.Protection;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.Graphics.Display;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MediaPlayer
{
    /// <summary>
    /// Partial MainPage class source code used for Fullscreen mode support on Windows Phone 8.1 
    /// </summary>
    public sealed partial class MainPage : Page
    {

        #region Fullscreen
        double initialHeight = 0;
        double initialWidth = 0;
        /// <summary>
        /// Invoked when Orientation is changed 
        /// </summary>
        private void OnOrientationChanged(DisplayInformation sender, object args)
        {

            System.Diagnostics.Debug.WriteLine("OnOrientationChanged invoked with Orientation:" + sender.CurrentOrientation.ToString());

            if ((sender.CurrentOrientation == DisplayOrientations.Landscape) ||
                (sender.CurrentOrientation == DisplayOrientations.LandscapeFlipped))
            {
                VisualStateManager.GoToState(this, "Landscape", false);
                SetFullScreen(true);
            }
            else
            {
                VisualStateManager.GoToState(this, "Portrait", false);
                SetFullScreen(false);
            }
        }

        /// <summary>
        /// Set Fullscreen mode 
        /// </summary>
        private async void SetFullScreen(bool bFullscreen)
        {
            if (bFullscreen == true)
            {

                await Windows.UI.ViewManagement.StatusBar.GetForCurrentView().HideAsync();
                Player.SetValue(Grid.RowProperty, 0);
                Player.SetValue(Grid.ColumnProperty, 0);
                Player.SetValue(Grid.RowSpanProperty, 4);
                Player.SetValue(Grid.ColumnSpanProperty, 3);
                Player.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
                if ((this.ActualWidth > 0) && (this.ActualHeight > 0))
                {
                    if (this.ActualWidth > this.ActualHeight)
                    {
                        Player.Height = this.ActualHeight;
                        Player.Width = this.ActualWidth;
                    }
                    else
                    {
                        Player.Height = this.ActualWidth;
                        Player.Width = this.ActualHeight;
                    }
                } 

                Player.Margin = new Thickness(0, 0, 0, 0);
            }
            else
            {

                await Windows.UI.ViewManagement.StatusBar.GetForCurrentView().ShowAsync();
                Player.SetValue(Grid.RowProperty, 3);
                Player.SetValue(Grid.ColumnProperty, 0);
                Player.SetValue(Grid.RowSpanProperty, 1);
                Player.SetValue(Grid.ColumnSpanProperty, 3);
                Player.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
                Player.Height = initialHeight;
                Player.Width = initialWidth ;

                Player.UpdateLayout();
                Player.Margin = new Thickness(10, 10, 10, 10);
            }
        }
        #endregion










    }
}
