using System.ComponentModel;

namespace KyushokuKanriSystem;

public sealed class IndividualAnnualMealCountForm : Form
{
    private readonly int _fiscalYear;
    private readonly IReadOnlyCollection<Person> _people;
    private readonly Func<Person, DateTime, MealStatus> _mealStatusProvider;
    private readonly Func<Person, DateTime, string> _mealReasonProvider;
    private readonly BindingList<PersonAnnualMealRow> _rows = [];
    private readonly DataGridView _grid = new();
    private readonly ComboBox _typeFilter = new();
    private readonly ComboBox _gradeFilter = new();
    private readonly ComboBox _classFilter = new();
    private readonly ComboBox _deliveryPlaceFilter = new();
    private readonly ComboBox _personFilter = new();
    private readonly TextBox _keywordFilter = new();
    private readonly Label _totalLabel = new();

    public IndividualAnnualMealCountForm(
        int fiscalYear,
        IReadOnlyCollection<Person> people,
        Func<Person, DateTime, MealStatus> mealStatusProvider,
        Func<Person, DateTime, string> mealReasonProvider)
    {
        _fiscalYear = fiscalYear;
        _people = people;
        _mealStatusProvider = mealStatusProvider;
        _mealReasonProvider = mealReasonProvider;

        Text = "個人別年間喫食数";
        Width = 1420;
        Height = 700;
        MinimumSize = new Size(1320, 520);
        StartPosition = FormStartPosition.CenterParent;
        ControlBox = false;

        Controls.Add(CreateLayout());
        ConfigureFilters();
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
            Text = $"{_fiscalYear}年度　個人別年間喫食数・喫食予定",
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        };

