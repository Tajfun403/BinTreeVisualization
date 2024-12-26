using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using BinTreeVisualization.UI;

namespace BinTreeVisualization.Algorithms;


public class Node<T> where T : IComparable<T>
{
    /// <summary>
    /// Backing element placed in the UI
    /// </summary>
    public NodeControl BackingControl { get; set; }
    private BinTree<T> Tree { get; init; }

    /// <summary>
    /// The node's parent.
    /// </summary>
    public Node<T> Parent
    {
        get;
        set
        {
            field = value;
            RefreshSelfArrow();
        }
    }
        = null;

    /// <summary>
    /// The node's value
    /// </summary>
    public T Value { get; init; }

    /// <summary>
    /// The left child. Use <see cref="AdoptChild(Node{T})"/> or <see cref="OrphanChildren(bool, bool)"/> to change this reference./>
    /// </summary>
    public Node<T> Left { get; private set; } = null;

    /// <summary>
    /// The right child. Use <see cref="AdoptChild(Node{T})"/> or <see cref="OrphanChildren(bool, bool)"/> to change this reference./>
    /// </summary>
    public Node<T> Right { get; private set; } = null;

    // public NodeArrow LeftArrow { get; set; } = null;
    // public NodeArrow RightArrow { get; set; } = null;
    public NodeArrow SelfArrow { get; set; } = null;

    /// <summary>
    /// Internal ctor. Use static method <see cref="CreateRoot{T}(T, BinTree{T})"/> to create a new tree, or <see cref="SpawnChild(T, bool)"/> to spawn a new node.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="tree"></param>
    private Node(T value, BinTree<T> tree)
    {
        Value = value;
        this.Tree = tree;
    }

    /// <summary>
    /// Refresh the arrow pointing to this node to account for changes in parents
    /// </summary>
    public void RefreshSelfArrow()
    {
        if (Parent is null)
        {
            SelfArrow?.RemoveSelf();
            SelfArrow = null;
            return;
        }
        else if (SelfArrow is null)
        {
            SelfArrow = Parent.CreateArrowTo(this);
        }
        SelfArrow?.MoveTargetToLoc(DesiredLoc);
        SelfArrow?.MoveSourceToLoc(Parent.DesiredLoc);
    }

    /// <summary>
    /// Spawn a new node as child of this node. This is the only way to spawn new nodes.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="bLeft"></param>
    /// <returns></returns>
    public Node<T> SpawnChild(T value, bool bLeft)
    {
        var node = new Node<T>(value, Tree);
        node.Parent = this;
        var UIControl = CreateUIElem(value, bLeft);
        node.BackingControl = UIControl;
        node.DesiredLoc = GetLocOf(UIControl);
        if (bLeft)
        {
            Left = node;
            node.RefreshSelfArrow();
        }
        else
        {
            Right = node; 
            node.RefreshSelfArrow();
        }
        node.Activate();

        return node;
    }

    public const int ToBottomOffset = 70;
    // public const int ToBottomOffset = 70;
    // public const int ToSideOffset = 150;
    public const int ToSideOffset = 100;
    public double CurrWidth => BackingControl.Width;
    public double CurrHeight => BackingControl.Height;
    public const double NodeWidth = 120;
    public const double NodeHeight = 70;

    /// <summary>
    /// Desired location of the node. Might be either the current location, or the location the new will finish at once its animation end. <para/>
    /// Use <see cref="MoveToLoc(Point)"/> to move the node to a new location.
    /// </summary>
    public Point DesiredLoc { get; private set; }

    /// <summary>
    /// Current location of the node in the UI. Might be volatile because it is affected by animations.<para/>
    /// Use <see cref="DesiredLoc"/> for location-related operations.
    /// </summary>
    public Point CurrLoc => GetLocOf(BackingControl);

    private Point GetLocOf(NodeControl control) => new(Canvas.GetLeft(control), Canvas.GetTop(control));

