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
            TriggerSpawnAnim();
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

        public Point DesiredTarget { get; private set; }
        public Point DesiredSource { get; private set; }

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

        /// <summary>
        /// Set the arrow to instantly rotate towards specified target.
        /// </summary>
        /// <param name="target"></param>
        public void RotateToTarget(Point target)
        {
            TransformGroup transformGroup = new();

/*            ScaleTransform scaleTransform = new(GetDistanceTo(target) / 100, 1);
            transformGroup.Children.Add(scaleTransform);*/

            RotateTransform rotateTransform = new(GetRotToTarget(target));
            transformGroup.Children.Add(rotateTransform);

            Vector toTarget = target - Source;
            toTarget.X = Math.Abs(toTarget.X);
            toTarget.Y = Math.Abs(toTarget.Y);

            Vector scaledVec = toTarget.Normal() * (GetDistanceTo(target) / 100);
            ScaleTransform scaleTransform = new(scaledVec.X, scaledVec.Y);
            transformGroup.Children.Add(scaleTransform);

            this.RenderTransform = transformGroup;
        }

        /// <summary>
        /// Remove this arrow from the canvas.
        /// </summary>
        public async void RemoveSelf()
        {
            TriggerDespawnAnim();
            await Task.Delay(200);
            GetCanvas().Children.Remove(this);
        }

        /// <summary>
        /// Get rotation in radians from current <see cref="Source"/> to the <paramref name="target"/>.<para/>
        /// Does NOT use <see cref="DesiredSource"/> as this method is being used in live calculations.
        /// </summary>
        /// <param name="target">Absolute location of point to rotate to</param>
        /// <returns>Degrees from source to target</returns>
        private double GetRotToTarget(Point target)
        {
            return Math.Atan2(target.Y - Source.Y, target.X - Source.X) * 180 / Math.PI;
        }

        /// <summary>
        /// Get distance from current <see cref="Source"/> to the <paramref name="target"/>.<para/>
        /// Does NOT use <see cref="DesiredSource"/> as this method is being used in live calculations.
        /// </summary>
        /// <param name="target">Absolute location of point to calculate the distance to</param>
        /// <returns>WPF length units from to the target</returns>
        private double GetDistanceTo(Point target)
        {
            return Math.Sqrt(Math.Pow(target.X - Source.X, 2) + Math.Pow(target.Y - Source.Y, 2));
        }

        /// <summary>
        /// Get a good angle counting from node's side (1, 0) for an arrow to go out of considering the amount of free space between nodes.<para/>
        /// Reads <see cref="Node{T}.ToBottomOffset"/> and <see cref="Node{T}.NodeHeight"/> for calculations.
        /// </summary>
        /// <returns>A visually pleasing angle.</returns>
        private static double GetGoodSideAngle()
        {
            double EmptySpaceBetweenNodes = Node<int>.ToBottomOffset - Node<int>.NodeHeight;
            if (EmptySpaceBetweenNodes <= 10)
                return 10;
            else
                return 45;
        }

        /// <summary>
        /// Get location of a socket suitable to be pointed to by node's parent.
        /// </summary>
        /// <param name="fromUpperLeftCorner">Upper left coord of child node to point to</param>
        /// <returns>Absolute position of the point on node's border</returns>
        public static Point GetUpperArrowSocket(Point fromUpperLeftCorner)
        {
            double XOffset = Node<int>.NodeWidth / 2;
            double YOffset = 0;
            return new(fromUpperLeftCorner.X + XOffset, fromUpperLeftCorner.Y + YOffset);
        }

        /// <summary>
        /// Get location of a socket suitable for a parent to point to the left node.
        /// </summary>
        /// <param name="fromUpperLeftCorner">Upper left coord of the parent for the arrow to be sourced from</param>
        /// <returns>Absolute position of the point on node's border</returns>
        public static Point GetLeftArrowSocket(Point fromUpperLeftCorner)
        {
            return GetNodeBorder(fromUpperLeftCorner, 180 - GetGoodSideAngle());
        }

        /// <summary>
        /// Get location of socket suitable for a parent to point to the right node.
        /// </summary>
        /// <param name="fromUpperLeftCorner">Upper left coord of the parent for the arrow to be sourced from</param>
        /// <returns>Absolute position of the point on node's border</returns>
        public static Point GetRightArrowSocket(Point fromUpperLeftCorner)
        {
            return GetNodeBorder(fromUpperLeftCorner, GetGoodSideAngle());
        }

        /// <summary>
        /// Get location of node's border on the specified agle, assuming <paramref name="degrees"/> = 0 is pointing to vec (1, 0)
        /// </summary>
        /// <param name="fromUpperLeftCorner">Upper left coord of the node</param>
        /// <param name="degrees">Degrees</param>
        /// <returns>Absolute position of the point on node's border</returns>
        public static Point GetNodeBorder(Point fromUpperLeftCorner, double degrees)
        {
            double h = fromUpperLeftCorner.X + Node<int>.NodeWidth / 2;
            double k = fromUpperLeftCorner.Y + Node<int>.NodeHeight / 2;

            double a = Node<int>.NodeWidth / 2;
            double b = Node<int>.NodeHeight / 2;

            double theta = (degrees * Math.PI) / 180;

            double x = h + a * Math.Cos(theta);
            double y = k + b * Math.Sin(theta);

            return new(x, y);
        }

        /// <summary>
        /// Get location of socket suitable for a parent to point right downwards.
        /// </summary>
        /// <param name="fromUpperLeftCorner">Upper left coord of the parent for the arrow to be sourced from</param>
        /// <returns>Absolute position of the point on node's border</returns>
        public static Point GetLowerArrowSocket(Point fromUpperLeftCorner)
        {
            double XOffset = Node<int>.NodeWidth / 2;
            double YOffset = Node<int>.NodeHeight;
            return new(fromUpperLeftCorner.X + XOffset, fromUpperLeftCorner.Y + YOffset);
        }

        bool LastSourceSideWasRight = false;

        /// <summary>
        /// Repoint the source's location to new loc in a 0.5s animation.
        /// </summary>
        /// <param name="newSource">Upper left coord of the node's expected position</param>
        public void MoveSourceToLoc(Point newSource)
        {
            bool bUseRightSocket = (DesiredTarget - newSource).X > 0;
            newSource = bUseRightSocket ? GetRightArrowSocket(newSource) : GetLeftArrowSocket(newSource);
            LastSourceSideWasRight = bUseRightSocket;
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
            DesiredSource = newSource;
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
        /// <param name="newTarget">Upper left coord of the node's expected position</param>
        public void MoveTargetToLoc(Point newTarget)
        {
            // check if source rotation is needed as well
            bool bUseRightSocket = (newTarget - DesiredSource).X > 0;
            if (bUseRightSocket != LastSourceSideWasRight)
                MoveSourceToLoc(bUseRightSocket ? GetRightArrowSocket(DesiredSource) : GetLeftArrowSocket(DesiredSource));

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
            DesiredTarget = newTarget;
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

        /// <summary>
        /// Trigger spawn fade-in animation.
        /// </summary>
        public void TriggerSpawnAnim()
        {
            BeginStoryboard((Storyboard)FindResource("AnimSpawn"));
        }

        /// <summary>
        /// Trigger despawn fade-out animation.
        /// </summary>
        public void TriggerDespawnAnim()
        {
            BeginStoryboard((Storyboard)FindResource("AnimDespawn"));
        }
    }
}

public static class PointExtensions
{
    /// <summary>
    /// Gets the size of the vector.
    /// </summary>
    /// <param name="p">The vector</param>
    /// <returns>The size of the vector</returns>
    public static double Size(this Vector p)
    {
        return Math.Sqrt(p.X * p.X + p.Y * p.Y);
    }

    /// <summary>
    /// Normalizes the vector.
    /// </summary>
    /// <param name="p">The vector</param>
    /// <returns>A new vector, which is a normalized copy of the source</returns>
    public static Vector Normal(this Vector p)
    {
        double size = p.Size();
        return new Vector(p.X / size, p.Y / size);
    }
}