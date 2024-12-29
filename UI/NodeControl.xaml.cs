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

        public static Color RedColor => Color.FromArgb(255, 225, 60, 60);

        /// <summary>
        /// Highlight the node in the UI
        /// </summary>
        public void Activate()
        {
            BeginStoryboard((Storyboard)FindResource("AnimActivate"));
        }

        /// <summary>
        /// Remove highlight of the UI node
        /// </summary>
        public void Deactivate()
        {
            BeginStoryboard((Storyboard)FindResource("AnimDeactivate"));
        }

        /// <summary>
        /// Briefly blink the node's borders in blue.
        /// </summary>
        public void Blink()
        {
            BeginStoryboard((Storyboard)FindResource("AnimBlink"));
        }

        /// <summary>
        /// Highlight the node in blue
        /// </summary>
        public void HighlightBlue()
        {
            BeginStoryboard((Storyboard)FindResource("AnimHighlightBlue"));
        }

        /// <summary>
        /// Remove the blue highlight from the node
        /// </summary>
        public void DeactivateBlue()
        {
            BeginStoryboard((Storyboard)FindResource("AnimDeactivateBlue"));
        }

        /// <summary>
        /// Highlight the node in blue in the UI
        /// </summary>
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

        /// <summary>
        /// Node's value
        /// </summary>
        public object Value
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ValToStr));
            } 
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Node's value as string. To be bound to the UI content.
        /// </summary>
        public string ValToStr => Value.ToString() ?? "null";

        /// <summary>
        /// Click node to quickly select it in the parent UI. <para/>
        /// Relies on the established structure of Main Window -> MainFrame -> BinTreeMan -> InputTextBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Node_OnClick(object sender, MouseButtonEventArgs e)
        {
            var binTreeMan = ((Frame)Window.GetWindow(this).FindName("MainFrame")).Content as BinTreeMan;
            if (binTreeMan is null)
                return;
            // var inputTextBox = (TextBox)Window.GetWindow(this).FindName("InputTextBox");
            var inputTextBox = binTreeMan.InputTextBox;
            if (inputTextBox is null)
                return;
            inputTextBox.Text = ValToStr;
            this.Activate();
            this.Deactivate();
        }
    }
}
