using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Xps;
using BinTreeVisualization.UI;

namespace BinTreeVisualization.Algorithms;

/// <summary>
/// A binary search tree, associated with a WPF backing control that nodes are being represented on.<para/>
/// Can function as a basic BST and an AVL tree.
/// </summary>
/// <typeparam name="T">Node type</typeparam>
public class BinTree<T> : INotifyPropertyChanged where T : IComparable<T>
{
    /// <summary>
    /// Tree's root node.
    /// </summary>
    public Node<T> Root { get; set; }

    /// <summary>
    /// The UI control associated with the tree which holds all the visual nodes, arrows, and progress text.
    /// </summary>
    public BinTreeControl BackingControl { get; init; }

    /// <summary>
    /// Ref to the backing control's canvas.
    /// </summary>
    /// <returns></returns>
    public Canvas GetCanvas() => BackingControl.MainCanvas;

    public int Height => GetHeight();
    public int LastHeight { get; private set; }

    private int GetHeight() => Root?.GetHeight() ?? 0;

    public BinTree()
    {
    }

    /// <summary>
    /// Create tree's root with the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value"></param>
    private void CreateRoot(T value)
    {
        Root = Node<T>.CreateRoot(value, this);
    }

    /// <summary>
    /// Find <paramref name="value"/> in the tree.
    /// </summary>
    /// <param name="value">The value to find</param>
    /// <returns></returns>
    public async Task<Node<T>> Find(T value)
    {
        return await Find(value, Root);
    }

    /// <summary>
    /// Recursively search the tree to find the specified <paramref name="value"/>. Search starts from <paramref name="currNode"/>.
    /// </summary>
    /// <param name="value">The value to find.</param>
    /// <param name="currNode">Subtree to search for the node in currently.</param>
    /// <returns></returns>
    public async Task<Node<T>> Find(T value, Node<T> currNode)
    {
        await ResetText();
        currNode.Activate();
        SetText($"Comparing {value} to {currNode.Value}");
        await Delay(500);

        if (currNode.Value.CompareTo(value) == 0)
        {
            SetText($"Found {value}", TextAction.Violet);
            await Delay(2000);
            await ResetText();
            currNode.Deactivate();
            return currNode;
        }

        bool bGoLeft = value < currNode;
        string s = bGoLeft ? $"{value} is smaller; searching into left subtree" :
                     $"{value} is larger; searching into right subtree";
        SetText(s);
        await Delay(1000);

        currNode.Deactivate();
        if ((bGoLeft && currNode.Left is null) || (!bGoLeft && currNode.Right is null))
        {
            SetText($"Couldn't find {value}", TextAction.Red);
            await Delay(2000);
            await ResetText();
            return null!;
        }

        currNode.Deactivate();
        SetText($"Non-empty leaf; going deeper", TextAction.Blink);
        var nextNode = bGoLeft ? currNode.Left : currNode.Right;
        nextNode.Blink(true);
        await Delay(1000);
        return await Find(value, bGoLeft ? currNode.Left : currNode.Right);
    }

    /// <summary>
    /// Get minimum value in the tree.
    /// </summary>
    /// <returns></returns>
    public async Task<T> GetMin() => (await GetMin(Root)).Value;
    /// <summary>
    /// Get maximum value in the tree.
    /// </summary>
    /// <returns></returns>
    public async Task<T> GetMax() => (await GetMax(Root)).Value;

    /// <summary>
    /// Blink a node for 500ms and return once the blink starts to fade.
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    private async Task Blink(Node<T> n)
    {
        n.Activate();
        await Delay(500);
        n.Deactivate();
    }

    /// <summary>
    /// Get minimum in the subtree starting by the specified node.
    /// </summary>
    /// <param name="treeBase"></param>
    /// <returns></returns>
    public async Task<Node<T>> GetMin(Node<T> treeBase)
    {
        var it = treeBase;
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
        return it;
    }

    /// <summary>
    /// Get max in the subtree starting by the specified node.
    /// </summary>
    /// <param name="treeBase"></param>
    /// <returns></returns>
    public async Task<Node<T>> GetMax(Node<T> treeBase)
    {
        var it = treeBase;
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
        return it;
    }

    /// <summary>
    /// Delete node containing the specified value from the tree.
    /// </summary>
    /// <param name="value"></param>
    public async void Delete(T value)
    {
        await OperationGuard();
        Debug.WriteLine($"Deleting {value}");
        if (Root is null)
        {
            Debug.WriteLine("Tree is empty");
            FinishOperation();
            return;
        }
        var victim = await Find(value);
        if (victim != null)
            await Delete(victim);
        FinishOperation();
    }