    /// <summary>
    /// Create a root node to start a new tree.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="parent"></param>
    /// <returns>The newly created <see cref="Node{T}"/></returns>
    public static Node<T> CreateRoot<T>(T value, BinTree<T> parent) where T : IComparable<T>
    {
        NodeControl el = new(value);
        Node<T> newNode = new(value, parent);
        newNode.BackingControl = el;
        Canvas.SetLeft(el, 0);
        Canvas.SetTop(el, 0);
        newNode.DesiredLoc = new(0, 0);
        parent.GetCanvas().Children.Add(el);
        el.Activate();
        return newNode;
    }

    /// <summary>
    /// Create a new UI element for a node and add it to the <see cref="Canvas"/>
    /// </summary>
    /// <param name="value">Value of the new node</param>
    /// <param name="bLeft">Whether to prepare the child on parent's left side.</param>
    /// <returns>The created UI element.</returns>
    private NodeControl CreateUIElem(T value, bool bLeft)
    {
        NodeControl el = new(value);
        Point oldLoc = DesiredLoc;
        Point newLoc = new(oldLoc.X + (bLeft ? -ToSideOffset : ToSideOffset), oldLoc.Y + ToBottomOffset);
        Canvas.SetLeft(el, newLoc.X);
        Canvas.SetTop(el, newLoc.Y);
        Tree.GetCanvas().Children.Add(el);
        return el;
    }

    /// <summary>
    /// Create a UI arrow from this node pointing to another node, add it to <see cref="Canvas"/>, and return it.
    /// </summary>
    /// <param name="node"></param>
    /// <returns>The created UI element.</returns>
    private NodeArrow CreateArrowTo(Node<T> node)
    {
        NodeArrow arrow = new();
        arrow.Target = node.DesiredLoc;
        arrow.Source = this.DesiredLoc;

        Canvas.SetLeft(arrow, this.DesiredLoc.X);
        Canvas.SetTop(arrow, this.DesiredLoc.Y);

        Tree.GetCanvas().Children.Add(arrow);
        return arrow;
    }

    /// <summary>
    /// Adopt a child node. The child along its entire subtree will be moved to the correct side of this node.<para/>
    /// If selves children slots are full, the child will itself adopt one of the current children in order to to create a chain.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public Node<T> AdoptChild(Node<T> node)
    {
        if (node is null)
            return null;

        Debug.Assert(node != this, "Tried to adopt itself as a child");

        Debug.WriteLine($"Setting parent of {{{node}}} to {{{this}}}");
        node.Parent = this;
        bool bLeft = node < this;

        Debug.WriteLine($"Adoption: {{{this}}} adopting {{{node}}} as its {(bLeft ? "left" : "right")} child");

        Point expectedLoc;
        if (bLeft)
        {
            // Debug.Assert(Left is null, "Tried to adopt a left child but this slot is already taken!");
            if (Left == node)
            {
                Debug.WriteLine("Tried to adopt its own child");
                return this;
            }
            if (Left != null)
            {
                Debug.WriteLine("Forced adoption");
                node.AdoptChild(Left);
            }
            Left = node;

            /*var oldLeft = Left;
            Left = node;
            node.AdoptChild(oldLeft);*/
            expectedLoc = new Point(DesiredLoc.X - ToSideOffset, DesiredLoc.Y + ToBottomOffset);
        }
        else
        {
            if (Right == node)
            {
                Debug.WriteLine("Tried to adopt its own child");
                return this;
            }
            // Debug.Assert(Right is null, "Tried to adopt a right child but this slot is already taken!");
            if (Right != null)
            {
                Debug.WriteLine("Forced adoption");
                node.AdoptChild(Right);
            }
            Right = node;

            expectedLoc = new Point(DesiredLoc.X + ToSideOffset, DesiredLoc.Y + ToBottomOffset);
        }
        node.MoveTreeToLoc(expectedLoc);
        node.RefreshSelfArrow();

        return node;
    }

    /// <summary>
    /// Orphan the children of this node
    /// </summary>
    /// <param name="OrphanLeftChild">Whether to orphan the left child</param>
    /// <param name="OrphanRightChild">Whether to orphan the right child</param>
    public void OrphanChildren(bool OrphanLeftChild = true, bool OrphanRightChild = true)
    {
        if (OrphanLeftChild)
            Left?.DetachFromParent();
        if (OrphanRightChild)
            Right?.DetachFromParent();
    }

