using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using BinTreeVisualization.UI;

namespace BinTreeVisualization.Algorithms;

public class BinTree<T>
{
    public Node<T> Root { get; set; }
    public BinTreeControl BackingControl { get; init; }
    public Canvas GetCanvas() => BackingControl.MainCanvas;

    public BinTree()
    {
    }

    private void CreateRoot(T value)
    {
        Root = Node<T>.CreateRoot(value, this);
    }

    public void Insert(T value)
    {
        if (Root == null)
        {
            CreateRoot(value);
        }
    }

    private void SetText(string text, Color color)
    {

    }
}

