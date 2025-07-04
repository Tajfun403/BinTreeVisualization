using BinTreeVisualization.UI;
using OpenTK.Graphics.ES30;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BinTreeVisualization.Algorithms;

internal class TreeAnims<T> where T : IComparable<T>
{
    public BinTree<T> Tree { get; set; }

    private Canvas Canvas => Tree.GetCanvas();

    public void HighlightSubtree(Node<T> parent)
    {
        var childrenRows = parent.GetChildrenRows();
        List < (int tier, double leftX, double rightX, double Y) > rowBorders = [];

        double topX = parent.DesiredLoc.X + Node<T>.NodeWidth / 2;
        double topY = parent.DesiredLoc.Y + Node<T>.NodeHeight / 2;

        List<Point> leftOutline = [];
        LinkedList<Point> rightOutline = [];
        // close the shape
        rightOutline.AddFirst(new Point(topX, topY));

        foreach (var (tier, rows) in childrenRows)
        {
            var leftNode = rows.First();
            var rightNode = rows.Last();

            double leftX = leftNode.DesiredLoc.X;
            double rightX = rightNode.DesiredLoc.X + Node<T>.NodeWidth;
            double YLoc = leftNode.DesiredLoc.Y + Node<T>.NodeHeight / 2;

            rowBorders.Add((tier, leftX, rightX, YLoc));

            Point leftPoint = new(leftX, YLoc);
            leftOutline.Add(leftPoint);

            Point rightPoint = new(rightX, YLoc);
            rightOutline.AddFirst(rightPoint);
        }

        // https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/geometry-overview

        PathFigure pathFigure = new();
        pathFigure.StartPoint = new(topX, topY);

        IEnumerable<Point> allPoints = leftOutline.Concat(rightOutline);
        if (false && allPoints.Count() > 10)
            Debugger.Break();
        Debug.WriteLine($"Creating outline with {allPoints.Count()} vertices");
        foreach(var point in allPoints)
        {
            pathFigure.Segments.Add(
                    new LineSegment(
                        point, true)
                );
        }

        PathGeometry pathGeometry = new();
        pathGeometry.Figures.Add(pathFigure);

        Path path = new();
        path.Fill = new SolidColorBrush(TextActionColorsHelper.GetColor(TextAction.Blink));
        path.Stroke = new SolidColorBrush(TextActionColorsHelper.GetColor(TextAction.Blink));
        path.StrokeThickness = 10; 

        path.HorizontalAlignment = HorizontalAlignment.Left;
        path.VerticalAlignment = VerticalAlignment.Top;
        path.Data = pathGeometry;

        Canvas.SetLeft(path, 0);
        Canvas.SetRight(path, 0);
        Tree.GetCanvas().Children.Add(path);
        Panel.SetZIndex(path, -100);
        Debug.WriteLine($"Curr children count: {Tree.GetCanvas().Children.Count}");
    }
}