    /// <summary>
    /// Detach this node from its parent
    /// </summary>
    public void DetachFromParent()
    {
        if (Parent is null)
            return;
        if (Parent.Left == this)
            Parent.Left = null;
        else if (Parent.Right == this)
            Parent.Right = null;
        Parent = null;
    }

    /// <summary>
    /// Get depth, i.e. how many levels deep is this node in the tree.
    /// </summary>
    /// <returns>Depth of the node</returns>
    public int GetDepth()
    {
        if (Parent is null)
            return 0;
        return 1 + Parent.GetDepth();
    }

    /// <summary>
    /// Get height of the node, i.e. how many levels deep is the deepest child of this node.
    /// </summary>
    /// <returns>Height of the node</returns>
    public int GetHeight()
    {
        return 1 + Math.Max(Left?.GetHeight() ?? 0, Right?.GetHeight() ?? 0);
    }

    /// <summary>
    /// Get the balance of the node - how much taller is its left children trace than the right one
    /// </summary>
    /// <returns></returns>
    public int GetNodeBalance()
    {
        return (Left?.GetHeight() ?? 0) - (Right?.GetHeight() ?? 0);
    }

    /// <summary>
    /// Get the node at a relative position to the right from the current node, as if all nodes on this level were taken.<para/>
    /// Returns null if the spot at this location is empty.
    /// </summary>
    /// <param name="toRight"></param>
    /// <returns></returns>
    public Node<T> GetRelativeNode(int toRight)
    {
        var row = Tree.GetRow(GetDepth());
        var selfIdx = row.IndexOf(this);
        var searchedIdx = selfIdx + toRight;
        if (searchedIdx < 0 || searchedIdx >= row.Count)
            return null;
        return row[searchedIdx];
    }

    public void Reposition(bool bIntoRight)
    {
        var row = Tree.GetRow(GetDepth());

        // I am a problematic node
        // check if I am within NodeWidth of any nodes in my row

        bool bOverlapping = row.Any(n => Math.Abs(n.CurrLoc.X - CurrLoc.X) < ToSideOffset);
        if (!bOverlapping)
            return;

        // overlapping
        if (bIntoRight)
        {
            // I want to insert myself into right, where space is taken
            // I need to be to the right of my parent, so I try to move my entire parent and its subtree to the left

        }
    }

    /// <summary>
    /// Highlight the associated node in the UI
    /// </summary>
    public void Activate()
    {
        BackingControl.Activate();
    }

    /// <summary>
    /// Remove highlight of the associated UI node
    /// </summary>
    public void Deactivate()
    {
        BackingControl.Deactivate();
    }

    public void HighlightBlue()
    {
        BackingControl.HighlightBlue();
    }

    public void ActivateBlue(bool bRecursive = false)
    {
        if (bRecursive)
        {
            Traverse().ToList().ForEach(x => x.ActivateBlue());
        }
        else
            BackingControl.ActivateBlue();
    }

    public void DeactivateBlue(bool bRecursive = false)
    {
        if (bRecursive)
        {
            Traverse().ToList().ForEach(x => x.DeactivateBlue());
        }
        else
            BackingControl.DeactivateBlue();
    }

    /// <summary>
    /// Blink the node's UI control in blue
    /// </summary>
    /// <param name="bRecursive">Whether to blink the node itself, or the node and all its children as well</param>
    public void Blink(bool bRecursive = false)
    {
        if (bRecursive)
            Traverse().ToList().ForEach(x => x.Blink());
        else
            BackingControl.Blink();
    }

    /// <summary>
    /// Move the associated node control to a location and make its all its children follow it.
    /// </summary>
    /// <param name="loc"></param>
    public void MoveTreeToLoc(Point loc)
    {
        var LocDelta = new Point(loc.X - DesiredLoc.X, loc.Y - DesiredLoc.Y);
        Traverse().ToList().ForEach(x => x.MoveByLoc(LocDelta));
    }

