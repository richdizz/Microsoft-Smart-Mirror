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
using System.Threading.Tasks;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SmartMirror.Controls
{
    public sealed partial class ClockPart : MirrorPartBase
    {
        public ClockPart()
        {
            this.InitializeComponent();
            this.Loaded += ClockPart_Loaded;
        }

        private async void ClockPart_Loaded(object sender, RoutedEventArgs e)
        {
            await updateTime();
        }

        private async Task updateTime()
        {
            theTime.Text = DateTime.Now.ToString("T");
            await Task.Delay(1000);
            await updateTime();
        }
    }
}
