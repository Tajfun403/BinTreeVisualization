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


public class Node<T>
{
    /// <summary>
    /// Backing element placed in the UI
    /// </summary>
    public NodeControl BackingControl { get; set; }
    private BinTree<T> parent;
    public T Value { get; init; }

    public Node(T value, BinTree<T> parent)
    {
        Value = value;
        this.parent = parent;
    }

    public void SpawnChild(T value, bool bLeft)
    {
        CreateUIElem(value, 100, 100);
    }

    public static Node<T> CreateRoot<T>(T value, BinTree<T> parent)
    {
        NodeControl el = new(value);
        Node<T> newNode = new(value, parent);
        newNode.BackingControl = el;
        Canvas.SetLeft(el, 100);
        Canvas.SetTop(el, 100);
        parent.GetCanvas().Children.Add(el);
        return newNode;
    }

    private NodeControl CreateUIElem(T value, double newX, double newY)
    {
        NodeControl el = new(value);
        Canvas.SetLeft(el, newX);
        Canvas.SetTop(el, newY);
        parent.GetCanvas().Children.Add(el);
        return el;
    }

    public static void SpawnUIElem()
    {
        throw new NotImplementedException();
    }

    public Node<T> Left { get; set; } = null;
    public Node<T> Right { get; set; } = null;

    /// <summary>
    /// Highlight the associated node in the UI
    /// </summary>
    public void Activate()
    {

    }

}
