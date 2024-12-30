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

/// <summary>
/// A binary tree's node
/// </summary>
/// <typeparam name="T">The type of data this node holds</typeparam>
public partial class Node<T> where T : IComparable<T>
{
    /// <summary>
    /// Node's tree
    /// </summary>
    private BinTree<T> Tree { get; init; }

    /// <summary>
    /// The node's parent.
    /// </summary>
    public Node<T>? Parent
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
    public T Value { get; private set; }

    /// <summary>
    /// The left child. Use <see cref="AdoptChild(Node{T})"/> or <see cref="OrphanChildren(bool, bool)"/> to change this reference.
    /// </summary>
    public Node<T>? Left { get; private set; } = null;

    /// <summary>
    /// The right child. Use <see cref="AdoptChild(Node{T})"/> or <see cref="OrphanChildren(bool, bool)"/> to change this reference.
    /// </summary>
    public Node<T>? Right { get; private set; } = null;

    /// <summary>
    /// Cached node height. Can be refreshed with <see cref="RefreshHeight"/>
    /// </summary>
    public int Height
    {
        get => field;
        // get => CalcHeight();
        private set;

    } = 1;

    /// <summary>
    /// Refresh node height. To be used after adoptions.
    /// </summary>
    public void RefreshHeight()
    {
        Height = GetHeight();
    }

    /// <summary>
    /// Internal ctor. Use static method <see cref="CreateRoot{T}(T, BinTree{T})"/> to create a new tree, or <see cref="SpawnChild(T, bool)"/> to spawn a new node.
    /// </summary>
    /// <param name="value">Value for the new node to be spawned with</param>
    /// <param name="tree">The tree the node is in</param>
    private Node(T value, BinTree<T> tree)
    {
        Value = value;
        this.Tree = tree;
    }

    /// <summary>
    /// Spawn a new node as child of this node. This is the only way to spawn new nodes.
    /// </summary>
    /// <param name="value">Value for the new node to be spawned with</param>
    /// <param name="bLeft">Whether to spawn new node as this node's left child</param>
    /// <returns>The newly spawned node</returns>
    public Node<T> SpawnChild(T value, bool bLeft)
    {
        var UIControl = CreateUIElem(value, bLeft);
        var node = new Node<T>(value, Tree)
        {
            BackingControl = UIControl,
            DesiredLoc = GetLocOf(UIControl)
        };
        node.Parent = this;
        if (bLeft)
        {
            Left = node;
        }
        else
        {
            Right = node; 
        }
        node.RefreshSelfArrow();
        node.Activate();

        return node;
    }

    /// <summary>
    /// Whether the node is a leaf (i.e. has no children)
    /// </summary>
    /// <returns>Whether the node is a leaf (i.e. has no children)</returns>
    public bool IsLeaf() => Left is null && Right is null;

    /// <summary>
    /// Create a root node to start a new tree.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value">Value to create the root with</param>
    /// <param name="tree">The tree in which the root should be created</param>
    /// <returns>The newly created <see cref="Node{T}"/></returns>
    public static Node<T> CreateRoot<T>(T value, BinTree<T> tree) where T : IComparable<T>
    {
        NodeControl el = new(value);
        Node<T> newNode = new(value, tree)
        {
            DesiredLoc = new(0, 0),
            BackingControl = el
        };
        Canvas.SetTop(el, 0);
        Canvas.SetLeft(el, 0);
        newNode.DesiredLoc = new(0, 0);
        tree.GetCanvas().Children.Add(el);
        el.Activate();
        return newNode;
    }

