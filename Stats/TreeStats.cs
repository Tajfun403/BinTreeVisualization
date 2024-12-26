using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ScottPlot;
using ScottPlot.WPF;

namespace BinTreeVisualization.Stats;

public class TreeStats
{
    List<OperationStats> InsertStats = [];
    List<OperationStats> DeleteStats = [];
    List<OperationStats> SearchStats = [];

    public void AddStats(OperationStats stats, OperationType type)
    {
        GetList(type).Add(stats);
        OnDataChanged?.Invoke();
    }

    public List<OperationStats> GetList(OperationType type)
    {
        return type switch
        {
            OperationType.Insert => InsertStats,
            OperationType.Delete => DeleteStats,
            OperationType.Search => SearchStats,
            _ => throw new ArgumentException("Invalid operation type")
        };
    }

    public IEnumerable<OperationStats> GetListWithAverages(OperationType type)
    {
        var list = GetList(type);
        var grouped = list.GroupBy(x => x.TreeSize);
        return grouped.Select(x => new OperationStats(
            Comparisons: x.Average(s => s.Comparisons),
            Traversals: x.Average(s => s.Traversals),
            TreeSize: x.Key
        ));
    }

    public void FillPlot(Plot plot, OperationType type)
    {
        Color white = Colors.LightGray;
        // Color white = new ScottPlot.Color("#EEE");
        plot.DataBackground.Color = new ScottPlot.Color("#333");
        plot.FigureBackground.Color = new ScottPlot.Color("#333");
        plot.Grid.MajorLineColor = new ScottPlot.Color("#EEE");

        plot.Axes.Color(white);
        plot.Axes.Left.Label.ForeColor = white;
        plot.Axes.Bottom.Label.ForeColor = white;
        plot.Axes.Title.Label.ForeColor = white;

        plot.XLabel("Tree size");
        plot.YLabel("Operations count");
        plot.Title($"{type} operations");
        var data = GetListWithAverages(type);

        plot.Clear();
        var n = data.Select(s => s.TreeSize);
        var traversals = data.Select(s => s.Traversals);
        var comparisons = data.Select(s => s.Comparisons);

        var traversalsLine = plot.Add.Scatter(n.ToArray(), traversals.ToArray());
        traversalsLine.LegendText = "Traversals";

        var comparisonsLine = plot.Add.Scatter(n.ToArray(), comparisons.ToArray());
        comparisonsLine.LegendText = "Comparisons";

        plot.ShowLegend();
    }

    public void FillPlot(WpfPlot plot, OperationType type)
    {
        plot.Reset();
        FillPlot(plot.Plot, type);
        plot.Refresh();
    }

    public void ShowWindow()
    {
        var window = new StatsWindow(this);
        FillPlot(window.InsertPlot, OperationType.Insert);
        FillPlot(window.DeletePlot, OperationType.Delete);
        FillPlot(window.SearchPlot, OperationType.Search);
        window.Show();
    }
    public void Refresh(StatsWindow window)
    {
        FillPlot(window.InsertPlot, OperationType.Insert);
        FillPlot(window.DeletePlot, OperationType.Delete);
        FillPlot(window.SearchPlot, OperationType.Search);
    }

    public delegate void StatsWindowEventHandler();
    public event StatsWindowEventHandler OnDataChanged;
}

public enum OperationType
{
    Insert,
    Delete,
    Search,
    Discard
}

/// <summary>
/// Represents the statistics of a single operation.
/// </summary>
/// <param name="Comparisons">The amount of comparisons between nodes' values</param>
/// <param name="Traversals">The amount of travels from one node to another</param>
/// <param name="TreeSize">The count of items in the tree at the beginning of the operation</param>
public record struct OperationStats(double Comparisons, double Traversals, int TreeSize);

