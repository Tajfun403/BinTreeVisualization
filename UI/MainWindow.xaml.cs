using BinTreeVisualization.UI;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BinTreeVisualization;

/// <summary>
/// Main window which holds the <see cref="BinTreeMan"/> page.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new UI.BinTreeMan();
        TitleBarHelper.EnableDarkMode(this);
    }
}