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

        /// <summary>
        /// Function to call when the tree's data changes.
        /// </summary>
        void OnDataChanged()
        {
            StatsRef.Refresh(this);
        }

        public StatsWindow(TreeStats stats) : this()
        {
            StatsRef = stats;
            DataContext = this;

            StatsRef.OnDataChanged += OnDataChanged;
            Closed += OnClosed;
        }

        /// <summary>
        /// Function to call on window's closing. Unsubcribes from the tree's data changed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClosed(object sender, EventArgs e)
        {
            StatsRef.OnDataChanged -= OnDataChanged;
        }

        /// <summary>
        /// Ref to the tree's data.
        /// </summary>
        public TreeStats StatsRef { get; set; }
    }
}
