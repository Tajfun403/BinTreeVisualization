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
    /// <summary>
    /// The associated binary tree whose data is being displayed.
    /// </summary>
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

    /// <summary>
    /// Backing bool for whether or not to perform AVL rotations.
    /// </summary>
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

    /// <summary>
    /// Click event - perform an insert operation.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    async void OnInsert(object sender, RoutedEventArgs e)
    {
        if (GetDoubleArg(out double arg))
            await BinTree.Insert(arg);

        lastOperation = OnInsert;
    }

    /// <summary>
    /// Click event - on get min.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnGetMin(object sender, RoutedEventArgs e)
    {
        BinTree.GetMin();
        lastOperation = OnGetMin;
    }

    /// <summary>
    /// Click event - on get max.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnGetMax(object sender, RoutedEventArgs e)
    {
        BinTree.GetMax();
        lastOperation = OnGetMax;
    }

    /// <summary>
    /// Try to get the current <see cref="InputTextBox"/>'s value as a double.
    /// </summary>
    /// <param name="arg">Out arg: the parsed value</param>
    /// <returns><see langword="true"/> if parse succeeded, <see langword="false"/> otherwise.</returns>
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

    /// <summary>
    /// Click event - on find.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnFind(object sender, RoutedEventArgs e)
    {
        if (GetDoubleArg(out double arg))
            BinTree.Find(arg);

        lastOperation = OnFind;
    }

    /// <summary>
    /// Click event - on delete
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnDelete(object sender, RoutedEventArgs e)
    {
        if (GetDoubleArg(out double arg))
            BinTree.Delete(arg);
        lastOperation = OnDelete;
    }

    /// <summary>
    /// Backing field for the count of items to add.
    /// </summary>
    public int AddItemsCount { get; set
        {
            field = value;
            // AddItemsCountText = $"Insert {value} items";
            OnPropertyChanged();
            OnPropertyChanged(nameof(AddItemsCountText));
        }
    } = 20;

    /// <summary>
    /// UI text for the "Insert X items" button.
    /// </summary>
    public string AddItemsCountText => $"Insert {AddItemsCount} items";

    /// <summary>
    /// List of possible values for <see cref="AddItemsCount"/>.
    /// </summary>
    List<int> AddItemsCounts = [10, 20, 50, 100, 200];

    /// <summary>
    /// Switch the value of <see cref="AddItemsCount"/> to the next or previous value.
    /// </summary>
    /// <param name="bToDown">Whether to switch the value down</param>
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

    /// <summary>
    /// Click event - on add random items.
    /// </summary>
    /// <param name="count">The amount of random items to add</param>
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

    /// <summary>
    /// On key pressed inside the <see cref="InputTextBox"/>.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnKeyDownTextbox(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            lastOperation?.Invoke(sender, new());
        }
    }

    /// <summary>
    /// On key pressed inside the entire main window.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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

    /// <summary>
    /// Click event - on insert many items.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnInsertManyItems(object sender, RoutedEventArgs e)
    {
        AddRandomItems(AddItemsCount);
    }

    /// <summary>
    /// Click event - on clear.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnOpenStatsWindow(object sender, RoutedEventArgs e)
    {
        BinTree.Stats.ShowWindow();
    }
}

