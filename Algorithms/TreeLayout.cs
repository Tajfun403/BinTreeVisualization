using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;

namespace BinTreeVisualization.Algorithms;

/// <summary>
/// Helper class that layouts a binary tree so that no nodes overlap.
/// </summary>
internal class TreeLayout
{
    /// <summary>
    /// Layout the tree with modified Wetherell and Shannon algorithm
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tree">The tree</param>
    public void LayoutTree<T>(BinTree<T> tree) where T : IComparable<T>
    {
        // return;
        TreeHeight = tree.GetHeight();
        LastUsedSlotsPerLevel = Enumerable.Repeat(double.NegativeInfinity, TreeHeight).ToList();
        LayoutNode(tree.Root);

        // layout starts placing all nodes from the left side of the canvas
        // tree needs to be re-centered afterwards
        tree.Root.MoveTreeToLoc(new(0, 0), true);

        // all movement changes were cached first; apply them now
        tree.Root.Traverse().ToList().ForEach(x => x.PlayDelayedAnimation());
        Debug.WriteLine("Auto layout finished");
    }

    /// <summary>
    /// Height of the tree
    /// </summary>
    private int TreeHeight { get; set; }

    /// <summary>
    /// A list that holds the last taken X location for each tree's level.<para/>
    /// Level's value is <see cref="double.NegativeInfinity"/> if no location had been taken yet.<para/> 
    /// Negative locations are possible if a specific loc was requested left to the canvas' left border.
    /// </summary>
    private List<double> LastUsedSlotsPerLevel { get; set; }

    /// <summary>
    /// Distance between nodes' left corners.
    /// </summary>
    private double DistBetweenNodes => Node<int>.ToSideOffset * 2;

    /// <summary>
    /// Attempt to take the next slot on the specified <paramref name="depth"/>. Start with center (0, 0).<para/>
    /// Internally moves the last taken slot offset forward on success.
    /// </summary>
    /// <param name="depth">The level to seek the space on.</param>
    /// <returns>The x-coordinate of the nearest free empty space.</returns>
    private double TakeNextSlot(int depth)
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
    /// as well as the its <paramref name="Offset"/> from the requested one.
    /// </summary>
    /// <param name="depth">Tree's depth to locate the node in</param>
    /// <param name="AttemptedLoc">The location that is wanted to be taken</param>
    /// <param name="Offset">Offset of the acquired location from the requested location</param>
    /// <returns>The nearest available location</returns>
    private double TryTakeLoc(int depth, double AttemptedLoc, out double Offset)
    {
        var minX = LastUsedSlotsPerLevel[depth];
        if (AttemptedLoc >= minX)
        {
            LastUsedSlotsPerLevel[depth] = AttemptedLoc + DistBetweenNodes;
            Offset = 0;
            return AttemptedLoc;
        }
        var newLoc = TakeNextSlot(depth);
        Offset = newLoc - AttemptedLoc;
        return newLoc;
    }

    /// <summary>
    /// Layout this <paramref name="node"/> in the tree. To be used in post-order traversal.<para/>
    /// Recursively layouts this <paramref name="node"/>'s children, and afterwards position this <paramref name="node"/> in appropriate space above its children.<para/>
    /// Shifts the entire tree after parent if the parent can't be placed on the wanted position directly.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="node"></param>
    private void LayoutNode<T>(Node<T> node) where T : IComparable<T>
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
            node.MoveToLoc(new(x, y), true);
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
        node.MoveToLoc(new(x, y), true);

        // if one could not put itself in the desired location,
        // move the entire tree by the gotten offset
        if (Offset != 0)
            node.MoveChildrenByLoc(new(Offset, 0), true);

        // since children were moved by an offset, the last taken location in the list
        // need to be updated to account for this
        for (int i = level + 1; i <= node.Traverse().Last().GetDepth(); i++)
        {
            LastUsedSlotsPerLevel[i] += Offset;
        }
    }
}