    /// <summary>
    /// Swap two nodes in the tree.
    /// </summary>
    /// <param name="one"></param>
    /// <param name="other"></param>
    private async Task SwapNodes(Node<T> one, Node<T> other)
    {
        SetText("Swapping nodes:");
        SetText($"Node 1: {one.Value}", TextAction.Violet);
        SetText($"Node 2: {other.Value}", TextAction.Blink);
        one.Activate();
        other.ActivateBlue(true);
        await Delay(1000);

        var oneParent = one.Parent;
        var otherParent = other.Parent;

        List<Node<T>> oneChildren = [one.Left, one.Right];
        List<Node<T>> otherChildren = [other.Left, other.Right];

        one.OrphanChildren();
        other.OrphanChildren();

        one.DetachFromParent();
        other.DetachFromParent();

        oneChildren.ForEach(x => other.AdoptChild(x));
        otherChildren.ForEach(x => one.AdoptChild(x));

        oneParent?.AdoptChild(other);
        if (otherParent == one)
            other.AdoptChild(one);
        else
            otherParent?.AdoptChild(one);

        if (oneParent == null)
        {
            Root = other;
            Root.MoveTreeToLoc(new(0, 0));
        }
        else if (otherParent == null)
        {
            Root = one;
            Root.MoveTreeToLoc(new(0, 0));
        }

        await Delay(1000);
        one.Deactivate();
        other.DeactivateBlue(true);
        await ResetText();
    }

    /// <summary>
    /// Delete specified node from the tree.<para/>
    /// You can find the node first with <see cref="Find(T)"/>
    /// </summary>
    /// <param name="currNode">Node to delete</param>
    /// <returns></returns>
    public async Task Delete(Node<T> victim)
    {
        if (victim.Left is null && victim.Right is null)
        {
            SetText($"Deleting leaf {victim.Value}");

            if (victim == Root)
                Root = null!;

            var victimsParentSquare = victim.Parent?.Parent;
            var victimParent = victim.Parent;

            await Delay(1000);
            victim.Deactivate();
            victim.DetachFromParent();
            GetCanvas().Children.Remove(victim.BackingControl);

            await ResetText();
            if (victimParent != null)
                await BalanceTreeIfNeeded(victimParent);
            if (victimsParentSquare != null)
                await BalanceTreeIfNeeded(victimsParentSquare);

            return;
        }

        Node<T> successor = null;
        if (victim.Right != null)
            successor = await GetMin(victim.Right);
        successor ??= victim.Left!;

        await SwapNodes(victim, successor);

        await Delete(victim);

        return;
    }

    /// <summary>
    /// Triggers debug break inside the tree.
    /// </summary>
    public void BreakInto()
    {
#if DEBUG
        Debugger.Break();
#endif
    }

    /// <summary>
    /// Insert <paramref name="value"/> into the tree.
    /// </summary>
    /// <param name="value">The value to insert.</param>
    public async void Insert(T value)
    {
        await OperationGuard();
        Debug.WriteLine($"Inserting {value}");
        if (Root == null)
        {
            CreateRoot(value);
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
            await Delay(2000);
            await ResetText();
        }
        FinishOperation();
    }

    private bool bSkipAnimations { get; set; }

    /// <summary>
    /// Delays the execution of the current task if in not-instant context.<para/>
    /// Otherwise, returns immediately.
    /// </summary>
    /// <param name="ms">Delay in miliseconds</param>
    /// <returns></returns>
    private async Task Delay(int ms)
    {
        if (bSkipAnimations)
            return;
        await Task.Delay(ms);
    }

    private SemaphoreSlim OperationSem { get; init; } = new(1, 1);
    private int CurrWaiting { get; set; } = 0;
    private int CurrInside { get; set; } = 0;

    /// <summary>
    /// Instantly finish current operation, if any is active.
    /// </summary>
    public async void FinishCurrOperation()
    {
        await OperationGuard();
        FinishOperation();
    }

    /// <summary>
    /// Ensures only one operation is being performed at a time.<para/>
    /// If one is already being performed when this func is called, try to skip all of its delays and wait for it to finish.
    /// </summary>
    /// <returns></returns>
    private async Task OperationGuard()
    {
        if (!OperationSem.Wait(0))
        {
            Debug.WriteLine("Operation in progress, waiting");
            bSkipAnimations = true;
            CurrWaiting++;
            Debug.WriteLine($"Curr waiting {CurrWaiting}");
            await OperationSem.WaitAsync();
            CurrWaiting--;
            Debug.WriteLine($"Curr waiting {CurrWaiting}");
            Debug.WriteLine("Entering operation");
        }
        if (CurrWaiting == 0)
            bSkipAnimations = false;
        CurrInside++;
        Debug.Assert(CurrInside == 1, "More than one task performing actions!");
    }

