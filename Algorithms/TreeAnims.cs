using BinTreeVisualization.UI;
using OpenTK.Graphics.ES30;
using System;
using System.Collections.Generic;
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

        double topX = parent.DesiredLoc.X + parent.CurrWidth / 2;
        double topY = parent.DesiredLoc.Y + parent.CurrHeight / 2;

        List<Point> leftOutline = [];
        LinkedList<Point> rightOutline = [];

        foreach (var (tier, rows) in childrenRows)
        {
            double leftX = rows.First().DesiredLoc.X;
            double rightX = rows.Last().DesiredLoc.X + rows.Last().CurrWidth;
            double YLoc = rows.First().DesiredLoc.Y - rows.Last().CurrHeight / 2;

            rowBorders.Add((tier, leftX, rightX, YLoc));

            var leftPoint = new Point(leftX, YLoc);
            leftOutline.Add(leftPoint);

            var rightPoint = new Point(rightX, YLoc);
            rightOutline.AddFirst(rightPoint);
        }

        // https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/geometry-overview

        PathFigure pathFigure = new();
        pathFigure.StartPoint = new(topX, topY);

        IEnumerable<Point> allPoints = leftOutline.Concat(rightOutline);
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

        path.HorizontalAlignment = HorizontalAlignment.Left;
        path.VerticalAlignment = VerticalAlignment.Top;
        path.Data = pathGeometry;

        Canvas.SetLeft(path, parent.DesiredLoc.X);
        Canvas.SetRight(path, parent.DesiredLoc.Y);
        Canvas.Children.Add(path);
    }
}
