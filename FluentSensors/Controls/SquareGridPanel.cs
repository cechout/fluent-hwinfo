using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Foundation;


namespace FluentSensors.Controls
{
    // arranges its children into a grid that grows roughly square as the child count grows (1-3 children get one row, 4-8
    // get two rows, 9-15 get three, and so on; a new row starts exactly at each perfect square), but caps the column count
    // once the available width gets too narrow for that many columns to stay usable
    // rows are never capped, they simply grow to absorb whatever the capped columns cant hold
    // all cells are the same size; children fill left-to-right, top-to-bottom, and the last row simply ends early if it isnt
    // completely full
    public class SquareGridPanel : Panel
    {
        // threshold width for 1 graph
        private const double MinCellWidth = 120;

        protected override Size MeasureOverride(Size availableSize)
        {
            int count = Children.Count;
            if (count == 0) return new Size(0, 0);

            // ItemsControl wraps this panel in a ScrollViewer by default, which measures its content with an effectively
            // infinite height to determine how much there is to scroll
            // a ScrollViewer cannot cope with an infinite DesiredSize coming back and silently fails to render anything, so
            // this is clamped to 0 here
            // the real, finite size arrives in ArrangeOverride once the Grid has resolved its actual star-row height, which
            // is what the cells actually get sized to
            double measureWidth = double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width;
            double measureHeight = double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height;

            var (rows, columns) = GetGridSize(count, measureWidth);
            var cellSize = new Size(measureWidth / columns, measureHeight / rows);
            foreach (var child in Children)
            {
                child.Measure(cellSize);
            }

            return new Size(measureWidth, measureHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            int count = Children.Count;
            if (count == 0) return finalSize;

            var (rows, columns) = GetGridSize(count, finalSize.Width);
            double cellWidth = finalSize.Width / columns;
            double cellHeight = finalSize.Height / rows;

            for (int i = 0; i < count; i++)
            {
                int row = i / columns;
                int column = i % columns;

                Children[i].Arrange(new Rect(column * cellWidth, row * cellHeight, cellWidth, cellHeight));
            }

            return finalSize;
        }

        // starts from the "roughly square" column count for this child count, then caps it down if the available width cant
        // actually fit that many columns at MinCellWidth each
        // rows simply grow to absorb whatever the capped columns cant hold, so they are never capped themselves
        private static (int rows, int columns) GetGridSize(int count, double availableWidth)
        {
            int idealRows = (int)Math.Floor(Math.Sqrt(count));
            int idealColumns = (int)Math.Ceiling(count / (double)idealRows);

            int maxColumnsForWidth = availableWidth > 0
                ? Math.Max(1, (int)Math.Floor(availableWidth / MinCellWidth))
                : 1;

            int columns = Math.Min(idealColumns, maxColumnsForWidth);
            int rows = (int)Math.Ceiling(count / (double)columns);

            return (rows, columns);
        }
    }
}