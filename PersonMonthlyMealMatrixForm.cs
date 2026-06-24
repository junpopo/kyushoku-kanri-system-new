namespace KyushokuKanriSystem;

public sealed class PersonMonthlyMealMatrixForm : Form
{
    private readonly DateTime _month;
    private readonly Person _person;
    private readonly IReadOnlyCollection<MealRecord> _mealRecords;
    private readonly DataGridView _grid = new();

    public PersonMonthlyMealMatrixForm(
        DateTime month,
        Person person,
        IReadOnlyCollection<MealRecord> mealRecords)
    {
        _month = new DateTime(month.Year, month.Month, 1);
        _person = person;
        _mealRecords = mealRecords;

        Text = "月間喫食状況";
        Width = 1320;
        Height = 310;
        MinimumSize = new Size(1000, 280);
        StartPosition = FormStartPosition.CenterParent;

        Controls.Add(CreateLayout());
        BuildMatrix();
    }

    private Control CreateLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var title = new Label
        {
            Text = $"{_month:yyyy年M月}  {_person.TypeLabel}  {_person.FullName}",
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        };
        var legend = new Label
        {
            Text = "○ 喫食　停 停止　欠 欠席　－ 非喫食日　外 在籍期間外",
            AutoSize = true,
            ForeColor = Color.FromArgb(55, 65, 75),
            Margin = new Padding(0, 0, 0, 8)
        };

        ConfigureGrid();

        var closePanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 8, 0, 0)
        };
        var close = new Button
        {
            Text = "閉じる",
            DialogResult = DialogResult.OK,
            AutoSize = true,
            Padding = new Padding(16, 5, 16, 5)
        };
        closePanel.Controls.Add(close);
        AcceptButton = close;
        CancelButton = close;

        root.Controls.Add(title, 0, 0);
        root.Controls.Add(legend, 0, 1);
        root.Controls.Add(_grid, 0, 2);
        root.Controls.Add(closePanel, 0, 3);
        return root;
    }

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.CellSelect;
        _grid.MultiSelect = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _grid.ColumnHeadersHeight = 42;
        _grid.RowTemplate.Height = 38;
        _grid.BackgroundColor = Color.White;
        _grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
    }

    private void BuildMatrix()
    {
        _grid.Columns.Clear();
        _grid.Rows.Clear();

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "氏名",
            Width = 120,
            Frozen = true
        });

        var daysInMonth = DateTime.DaysInMonth(_month.Year, _month.Month);
        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(_month.Year, _month.Month, day);
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = $"{day}\n{DayLabel(date.DayOfWeek)}",
                Width = 35,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
        }

        var values = new object[daysInMonth + 1];
        values[0] = _person.FullName;
        for (var day = 1; day <= daysInMonth; day++)
        {
            values[day] = StatusLabel(new DateTime(_month.Year, _month.Month, day));
        }

        var row = _grid.Rows[_grid.Rows.Add(values)];
        row.Cells[0].Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        row.Cells[0].Style.Font = new Font(_grid.Font, FontStyle.Bold);
        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(_month.Year, _month.Month, day);
            StyleStatusCell(row.Cells[day], date);
        }
    }

    private string StatusLabel(DateTime date)
    {
        if (!IsActive(date))
        {
            return "外";
        }

        var record = _mealRecords.FirstOrDefault(item =>
            item.PersonId == _person.Id && item.Date.Date == date.Date);
        if (record is not null)
        {
            return record.Status switch
            {
                MealStatus.Serve => "○",
                MealStatus.Absent => "欠",
                _ => "停"
            };
        }

        return _person.EatsOn(date.DayOfWeek) ? "○" : "－";
    }

    private void StyleStatusCell(DataGridViewCell cell, DateTime date)
    {
        var label = Convert.ToString(cell.Value) ?? "";
        cell.ToolTipText = $"{date:yyyy年M月d日} {FullStatusLabel(label)}";
        switch (label)
        {
            case "○":
                cell.Style.BackColor = Color.FromArgb(220, 242, 228);
                cell.Style.ForeColor = Color.FromArgb(24, 105, 58);
                break;
            case "停":
                cell.Style.BackColor = Color.FromArgb(255, 235, 200);
                cell.Style.ForeColor = Color.FromArgb(145, 78, 0);
                break;
            case "欠":
                cell.Style.BackColor = Color.FromArgb(255, 218, 218);
                cell.Style.ForeColor = Color.FromArgb(155, 35, 35);
                break;
            default:
                cell.Style.BackColor = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday
                    ? Color.FromArgb(235, 235, 235)
                    : Color.FromArgb(245, 245, 245);
                cell.Style.ForeColor = Color.Gray;
                break;
        }
    }

    private bool IsActive(DateTime date)
    {
        return _person.ActiveFrom.Date <= date.Date &&
               (_person.ActiveTo is null || _person.ActiveTo.Value.Date >= date.Date);
    }

    private static string FullStatusLabel(string label)
    {
        return label switch
        {
            "○" => "喫食",
            "停" => "停止",
            "欠" => "欠席",
            "外" => "在籍期間外",
            _ => "非喫食日"
        };
    }

    private static string DayLabel(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => "月",
            DayOfWeek.Tuesday => "火",
            DayOfWeek.Wednesday => "水",
            DayOfWeek.Thursday => "木",
            DayOfWeek.Friday => "金",
            DayOfWeek.Saturday => "土",
            _ => "日"
        };
    }
}
