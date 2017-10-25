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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SmartMirror.Controls
{
    public sealed partial class NewsPart : MirrorPartBase
    {
        public NewsPart()
        {
            this.InitializeComponent();
            this.Loaded += NewsPart_Loaded;
            this.SizeChanged += NewsPart_SizeChanged;
        }

        private void NewsPart_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = e.NewSize.Width;
            var height = (width / 640) * 360;
            theWebView.Width = width;
            theWebView.Height = height;
            var streamSource = $"https://livestream.com/accounts/6372917/events/2592483/player?width={width}&height={height}&enableInfoAndActivity=true&defaultDrawer=&autoPlay=true&mute=false";
            theWebView.Source = new Uri(streamSource);
        }

        private void NewsPart_Loaded(object sender, RoutedEventArgs e)
        {
            var width = this.ActualWidth;
            var height = (width / 640) * 360;
            theWebView.Width = width;
            theWebView.Height = height;
            var streamSource = $"https://livestream.com/accounts/6372917/events/2592483/player?width={width}&height={height}&enableInfoAndActivity=true&defaultDrawer=&autoPlay=true&mute=false";
            theWebView.Source = new Uri(streamSource);
        }
    }
}
