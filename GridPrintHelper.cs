using System.Drawing.Printing;

namespace KyushokuKanriSystem;

public static class GridPrintHelper
{
    public static void ShowPreview(
        IWin32Window owner,
        string title,
        DataGridView grid,
        string? subtitle = null)
    {
        grid.EndEdit();
        var rows = grid.Rows
            .Cast<DataGridViewRow>()
            .Where(row => !row.IsNewRow && row.Visible)
            .ToList();
        if (rows.Count == 0)
        {
            MessageBox.Show("印刷するデータがありません。", title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var state = new PrintState(grid, rows);
        var document = new PrintDocument
        {
            DocumentName = title
        };
        document.DefaultPageSettings.Landscape = true;
        document.BeginPrint += (_, _) => state.Reset();
        document.PrintPage += (_, eventArgs) =>
        {
            if (eventArgs.Graphics is null)
            {
                eventArgs.HasMorePages = false;
                return;
            }

            using var titleFont = new Font("Meiryo UI", 12, FontStyle.Bold);
            using var subFont = new Font("Meiryo UI", 8);
            using var cellFont = new Font("Meiryo UI", 7);
            using var headerFont = new Font("Meiryo UI", 7, FontStyle.Bold);
            using var pen = new Pen(Color.FromArgb(170, 170, 170));
            using var headerBrush = new SolidBrush(Color.FromArgb(238, 242, 246));
            using var textBrush = new SolidBrush(Color.Black);

            var bounds = eventArgs.MarginBounds;
            var y = bounds.Top;
            var graphics = eventArgs.Graphics;
            graphics.DrawString(title, titleFont, textBrush, bounds.Left, y);
            y += titleFont.Height + 4;
            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                graphics.DrawString(subtitle, subFont, textBrush, bounds.Left, y);
                y += subFont.Height + 6;
            }

            var columns = state.Columns;
            var widths = state.ColumnWidths(bounds.Width);
            var headerHeight = 24;
            DrawRow(graphics, columns.Select(column => column.HeaderText).ToArray(), widths, bounds.Left, y, headerHeight, headerFont, textBrush, headerBrush, pen);
            y += headerHeight;

            var rowHeight = 22;
            while (state.RowIndex < rows.Count && y + rowHeight <= bounds.Bottom)
            {
                var values = columns
                    .Select(column => Convert.ToString(rows[state.RowIndex].Cells[column.Index].FormattedValue) ?? "")
                    .ToArray();
                DrawRow(graphics, values, widths, bounds.Left, y, rowHeight, cellFont, textBrush, Brushes.White, pen);
                y += rowHeight;
                state.RowIndex++;
            }

            eventArgs.HasMorePages = state.RowIndex < rows.Count;
        };

        using var preview = new PrintPreviewDialog
        {
            Document = document,
            Width = 1100,
            Height = 800,
            StartPosition = FormStartPosition.CenterParent,
            UseAntiAlias = true
        };
        preview.ShowDialog(owner);
    }

    private static void DrawRow(
        Graphics graphics,
        IReadOnlyList<string> values,
        IReadOnlyList<float> widths,
        float x,
        float y,
        float height,
        Font font,
        Brush textBrush,
        Brush backgroundBrush,
        Pen pen)
    {
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.NoWrap
        };

        for (var index = 0; index < values.Count; index++)
        {
            var rectangle = new RectangleF(x, y, widths[index], height);
            graphics.FillRectangle(backgroundBrush, rectangle);
            graphics.DrawRectangle(pen, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
            rectangle.Inflate(-3, -1);
            graphics.DrawString(values[index], font, textBrush, rectangle, format);
            x += widths[index];
        }
    }

    private sealed class PrintState
    {
        private readonly DataGridView _grid;
        private readonly List<DataGridViewRow> _rows;

        public PrintState(DataGridView grid, List<DataGridViewRow> rows)
        {
            _grid = grid;
            _rows = rows;
            Columns = _grid.Columns
                .Cast<DataGridViewColumn>()
                .Where(column => column.Visible)
                .OrderBy(column => column.DisplayIndex)
                .ToList();
        }

        public IReadOnlyList<DataGridViewColumn> Columns { get; }
        public int RowIndex { get; set; }

        public void Reset()
        {
            RowIndex = 0;
        }

        public List<float> ColumnWidths(float pageWidth)
        {
            var rawWidths = Columns
                .Select(column => Math.Max(30, column.Width))
                .ToList();
            var total = rawWidths.Sum();
            if (total <= 0)
            {
                return Columns.Select(_ => pageWidth / Math.Max(1, Columns.Count)).ToList();
            }

            var scale = pageWidth / total;
            return rawWidths.Select(width => width * scale).ToList();
        }
    }
}
