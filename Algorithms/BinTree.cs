using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using BinTreeVisualization.UI;

namespace BinTreeVisualization.Algorithms;

public class BinTree<T> where T : IComparable<T>
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

    public void Insert(T value)
    {
        if (Root == null)
        {
            CreateRoot(value);
        }
        else
            Insert(value, Root);
    }

    private async Task Delay(int ms)
    {
        await Task.Delay(ms);
    }

    public async void Insert(T value, Node<T> currNode)
    {
        currNode.Activate();
        SetText($"Comparing {value} to {currNode.Value}");
        await Delay(1000);
        bool bGoLeft = value.CompareTo(currNode.Value) < 0;

        string s = bGoLeft ? $"{value} is smaller; going left" :
                             $"{value} is larger; going right";

        SetText(s);
        if (bGoLeft)
        {
            if (currNode.Left is null)
            {
                SetText($"Empty leaf; inserting into it");
                currNode.SpawnChild(value, true);
                currNode.Deactivate();
            }
            else
            {
                SetText($"Non-empty leaf; going deeper");
                currNode.Deactivate();
                Insert(value, currNode.Left);
            }
        }
        else
        {
            if (currNode.Right is null)
            {
                SetText($"Empty leaf; inserting into it");
                currNode.SpawnChild(value, false);
                currNode.Deactivate();
            }
            else
            {
                SetText($"Non-empty leaf; going deeper");
                currNode.Deactivate();
                Insert(value, currNode.Right);
            }
        }
    }

    private void SetText(string text)
    {

    }
}