    /// <summary>
    /// Signal that an operation is finished and another one can be started.
    /// </summary>
    private void FinishOperation()
    {
        OperationSem.Release();
        CurrInside--;
        Debug.Assert(CurrInside == 0, "More than one task performing actions!");
        Debug.Assert(OperationSem.CurrentCount <= 1);
        AssertTreeIsValid();
    }

    /// <summary>
    /// Internal insertion. Try to insert the <paramref name="value"/> into the subtree starting with <paramref name="currNode"/>.
    /// </summary>
    /// <param name="value">The value to insert.</param>
    /// <param name="currNode">Subtree which starts by this node.</param>
    /// <returns>Passed-in subtree with inserted <paramref name="value"/></returns>
    public async Task<Node<T>> Insert(T value, Node<T> currNode)
    {
        await ResetText();
        currNode.Activate();
        SetText($"Comparing {value} to {currNode.Value}");
        await Delay(500);

        bool bGoLeft = value < currNode;
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
        nextNode.Blink(true);
        await Delay(1000);
        var ret = await Insert(value, bGoLeft ? currNode.Left : currNode.Right);


        // return ret;

        await ResetText();
        // SetText($"Inserted into subtree, rebalacing");

        await BalanceTreeIfNeeded(currNode);

        return ret;
    }

    /// <summary>
    /// Check if subtree starting with the passed node is not skewed to a side, 
    /// and performs a rotation if it is
    /// </summary>
    /// <param name="currNode">Subtree which starts by this node</param>
    /// <returns></returns>
    private async Task BalanceTreeIfNeeded(Node<T> currNode)
    {
        var nodeBalance = currNode.GetNodeBalance();
        SetText($"Subtree balance: {nodeBalance}");
        await Delay(1000);

        if (nodeBalance > 1)
        {
            SetText($"Left subtree is too high; rotating right");
            currNode = await RotateRight(currNode);
            await Delay(1000);
        }

        else if (nodeBalance < -1)
        {
            SetText($"Right subtree is too high; rotating left");
            currNode = await RotateLeft(currNode);
            await Delay(1000);
        }

    }

    /// <summary>
    /// Perform adoption of a node by a parent with a 3-second long animation.
    /// </summary>
    /// <param name="parent">The new parent</param>
    /// <param name="child">The child to be adopted</param>
    /// <returns></returns>
    private async Task AnimAdoption(Node<T> parent, Node<T> child)
    {
        SetText($"{parent.Value} adopts {child.Value}");
        SetText($"Parent is {parent.Value}", TextAction.Violet);
        SetText($"Child is {child.Value}", TextAction.Blink);
        parent.Activate();
        child.ActivateBlue(true);
        await Delay(1500);
        parent.AdoptChild(child);
        await Delay(1500);
        parent.Deactivate();
        child.DeactivateBlue(true);
        await ResetText();
    }

    /// <summary>
    /// Side-heavy subtree. Rotate to the specified direction.<para/>
    /// The subtree should be rotated left if it's right-heavy (<paramref name="bLeftRotation"/> = <see langword="true"/>), and right in the opposite case.
    /// </summary>
    /// <param name="currNode">Node which the unbalanced subtree starts with</param>
    /// <param name="bLeftRotation"><see langword="true"/> if the subtree is right-heavy and should be rotated left. <para/>
    /// <see langword="false"/> if the subtree is left-heavy and should be rotated right</param>
    /// <returns>New top child who the subtree starts with</returns>
    private async Task<Node<T>> Rotate(Node<T> currNode, bool bLeftRotation)
    {
        var parent = currNode.Parent;

        Node<T> topChild = currNode;
        Node<T> middleChild = null;
        Node<T> bottomChild = null;
        bool bMiddleChildSwapped = false;

        // pick the top, middle, and bottom children from the chain
        // they are picked according to their "sorted" order and not necessarily their position in the tree
        // (bMiddleChildSwapped = true if the latter is not true)
        if (!bLeftRotation)
        {
            // right-heavy tree
            // 1 -> 2 -> 3, forming all left leaves
            if (currNode.Left.Left is not null)
            {
                middleChild = currNode.Left;
                bottomChild = currNode.Left.Left;
            }
            // 1 -> 2 -> 2.5, forming left-right leaves
            else if (currNode.Left.Right is not null)
            {
                middleChild = currNode.Left.Right;
                bottomChild = currNode.Left;
                bMiddleChildSwapped = true;
            }
            else
                Debug.Fail("Invalid tree passed for a rotation!");
        }
        else
        {
            // left-heavy tree
            // 3 -> 2 -> 1, forming all right leaves
            if (currNode.Right.Right is not null)
            {
                middleChild = currNode.Right;
                bottomChild = currNode.Right.Right;
            }
            // 3 -> 2 -> 2.5, forming right-left leaves
            else if (currNode.Right.Left is not null)
            {
                middleChild = currNode.Right.Left;
                bottomChild = currNode.Right;
                bMiddleChildSwapped = true;
            }
            else
                Debug.Fail("Invalid tree passed for a rotation!");
        }

        topChild.DetachFromParent();
        middleChild.DetachFromParent();

        // parent?.OrphanChildren(true, false);
        // topChild.OrphanChildren(true, false);

        SetText($"Rotating right around pivot {middleChild.Value}", TextAction.Violet);

        await AnimAdoption(middleChild, topChild);
        // middleChild.AdoptChild(topChild);
        if (parent != null)
            await AnimAdoption(parent, middleChild);
        // parent?.AdoptChild(middleChild);

        if (bMiddleChildSwapped)
        {
            bottomChild.DetachFromParent();
            // bottomChild.OrphanChildren(false, true);
            await AnimAdoption(middleChild, bottomChild);
            // middleChild.AdoptChild(bottomChild);
        }

        middleChild.Parent = parent;
        // middleChild.AdoptChild(bottomChild);

        if (topChild == Root)
        {
            Root = middleChild;
            Debug.WriteLine("Moving root");
            Root.MoveTreeToLoc(new(0, 0));
        }

        return middleChild;
    }

