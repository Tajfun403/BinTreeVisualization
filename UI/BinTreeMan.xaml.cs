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

    private async void Insert(double value)
    {
        await BinTree.Insert(value);
    }

    async void OnInsert(object sender, RoutedEventArgs e)
    {
        if (GetDoubleArg(out double arg))
            await BinTree.Insert(arg);

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

    public int AddItemsCount { get; set
        {
            field = value;
            AddItemsCountText = $"Insert {value} items";
            OnPropertyChanged();
        }
    } = 20;

    public string AddItemsCountText
    {
        get => $"Insert {AddItemsCount} items";
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    List<int> AddItemsCounts = [10, 20, 50, 100, 200];

    void SwitchAddItemsCount(bool bToDown)
    {
        int index = AddItemsCounts.IndexOf(AddItemsCount);
        if (bToDown)
        {
            if (index == 0)
                return;
            AddItemsCount = AddItemsCounts[index - 1];
        }
        else
        {
            if (index == AddItemsCounts.Count - 1)
                return;
            AddItemsCount = AddItemsCounts[index + 1];
        }
    }

    async void AddRandomItems(int count)
    {
        Stopwatch watch = new();
        watch.Start();
        Random rand = new();
        for (int i = 0; i < count; i++)
        {
            BinTree.Insert(Double.Round(rand.NextDouble() * 100, 2));
        }
        await BinTree.FinishCurrOperation();
        Debug.WriteLine($"Inserting {count} items took {watch.Elapsed} s");
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
        else if (e.Key == Key.F2)
        {
            SwitchAddItemsCount(true);
        }
        else if (e.Key == Key.F3)
        {
            SwitchAddItemsCount(false);
        }
        else if (e.Key == Key.Down)
        {
            SwitchAddItemsCount(true);
        }
        else if (e.Key == Key.Up)
        {
            SwitchAddItemsCount(false);
        }
    }

    private void OnInsertManyItems(object sender, RoutedEventArgs e)
    {
        AddRandomItems(AddItemsCount);
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

