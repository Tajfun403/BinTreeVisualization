using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
    private BinTree<T> tree;
    public Node<T> Parent { get; set; } = null;

    public T Value { get; init; }

    public Node<T> Left { get; set; } = null;
    public Node<T> Right { get; set; } = null;

    private Node(T value, BinTree<T> tree)
    {
        Value = value;
        this.tree = tree;
    }

    public Node<T> SpawnChild(T value, bool bLeft)
    {
        var node = new Node<T>(value, tree);
        node.Parent = this;
        var UIControl = CreateUIElem(value, bLeft);
        node.BackingControl = UIControl;
        if (bLeft)
            Left = node;
        else
            Right = node;
        node.Activate();
        node.DesiredLoc = GetLocOf(UIControl);
        return node;
    }

    public const int ToBottomOffset = 70;
    // public const int ToSideOffset = 150;
    public const int ToSideOffset = 100;
    public Point DesiredLoc { get; set; }
    public Point CurrLoc => GetLocOf(BackingControl);

    public Point GetLocOf(NodeControl control) => new(Canvas.GetLeft(control), Canvas.GetTop(control));

    public static Node<T> CreateRoot<T>(T value, BinTree<T> parent) where T : IComparable<T>
    {
        NodeControl el = new(value);
        Node<T> newNode = new(value, parent);
        newNode.BackingControl = el;
        el.VerticalAlignment = VerticalAlignment.Top;
        el.HorizontalAlignment = HorizontalAlignment.Center;
        Canvas.SetLeft(el, 0);
        Canvas.SetTop(el, 0);
        newNode.DesiredLoc = new(0, 0);
        parent.GetCanvas().Children.Add(el);
        el.Activate();
        return newNode;
    }

    private NodeControl CreateUIElem(T value, bool bLeft)
    {
        NodeControl el = new(value);
        Point oldLoc = DesiredLoc;
        Point newLoc = new(oldLoc.X + (bLeft ? -ToSideOffset : ToSideOffset), oldLoc.Y + ToBottomOffset);
        el.VerticalAlignment = VerticalAlignment.Top;
        el.HorizontalAlignment = HorizontalAlignment.Center;
        Canvas.SetLeft(el, newLoc.X);
        Canvas.SetTop(el, newLoc.Y);
        tree.GetCanvas().Children.Add(el);
        return el;
    }

    public Node<T> AdoptChild(Node<T> node)
    {
        node.Parent = this;
        bool bLeft = node.Value.CompareTo(Value) < 0;

        Point expectedLoc;
        if (bLeft)
        {
            Debug.Assert(Left is null, "Tried to adopt a left child but this slot is already taken!");
            Left = node;
            expectedLoc = new Point(DesiredLoc.X - ToSideOffset, DesiredLoc.Y + ToBottomOffset);
        }
        else
        {
            Debug.Assert(Right is null, "Tried to adopt a right child but this slot is already taken!");
            Right = node;
            expectedLoc = new Point(DesiredLoc.X + ToSideOffset, DesiredLoc.Y + ToBottomOffset);
        }
        node.MoveTreeToLoc(expectedLoc);
        Debug.WriteLine($"{{{this}}} adopted {{{node}}} as its {(bLeft ? "left" : "right")} child");

        return node;
    }

    public void OrphanChildren(bool OrphanLeftChild = true, bool OrphanRightChild = true)
    {
        if (OrphanLeftChild)
            Left = null;
        if (OrphanRightChild)
            Right = null;
    }

    public int GetDepth()
    {
        if (Parent is null)
            return 0;
        return 1 + Parent.GetDepth();
    }

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
        var row = tree.GetRow(GetDepth());
        var selfIdx = row.IndexOf(this);
        var searchedIdx = selfIdx + toRight;
        if (searchedIdx < 0 || searchedIdx >= row.Count)
            return null;
        return row[searchedIdx];
    }

    public void Reposition(bool bIntoRight)
    {
        var row = tree.GetRow(GetDepth());

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
    /// <param name="loc"></param>
    public void MoveToLoc(Point loc)
    {
        Debug.WriteLine($"Moving {this} from {DesiredLoc} to {loc}");
        float dur = 0.5f;
        DoubleAnimation xAnim = new()
        {
            To = loc.X,
            Duration = TimeSpan.FromSeconds(dur)
        };

        DoubleAnimation yAnim = new()
        {
            To = loc.Y,
            Duration = TimeSpan.FromSeconds(dur)
        };

        Storyboard storyboard = new();

        Storyboard.SetTarget(xAnim, BackingControl);
        Storyboard.SetTargetProperty(xAnim, new PropertyPath("(Canvas.Left)"));

        Storyboard.SetTarget(yAnim, BackingControl);
        Storyboard.SetTargetProperty(yAnim, new PropertyPath("(Canvas.Top)"));

        storyboard.Children.Add(xAnim);
        storyboard.Children.Add(yAnim);

        DesiredLoc = loc;
        storyboard.Begin();
    }

    public void BlinkSubtree()
    {
        BackingControl.Blink();
        Left?.BlinkSubtree();
        Right?.BlinkSubtree();
    }

    public void Blink()
    {
        BackingControl.Blink();
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

}
