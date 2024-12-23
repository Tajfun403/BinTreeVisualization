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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BinTreeVisualization.Algorithms;

namespace BinTreeVisualization.UI;

/// <summary>
/// Interaction logic for BinTreeMan.xaml
/// </summary>
public partial class BinTreeMan : Page, INotifyPropertyChanged
{
    public BinTree<double> BinTree { get; set; }

    public string OperationArgument
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public BinTreeMan()
    {
        InitializeComponent();
        BinTree = new()
        {
            BackingControl = this.TreeNode
        };
        DataContext = this;
    }

    void OnInsert(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(OperationArgument))
            return;
        BinTree.Insert(double.Parse(OperationArgument));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

