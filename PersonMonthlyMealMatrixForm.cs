namespace KyushokuKanriSystem;

public sealed class PersonMonthlyMealMatrixForm : Form
{
    private readonly DateTime _month;
    private readonly Person _person;
    private readonly Func<DateTime, MealStatus> _mealStatusProvider;
    private readonly Func<DateTime, string> _mealReasonProvider;
    private readonly DataGridView _grid = new();
    private readonly Label _reasonLabel = new();

    public PersonMonthlyMealMatrixForm(
        DateTime month,
        Person person,
        Func<DateTime, MealStatus> mealStatusProvider,
        Func<DateTime, string> mealReasonProvider)
    {
        _month = new DateTime(month.Year, month.Month, 1);
        _person = person;
        _mealStatusProvider = mealStatusProvider;
        _mealReasonProvider = mealReasonProvider;

        Text = "月間喫食状況";
        Width = 1320;
        Height = 560;
        MinimumSize = new Size(1000, 500);
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
            RowCount = 5,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
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
        _reasonLabel.Text = "停止・欠席のセルをクリックすると理由を表示します。";
        _reasonLabel.AutoSize = true;
        _reasonLabel.ForeColor = Color.FromArgb(75, 65, 45);
        _reasonLabel.Padding = new Padding(4, 8, 4, 4);

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
        root.Controls.Add(_reasonLabel, 0, 3);
        root.Controls.Add(closePanel, 0, 4);
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
        _grid.CellClick += (_, eventArgs) => ShowCellReason(
            eventArgs.RowIndex,
            eventArgs.ColumnIndex);
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
        var reasonValues = new object[daysInMonth + 1];
        mealValues[0] = "給食";
        milkValues[0] = "牛乳";
        reasonValues[0] = "理由";
        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(_month.Year, _month.Month, day);
            mealValues[day] = MealStatusLabel(date);
            milkValues[day] = MilkStatusLabel(date);
            reasonValues[day] = VerticalReason(date);
        }

        var mealRow = _grid.Rows[_grid.Rows.Add(mealValues)];
        var milkRow = _grid.Rows[_grid.Rows.Add(milkValues)];
        var reasonRow = _grid.Rows[_grid.Rows.Add(reasonValues)];
        mealRow.Cells[0].Style.Font = new Font(_grid.Font, FontStyle.Bold);
        milkRow.Cells[0].Style.Font = new Font(_grid.Font, FontStyle.Bold);
        reasonRow.Cells[0].Style.Font = new Font(_grid.Font, FontStyle.Bold);
        reasonRow.Height = 190;
        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(_month.Year, _month.Month, day);
            StyleStatusCell(mealRow.Cells[day], date, "給食");
            StyleStatusCell(milkRow.Cells[day], date, "牛乳");
            StyleReasonCell(reasonRow.Cells[day], date);
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
        var reason = StatusReason(date, label);
        cell.ToolTipText = $"{date:yyyy年M月d日} {item}: {FullStatusLabel(label, item)}" +
                           (reason.Length > 0 ? $"　理由: {reason}" : "");
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

    private void ShowCellReason(int rowIndex, int columnIndex)
    {
        if (rowIndex < 0 || columnIndex <= 0)
        {
            return;
        }

        var date = new DateTime(_month.Year, _month.Month, columnIndex);
        var item = rowIndex switch
        {
            0 => "給食",
            1 => "牛乳",
            _ => "理由"
        };
        var label = Convert.ToString(_grid.Rows[rowIndex].Cells[columnIndex].Value) ?? "";
        var reason = rowIndex == 2
            ? _mealReasonProvider(date)
            : StatusReason(date, label);
        if (rowIndex == 2)
        {
            _reasonLabel.Text = reason.Length > 0
                ? $"{date:yyyy年M月d日}　理由: {reason}"
                : $"{date:yyyy年M月d日}　理由なし";
            return;
        }

        _reasonLabel.Text = reason.Length > 0
            ? $"{date:yyyy年M月d日}　{item}: {FullStatusLabel(label, item)}　理由: {reason}"
            : $"{date:yyyy年M月d日}　{item}: {FullStatusLabel(label, item)}";
    }

    private string VerticalReason(DateTime date)
    {
        if (!IsActive(date))
        {
            return "";
        }

        var status = _mealStatusProvider(date);
        if (status is not (MealStatus.Stop or MealStatus.Absent) ||
            date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            return "";
        }

        var reason = _mealReasonProvider(date).Trim();
        var verticalReason = reason
            .Replace("（", "", StringComparison.Ordinal)
            .Replace("）", "", StringComparison.Ordinal)
            .Replace("(", "", StringComparison.Ordinal)
            .Replace(")", "", StringComparison.Ordinal);
        return verticalReason.Length == 0
            ? ""
            : string.Join("\n", verticalReason.ToCharArray());
    }

    private void StyleReasonCell(DataGridViewCell cell, DateTime date)
    {
        var hasReasonDisplay = Convert.ToString(cell.Value)?.Length > 0;
        var reason = hasReasonDisplay ? _mealReasonProvider(date).Trim() : "";
        cell.Style.ForeColor = Color.Firebrick;
        cell.Style.Font = new Font(_grid.Font.FontFamily, 8, FontStyle.Bold);
        cell.Style.Alignment = DataGridViewContentAlignment.TopCenter;
        cell.Style.WrapMode = DataGridViewTriState.True;
        cell.Style.BackColor = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday
            ? Color.FromArgb(235, 235, 235)
            : Color.FromArgb(255, 248, 248);
        if (reason.Length > 0)
        {
            cell.ToolTipText = $"{date:yyyy年M月d日}　理由: {reason}";
        }
    }

    private string StatusReason(DateTime date, string label)
    {
        return label is "✕" or "欠"
            ? _mealReasonProvider(date)
            : "";
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
