using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

    private const int ToBottomOffset = 100;
    private const int ToSideOffset = 150;

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
        Canvas.SetLeft(el, newLoc.X);
        Canvas.SetTop(el, newLoc.Y);
        tree.GetCanvas().Children.Add(el);
        return el;
    }

    public static void SpawnUIElem()
    {
        throw new NotImplementedException();
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

}
