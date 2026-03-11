using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace XamlPearls.OneSecond
{
    public class MaskAdorner : Adorner, IDisposable
    {
        private Border _border;
        private VisualCollection _child;
        private ContentControl _content;
        private FrameworkElement _owner;

        public MaskAdorner(FrameworkElement owner, object content, Action completed = null) : base(owner)
        {
            _owner = owner;
            _owner.SizeChanged += (sender, e) => InvalidateVisual();

            _content = new ContentControl
            {
                Content = content,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _border = new Border
            {
                Background = new SolidColorBrush() { Color = (Color)ColorConverter.ConvertFromString("#B2000000") },
                Child = _content
            };
            _child = new VisualCollection(this) { _border };

            var storyboard = new Storyboard();
            if (completed != null)
                storyboard.Completed += (sender, e) => completed();
            AddDoubleAnimationUsingKeyFrames(storyboard, "(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)", _content);
            AddDoubleAnimationUsingKeyFrames(storyboard, "(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)", _content);
            AddRenderTransform(_content);
            AddTrigger(_content, storyboard);
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return _child == null ? 0 : _child.Count;
            }
        }

        public void Dispose()
        {
            _content.Content = null;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_border != null)
            {
                _border.Arrange(new Rect(new Point(0, 0), new Size(_owner.ActualWidth, _owner.ActualHeight)));
            }
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return _child == null ? null : _child[index];
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (_border != null)
            {
                _border.Arrange(new Rect(new Point(0, 0), new Size(_owner.ActualWidth, _owner.ActualHeight)));
            }
            return base.MeasureOverride(constraint);
        }

        private void AddDoubleAnimationUsingKeyFrames(Storyboard storyboard, string property, FrameworkElement target)
        {
            var daukf = new DoubleAnimationUsingKeyFrames();
            Storyboard.SetTargetProperty(daukf, new PropertyPath(property));
            Storyboard.SetTarget(daukf, target);

            var edkf = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)),
                Value = 0
            };
            daukf.KeyFrames.Add(edkf);

            var quinticEase = new QuinticEase { EasingMode = EasingMode.EaseOut };
            edkf = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.6)),
                Value = 1,
                EasingFunction = quinticEase
            };

            daukf.KeyFrames.Add(edkf);
            storyboard.Children.Add(daukf);
        }

        private void AddRenderTransform(FrameworkElement element)
        {
            var group = new TransformGroup();
            group.Children.Add(new ScaleTransform() { ScaleX = 0, ScaleY = 0 });
            group.Children.Add(new SkewTransform());
            group.Children.Add(new RotateTransform());
            group.Children.Add(new TranslateTransform());

            element.RenderTransform = group;
            element.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        private void AddTrigger(FrameworkElement target, Storyboard storyboard)
        {
            var trigger = new EventTrigger { RoutedEvent = LoadedEvent };
            var beginStoryboard = new BeginStoryboard() { Storyboard = storyboard };
            trigger.Actions.Add(beginStoryboard);
            target.Triggers.Add(trigger);
        }
    }
}