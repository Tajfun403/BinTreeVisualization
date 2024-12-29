using BinTreeVisualization.Algorithms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BinTreeVisualization.UI;

/// <summary>
/// WPF UserControl that holds all binary tree's node.
/// </summary>
public partial class BinTreeControl : UserControl
{
    public BinTreeControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Current canvas desired scale after animations are finished.
    /// </summary>
    private double CanvasDesiredScale = 1.0;

    /// <summary>
    /// Set the canvas scale with an animation.
    /// </summary>
    /// <param name="newScale"></param>
    public void SetScaleAnim(double newScale)
    {
        Debug.WriteLine($"Setting tree scale to {newScale}");
        CanvasScale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(newScale, TimeSpan.FromMilliseconds(500)));
        CanvasScale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(newScale, TimeSpan.FromMilliseconds(500)));
        CanvasDesiredScale = newScale;
    }

    /// <summary>
    /// Unused.
    /// </summary>
    private double RescaleMultiplier = 1.3;

    /// <summary>
    /// Minimum scale of the tree. Do not make it smaller than that.
    /// </summary>
    private double MinScale = 1;

    /// <summary>
    /// The amount of real space that the tree can accumulate at the current scale.
    /// </summary>
    private double currVirtualWidth => this.ActualWidth / CanvasDesiredScale;

    /// <summary>
    /// Check whether a new scale is needed for all elements to fit.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tree">The tree</param>
    public void VerifyScale<T>(BinTree<T> tree) where T : IComparable<T>
    {
        var newScale = GetNewScale(tree);

        SetScaleAnim(newScale);
    }

    /// <summary>
    /// Get the new scale for the tree to fit the window.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tree">The tree</param>
    /// <returns>A new scale for the canvas</returns>
    private double GetNewScale<T>(BinTree<T> tree) where T : IComparable<T>
    {
        var treeWidth = GetNodesTotalWidth(tree);
        var windowWidth = this.ActualWidth;
        Debug.WriteLine($"TreeWidth {treeWidth}, window width {windowWidth}");

        var scaleMult = treeWidth / windowWidth;
        if (scaleMult < MinScale)
            scaleMult = 1.0;

        return 1 / scaleMult;
    }

    /// <summary>
    /// Get the maximum spread between the westmost and the eastmost nodes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tree">The tree</param>
    /// <returns>Spread of the tree, rounded up so that the tree stays symmetrical.</returns>
    private double GetNodesTotalWidth<T>(BinTree<T> tree) where T : IComparable<T>
    {
        var nodes = tree.Traverse();
        var leftMostNode = nodes.Min(x => x.DesiredLoc.X);
        var rightMostNode = nodes.Max(x => x.DesiredLoc.X) + Node<T>.NodeWidth;
        var maxDistance = new double[] { leftMostNode, rightMostNode }.Select(x => Math.Abs(x)).Max();
        var spread = maxDistance * 2;
        return spread;
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
    public async Task ResetText(bool bWait)
    {
        var LabelsContainer = ProgressStackPanel.Children.OfType<ProgressLabel>().ToList();
        if (LabelsContainer.Count == 0)
            return;

        LabelsContainer.ForEach(x => x.TriggerDespawnAnim());
        if (bWait)
            await Task.Delay(200);
        ProgressStackPanel.Children.Clear();
    }

    /// <summary>
    /// Auto layout the tree, allocating double the space for every next row. Does not account for empty branches.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tree">The tree</param>
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

}
