using System;
using System.Collections.Generic;
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
    public Node<T> parent { get; set; } = null;

    public T Value { get; init; }

    public Node<T> Left { get; set; } = null;
    public Node<T> Right { get; set; } = null;

    public Node(T value, BinTree<T> tree)
    {
        Value = value;
        this.tree = tree;
    }

    public Node<T> SpawnChild(T value, bool bLeft)
    {
        var node = new Node<T>(value, tree);
        node.parent = this;
        var UIControl = CreateUIElem(value, bLeft);
        node.BackingControl = UIControl;
        if (bLeft)
            Left = node;
        else
            Right = node;
        node.Activate();
        return node;
    }

    public int GetDepth()
    {
        if (parent is null)
            return 0;
        return 1 + parent.GetDepth();
    }

    /// <summary>
    /// Get the balance of the node - how much taller is its left children trace than the right one
    /// </summary>
    /// <returns></returns>
    public int GetNodeBalance()
    {
        return (Left?.GetDepth() ?? 0) - (Right?.GetDepth() ?? 0);
    }

    public const int ToBottomOffset = 100;
    // public const int ToSideOffset = 150;
    public const int ToSideOffset = 150;
    public Point DesiredLoc { get; set; }
    public Point CurrLoc => new(Canvas.GetLeft(BackingControl), Canvas.GetTop(BackingControl));

    public static Node<T> CreateRoot<T>(T value, BinTree<T> parent) where T : IComparable<T>
    {
        NodeControl el = new(value);
        Node<T> newNode = new(value, parent);
        newNode.BackingControl = el;
        Canvas.SetLeft(el, 0);
        Canvas.SetTop(el, 0);
        parent.GetCanvas().Children.Add(el);
        el.Activate();
        return newNode;
    }

    private NodeControl CreateUIElem(T value, bool bLeft)
    {
        NodeControl el = new(value);
        Point oldLoc = new(Canvas.GetLeft(BackingControl), Canvas.GetTop(BackingControl));
        Point newLoc = new(oldLoc.X + (bLeft ? -ToSideOffset : ToSideOffset), oldLoc.Y + ToBottomOffset);
        el.VerticalAlignment = VerticalAlignment.Top;
        el.HorizontalAlignment = HorizontalAlignment.Center;
        Canvas.SetLeft(el, newLoc.X);
        Canvas.SetTop(el, newLoc.Y);
        tree.GetCanvas().Children.Add(el);
        return el;
    }

    /// <summary>
    /// Highlight the associated node in the UI
    /// </summary>
    public void Activate()
    {
        BackingControl.Activate();
    }

    public void Deactivate()
    {
        BackingControl.Deactivate();
    }

    public void MoveToLoc(Point loc)
    {
        float dur = 0.5f;
        DoubleAnimation xAnimation = new()
        {
            To = loc.X,
            Duration = TimeSpan.FromSeconds(dur)
        };

        DoubleAnimation yAnimation = new()
        {
            To = loc.Y,
            Duration = TimeSpan.FromSeconds(dur)
        };

        // Apply animations to Canvas.Left and Canvas.Top
        Storyboard storyboard = new();

        Storyboard.SetTarget(xAnimation, BackingControl);
        Storyboard.SetTargetProperty(xAnimation, new PropertyPath("(Canvas.Left)"));

        Storyboard.SetTarget(yAnimation, BackingControl);
        Storyboard.SetTargetProperty(yAnimation, new PropertyPath("(Canvas.Top)"));

        storyboard.Children.Add(xAnimation);
        storyboard.Children.Add(yAnimation);

        DesiredLoc = loc;
        storyboard.Begin();
    }

    public void BlinkSubtree()
    {
        BackingControl.Blink();
        Left?.BlinkSubtree();
        Right?.BlinkSubtree();
    }

}
