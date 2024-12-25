using BinTreeVisualization.Algorithms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BinTreeVisualization.UI;

/// <summary>
/// Interaction logic for BinTree.xaml
/// </summary>
public partial class BinTreeControl : UserControl
{
    public BinTreeControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Add a new text describing the current step. The newly spawned text is animated.
    /// </summary>
    /// <param name="text">The text to add</param>
    /// <param name="act">Text's color</param>
    public void SetText(string text, TextAction act = TextAction.Base)
    {
        ProgressLabel label = new();
        label.Content = text;
        label.Foreground = new SolidColorBrush(act.GetColor());
        ProgressStackPanel.Children.Add(label);
        label.TriggerSpawnAnim();
    }

    /// <summary>
    /// Reset all current progress texts.<para />
    /// If any texts are currently present, they will be despawned in a 200ms long animation.<para />
    /// If empty, returns immediately.
    /// </summary>
    /// <returns>True if any texts were hidden, false otherwise</returns>
    public bool ResetText()
    {
        var LabelsContainer = ProgressStackPanel.Children.OfType<ProgressLabel>().ToList();
        if (LabelsContainer.Count == 0)
            return true;

        LabelsContainer.ForEach(x => x.TriggerDespawnAnim());

        ProgressStackPanel.Children.Clear();
        return false;
    }

    /// <summary>
    /// Auto layout the tree, allocating double the space for every next row. Does not account for empty branches.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tree"></param>
    public void LayoutTreeNSquare<T>(BinTree<T> tree) where T : IComparable<T>
    {
        int maxHeight = tree.Height;
        Debug.WriteLine($"Curr height is {Height}");
        // double distanceBetweenNodesOnHeight(int height) => Math.Pow(2, maxHeight - height) * Node<T>.ToSideOffset;
        double distanceBetweenNodesOnHeight(int height) => Math.Pow(2, maxHeight - height) * 20;

        void Traverse(Node<T> curr)
        {
            if (curr is null)
                return;
            int depth = curr.GetDepth();
            var currLoc = curr.DesiredLoc;
            double baseLeft = currLoc.X;
            double Y = currLoc.Y + Node<T>.ToBottomOffset;
            double offset = distanceBetweenNodesOnHeight(depth);
            curr.Right?.MoveToLoc(new((baseLeft + offset), Y));
            curr.Left?.MoveToLoc(new((baseLeft - offset), Y));
            Debug.WriteLine($"Node {curr.Value} moved to {baseLeft}, {Y}");

            Traverse(curr.Left);
            Traverse(curr.Right);
        }

        Traverse(tree.Root);
    }

    /// <summary>
    /// Attempts to layout the tree in such a way that nodes are evenly spaced out accounting for empty branches.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tree"></param>
    public void LayoutTree<T>(BinTree<T> tree) where T : IComparable<T>
    {
        float nodeWidth = Node<T>.ToSideOffset;
        float levelHeight = Node<T>.ToBottomOffset;

        // Helper function to calculate positions and bounds of subtrees
        // This will also return the width of the subtree for dynamic layout
        (float leftBound, float rightBound, float subtreeWidth) Traverse(Node<T> curr, float x, float y)
        {
            if (curr == null)
                return (x, x, 0); // No node, no width

            // Calculate the bounds for the left and right subtrees
            float leftBound = x, rightBound = x;
            float leftWidth = 0, rightWidth = 0;

            if (curr.Left != null)
            {
                var (leftLeft, leftRight, leftSubtreeWidth) = Traverse(
                    curr.Left,
                    x - nodeWidth - leftWidth, // Offset for left subtree
                    y + levelHeight // Move down a level
                );
                leftBound = leftLeft;
                leftWidth = leftSubtreeWidth;
            }

            if (curr.Right != null)
            {
                var (rightLeft, rightRight, rightSubtreeWidth) = Traverse(
                    curr.Right,
                    x + nodeWidth + rightWidth, // Offset for right subtree
                    y + levelHeight // Move down a level
                );
                rightBound = rightRight;
                rightWidth = rightSubtreeWidth;
            }

            // Calculate the total width of the subtree at the current node
            float subtreeWidth = leftWidth + nodeWidth + rightWidth;

            // Determine the center position of the current node
            float midX = (leftBound + rightBound) / 2;

            // If it's the root node, ensure it stays at (0, 0)
            if (curr == tree.Root)
            {
                midX = 0;
            }

            // Move the current node to the calculated position
            curr.MoveToLoc(new(midX, y));
            Debug.WriteLine($"Node {curr.Value} moved to ({midX}, {y})");

            // Return the left and right bounds and the total width of the subtree
            return (Math.Min(leftBound, midX), Math.Max(rightBound, midX), subtreeWidth);
        }

        // Start the layout from the root node, which is fixed at (0, 0)

       Traverse(tree.Root, 0, 0); // Start with the root at (0, 0)
    }

}
