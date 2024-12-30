using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using BinTreeVisualization.UI;

namespace BinTreeVisualization.Algorithms;

public partial class Node<T> where T : IComparable<T>
{
    /// <summary>
    /// Backing element placed in the UI
    /// </summary>
    public required NodeControl BackingControl { get; init; }

    /// <summary>
    /// UI arrow pointing to this node
    /// </summary>
    public NodeArrow? SelfArrow { get; private set; } = null;

    /// <summary>
    /// Offset by which nodes should be separated vertically.
    /// </summary>
    public const int ToBottomOffset = 70;
    // public const int ToBottomOffset = 70;
    // public const int ToSideOffset = 150;
    /// <summary>
    /// Offset by which children nodes should be put to side by from their parent nodes horizontally..
    /// </summary>
    public const int ToSideOffset = 100;

    /// <summary>
    /// Current width of the UI control.
    /// </summary>
    public double CurrWidth => BackingControl.Width;

    /// <summary>
    /// Current height of the UI control.
    /// </summary>
    public double CurrHeight => BackingControl.Height;

    /// <summary>
    /// Node width
    /// </summary>
    public const double NodeWidth = 120;

    /// <summary>
    /// Node height.
    /// </summary>
    public const double NodeHeight = 70;

    /// <summary>
    /// Desired location of the node. Might be either the current location, or the location the new will finish at once its animation end. <para/>
    /// Use <see cref="MoveToLoc(Point)"/> to move the node to a new location.
    /// </summary>
    public Point DesiredLoc { get; private set; }

    /// <summary>
    /// Current location of the node in the UI. Might be volatile because it is affected by animations.<para/>
    /// Use <see cref="DesiredLoc"/> for location-related operations.
    /// </summary>
    public Point CurrLoc => GetLocOf(BackingControl);

    /// <summary>
    /// Get location of a specified control
    /// </summary>
    /// <param name="control">The control to get location of</param>
    /// <returns>The location of the specified control</returns>
    private static Point GetLocOf(NodeControl control) => new(Canvas.GetLeft(control), Canvas.GetTop(control));

    /// <summary>
    /// Refresh the arrow pointing to this node to account for changes in parents
    /// </summary>
    public void RefreshSelfArrow()
    {
        if (Parent is null)
        {
            SelfArrow?.RemoveSelf();
            SelfArrow = null;
            return;
        }
        else if (SelfArrow is null)
        {
            SelfArrow = Parent.CreateArrowTo(this);
        }
        SelfArrow?.MoveTargetToLoc(DesiredLoc);
        SelfArrow?.MoveSourceToLoc(Parent.DesiredLoc);
    }

    /// <summary>
    /// Create a new UI element for a node and add it to the <see cref="Canvas"/>
    /// </summary>
    /// <param name="value">Value of the new node</param>
    /// <param name="bLeft">Whether to prepare the child on parent's left side.</param>
    /// <returns>The created UI element.</returns>
    private NodeControl CreateUIElem(T value, bool bLeft)
    {
        NodeControl el = new(value);
        Point oldLoc = DesiredLoc;
        Point newLoc = new(oldLoc.X + (bLeft ? -ToSideOffset : ToSideOffset), oldLoc.Y + ToBottomOffset);
        Canvas.SetLeft(el, newLoc.X);
        Canvas.SetTop(el, newLoc.Y);
        Tree.GetCanvas().Children.Add(el);
        return el;
    }

    /// <summary>
    /// Create a UI arrow from this node pointing to another node, add it to the <see cref="Canvas"/>, and return it.
    /// </summary>
    /// <param name="node">The node the arrow should point to</param>
    /// <returns>The created UI element.</returns>
    private NodeArrow CreateArrowTo(Node<T> node)
    {
        NodeArrow arrow = new();
        arrow.Target = node.DesiredLoc;
        arrow.Source = this.DesiredLoc;

        Canvas.SetLeft(arrow, this.DesiredLoc.X);
        Canvas.SetTop(arrow, this.DesiredLoc.Y);

        Tree.GetCanvas().Children.Add(arrow);
        return arrow;
    }

    /// <inheritdoc cref="NodeControl.Activate"/>
    public void Activate()
    {
        BackingControl.Activate();
    }

    /// <inheritdoc cref="NodeControl.Deactivate"/>
    public void Deactivate()
    {
        BackingControl.Deactivate();
    }

    /// <inheritdoc cref="NodeControl.HighlightBlue"/>
    public void HighlightBlue()
    {
        BackingControl.HighlightBlue();
    }

    /// <summary>
    /// Change node's fill color to blue with an animation.
    /// </summary>
    /// <param name="bRecursive">Whether to color the node itself, or the node and all its children as well</param>
    public void ActivateBlue(bool bRecursive = false)
    {
        if (bRecursive)
        {
            Traverse().ToList().ForEach(x => x.ActivateBlue());
        }
        else
            BackingControl.ActivateBlue();
    }

    /// <summary>
    /// Reset node's fill color.
    /// </summary>
    /// <param name="bRecursive">Whether to blink the node itself, or the node and all its children as well</param>
    public void DeactivateBlue(bool bRecursive = false)
    {
        if (bRecursive)
        {
            Traverse().ToList().ForEach(x => x.DeactivateBlue());
        }
        else
            BackingControl.DeactivateBlue();
    }

