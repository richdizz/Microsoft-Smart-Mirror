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
    public sealed partial class MirrorPartContainer : UserControl
    {
        public string Title
        {
            set { tbTitle.Text = value; }
        }
        public static readonly DependencyProperty PlaceHolderProperty =
            DependencyProperty.Register("PlaceHolder", typeof(object), typeof(MirrorPartContainer), new PropertyMetadata(null));
        public object PlaceHolder
        {
            get { return (object)GetValue(PlaceHolderProperty); }
            set { SetValue(PlaceHolderProperty, value); }
        }

        public MirrorPartContainer()
        {
            InitializeComponent();
            this.Loaded += MirrorPartContainer_Loaded;
        }

        private void MirrorPartContainer_Loaded(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(tbTitle.Text))
                tbTitle.Visibility = Visibility.Collapsed;
        }
    }
}
