using BinTreeVisualization.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BinTreeVisualization.Stats
{
    /// <summary>
    /// Interaction logic for StatsWindow.xaml
    /// </summary>
    public partial class StatsWindow : Window
    {
        public StatsWindow()
        {
            InitializeComponent();
            TitleBarHelper.EnableDarkMode(this);
        }

        void OnDataChanged()
        {
            statsRef.Refresh(this);
        }

        public StatsWindow(TreeStats stats) : this()
        {
            statsRef = stats;
            DataContext = this;

            statsRef.OnDataChanged += OnDataChanged;
            Closed += OnClosed;
        }

        private void OnClosed(object sender, EventArgs e)
        {
            statsRef.OnDataChanged -= OnDataChanged;
        }

        public TreeStats statsRef;
    }
}
