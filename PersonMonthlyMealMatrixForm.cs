namespace KyushokuKanriSystem;

public sealed class PersonMonthlyMealMatrixForm : Form
{
    private readonly DateTime _month;
    private readonly Person _person;
    private readonly Func<DateTime, MealStatus> _mealStatusProvider;
    private readonly DataGridView _grid = new();

    public PersonMonthlyMealMatrixForm(
        DateTime month,
        Person person,
        Func<DateTime, MealStatus> mealStatusProvider)
    {
        _month = new DateTime(month.Year, month.Month, 1);
        _person = person;
        _mealStatusProvider = mealStatusProvider;

        Text = "月間喫食状況";
        Width = 1320;
        Height = 350;
        MinimumSize = new Size(1000, 320);
        StartPosition = FormStartPosition.CenterParent;
        ControlBox = false;

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
            Text = "給食: ○ 喫食　牛乳: ○ あり／無 なし　✕ 停止　欠 欠席　－ 非喫食日　外 在籍期間外",
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
            AutoSize = true,
            Padding = new Padding(16, 5, 16, 5)
        };
        close.Click += (_, _) => Close();
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
            HeaderText = "項目",
            Width = 70,
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

        var mealValues = new object[daysInMonth + 1];
        var milkValues = new object[daysInMonth + 1];
        mealValues[0] = "給食";
        milkValues[0] = "牛乳";
        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(_month.Year, _month.Month, day);
            mealValues[day] = MealStatusLabel(date);
            milkValues[day] = MilkStatusLabel(date);
        }

        var mealRow = _grid.Rows[_grid.Rows.Add(mealValues)];
        var milkRow = _grid.Rows[_grid.Rows.Add(milkValues)];
        mealRow.Cells[0].Style.Font = new Font(_grid.Font, FontStyle.Bold);
        milkRow.Cells[0].Style.Font = new Font(_grid.Font, FontStyle.Bold);
        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(_month.Year, _month.Month, day);
            StyleStatusCell(mealRow.Cells[day], date, "給食");
            StyleStatusCell(milkRow.Cells[day], date, "牛乳");
        }
    }

    private string MealStatusLabel(DateTime date)
    {
        if (!IsActive(date))
        {
            return "外";
        }

        var status = _mealStatusProvider(date);
        if (status != MealStatus.Serve)
        {
            return status switch
            {
                MealStatus.Absent => "欠",
                _ => "✕"
            };
        }

        return "○";
    }

    private string MilkStatusLabel(DateTime date)
    {
        var mealStatus = MealStatusLabel(date);
        return mealStatus == "○"
            ? _person.HasMilk ? "○" : "無"
            : mealStatus;
    }

    private void StyleStatusCell(DataGridViewCell cell, DateTime date, string item)
    {
        var label = Convert.ToString(cell.Value) ?? "";
        cell.ToolTipText = $"{date:yyyy年M月d日} {item}: {FullStatusLabel(label, item)}";
        switch (label)
        {
            case "○":
                cell.Style.BackColor = item == "牛乳"
                    ? Color.FromArgb(218, 235, 252)
                    : Color.FromArgb(220, 242, 228);
                cell.Style.ForeColor = item == "牛乳"
                    ? Color.FromArgb(25, 80, 135)
                    : Color.FromArgb(24, 105, 58);
                break;
            case "無":
                cell.Style.BackColor = Color.FromArgb(240, 240, 240);
                cell.Style.ForeColor = Color.FromArgb(90, 90, 90);
                break;
            case "✕":
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

    private static string FullStatusLabel(string label, string item)
    {
        return label switch
        {
            "○" => item == "牛乳" ? "あり" : "喫食",
            "無" => "なし",
            "✕" => "停止",
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