    /// <summary>
    /// Adopt a child node. The child along its entire subtree will be moved to the correct side of this node.<para/>
    /// If selves children slots are full, the passed-in <paramref name="node"/> will itself adopt one of the current children in order to create a chain.
    /// </summary>
    /// <param name="node">The node to adopt</param>
    /// <returns>The inserted node</returns>
    public Node<T> AdoptChild(Node<T> node)
    {
        if (node is null)
            return null;

        // Debug.Assert(node != this, "Tried to adopt itself as a child");
        if (node == this)
        {
            Debug.WriteLine("Tried to adopt itself as a child");
            return this;
        }

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

        RefreshHeight();
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
    /// Swap value of this node with value from another one. Does NOT ensure binary tree properties are maintained!
    /// </summary>
    /// <param name="other">The other node</param>
    public void SwapValues(Node<T> other)
    {
        (Value, other.Value) = (other.Value, Value);
        this.BackingControl.Value = Value;
        other.BackingControl.Value = other.Value;
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
        return 1 + Math.Max(Left?.Height ?? 0, Right?.Height ?? 0);
        // return 1 + Math.Max(Left?.GetHeight() ?? 0, Right?.GetHeight() ?? 0);
    }

    /// <summary>
    /// Calculate node's height recursively
    /// </summary>
    /// <returns>Height of the node</returns>
    public int CalcHeight()
    {
        return 1 + Math.Max(Left?.Height ?? 0, Right?.Height ?? 0);
    }

    /// <summary>
    /// Get the balance of the node - how much taller is its left children trace than the right one
    /// </summary>
    /// <returns>The node balance</returns>
    public int GetNodeBalance()
    {
        return (Left?.GetHeight() ?? 0) - (Right?.GetHeight() ?? 0);
    }

    /// <summary>
    /// Get the node at a relative position to the right from the current node, as if all nodes on this level were taken.<para/>
    /// Returns null if the spot at this location is empty.
    /// </summary>
    /// <param name="toRight">Relative amount of node-slots to the right</param>
    /// <returns>The node at the requested relative position.</returns>
    public Node<T> GetRelativeNode(int toRight)
    {
        var row = Tree.GetRow(GetDepth());
        var selfIdx = row.IndexOf(this);
        var searchedIdx = selfIdx + toRight;
        if (searchedIdx < 0 || searchedIdx >= row.Count)
            return null;
        return row[searchedIdx];
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

    /// <summary>
    /// Traverse ancestors of this node in the order parent -> grandparent -> ... -> root
    /// </summary>
    /// <returns>Returns IEnumerable that contains this and all parent nodes going from the bottom</returns>
    public IEnumerable<Node<T>> TraverseAncestors()
    {
        var curr = this;
        while (curr != null)
        {
            yield return curr;
            curr = curr.Parent;
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
        if (IsLeaf())
            parts.Add("no children");
        return String.Join("; ", parts);
    }

    /// <summary>
    /// Compare nodes value-wise
    /// </summary>
    /// <param name="self">Left item to compare</param>
    /// <param name="other">Right item to compare</param>
    /// <returns>Whether the value of left is smaller than the value of right</returns>
    public static bool operator <(Node<T> self, Node<T> other) => self.Value.CompareTo(other.Value) < 0;

    /// <summary>
    /// Compare nodes value-wise
    /// </summary>
    /// <param name="self">Left item to compare</param>
    /// <param name="other">Right item to compare</param>
    /// <returns>Whether the value of left is bigger than the value of right</returns>
    public static bool operator >(Node<T> self, Node<T> other) => self.Value.CompareTo(other.Value) > 0;

    /// <inheritdoc cref="operator }=(Node{T}, Node{T})"/>
    public static bool operator <(T self, Node<T> other) => self.CompareTo(other.Value) < 0;

    /// <inheritdoc cref="operator {(Node{T}, Node{T})"/>
    public static bool operator >(T self, Node<T> other) => self.CompareTo(other.Value) > 0;

    /// <summary>
    /// Compare nodes value-wise
    /// </summary>
    /// <param name="self">Left item to compare</param>
    /// <param name="other">Right item to compare</param>
    /// <returns>Whether the value of left is smaller or equals to the value of right</returns>
    public static bool operator <=(Node<T> self, Node<T> other) => self.Value.CompareTo(other.Value) <= 0;

    /// <summary>
    /// Compare nodes value-wise
    /// </summary>
    /// <param name="self">Left item to compare</param>
    /// <param name="other">Right item to compare</param>
    /// <returns>Whether the value of left is bigger or equals to the value of right</returns>
    public static bool operator >=(Node<T> self, Node<T> other) => self.Value.CompareTo(other.Value) >= 0;

    /// <inheritdoc cref="operator {=(Node{T}, Node{T})"/>
    public static bool operator <=(T self, Node<T> other) => self.CompareTo(other.Value) <= 0;

    /// <inheritdoc cref="operator }=(Node{T}, Node{T})"/>
    public static bool operator >=(T self, Node<T> other) => self.CompareTo(other.Value) >= 0;

    /// <summary>
    /// Get or set a node by its side
    /// </summary>
    /// <param name="side">The side</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public Node<T>? this[Side side]
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

/// <summary>
/// Extensions for <see cref="Side"/>
/// </summary>
internal static class SideExtensions
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

    /// <summary>
    /// Convert a bool <paramref name="bIsLeft"/> to a side.
    /// </summary>
    /// <param name="side"></param>
    /// <param name="bIsLeft"></param>
    /// <returns></returns>
    internal static Side FromIsLeft(this Side side, bool bIsLeft) => bIsLeft ? Side.Left : Side.Right;
}

