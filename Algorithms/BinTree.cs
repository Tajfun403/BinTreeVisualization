using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using BinTreeVisualization.UI;

namespace BinTreeVisualization.Algorithms;


public class BinTree<T> : INotifyPropertyChanged where T : IComparable<T>
{
    public Node<T> Root { get; set; }
    public BinTreeControl BackingControl { get; init; }
    public Canvas GetCanvas() => BackingControl.MainCanvas;

    public int Height => GetHeight(Root);
    public int LastHeight { get; private set; }

    private int GetHeight(Node<T> root)
    {
        if (root is null)
            return 0;
        return 1 + Math.Max(GetHeight(root.Left), GetHeight(root.Right));
    }

    public BinTree()
    {
    }

    private void CreateRoot(T value)
    {
        Root = Node<T>.CreateRoot(value, this);
    }

    public bool Find(T value)
    {
        return Find(value, Root);
    }

    public bool Find(T value, Node<T> currNode)
    {
        if (currNode is null)
            return false;
        if (currNode.Value.CompareTo(value) == 0)
            return true;
        if (value.CompareTo(currNode.Value) < 0)
            return Find(value, currNode.Left);
        else
            return Find(value, currNode.Right);
    }

    public async Task<T> GetMin()
    {
        async Task Blink(Node<T> n)
        {
            n.Activate();
            await Delay(500);
            n.Deactivate();
        }

        var it = Root;
        await Blink(it);
        SetText($"Traversing to the leftmost node");
        while (it.Left != null)
        {
            it = it.Left;
            await Blink(it);
        }
        it.Activate();
        SetText($"Found min == {it.Value}", TextAction.Violet);
        await Delay(2000);
        await ResetText();
        it.Deactivate();
        return it.Value;
    }

    public async Task<T> GetMax()
    {
        async Task Blink(Node<T> n)
        {
            n.Activate();
            await Delay(500);
            n.Deactivate();
        }


        var it = Root;
        await Blink(it);
        SetText($"Traversing to the rightmost node");
        while (it.Right != null)
        {
            it = it.Right;
            await Blink(it);
        }
        it.Activate();
        SetText($"Found max == {it.Value}", TextAction.Violet);
        await Delay(2000);
        await ResetText();
        it.Deactivate();
        return it.Value;
    }

    public async void Insert(T value)
    {
        await OperationGuard();
        if (Root == null)
        {
            CreateRoot(value);
            IsOperationInProgress = false;
        }
        else
        {
            await Insert(value, Root);
            LayoutTree();
            var currHeight = Height;
            if (LastHeight != Height)
            {
                LastHeight = Height;
                LayoutTree();
            }
            LastHeight = currHeight;
            IsOperationInProgress = false;
            await Delay(2000);
            await ResetText();
        }
    }

    private bool bSkipAnimations = false;
    private bool IsOperationInProgress
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Delays the execution of the current task if in not-instant context.
    /// </summary>
    /// <param name="ms"></param>
    /// <returns></returns>
    private async Task Delay(int ms)
    {
        if (bSkipAnimations)
            return;
        await Task.Delay(ms);
    }

    /// <summary>
    /// Ensures only one operation is being performed at a time
    /// </summary>
    /// <returns></returns>
    private async Task OperationGuard()
    {
        if (IsOperationInProgress)
        {
            bSkipAnimations = true;

            var tcs = new TaskCompletionSource<bool>();

            PropertyChangedEventHandler handler = null;
            handler = (sender, args) =>
            {
                if (args.PropertyName == nameof(IsOperationInProgress))
                {
                    tcs.SetResult(true);
                    PropertyChanged -= handler;
                }
            };

            PropertyChanged += handler;
            await tcs.Task;
            bSkipAnimations = false;
        }
        bSkipAnimations = false;
        IsOperationInProgress = true;
    }

