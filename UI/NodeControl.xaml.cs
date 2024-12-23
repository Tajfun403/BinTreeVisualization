using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for NodeControl.xaml
    /// </summary>
    public partial class NodeControl : UserControl, INotifyPropertyChanged
    {
        public NodeControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public NodeControl(object value) : this()
        {
            Value = value;
        }

        public static Color InactiveColor => Color.FromArgb(0, 0, 0, 0);
        public static Color ActiveColor => Color.FromArgb(255, 180, 120, 225);

        public static Color StrokeBase => Color.FromArgb(255, 238, 238, 238);

        public static Color StrokeBlue => Color.FromArgb(255, 67, 149, 212);

        /*        public static SolidColorBrush InactiveColor => new(Color.FromArgb(0, 0, 0, 0));
                public static SolidColorBrush ActiveColor => new(Color.FromArgb(255, 180, 120, 225));*/

        // public event 

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
