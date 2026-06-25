using System.ComponentModel;

namespace KyushokuKanriSystem;

public sealed class IndividualMonthlyMealCountForm : Form
{
    private readonly DateTime _month;
    private readonly IReadOnlyCollection<Person> _people;
    private readonly Func<Person, DateTime, MealStatus> _mealStatusProvider;
    private readonly BindingList<PersonMealCountRow> _rows = [];
    private readonly DataGridView _grid = new();
    private readonly Label _totalLabel = new();

    public IndividualMonthlyMealCountForm(
        DateTime month,
        IReadOnlyCollection<Person> people,
        Func<Person, DateTime, MealStatus> mealStatusProvider)
    {
        _month = new DateTime(month.Year, month.Month, 1);
        _people = people;
        _mealStatusProvider = mealStatusProvider;

        Text = "個人別月間喫食数";
        Width = 1120;
        Height = 650;
        MinimumSize = new Size(900, 500);
        StartPosition = FormStartPosition.CenterParent;
        ControlBox = false;

        Controls.Add(CreateLayout());
        RefreshRows();
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
            Text = $"{_month:yyyy年M月}　個人別月間喫食数",
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        };

        var top = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false
        };
        top.Controls.Add(CreateButton("更新", RefreshRows));
        top.Controls.Add(new Label
        {
            Text = "行をダブルクリックすると、日別の喫食・牛乳状況を表示します。",
            AutoSize = true,
            Padding = new Padding(8, 7, 0, 0),
            ForeColor = Color.FromArgb(65, 75, 85)
        });

        ConfigureGrid();

        var bottom = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2
        };
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _totalLabel.AutoSize = true;
        _totalLabel.Padding = new Padding(0, 8, 0, 0);
        var close = CreateButton("閉じる", Close);
        bottom.Controls.Add(_totalLabel, 0, 0);
        bottom.Controls.Add(close, 1, 0);

        root.Controls.Add(title, 0, 0);
        root.Controls.Add(top, 0, 1);
        root.Controls.Add(_grid, 0, 2);
        root.Controls.Add(bottom, 0, 3);
        return root;
    }

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.AutoGenerateColumns = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.DataSource = _rows;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "区分", DataPropertyName = nameof(PersonMealCountRow.Type), FillWeight = 65 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "学年", DataPropertyName = nameof(PersonMealCountRow.Grade), FillWeight = 40 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "組", DataPropertyName = nameof(PersonMealCountRow.ClassName), FillWeight = 40 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "番号", DataPropertyName = nameof(PersonMealCountRow.StudentNumber), FillWeight = 48 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "氏名", DataPropertyName = nameof(PersonMealCountRow.Name), FillWeight = 115 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "主な配膳場所", DataPropertyName = nameof(PersonMealCountRow.DeliveryPlace), FillWeight = 100 });
        _grid.Columns.Add(CreateCountColumn("給食数", nameof(PersonMealCountRow.MealCount)));
        _grid.Columns.Add(CreateCountColumn("牛乳数", nameof(PersonMealCountRow.MilkCount)));
        _grid.Columns.Add(CreateCountColumn("停止数", nameof(PersonMealCountRow.StopCount)));
        _grid.Columns.Add(CreateCountColumn("欠席数", nameof(PersonMealCountRow.AbsentCount)));
        _grid.CellDoubleClick += (_, eventArgs) => ShowPersonDetails(eventArgs.RowIndex);
    }

    private void RefreshRows()
    {
        _rows.Clear();
        var daysInMonth = DateTime.DaysInMonth(_month.Year, _month.Month);
        var dates = Enumerable.Range(0, daysInMonth)
            .Select(offset => _month.AddDays(offset))
            .ToList();

        foreach (var person in _people
            .Where(person => dates.Any(date => IsActive(person, date)))
            .OrderBy(person => person.Type)
            .ThenBy(person => SortNumber(person.Grade))
            .ThenBy(person => person.Grade)
            .ThenBy(person => SortNumber(person.ClassName))
            .ThenBy(person => person.ClassName)
            .ThenBy(person => SortNumber(person.StudentNumber))
            .ThenBy(person => person.FullName))
        {
            var activeDates = dates
                .Where(date => IsActive(person, date))
                .ToList();
            var statuses = activeDates
                .Select(date => new
                {
                    Date = date,
                    Status = _mealStatusProvider(person, date)
                })
                .ToList();

            _rows.Add(new PersonMealCountRow
            {
                Person = person,
                Type = person.TypeLabel,
                Grade = person.Grade,
                ClassName = person.ClassName,
                StudentNumber = person.StudentNumber,
                Name = person.FullName,
                DeliveryPlace = MainDeliveryPlace(person, activeDates),
                MealCount = statuses.Count(item => item.Status == MealStatus.Serve),
                MilkCount = statuses.Count(item =>
                    item.Status == MealStatus.Serve && person.HasMilk),
                StopCount = statuses.Count(item =>
                    person.EatsOn(item.Date.DayOfWeek) &&
                    item.Status == MealStatus.Stop),
                AbsentCount = statuses.Count(item =>
                    item.Status == MealStatus.Absent)
            });
        }

        _totalLabel.Text =
            $"人数: {_rows.Count}人　給食合計: {_rows.Sum(row => row.MealCount)}　" +
            $"牛乳合計: {_rows.Sum(row => row.MilkCount)}";
    }

    private void ShowPersonDetails(int rowIndex)
    {
        if (rowIndex < 0 ||
            _grid.Rows[rowIndex].DataBoundItem is not PersonMealCountRow row)
        {
            return;
        }

        var dialog = new PersonMonthlyMealMatrixForm(
            _month,
            row.Person,
            date => _mealStatusProvider(row.Person, date));
        dialog.Show(this);
    }

    private static string MainDeliveryPlace(Person person, IReadOnlyCollection<DateTime> dates)
    {
        return dates
            .Select(person.GetDeliveryPlace)
            .Where(place => !string.IsNullOrWhiteSpace(place))
            .GroupBy(place => place.Trim(), StringComparer.CurrentCultureIgnoreCase)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .Select(group => group.Key)
            .FirstOrDefault() ?? "未設定";
    }

    private static bool IsActive(Person person, DateTime date)
    {
        return person.ActiveFrom.Date <= date.Date &&
               (person.ActiveTo is null || person.ActiveTo.Value.Date >= date.Date);
    }

    private static int SortNumber(string value)
    {
        return int.TryParse(value, out var number) ? number : int.MaxValue;
    }

    private static DataGridViewTextBoxColumn CreateCountColumn(
        string headerText,
        string propertyName)
    {
        return new DataGridViewTextBoxColumn
        {
            HeaderText = headerText,
            DataPropertyName = propertyName,
            FillWeight = 55,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleRight
            }
        };
    }

    private static Button CreateButton(string text, Action action)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            Padding = new Padding(12, 5, 12, 5),
            Margin = new Padding(0, 0, 8, 0)
        };
        button.Click += (_, _) => action();
        return button;
    }

    private sealed class PersonMealCountRow
    {
        public required Person Person { get; init; }
        public string Type { get; init; } = "";
        public string Grade { get; init; } = "";
        public string ClassName { get; init; } = "";
        public string StudentNumber { get; init; } = "";
        public string Name { get; init; } = "";
        public string DeliveryPlace { get; init; } = "";
        public int MealCount { get; init; }
        public int MilkCount { get; init; }
        public int StopCount { get; init; }
        public int AbsentCount { get; init; }
    }
}