    public async Task<Node<T>> Insert(T value, Node<T> currNode)
    {
        await ResetText();
        currNode.Activate();
        SetText($"Comparing {value} to {currNode.Value}");
        await Delay(500);

        bool bGoLeft = value.CompareTo(currNode.Value) < 0;
        string s = bGoLeft ? $"{value} is smaller; inserting into left subtree" :
                     $"{value} is larger; inserting into right subtree";
        SetText(s);
        await Delay(1000);

        if (bGoLeft && currNode.Left is null)
        {
            SetText($"Empty leaf; inserting into it");
            var child = currNode.SpawnChild(value, true);
            currNode.Deactivate();
            await Delay(1000);
            child.Deactivate();
            return currNode.Left!;
        }
        else if (!bGoLeft && currNode.Right is null)
        {
            SetText($"Empty leaf; inserting into it");
            var child = currNode.SpawnChild(value, false);
            currNode.Deactivate();
            await Delay(1000);
            child.Deactivate();
            return currNode.Right!;
        }

        currNode.Deactivate();
        SetText($"Non-empty leaf; going deeper", TextAction.Blink);
        var nextNode = bGoLeft ? currNode.Left : currNode.Right;
        nextNode.BlinkSubtree();
        await Delay(1000);
        var ret = await Insert(value, bGoLeft ? currNode.Left : currNode.Right);


        // return ret;

        // SetText($"Inserted into subtree, rebalacing");

        // currNode.Activate();
        if (bGoLeft)
        {
            if (currNode.GetNodeBalance() > 1)
            {
                SetText($"Left subtree is too high; rotating right");
                currNode = RotateRight(currNode);
                await Delay(1000);
            }
        }
        else
        {
            if (currNode.GetNodeBalance() < -1)
            {
                SetText($"Right subtree is too high; rotating left");
                currNode = RotateLeft(currNode);
                await Delay(1000);
            }
        }

        return ret;
    }

    /// <summary>
    /// Left-heavy subtree. Rotate middle nodes to the right.
    /// </summary>
    /// <param name="currNode"></param>
    /// <returns></returns>
    private Node<T> RotateRight(Node<T> currNode)
    {
        // left-heavy tree
        var parent = currNode.Parent;

        Node<T> topChild = currNode;
        Node<T> middleChild = null;
        Node<T> bottomChild = null;

        // 1 -> 2 -> 3, forming all left leaves
        if (currNode.Left.Left is not null)
        {
            middleChild = currNode.Left;
            bottomChild = currNode.Left.Left;
        }
        // 1 -> 2 -> 2.5, forming left-right leaves
        else if (currNode.Left.Right is not null)
        {
            bottomChild = currNode.Left.Right;
            middleChild = currNode.Left;
        }
        else
            Debug.Assert(false, "Invalid tree state!");

        topChild.OrphanChildren();
        middleChild.AdoptChild(topChild);
        parent?.OrphanChildren(true, false);
        parent?.AdoptChild(middleChild);
        // middleChild.AdoptChild(bottomChild);

        if (topChild == Root)
        {
            Root = middleChild;
            Debug.WriteLine("Moving root");
            Root.MoveTreeToLoc(new(0, 0));
        }

        return topChild;
    }

    private Node<T> RotateLeft(Node<T> currNode)
    {
        throw new NotImplementedException();
    }

    private void SetText(string text, TextAction act = TextAction.Base)
    {
        BackingControl.SetText(text, act);
    }

    private async Task ResetText()
    {
        await BackingControl.ResetText();
    }

    private void LayoutTree()
    {
        // BackingControl.LayoutTree(this);
        // BackingControl.LayoutTreeNSquare(this);
    }

    IEnumerable<Node<T>> Traverse()
    {
        return Root?.Traverse() ?? [];
    }

    public List<BinTreeRow<T>> GetRows()
    {
        var nodes = Traverse().ToList();
        var grouped = nodes.GroupBy(x => x.GetDepth());
        var rows = grouped.Select(x => new BinTreeRow<T>(x.Key, x.ToList())).ToList();
        return rows;
    }

    public List<BinTreeRow<T>> Rows { get; private set; }

    private void RefreshRowsCache() => Rows = GetRows();

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public List<Node<T>> GetRow(int Depth)
    {
        List<Node<T>> result = new();

        var queue = new Queue<(Node<T>, int)>();
        queue.Enqueue((Root, 0));

        while (queue.Count > 0)
        {
            var (node, currHeight) = queue.Dequeue();

            if (currHeight == Depth)
            {
                result.Add(node);
                continue;
            }

            if (currHeight > Depth) break;

            queue.Enqueue((node?.Left, currHeight + 1));
            queue.Enqueue((node?.Right, currHeight + 1));
        }

        return result;
    }

}

public enum TextAction
{
    Base,
    Blink,
    Violet
}

public static class TextActionColors
{
    public static System.Windows.Media.Color GetColor(this TextAction textAction)
    {
        return textAction switch
        {
            TextAction.Base => NodeControl.StrokeBase,
            TextAction.Blink => NodeControl.StrokeBlue,
            TextAction.Violet => NodeControl.ActiveColor,
            _ => throw new NotImplementedException()
        };
    }
}

public record struct BinTreeRow<T>(int tier, List<Node<T>> nodes) where T : IComparable<T>;
