using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ScottPlot;
using ScottPlot.WPF;

namespace BinTreeVisualization.Stats;

/// <summary>
/// Holds statistics of operations performed on a binary tree.
/// </summary>
public class TreeStats
{
    private List<OperationStats> InsertStats = [];
    private List<OperationStats> DeleteStats = [];
    private List<OperationStats> SearchStats = [];

    /// <summary>
    /// Add stats performed during a specified operation
    /// </summary>
    /// <param name="stats">The stats to add</param>
    /// <param name="type">The type of the operation</param>
    public void AddStats(OperationStats stats, OperationType type)
    {
        GetList(type).Add(stats);
        OnDataChanged?.Invoke();
    }

    /// <summary>
    /// Get list associated with the specified operation type
    /// </summary>
    /// <param name="type">The operation type</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private List<OperationStats> GetList(OperationType type)
    {
        return type switch
        {
            OperationType.Insert => InsertStats,
            OperationType.Delete => DeleteStats,
            OperationType.Search => SearchStats,
            _ => throw new ArgumentException("Invalid operation type")
        };
    }

    /// <summary>
    /// Calculate the list of statistics, with the average amount of operations per the count of items in the tree.
    /// </summary>
    /// <param name="type">The operation type to get results for</param>
    /// <returns>An <see cref="IEnumerable{T}"/> with average comparisons and travels for each recorded tree size.</returns>
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

    /// <summary>
    /// Fill the plot with the data of the specified operation type.
    /// </summary>
    /// <param name="plot">The plot</param>
    /// <param name="type">The operaion type</param>
    private void FillPlot(Plot plot, OperationType type)
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

    /// <inheritdoc cref="FillPlot(Plot, OperationType)"/>
    private void FillPlot(WpfPlot plot, OperationType type)
    {
        plot.Reset();
        FillPlot(plot.Plot, type);
        plot.Refresh();
    }

    /// <summary>
    /// Show a window with plots holding the stats.
    /// </summary>
    public void ShowWindow()
    {
        var window = new StatsWindow(this);
        Refresh(window);
        window.Show();
    }

    /// <summary>
    /// Refresh a plots window with the newest data.
    /// </summary>
    /// <param name="window"></param>
    public void Refresh(StatsWindow window)
    {
        FillPlot(window.InsertPlot, OperationType.Insert);
        FillPlot(window.DeletePlot, OperationType.Delete);
        FillPlot(window.SearchPlot, OperationType.Search);
    }

    /// <summary>
    /// Event invoked when new stats are added.
    /// </summary>
    public delegate void StatsWindowEventHandler();

    /// <summary>
    /// Event invoked when new stats are added.
    /// </summary>
    public event StatsWindowEventHandler OnDataChanged;
}

/// <summary>
/// Represents the type of operation performed on a binary tree.
/// </summary>
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

