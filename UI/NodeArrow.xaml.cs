using BinTreeVisualization.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace BinTreeVisualization.UI
{
    /// <summary>
    /// Represents an arrow that can point between nodes.
    /// </summary>
    public partial class NodeArrow : UserControl
    {
        public NodeArrow()
        {
            InitializeComponent();
            VerticalAlignment = VerticalAlignment.Top;
            HorizontalAlignment = HorizontalAlignment.Center;
            RenderTransformOrigin = new(0.0, 0.5);
        }

        private Canvas GetCanvas() => VisualTreeHelper.GetParent(this) as Canvas;

        /// <summary>
        /// Current in UI loc
        /// </summary>
        public Point SelfLoc => GetLocOf(this);

        private Point GetLocOf(NodeArrow control) => new(Canvas.GetLeft(control), Canvas.GetTop(control));

/*        public Point Target
        {
            get;
            set
            {
                field = value;
                RotateToTarget(value);
            }
        }
        public Point Source
        {
            get;
            set
            {
                field = value;
                RotateToTarget(Target);
                Canvas.SetTop(this, value.Y);
                Canvas.SetLeft(this, value.X);
                RotateToTarget(Target);
            }
        }*/

        /// <summary>
        /// The current target the arrow points to.
        /// </summary>
        public Point Target
        {
            get => (Point)GetValue(TargetProp);
            set => SetValue(TargetProp, value);
        }

        /// <summary>
        /// The current source the arrow points from.
        /// </summary>
        public Point Source
        {
            get => (Point)GetValue(SourceProp);
            set => SetValue(SourceProp, value);
        }

        public static readonly DependencyProperty SourceProp =
    DependencyProperty.Register(
        "Source",
        typeof(Point),
        typeof(NodeArrow),
        new PropertyMetadata(new Point(0, 0), OnSourceChanged));

        public static readonly DependencyProperty TargetProp =
    DependencyProperty.Register(
        "Target",
        typeof(Point),
        typeof(NodeArrow),
        new PropertyMetadata(new Point(0, 0), OnTargetChanged));

        private static void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NodeArrow)d;
            if (e.NewValue is Point newTarget)
            {
                control.RotateToTarget(newTarget);
            }
        }
        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NodeArrow)d;
            if (e.NewValue is Point newSource)
            {
                Canvas.SetTop(control, newSource.Y);
                Canvas.SetLeft(control, newSource.X);
                control.RotateToTarget(control.Target);
            }
        }

        public double TargetX => Target.X;
        public double TargetY => Target.Y;

        /// <summary>
        /// Set the arrow to instantly rotate towards specified target.
        /// </summary>
        /// <param name="target"></param>
        public void RotateToTarget(Point target)
        {
            TransformGroup transformGroup = new();

            RotateTransform rotateTransform = new(GetRotToTarget(target));
            transformGroup.Children.Add(rotateTransform);

            ScaleTransform scaleTransform = new(GetDistanceTo(target) / 100, 1);
            transformGroup.Children.Add(scaleTransform);

            this.RenderTransform = transformGroup;
        }

        /// <summary>
        /// Remove this arrow from the canvas.
        /// </summary>
        public void RemoveSelf()
        {
            GetCanvas().Children.Remove(this);
        }

        private double GetRotToTarget(Point target)
        {
            return Math.Atan2(target.Y - Source.Y, target.X - Source.X) * 180 / Math.PI;
        }

        private double GetDistanceTo(Point target)
        {
            return Math.Sqrt(Math.Pow(target.X - Source.X, 2) + Math.Pow(target.Y - Source.Y, 2));
        }

        public static Point GetUpperArrowSocket(Point fromUpperLeftCorner)
        {
            return new(fromUpperLeftCorner.X + Node<int>.NodeWidth / 2, fromUpperLeftCorner.Y);
        }

        public static Point GetLowerArrowSocket(Point fromUpperLeftCorner)
        {
            return new(fromUpperLeftCorner.X + Node<int>.NodeWidth / 2, fromUpperLeftCorner.Y + Node<int>.NodeHeight);
        }

        /// <summary>
        /// Repoint the source's location to new loc in a 0.5s animation.
        /// </summary>
        /// <param name="newSource"></param>
        public void MoveSourceToLoc(Point newSource)
        {
            newSource = GetLowerArrowSocket(newSource);
            float dur = 0.5f;
            PointAnimation sourceAnim = new()
            {
                To = newSource,
                Duration = TimeSpan.FromSeconds(dur)
            };
            Storyboard storyboard = new();
            Storyboard.SetTarget(sourceAnim, this);
            Storyboard.SetTargetProperty(sourceAnim, new PropertyPath("(Source)"));
            storyboard.Children.Add(sourceAnim);
            storyboard.Begin();
        }

        public void MoveSourceTo(NodeControl node)
        {
            MoveSourceToLoc(node.GetLowerArrowSocket());
        }

        public void MoveTargetTo(NodeControl node)
        {
            MoveTargetToLoc(node.GetUpperArrowSocket());
        }

        /// <summary>
        /// Repoint the target's location to new loc in a 0.5s animation.
        /// </summary>
        /// <param name="newTarget"></param>
        public void MoveTargetToLoc(Point newTarget)
        {
            newTarget = GetUpperArrowSocket(newTarget);
            float dur = 0.5f;
            PointAnimation targetAnim = new()
            {
                To = newTarget,
                Duration = TimeSpan.FromSeconds(dur)
            };
            Storyboard storyboard = new();
            Storyboard.SetTarget(targetAnim, this);
            Storyboard.SetTargetProperty(targetAnim, new PropertyPath("(Target)"));
            storyboard.Children.Add(targetAnim);
            storyboard.Begin();
        }

        /// <summary>
        /// Repoint the source and target locations to new locs in a 0.5s animation.
        /// </summary>
        /// <param name="newSource"></param>
        /// <param name="newTarget"></param>
        public void RotateToLoc(Point newSource, Point newTarget)
        {
            MoveSourceToLoc(newSource);
            MoveTargetToLoc(newTarget);
        }
    }
}
