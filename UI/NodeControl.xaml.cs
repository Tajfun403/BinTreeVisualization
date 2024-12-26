using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

namespace BinTreeVisualization.UI
{
    /// <summary>
    /// UI element that represents a node in a binary tree.
    /// </summary>
    public partial class NodeControl : UserControl, INotifyPropertyChanged
    {
        public NodeControl()
        {
            InitializeComponent();
            DataContext = this;
            VerticalAlignment = VerticalAlignment.Top;
            HorizontalAlignment = HorizontalAlignment.Center;
        }

        public NodeControl(object value) : this()
        {
            Value = value;
        }
        public Point CurrLoc => new(Canvas.GetLeft(this), Canvas.GetTop(this));

        public static Color InactiveColor => Color.FromArgb(0, 0, 0, 0);
        public static Color ActiveColor => Color.FromArgb(255, 180, 120, 225);

        public static Color StrokeBase => Color.FromArgb(255, 238, 238, 238);

        public static Color StrokeBlue => Color.FromArgb(255, 67, 149, 212);

        public void Activate()
        {
            BeginStoryboard((Storyboard)FindResource("AnimActivate"));
        }

        public void Deactivate()
        {
            BeginStoryboard((Storyboard)FindResource("AnimDeactivate"));
        }

        public void Blink()
        {
            BeginStoryboard((Storyboard)FindResource("AnimBlink"));
        }

        public void HighlightBlue()
        {
            BeginStoryboard((Storyboard)FindResource("AnimHighlightBlue"));
        }

        public void DeactivateBlue()
        {
            BeginStoryboard((Storyboard)FindResource("AnimDeactivateBlue"));
        }

        public void ActivateBlue()
        {
            BeginStoryboard((Storyboard)FindResource("AnimActivateBlue"));
        }

        /// <summary>
        /// Move the associated node control to specified location over 0.5 seconds
        /// </summary>
        /// <param name="loc">Location to move to</param>
        public void MoveToLoc(Point loc)
        {
            float dur = 0.5f;
            DoubleAnimation xAnim = new()
            {
                To = loc.X,
                Duration = TimeSpan.FromSeconds(dur)
            };

            DoubleAnimation yAnim = new()
            {
                To = loc.Y,
                Duration = TimeSpan.FromSeconds(dur)
            };

            Storyboard storyboard = new();

            Storyboard.SetTarget(xAnim, this);
            Storyboard.SetTargetProperty(xAnim, new PropertyPath("(Canvas.Left)"));

            Storyboard.SetTarget(yAnim, this);
            Storyboard.SetTargetProperty(yAnim, new PropertyPath("(Canvas.Top)"));

            storyboard.Children.Add(xAnim);
            storyboard.Children.Add(yAnim);

            storyboard.Begin();
        }

        public Point GetUpperArrowSocket()
        {
            return new Point(CurrLoc.X + Width / 2, CurrLoc.Y);
        }

        public Point GetLowerArrowSocket()
        {
            return new Point(CurrLoc.X + Width / 2, CurrLoc.Y + Height);
        }

        public object Value
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            } 
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string ValToStr => Value.ToString() ?? "null";
    }
}