        var search = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false
        };
        search.Controls.Add(CreateLabel("区分"));
        _typeFilter.Width = 95;
        search.Controls.Add(_typeFilter);
        search.Controls.Add(CreateLabel("学年"));
        _gradeFilter.Width = 75;
        search.Controls.Add(_gradeFilter);
        search.Controls.Add(CreateLabel("組"));
        _classFilter.Width = 70;
        search.Controls.Add(_classFilter);
        search.Controls.Add(CreateLabel("配膳場所"));
        _deliveryPlaceFilter.Width = 120;
        search.Controls.Add(_deliveryPlaceFilter);
        search.Controls.Add(CreateLabel("氏名選択"));
        _personFilter.Width = 210;
        search.Controls.Add(_personFilter);
        search.Controls.Add(CreateLabel("氏名・番号"));
        _keywordFilter.Width = 140;
        search.Controls.Add(_keywordFilter);
        search.Controls.Add(CreateButton("検索", RefreshRows));
        search.Controls.Add(CreateButton("条件クリア", ClearFilters));

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
        bottom.Controls.Add(_totalLabel, 0, 0);
        bottom.Controls.Add(CreateButton("閉じる", Close), 1, 0);

        root.Controls.Add(title, 0, 0);
        root.Controls.Add(search, 0, 1);
        root.Controls.Add(_grid, 0, 2);
        root.Controls.Add(bottom, 0, 3);
        return root;
    }

    private void ConfigureFilters()
    {
        _typeFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        _typeFilter.Items.AddRange(
        [
            "すべて", "生徒", "職員", "ALT", "教育実習生", "試食会", "ゲスト"
        ]);
        _typeFilter.SelectedIndex = 0;
        _typeFilter.SelectedIndexChanged += (_, _) => RefreshRows();

        _gradeFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        _gradeFilter.Items.Add("すべて");
        _gradeFilter.Items.AddRange(_people
            .Where(person =>
                person.Type == PersonType.Student &&
                !string.IsNullOrWhiteSpace(person.Grade))
            .Select(person => person.Grade.Trim())
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .OrderBy(SortNumber)
            .ThenBy(value => value)
            .Cast<object>()
            .ToArray());
        _gradeFilter.SelectedIndex = 0;
        _gradeFilter.SelectedIndexChanged += (_, _) => RefreshRows();

        _classFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        _classFilter.Items.Add("すべて");
        _classFilter.Items.AddRange(_people
            .Where(person =>
                person.Type == PersonType.Student &&
                !string.IsNullOrWhiteSpace(person.ClassName))
            .Select(person => person.ClassName.Trim())
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .OrderBy(SortNumber)
            .ThenBy(value => value)
            .Cast<object>()
            .ToArray());
        _classFilter.SelectedIndex = 0;
        _classFilter.SelectedIndexChanged += (_, _) => RefreshRows();

        _deliveryPlaceFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        _deliveryPlaceFilter.Items.Add("すべて");
        _deliveryPlaceFilter.Items.AddRange(_people
            .Select(CurrentDeliveryPlace)
            .Where(place => !string.IsNullOrWhiteSpace(place))
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .OrderBy(place => place)
            .Cast<object>()
            .ToArray());
        _deliveryPlaceFilter.SelectedIndex = 0;
        _deliveryPlaceFilter.SelectedIndexChanged += (_, _) => RefreshRows();

        _personFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        _personFilter.DisplayMember = nameof(PersonOption.Label);
        _personFilter.ValueMember = nameof(PersonOption.Id);
        _personFilter.DataSource = new[]
            {
                new PersonOption(null, "すべて")
            }
            .Concat(_people
                .OrderBy(person => person.Type)
                .ThenBy(person => SortNumber(person.Grade))
                .ThenBy(person => person.Grade)
                .ThenBy(person => SortNumber(person.ClassName))
                .ThenBy(person => person.ClassName)
                .ThenBy(person => SortNumber(person.StudentNumber))
                .ThenBy(person => person.FullName)
                .Select(person => new PersonOption(
                    person.Id,
                    PersonOptionLabel(person))))
            .ToList();
        _personFilter.SelectedIndex = 0;
        _personFilter.SelectedIndexChanged += (_, _) => RefreshRows();

        _keywordFilter.KeyDown += (_, eventArgs) =>
        {
            if (eventArgs.KeyCode == Keys.Enter)
            {
                RefreshRows();
                eventArgs.SuppressKeyPress = true;
            }
        };
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
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _grid.ScrollBars = ScrollBars.Vertical;
        _grid.RowTemplate.Height = 36;
        _grid.DataSource = _rows;

        AddTextColumn("区分", nameof(PersonAnnualMealRow.Type), 58, frozen: true);
        AddTextColumn("学年", nameof(PersonAnnualMealRow.Grade), 38, frozen: true);
        AddTextColumn("組", nameof(PersonAnnualMealRow.ClassName), 34, frozen: true);
        AddTextColumn("番号", nameof(PersonAnnualMealRow.StudentNumber), 43, frozen: true);
        AddTextColumn("氏名", nameof(PersonAnnualMealRow.Name), 105, frozen: true);
        AddTextColumn("現在の配膳場所", nameof(PersonAnnualMealRow.DeliveryPlace), 95, frozen: true);

        for (var fiscalMonthIndex = 0; fiscalMonthIndex < 12; fiscalMonthIndex++)
        {
            var month = FiscalMonth(fiscalMonthIndex);
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = $"{month.Month}月",
                DataPropertyName = $"Month{fiscalMonthIndex + 1}",
                Width = 55,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Tag = month,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    WrapMode = DataGridViewTriState.True
                }
            });
        }

        AddCountColumn("喫食", nameof(PersonAnnualMealRow.ActualCount), 48);
        AddCountColumn("予定", nameof(PersonAnnualMealRow.PlannedCount), 48);
        AddCountColumn("年間", nameof(PersonAnnualMealRow.TotalCount), 48);
        AddCountColumn("牛乳", nameof(PersonAnnualMealRow.MilkCount), 48);
        _grid.CellDoubleClick += (_, eventArgs) => ShowMonthDetails(
            eventArgs.RowIndex,
            eventArgs.ColumnIndex);
        _grid.CellFormatting += (_, eventArgs) => StyleMonthCell(
            eventArgs.ColumnIndex,
            eventArgs.CellStyle);
    }

    private void RefreshRows()
    {
        _rows.Clear();
        var fiscalStart = new DateTime(_fiscalYear, 4, 1);
        var fiscalEnd = new DateTime(_fiscalYear + 1, 3, 31);
        var dates = Enumerable.Range(0, (fiscalEnd - fiscalStart).Days + 1)
            .Select(offset => fiscalStart.AddDays(offset))
            .ToList();
        var keyword = NormalizeSearchText(_keywordFilter.Text);

        foreach (var person in _people
            .Where(person => dates.Any(date => IsActive(person, date)))
            .Where(MatchesType)
            .Where(MatchesGrade)
            .Where(MatchesClass)
            .Where(MatchesDeliveryPlace)
            .Where(MatchesPerson)
            .Where(person => MatchesKeyword(person, keyword))
            .OrderBy(person => person.Type)
            .ThenBy(person => SortNumber(person.Grade))
            .ThenBy(person => person.Grade)
            .ThenBy(person => SortNumber(person.ClassName))
            .ThenBy(person => person.ClassName)
            .ThenBy(person => SortNumber(person.StudentNumber))
            .ThenBy(person => person.FullName))
        {
            var row = BuildRow(person, dates);
            _rows.Add(row);
        }

        _totalLabel.Text =
            $"該当者: {_rows.Count}人　喫食: {_rows.Sum(row => row.ActualCount)}　" +
            $"喫食予定: {_rows.Sum(row => row.PlannedCount)}　" +
            $"年間合計: {_rows.Sum(row => row.TotalCount)}　" +
            $"牛乳: {_rows.Sum(row => row.MilkCount)}";
    }

    private PersonAnnualMealRow BuildRow(Person person, IReadOnlyCollection<DateTime> dates)
    {
        var servedDates = dates
            .Where(date =>
                IsActive(person, date) &&
                _mealStatusProvider(person, date) == MealStatus.Serve)
            .ToList();
        var row = new PersonAnnualMealRow
        {
            Person = person,
            Type = person.TypeLabel,
            Grade = person.Grade,
            ClassName = person.ClassName,
            StudentNumber = person.StudentNumber,
            Name = person.FullName,
            DeliveryPlace = CurrentDeliveryPlace(person),
            ActualCount = servedDates.Count(date => date.Date <= DateTime.Today),
            PlannedCount = servedDates.Count(date => date.Date > DateTime.Today),
            TotalCount = servedDates.Count,
            MilkCount = person.HasMilk ? servedDates.Count : 0
        };

        for (var index = 0; index < 12; index++)
        {
            var month = FiscalMonth(index);
            var monthDates = servedDates
                .Where(date => date.Year == month.Year && date.Month == month.Month)
                .ToList();
            var actual = monthDates.Count(date => date.Date <= DateTime.Today);
            var planned = monthDates.Count(date => date.Date > DateTime.Today);
            row.SetMonth(index, MonthLabel(actual, planned));
        }

        return row;
    }

    private void ShowMonthDetails(int rowIndex, int columnIndex)
    {
        if (rowIndex < 0 ||
            columnIndex < 0 ||
            _grid.Rows[rowIndex].DataBoundItem is not PersonAnnualMealRow row ||
            _grid.Columns[columnIndex].Tag is not DateTime month)
        {
            return;
        }

        var dialog = new PersonMonthlyMealMatrixForm(
            month,
            row.Person,
            date => _mealStatusProvider(row.Person, date),
            date => _mealReasonProvider(row.Person, date));
        dialog.Show(this);
    }

    private void ClearFilters()
    {
        _typeFilter.SelectedIndex = 0;
        _gradeFilter.SelectedIndex = 0;
        _classFilter.SelectedIndex = 0;
        _deliveryPlaceFilter.SelectedIndex = 0;
        _personFilter.SelectedIndex = 0;
        _keywordFilter.Clear();
        RefreshRows();
    }

    private bool MatchesType(Person person)
    {
        return _typeFilter.SelectedIndex <= 0 ||
               person.TypeLabel == Convert.ToString(_typeFilter.SelectedItem);
    }

    private bool MatchesGrade(Person person)
    {
        return _gradeFilter.SelectedIndex <= 0 ||
               person.Grade.Equals(
                   Convert.ToString(_gradeFilter.SelectedItem),
                   StringComparison.CurrentCultureIgnoreCase);
    }

    private bool MatchesClass(Person person)
    {
        return _classFilter.SelectedIndex <= 0 ||
               person.ClassName.Equals(
                   Convert.ToString(_classFilter.SelectedItem),
                   StringComparison.CurrentCultureIgnoreCase);
    }

    private bool MatchesDeliveryPlace(Person person)
    {
        if (_deliveryPlaceFilter.SelectedIndex <= 0)
        {
            return true;
        }

        var selectedPlace = Convert.ToString(_deliveryPlaceFilter.SelectedItem) ?? "";
        return CurrentDeliveryPlace(person).Equals(
            selectedPlace,
            StringComparison.CurrentCultureIgnoreCase);
    }

    private bool MatchesPerson(Person person)
    {
        return _personFilter.SelectedItem is not PersonOption option ||
               option.Id is null ||
               option.Id == person.Id;
    }

    private static bool MatchesKeyword(Person person, string keyword)
    {
        if (keyword.Length == 0)
        {
            return true;
        }

        var target = NormalizeSearchText(
            $"{person.FullName}{person.LastName}{person.FirstName}" +
            $"{person.Grade}{person.ClassName}{person.StudentNumber}");
        return target.Contains(keyword, StringComparison.CurrentCultureIgnoreCase);
    }

    private DateTime FiscalMonth(int index)
    {
        return index < 9
            ? new DateTime(_fiscalYear, index + 4, 1)
            : new DateTime(_fiscalYear + 1, index - 8, 1);
    }

    private void AddTextColumn(
        string header,
        string property,
        int width,
        bool frozen)
    {
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = header,
            DataPropertyName = property,
            Width = width,
            Frozen = frozen,
            SortMode = DataGridViewColumnSortMode.Automatic
        });
    }

    private void AddCountColumn(string header, string property, int width)
    {
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = header,
            DataPropertyName = property,
            Width = width,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleRight
            }
        });
    }

    private void StyleMonthCell(int columnIndex, DataGridViewCellStyle style)
    {
        if (columnIndex < 0 ||
            _grid.Columns[columnIndex].Tag is not DateTime month)
        {
            return;
        }

        var monthEnd = month.AddMonths(1).AddDays(-1);
        style.BackColor = monthEnd.Date <= DateTime.Today
            ? Color.FromArgb(225, 241, 230)
            : month.Date > DateTime.Today
                ? Color.FromArgb(222, 237, 252)
                : Color.FromArgb(255, 244, 210);
    }

    private static string MonthLabel(int actual, int planned)
    {
        if (actual > 0 && planned > 0)
        {
            return $"実{actual}\n予{planned}";
        }

        return planned > 0 ? $"予{planned}" : $"実{actual}";
    }

    private static string CurrentDeliveryPlace(Person person)
    {
        var place = person.GetDeliveryPlace(DateTime.Today);
        return string.IsNullOrWhiteSpace(place) ? "未設定" : place.Trim();
    }

    private static bool IsActive(Person person, DateTime date)
    {
        return person.ActiveFrom.Date <= date.Date &&
               (person.ActiveTo is null || person.ActiveTo.Value.Date >= date.Date);
    }

    private static string NormalizeSearchText(string value)
    {
        return value
            .Replace(" ", "", StringComparison.Ordinal)
            .Replace("　", "", StringComparison.Ordinal)
            .Trim();
    }

    private static int SortNumber(string value)
    {
        return int.TryParse(value, out var number) ? number : int.MaxValue;
    }

    private static string PersonOptionLabel(Person person)
    {
        return person.Type == PersonType.Student
            ? $"{person.Grade}年{person.ClassName}組{person.StudentNumber}番　{person.FullName}"
            : $"{person.TypeLabel}　{person.FullName}";
    }

    private static Label CreateLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Padding = new Padding(0, 7, 4, 0)
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

    private sealed class PersonAnnualMealRow
    {
        public required Person Person { get; init; }
        public string Type { get; init; } = "";
        public string Grade { get; init; } = "";
        public string ClassName { get; init; } = "";
        public string StudentNumber { get; init; } = "";
        public string Name { get; init; } = "";
        public string DeliveryPlace { get; init; } = "";
        public string Month1 { get; private set; } = "";
        public string Month2 { get; private set; } = "";
        public string Month3 { get; private set; } = "";
        public string Month4 { get; private set; } = "";
        public string Month5 { get; private set; } = "";
        public string Month6 { get; private set; } = "";
        public string Month7 { get; private set; } = "";
        public string Month8 { get; private set; } = "";
        public string Month9 { get; private set; } = "";
        public string Month10 { get; private set; } = "";
        public string Month11 { get; private set; } = "";
        public string Month12 { get; private set; } = "";
        public int ActualCount { get; init; }
        public int PlannedCount { get; init; }
        public int TotalCount { get; init; }
        public int MilkCount { get; init; }

        public void SetMonth(int index, string value)
        {
            switch (index)
            {
                case 0: Month1 = value; break;
                case 1: Month2 = value; break;
                case 2: Month3 = value; break;
                case 3: Month4 = value; break;
                case 4: Month5 = value; break;
                case 5: Month6 = value; break;
                case 6: Month7 = value; break;
                case 7: Month8 = value; break;
                case 8: Month9 = value; break;
                case 9: Month10 = value; break;
                case 10: Month11 = value; break;
                case 11: Month12 = value; break;
            }
        }
    }

    private sealed record PersonOption(Guid? Id, string Label);
}
