using GuiP31.Models;
using System.Linq;
using System.Windows;

namespace GuiP31
{
    public partial class StatisticsWindow : Window
    {
        public StatisticsWindow(MovieStatisticsDto statistics)
        {
            InitializeComponent();
            SummaryTextBlock.Text = $"Всего фильмов: {statistics.TotalMovies}. Средний год выпуска: {statistics.AverageYear}.";

            var scale = statistics.ByDirector.Any() ? 180.0 / statistics.ByDirector.Max(item => item.Count) : 0;
            DirectorsChartItemsControl.ItemsSource = statistics.ByDirector
                .Select(item => new DirectorChartRow
                {
                    Name = item.Name,
                    BarWidth = scale == 0 ? 24 : (int)(item.Count * scale) < 24 ? 24 : (int)(item.Count * scale),
                    Count = item.Count
                })
                .ToList();

            GenresGrid.ItemsSource = statistics.ByGenre;
        }
    }

    public class DirectorChartRow
    {
        public string Name { get; set; } = string.Empty;

        public int Count { get; set; }

        public int BarWidth { get; set; }
    }
}
