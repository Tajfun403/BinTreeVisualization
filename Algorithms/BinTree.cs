using System;
using System.Collections.Generic;
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

    public async void Insert(T value)
    {
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
                ReLayoutNodes();
            }
            LastHeight = currHeight;
            await Delay(2000);
            await ResetText();
        }
    }

    private async Task Delay(int ms)
    {
        await Task.Delay(ms);
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


        return ret;

        SetText($"Inserted into subtree, rebalacing");

        currNode.Activate();
        if (bGoLeft)
        {
            if (currNode.GetNodeBalance() > 1)
            {
                SetText($"Left subtree is too high; rotating right");
                currNode = RotateRight(currNode);
            }
        }
        else
        {
            if (currNode.GetNodeBalance() < -1)
            {
                SetText($"Right subtree is too high; rotating left");
                currNode = RotateLeft(currNode);
            }
        }

        return ret;
    }

    private Node<T> RotateRight(Node<T> currNode)
    {
        var newRoot = currNode.Left!;
        currNode.Left = newRoot.Right;
        newRoot.Right = currNode;
        return newRoot;
    }

    private Node<T> RotateLeft(Node<T> currNode)
    {
        var newRoot = currNode.Right!;
        currNode.Right = newRoot.Left;
        newRoot.Left = currNode;
        return newRoot;
    }

    private void SetText(string text, TextAction act = TextAction.Base)
    {
        BackingControl.SetText(text, act);
    }

    private async Task ResetText()
    {
        await BackingControl.ResetText();
    }


    private void ReLayoutNodes()
    {
        BackingControl.LayoutTree(this);
    }
    IEnumerable<Node<T>> Traverse()
    {
        var stack = new Stack<Node<T>>();
        stack.Push(Root);
        while (stack.Count > 0)
        {
            var curr = stack.Pop();
            yield return curr;
            if (curr.Left != null)
                stack.Push(curr.Left);
            if (curr.Right != null)
                stack.Push(curr.Right);
        }
    }
}

public enum TextAction
{
    Base,
    Blink
}

public static class TextActionColors
{
    public static System.Windows.Media.Color GetColor(this TextAction textAction)
    {
        return textAction switch
        {
            TextAction.Base => NodeControl.StrokeBase,
            TextAction.Blink => NodeControl.StrokeBlue,
            _ => throw new NotImplementedException()
        };
    }
}