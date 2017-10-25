using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

namespace SmartMirror.Controls
{
    public class MirrorPartBase : UserControl
    {
        protected Rectangle rec;
        protected TextBlock label;
        public int Index { get; set; }

        public MirrorPartBase()
        {
            this.SizeChanged += MirrorPartBase_SizeChanged;
        }

        public Models.User ActiveUser { get; set; }
        private bool isEditMode = false;
        public virtual void Initialize(Models.User user, bool edit = false)
        {
            this.ActiveUser = user;
            isEditMode = edit;
        }

        public void ToggleEditMode(bool edit)
        {
            isEditMode = edit;
            rec.Visibility = (isEditMode) ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;
            label.Visibility = (isEditMode) ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void MirrorPartBase_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {

            // Get the grid
            //Grid root = (Grid)this.Content;
            //rec = new Rectangle()
            //{
            //    Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
            //    StrokeThickness = 3,
            //    StrokeDashArray = new DoubleCollection() { 4, 4 },
            //    RadiusX = 20,
            //    RadiusY = 20,
            //    HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch,
            //    VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch,
            //    Margin = new Windows.UI.Xaml.Thickness(20, 20, 20, 20),
            //    RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5)
            //};
            //rec.RenderTransform = new RotateTransform();

            //Storyboard storyboard = new Storyboard();
            //DoubleAnimationUsingKeyFrames animation = new DoubleAnimationUsingKeyFrames();
            //animation.RepeatBehavior = RepeatBehavior.Forever;
            //storyboard.Children.Add(animation);
            //Random rand = new Random();
            //animation.KeyFrames.Add(new LinearDoubleKeyFrame() { Value = 2, KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(.05)) });
            //animation.KeyFrames.Add(new LinearDoubleKeyFrame() { Value = -3, KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(.1)) });
            //animation.KeyFrames.Add(new LinearDoubleKeyFrame() { Value = 4, KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(.15)) });
            //animation.KeyFrames.Add(new LinearDoubleKeyFrame() { Value = -1, KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(.2)) });
            //Storyboard.SetTarget(storyboard, rec);
            //Storyboard.SetTargetProperty(storyboard, "(UIElement.RenderTransform).(RotateTransform.Angle)");

            //label = new TextBlock();
            //label.Text = Index.ToString();
            //label.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
            //label.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
            //label.FontSize = 90;
            //root.Children.Add(label);

            //root.Children.Add(rec);
            //rec.Visibility = (isEditMode) ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;
            //label.Visibility = (isEditMode) ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;
            //storyboard.Begin();
        }
    }
}
