namespace KyushokuKanriSystem;

public sealed class SearchResultMonthlyMealMatrixForm : Form
{
    private readonly DateTime _month;
    private readonly IReadOnlyCollection<Person> _people;
    private readonly Func<Person, DateTime, MealStatus> _mealStatusProvider;
    private readonly Func<Person, DateTime, string> _mealReasonProvider;
    private readonly Func<DateTime, bool> _noMealDateProvider;
    private readonly DataGridView _grid = new();

    public SearchResultMonthlyMealMatrixForm(
        DateTime month,
        IReadOnlyCollection<Person> people,
        Func<Person, DateTime, MealStatus> mealStatusProvider,
        Func<Person, DateTime, string> mealReasonProvider,
        Func<DateTime, bool> noMealDateProvider)
    {
        _month = new DateTime(month.Year, month.Month, 1);
        _people = people;
        _mealStatusProvider = mealStatusProvider;
        _mealReasonProvider = mealReasonProvider;
        _noMealDateProvider = noMealDateProvider;

        Text = "検索結果 月間喫食状況";
        Width = 1420;
        Height = 700;
        MinimumSize = new Size(1250, 520);
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
            Text = $"{_month:yyyy年M月}　検索結果: {_people.Count}人",
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        };
        var legend = new Label
        {
            Text = "○：喫食　✕：停止　－：土日・非喫食日　外：在籍期間外",
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
        var print = new Button
        {
            Text = "印刷",
            AutoSize = true,
            Padding = new Padding(16, 5, 16, 5)
        };
        print.Click += (_, _) => PrintGrid();
        closePanel.Controls.Add(print);
        closePanel.Controls.Add(close);

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
        _grid.ScrollBars = ScrollBars.Vertical;
        _grid.ColumnHeadersHeight = 42;
        _grid.RowTemplate.Height = 28;
        _grid.BackgroundColor = Color.White;
        _grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
    }

    private void PrintGrid()
    {
        GridPrintHelper.ShowPreview(
            this,
            Text,
            _grid,
            $"{_month:yyyy年M月}  検索結果: {_people.Count}人");
    }

    private void BuildMatrix()
    {
        AddColumn("区分", 58, frozen: true);
        AddColumn("学年", 38, frozen: true);
        AddColumn("組", 34, frozen: true);
        AddColumn("番号", 43, frozen: true);
        AddColumn("氏名", 110, frozen: true);

        var daysInMonth = DateTime.DaysInMonth(_month.Year, _month.Month);
        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(_month.Year, _month.Month, day);
            var column = new DataGridViewTextBoxColumn
            {
                HeaderText = $"{day}\n{DayLabel(date.DayOfWeek)}",
                Width = 34,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Tag = date
            };
            if (_noMealDateProvider(date))
            {
                column.HeaderCell.Style.ForeColor = Color.Firebrick;
                column.HeaderCell.Style.Font = new Font(_grid.Font, FontStyle.Bold);
            }

            _grid.Columns.Add(column);
        }

        foreach (var person in _people)
        {
            var values = new object[daysInMonth + 5];
            values[0] = person.TypeLabel;
            values[1] = person.Grade;
            values[2] = person.ClassName;
            values[3] = person.StudentNumber;
            values[4] = person.FullName;
            for (var day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(_month.Year, _month.Month, day);
                values[day + 4] = StatusLabel(person, date);
            }

            var row = _grid.Rows[_grid.Rows.Add(values)];
            for (var day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(_month.Year, _month.Month, day);
                StyleStatusCell(row.Cells[day + 4], person, date);
            }
        }
    }

    private string StatusLabel(Person person, DateTime date)
    {
        if (!IsActive(person, date))
        {
            return "外";
        }

        if (_noMealDateProvider(date) ||
            date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            return "－";
        }

        var status = _mealStatusProvider(person, date);
        return status switch
        {
            MealStatus.Serve => "○",
            MealStatus.Absent => "✕",
            _ when !person.EatsOn(date.DayOfWeek) => "－",
            _ => "✕"
        };
    }

    private void StyleStatusCell(
        DataGridViewCell cell,
        Person person,
        DateTime date)
    {
        var label = Convert.ToString(cell.Value) ?? "";
        var reason = label is "✕"
            ? _mealReasonProvider(person, date)
            : "";
        if (_noMealDateProvider(date))
        {
            reason = _mealReasonProvider(person, date);
        }

        cell.ToolTipText = $"{date:yyyy年M月d日}: {StatusText(label)}" +
                           (reason.Length > 0 ? $"　理由: {reason}" : "");
        switch (label)
        {
            case "○":
                cell.Style.BackColor = Color.FromArgb(220, 242, 228);
                cell.Style.ForeColor = Color.FromArgb(24, 105, 58);
                break;
            case "✕":
                cell.Style.BackColor = Color.FromArgb(255, 235, 200);
                cell.Style.ForeColor = Color.FromArgb(145, 78, 0);
                break;
            default:
                cell.Style.BackColor = Color.FromArgb(240, 240, 240);
                cell.Style.ForeColor = Color.Gray;
                break;
        }
    }

    private void AddColumn(string header, int width, bool frozen)
    {
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = header,
            Width = width,
            Frozen = frozen,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
    }

    private static bool IsActive(Person person, DateTime date)
    {
        return person.ActiveFrom.Date <= date.Date &&
               (person.ActiveTo is null || person.ActiveTo.Value.Date >= date.Date);
    }

    private static string StatusText(string label)
    {
        return label switch
        {
            "○" => "喫食",
            "✕" => "停止",
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