    /// <summary>
    /// Blink the node's UI control in blue
    /// </summary>
    /// <param name="bRecursive">Whether to blink the node itself, or the node and all its children as well</param>
    public void Blink(bool bRecursive = false)
    {
        if (Tree.bSkipAnimations)
            return;
        if (bRecursive)
            Traverse().ToList().ForEach(x => x.Blink());
        else
            BackingControl.Blink();
    }

    /// <summary>
    /// Move the associated node control to a location and make its all its children follow it.
    /// </summary>
    /// <param name="loc">Final location to move the node to</param>
    /// <param name="bDelayAnimation">If <see langword="true"/>, cache the desired location and delay execution of the animation
    /// until <see cref="PlayDelayedAnimation"/> is called.</param>
    public void MoveTreeToLoc(Point loc, bool bDelayAnimation = false)
    {
        var LocDelta = new Point(loc.X - DesiredLoc.X, loc.Y - DesiredLoc.Y);
        Traverse().ToList().ForEach(x => x.MoveByLoc(LocDelta, bDelayAnimation));
    }

    /// <summary>
    /// Move the associated node control by a delta location and make its all its children follow it.
    /// </summary>
    /// <param name="deltaLoc">Delta location to move the tree by</param>
    /// <param name="bDelayAnimation">If <see langword="true"/>, cache the desired location and delay execution of the animation
    /// until <see cref="PlayDelayedAnimation"/> is called.</param>
    public void MoveTreeByLoc(Point deltaLoc, bool bDelayAnimation = false)
    {
        Traverse().ToList().ForEach(x => x.MoveByLoc(deltaLoc, bDelayAnimation));
    }

    /// <summary>
    /// Move all the children of this node (but NOT this node itself) by a delta location.
    /// </summary>
    /// <param name="deltaLoc">Delta location to move the tree by</param>
    /// <param name="bDelayAnimation">If <see langword="true"/>, cache the desired location and delay execution of the animation 
    /// until <see cref="PlayDelayedAnimation"/> is called.</param>
    public void MoveChildrenByLoc(Point deltaLoc, bool bDelayAnimation = false)
    {
        // for some reason Traverse().Where(x => x != this) throws type constraint exception
        Left?.Traverse().ToList().ForEach(x => x.MoveByLoc(deltaLoc, bDelayAnimation));
        Right?.Traverse().ToList().ForEach(x => x.MoveByLoc(deltaLoc, bDelayAnimation));
    }

    /// <summary>
    /// Move the associated node control by a specified amount
    /// </summary>
    /// <param name="loc">Final location to move the node to</param>
    /// <param name="bDelayAnimation">If <see langword="true"/>, cache the desired location and delay execution of the animation 
    /// until <see cref="PlayDelayedAnimation"/> is called.</param>
    public void MoveByLoc(Point loc, bool bDelayAnimation = false)
    {
        var newLoc = new Point(DesiredLoc.X + loc.X, DesiredLoc.Y + loc.Y);
        MoveToLoc(newLoc);
    }

    /// <summary>
    /// Move the associated node control to specified location over 0.5 seconds
    /// </summary>
    /// <param name="loc">Location to move to</param>
    /// <param name="bDelayAnimation">If <see langword="true"/>, cache the desired location and delay execution of the animation
    /// until <see cref="PlayDelayedAnimation"/> is called.</param>
    public void MoveToLoc(Point loc, bool bDelayAnimation = false)
    {
        if (loc == DesiredLoc)
            return;

        if (Tree.bSkipAnimations || bDelayAnimation)
        {
            Debug.WriteLine($"Caching movement of {{{this}}} from {DesiredLoc} to {loc}");
            DesiredLoc = loc;
            if (!HasCachedDesiredLoc)
            {
                HasCachedDesiredLoc = true;
                Tree.OnInstantModeFinished += PlayDelayedAnimation;
            }
        }
        else
        {
            MoveToLocWithAnim(loc);
        }
    }

    /// <summary>
    /// Whether the node has cached loc that it should move to with <see cref="PlayDelayedAnimation"/> once available.
    /// </summary>
    private bool HasCachedDesiredLoc { get; set; } = false;

    /// <summary>
    /// Move the associated node control to specified location over 0.5 seconds. Always play the animation - regardless of instant mode or others.
    /// </summary>
    /// <param name="loc">Location to move to</param>
    /// <param name="bDelayAnimation">If <see langword="true"/>, cache the desired location and delay execution of the animation until <see cref="PlayDelayedAnimation"/> is called.</param>
    public void MoveToLocWithAnim(Point loc, bool bDelayAnimation = false)
    {
        Debug.WriteLine($"Moving {{{this}}} from {DesiredLoc} to {loc}");
        DesiredLoc = loc;
        BackingControl.MoveToLoc(loc);
        RefreshSelfArrow();
        Left?.RefreshSelfArrow();
        Right?.RefreshSelfArrow();
    }

    /// <summary>
    /// Play cached animation now. Reset the cache state.
    /// </summary>
    public void PlayDelayedAnimation()
    {
        if (!HasCachedDesiredLoc)
            return;
        MoveToLoc(DesiredLoc);
        HasCachedDesiredLoc = false;
        Tree.OnInstantModeFinished -= PlayDelayedAnimation;
    }
}
