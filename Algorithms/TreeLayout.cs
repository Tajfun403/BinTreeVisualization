using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;

namespace BinTreeVisualization.Algorithms;

internal class TreeLayout
{
    /// <summary>
    /// Layout the tree with WS algorithm
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="root"></param>
    public void LayoutTree<T>(Node<T> root) where T : IComparable<T>
    {
        // return;
        treeHeight = root.GetHeight();
        LastUsedSlotsPerLevel = Enumerable.Repeat(double.NegativeInfinity, treeHeight).ToList();
        LayoutNode(root);
        // var RootDelta = root.DesiredLoc - new System.Windows.Point(0, 0);
        root.MoveTreeToLoc(new(0, 0));
    }

    int treeHeight;

    /// <summary>
    /// A list that holds the last taken X location for each tree's level
    /// </summary>
    private List<double> LastUsedSlotsPerLevel { get; set; }
    double DistBetweenNodes = Node<int>.ToSideOffset * 2;

    /// <summary>
    /// Attempt to take the next slot. Start with center (0, 0)
    /// </summary>
    /// <param name="depth"></param>
    /// <returns></returns>
    public double TakeNextSlot(int depth)
    {
        var ret = LastUsedSlotsPerLevel[depth];
        if (ret == double.NegativeInfinity)
            ret = 0;
        LastUsedSlotsPerLevel[depth] = ret + DistBetweenNodes;
        return ret;
    }

    /// <summary>
    /// Attempt to place node in the specified <paramref name="AttemptedLoc"/>.<para/>
    /// If the location is already taken, finds the next available location, returns the newly find location,
    /// as well as the its <paramref name="Offset"/>
    /// </summary>
    /// <param name="depth">Tree's depth to locate the node in</param>
    /// <param name="AttemptedLoc">The location that is wanted to be taken</param>
    /// <param name="Offset">Offset of the acquired location from the requested location</param>
    /// <returns>The nearest available location</returns>
    public double TryTakeLoc(int depth, double AttemptedLoc, out double Offset)
    {
        var minX = LastUsedSlotsPerLevel[depth];
        if (AttemptedLoc >= minX)
        {
            LastUsedSlotsPerLevel[depth] = AttemptedLoc;
            Offset = 0;
            return AttemptedLoc;
        }
        var newLoc = TakeNextSlot(depth);
        Offset = newLoc - AttemptedLoc;
        return newLoc;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="node"></param>
    public void LayoutNode<T>(Node<T> node) where T : IComparable<T>
    {
        if (node == null)
            return;

        // decide children locations first
        LayoutNode(node.Left);
        LayoutNode(node.Right);

        var level = node.GetDepth();
        var y = level * Node<T>.ToBottomOffset;
        double x = 0;

        if (node.IsLeaf())
        {
            x = TakeNextSlot(level);
            node.MoveToLoc(new(x, y));
            return;
        }
        // place itself according to parents location
        else if (node.Left != null && node.Right != null)
        {
            var left = node.Left.DesiredLoc.X;
            var right = node.Right.DesiredLoc.X;
            x = (left + right) / 2;
        }
        else if (node.Left != null)
        {
            x = node.Left.DesiredLoc.X + Node<T>.ToSideOffset;
        }
        else if (node.Right != null)
        {
            x = node.Right.DesiredLoc.X - Node<T>.ToSideOffset;  
        }

        x = TryTakeLoc(level, x, out double Offset);
        node.MoveToLoc(new(x, y));

        // if one could not put itself in the desired location,
        // move the entire tree by the gotten offset
        if (Offset != 0)
            node.MoveChildrenByLoc(new(Offset, 0));

        return;
        for (int i = level; i < treeHeight; i++)
        {
            LastUsedSlotsPerLevel[i] += Offset;
        }
    }
}