    /// <summary>
    /// Left-heavy subtree. Rotate middle nodes to the right.
    /// </summary>
    /// <param name="currNode"></param>
    /// <returns>New top child who the subtree starts with</returns>
    private async Task<Node<T>> RotateRight(Node<T> currNode)
    {
        return await Rotate(currNode, false);
    }

    /// <summary>
    /// Right-heavy subtree. Rotate middle nodes to the right.
    /// </summary>
    /// <param name="currNode"></param>
    /// <returns>New top child who the subtree starts with</returns>
    private async Task<Node<T>> RotateLeft(Node<T> currNode)
    {
        return await Rotate(currNode, true);
    }

    /// <inheritdoc cref="BinTreeControl.SetText(string, TextAction)"/>
    private void SetText(string text, TextAction act = TextAction.Base)
    {
        BackingControl.SetText(text, act);
    }

    /// <inheritdoc cref="BinTreeControl.ResetText(bool)"/>
    private async Task ResetText()
    {
        await BackingControl.ResetText(!bSkipAnimations);
    }

    private void LayoutTree()
    {
        // BackingControl.LayoutTree(this);
        // BackingControl.LayoutTreeNSquare(this);
    }

    /// <inheritdoc cref="Node{T}.Traverse"/>
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

    /// <summary>
    /// Assert <see cref="AssertNoOrphans"/> and <see cref="AssertIsBinaryTree"/>
    /// </summary>
    public void AssertTreeIsValid()
    {
        AssertNoOrphans();
        AssertIsBinaryTree();
    }

    /// <summary>
    /// Assert that there are no orphaned nodes.
    /// </summary>
    public void AssertNoOrphans()
    {
        var nodes = Traverse().Select(x => x.BackingControl).ToList();
        var UINodes = GetCanvas().Children.OfType<NodeControl>().ToList();
        HashSet<NodeControl> trackedNodes = new(nodes);
        Debug.Assert(!UINodes.Any(x => !trackedNodes.Contains(x)), "Some nodes are not represented in the UI");
    }

    /// <summary>
    /// Assert that the core property of the binary tree (lesser children on the left, bigger children on the right) 
    /// is satisfied for every node in the tree.
    /// </summary>
    public void AssertIsBinaryTree()
    {
        foreach (var node in Traverse())
        {
            if (node.Left != null)
                Debug.Assert(node.Left <= node, "Left child is larger than parent");
            if (node.Right != null)
                Debug.Assert(node.Right >= node, "Right child is smaller than parent");
        }
    }
}

/// <summary>
/// Colors supported by the UI.
/// </summary>
public enum TextAction
{
    Base,
    Blink,
    Violet,
    Red
}

public static class TextActionColors
{
    /// <summary>
    /// Convert value from a color enum to a <see cref="System.Windows.Media.Color"/> color.
    /// </summary>
    /// <param name="textAction"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static System.Windows.Media.Color GetColor(this TextAction textAction)
    {
        return textAction switch
        {
            TextAction.Base => NodeControl.StrokeBase,
            TextAction.Blink => NodeControl.StrokeBlue,
            TextAction.Violet => NodeControl.ActiveColor,
            TextAction.Red => NodeControl.RedColor,
            _ => throw new NotImplementedException()
        };
    }
}

public record struct BinTreeRow<T>(int Tier, List<Node<T>> Nodes) where T : IComparable<T>;
