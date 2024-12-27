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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BinTreeVisualization.Algorithms;

namespace BinTreeVisualization.UI;

/// <summary>
/// Page which holds both a BinTreeControl and other controls that allow the user to interact with it.
/// </summary>
public partial class BinTreeMan : Page, INotifyPropertyChanged
{
    public BinTree<double> BinTree { get; set; }

    /// <summary>
    /// The argument of the operation to be performed.
    /// </summary>
    public string OperationArgument
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool PerformRotations
    {
        get;
        set
        {
            field = value;
            BinTree.PerformRotations = value;
            OnPropertyChanged();
        }
    } = true;

    public BinTreeMan()
    {
        InitializeComponent();
        BinTree = new()
        {
            BackingControl = this.TreeNode
        };
        DataContext = this;
        lastOperation = OnInsert;
    }

    void OnInsert(object sender, RoutedEventArgs e)
    {
        if (GetDoubleArg(out double arg))
            BinTree.Insert(arg);

        lastOperation = OnInsert;
    }

    void OnGetMin(object sender, RoutedEventArgs e)
    {
        BinTree.GetMin();
        lastOperation = OnGetMin;
    }

    void OnGetMax(object sender, RoutedEventArgs e)
    {
        BinTree.GetMax();
        lastOperation = OnGetMax;
    }

    bool GetDoubleArg(out double arg)
    {
        if (string.IsNullOrEmpty(OperationArgument))
        {
            arg = 0;
            return false;
        }
        if (double.TryParse(OperationArgument, out arg))
            return true;
        Debug.WriteLine($"Invalid input of {OperationArgument}");
        return false;
    }

    void OnFind(object sender, RoutedEventArgs e)
    {
        if (GetDoubleArg(out double arg))
            BinTree.Find(arg);

        lastOperation = OnFind;
    }

    void OnDelete(object sender, RoutedEventArgs e)
    {
        if (GetDoubleArg(out double arg))
            BinTree.Delete(arg);
        lastOperation = OnDelete;
    }

    void AddRandomItems(int count)
    {
        Random rand = new();
        for (int i = 0; i < count; i++)
        {
            BinTree.Insert(rand.NextDouble() * 100);
        }
        BinTree.FinishCurrOperation();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    Action<object, RoutedEventArgs> lastOperation;

    private void OnKeyDownTextbox(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            lastOperation?.Invoke(sender, new());
        }
    }

    private void OnKeyDownBackground(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            BinTree.FinishCurrOperation();
        }
        else if (e.Key == Key.F1)
        {
            BinTree.BreakInto();
        }
    }

    private void OnInsertManyItems(object sender, RoutedEventArgs e)
    {
        AddRandomItems(100);
    }

    private void OnOpenStatsWindow(object sender, RoutedEventArgs e)
    {
        BinTree.Stats.ShowWindow();
    }

    private void CheckBox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        BinTree.PerformRotations = e.NewValue as bool? ?? false;
    }
}