    /// <summary>
    /// Move the associated node control by a specified amount
    /// </summary>
    /// <param name="loc"></param>
    public void MoveByLoc(Point loc)
    {
        var newLoc = new Point(DesiredLoc.X + loc.X, DesiredLoc.Y + loc.Y);
        MoveToLoc(newLoc);
    }

    /// <summary>
    /// Move the associated node control to specified location over 0.5 seconds
    /// </summary>
    /// <param name="loc">Location to move to</param>
    public void MoveToLoc(Point loc)
    {
        Debug.WriteLine($"Moving {this} from {DesiredLoc} to {loc}");
        DesiredLoc = loc;
        BackingControl.MoveToLoc(loc);
        RefreshSelfArrow();
        Left?.RefreshSelfArrow();
        Right?.RefreshSelfArrow();
    }

    /// <summary>
    /// Traverse the tree in Level Order Traversal
    /// </summary>
    /// <returns>Returns IEnumerable that contains this and all children nodes</returns>
    public IEnumerable<Node<T>> Traverse()
    {
        var queue = new Queue<Node<T>>();
        queue.Enqueue(this);
        while (queue.Count > 0)
        {
            var curr = queue.Dequeue();
            yield return curr;
            if (curr.Left != null)
                queue.Enqueue(curr.Left);
            if (curr.Right != null)
                queue.Enqueue(curr.Right);
        }
    }

    /// <inheritdoc cref="object.ToString"/>
    public override string ToString()
    {
        List<string> parts = [$"Node {Value}"];
        if (Left is not null)
            parts.Add($"left -> {Left.Value}");
        if (Right is not null)
            parts.Add($"right -> {Right.Value}");
        if (Left is null && Right is null)
            parts.Add("no children");
        return String.Join("; ", parts);
    }

    /// <summary>
    /// Compare nodes value-wise
    /// </summary>
    /// <param name="self"></param>
    /// <param name="other"></param>
    /// <returns>Whether the value of left is smaller than the value of right</returns>
    public static bool operator <(Node<T> self, Node<T> other) => self.Value.CompareTo(other.Value) < 0;

    /// <summary>
    /// Compare nodes value-wise
    /// </summary>
    /// <param name="self"></param>
    /// <param name="other"></param>
    /// <returns>Whether the value of left is bigger than the value of right</returns>
    public static bool operator >(Node<T> self, Node<T> other) => self.Value.CompareTo(other.Value) > 0;

    /// <summary>
    /// Compare nodes value-wise
    /// </summary>
    /// <param name="self"></param>
    /// <param name="other"></param>
    /// <returns>Whether the value of left is smaller than the value of right</returns>
    public static bool operator <(T self, Node<T> other) => self.CompareTo(other.Value) < 0;

    /// <summary>
    /// Compare nodes value-wise
    /// </summary>
    /// <param name="self"></param>
    /// <param name="other"></param>    
    /// /// <returns>Whether the value of left is smaller than the value of right</returns>
    public static bool operator >(T self, Node<T> other) => self.CompareTo(other.Value) > 0;

    /// <summary>
    /// Get or set a node by its side
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public Node<T> this[Side side]
    {
        get => side switch
        {
            Side.Left => Left,
            Side.Right => Right,
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
        };

        set
        {
            switch (side)
            {
                case Side.Left:
                    Left = value;
                    break;
                case Side.Right:
                    Right = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }
    }
}

/// <summary>
/// Represents a side of a binary tree node
/// </summary>
public enum Side
{
    Left,
    Right
}

internal static class NodeExtension
{
    /// <summary>
    /// Get the opposite side
    /// </summary>
    /// <param name="side">Current side</param>
    /// <returns>The opposite side</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal static Side Opposite(this Side side) => side switch
    {
        Side.Left => Side.Right,
        Side.Right => Side.Left,
        _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
    };

    internal static Side FromIsLeft(this Side side, bool bIsLeft) => bIsLeft ? Side.Left : Side.Right;
}

