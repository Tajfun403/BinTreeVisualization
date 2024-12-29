using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
/// A label that can represent progress which supports spawn/despawn animations and colors.
/// </summary>
public partial class ProgressLabel : Label
{
    public ProgressLabel()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Trigger spawn fade-in animation.
    /// </summary>
    public void TriggerSpawnAnim()
    {
        BeginStoryboard((Storyboard)FindResource("AnimSpawn"));
    }

    /// <summary>
    /// Trigger despawn fade-out animation.
    /// </summary>
    public void TriggerDespawnAnim()
    {
        BeginStoryboard((Storyboard)FindResource("AnimDespawn"));
    }

}
