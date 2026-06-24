using System.ComponentModel;

namespace KyushokuKanriSystem;

public sealed class MainForm : Form
{
    private readonly AppRepository _repository = new();
    private readonly AppData _data;
    private readonly BindingList<PersonRow> _personRows = [];
    private readonly BindingList<DailyMealRow> _dailyRows = [];
    private readonly BindingList<MonthlyMealRow> _monthlyRows = [];
    private readonly List<MonthlyMealRow> _monthlyAllRows = [];
    private readonly BindingList<SummaryRow> _summaryRows = [];
    private readonly AppUser? _currentUser;
    private readonly bool _isReadOnly;

    private readonly DataGridView _peopleGrid = new();
    private readonly DataGridView _dailyGrid = new();
    private readonly DataGridView _monthlyGrid = new();
    private readonly DataGridView _monthlyMatrixGrid = new();
    private readonly TableLayoutPanel _monthlyCalendar = new();
    private readonly DataGridView _summaryGrid = new();
    private readonly DateTimePicker _mealDatePicker = new();
    private readonly Label _registeredFiscalYearLabel = new();
    private readonly Label _mealYearLabel = new();
    private readonly NumericUpDown _mealMonthInput = new();
    private readonly Label _dailyTotalLabel = new();
    private readonly Label _monthlyTotalLabel = new();
    private readonly Label _monthlyDetailLabel = new();
    private DateTime _selectedMonthlyDate = DateTime.Today;
    private int _registeredFiscalYear;
    private int _selectedMealYear;
    private int _selectedMealMonth;
    private bool _updatingMealMonth;

    public MainForm(AppUser? currentUser = null)
    {
        _currentUser = currentUser;
        _isReadOnly = _currentUser?.Role != UserRole.Admin;
        _data = _repository.Load();
        _registeredFiscalYear = _data.RegisteredFiscalYear > 0
            ? _data.RegisteredFiscalYear
            : CurrentFiscalYear();
        _data.RegisteredFiscalYear = _registeredFiscalYear;
        _selectedMealMonth = DateTime.Today.Month;
        _selectedMealYear = YearForFiscalMonth(_registeredFiscalYear, _selectedMealMonth);
        NormalizePeople();
        NormalizeDeliveryPlaces();
        NormalizeDeliveryPlaceHistories();
        Text = _currentUser is null
            ? "給食管理システム"
            : $"給食管理システム - {_currentUser.DisplayName}{(_isReadOnly ? "（閲覧のみ）" : "（管理者）")}";
        Width = 1420;
        Height = 820;
        MinimumSize = new Size(1320, 700);
        StartPosition = FormStartPosition.CenterScreen;

        Controls.Add(CreateLayout());
        RefreshPeople();
        RefreshMonthly();
        RefreshSummary();
    }

    private void NormalizePeople()
    {
        foreach (var person in _data.People)
        {
            if (!string.IsNullOrWhiteSpace(person.LastName + person.FirstName))
            {
                person.Name = person.FullName;
                continue;
            }

            if (string.IsNullOrWhiteSpace(person.Name))
            {
                continue;
            }

            var parts = person.Name.Split([' ', '　'], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                person.LastName = parts[0];
                person.FirstName = string.Join("", parts.Skip(1));
            }
            else
            {
                person.LastName = person.Name;
            }
        }
    }

    private void NormalizeDeliveryPlaces()
    {
        foreach (var place in _data.People
            .SelectMany(person => new[] { person.DeliveryPlace1 }.Concat(person.DeliveryPlaceHistories.Select(history => history.DeliveryPlace)))
            .Where(place => !string.IsNullOrWhiteSpace(place)))
        {
            AddDeliveryPlaceIfMissing(place);
        }

        _data.DeliveryPlaces = _data.DeliveryPlaces
            .Where(place => !string.IsNullOrWhiteSpace(place))
            .Select(place => place.Trim())
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .OrderBy(place => place)
            .ToList();
    }

    private void NormalizeDeliveryPlaceHistories()
    {
        foreach (var person in _data.People)
        {
            if (person.DeliveryPlaceHistories.Count == 0 && !string.IsNullOrWhiteSpace(person.DeliveryPlace1))
            {
                person.DeliveryPlaceHistories.Add(new DeliveryPlaceHistory
                {
                    DeliveryPlace = person.DeliveryPlace1.Trim(),
                    StartDate = person.ActiveFrom.Date
                });
            }
        }
    }

    private void AddDeliveryPlaceIfMissing(string place)
    {
        var trimmed = place.Trim();
        if (trimmed.Length == 0)
        {
            return;
        }

        if (!_data.DeliveryPlaces.Contains(trimmed, StringComparer.CurrentCultureIgnoreCase))
        {
            _data.DeliveryPlaces.Add(trimmed);
        }
    }

    private Control CreateLayout()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(CreatePeoplePage());
        tabs.TabPages.Add(CreateMonthlyPage());
        ConfigureColoredTabs(tabs,
        [
            Color.FromArgb(214, 235, 252),
            Color.FromArgb(218, 242, 226)
        ]);
        return tabs;
    }

    private TabPage CreateMonthlyPage()
    {
        var page = new TabPage("月別給食数");
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var top = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false
        };
        _registeredFiscalYearLabel.Text = $"登録年度: {_registeredFiscalYear}年度";
        _registeredFiscalYearLabel.AutoSize = true;
        _registeredFiscalYearLabel.Padding = new Padding(0, 8, 12, 0);
        top.Controls.Add(_registeredFiscalYearLabel);
        top.Controls.Add(CreateButton("年度登録", RegisterFiscalYear, requiresAdmin: true));
        top.Controls.Add(new Label { Text = "対象月", AutoSize = true, Padding = new Padding(0, 8, 6, 0) });
        _mealYearLabel.Text = $"{_selectedMealYear}年";
        _mealYearLabel.AutoSize = true;
        _mealYearLabel.Padding = new Padding(0, 8, 4, 0);
        top.Controls.Add(_mealYearLabel);

        _mealMonthInput.Minimum = 1;
        _mealMonthInput.Maximum = 12;
        _mealMonthInput.Width = 55;
        _mealMonthInput.TextAlign = HorizontalAlignment.Right;
        _updatingMealMonth = true;
        _mealMonthInput.Value = _selectedMealMonth;
        _updatingMealMonth = false;
        _mealMonthInput.ValueChanged += (_, _) => ChangeMealMonth();
        top.Controls.Add(_mealMonthInput);
        top.Controls.Add(new Label { Text = "月", AutoSize = true, Padding = new Padding(2, 8, 4, 0) });
        top.Controls.Add(CreateButton("更新", RefreshMonthly));

        _monthlyTotalLabel.AutoSize = true;
        _monthlyTotalLabel.Padding = new Padding(4, 8, 0, 0);

        ConfigureMonthlyMatrixGrid();

        panel.Controls.Add(top, 0, 0);
        panel.Controls.Add(_monthlyMatrixGrid, 0, 1);
        panel.Controls.Add(_monthlyTotalLabel, 0, 2);
        page.Controls.Add(panel);
        return page;
    }

    private TabPage CreatePeoplePage()
    {
        var page = new TabPage("名簿");
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(12)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false
        };
        buttons.Controls.Add(CreateButton("CSV名簿を読み込み", ImportRoster, requiresAdmin: true));
        buttons.Controls.Add(CreateButton("1人追加", AddPerson, requiresAdmin: true));
        buttons.Controls.Add(CreateButton("選択を編集", EditSelectedPerson, requiresAdmin: true));
        buttons.Controls.Add(CreateButton("選択を削除", DeleteSelectedPerson, requiresAdmin: true));
        buttons.Controls.Add(CreateButton("名簿を全員削除", DeleteAllPeople, requiresAdmin: true));
        buttons.Controls.Add(CreateButton("配膳場所管理", ManageDeliveryPlaces, requiresAdmin: true));
        buttons.Controls.Add(CreateButton("配膳別基本数", ManageDeliveryPlaceBasicCounts, requiresAdmin: true));

        ConfigurePeopleGrid();
        panel.Controls.Add(buttons, 0, 0);
        panel.Controls.Add(_peopleGrid, 0, 1);
        page.Controls.Add(panel);
        return page;
    }

    private TabPage CreateDailyPage()
    {
        var page = new TabPage("日別管理");
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var top = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false
        };
        _mealDatePicker.Format = DateTimePickerFormat.Short;
        _mealDatePicker.ValueChanged += (_, _) => RefreshDaily();
        top.Controls.Add(new Label { Text = "日付", AutoSize = true, Padding = new Padding(0, 8, 6, 0) });
        top.Controls.Add(_mealDatePicker);
        top.Controls.Add(CreateButton("全員提供", MarkAllServed, requiresAdmin: true));
        top.Controls.Add(CreateButton("保存", SaveDaily, requiresAdmin: true));

        ConfigureDailyGrid();
        _dailyTotalLabel.AutoSize = true;
        _dailyTotalLabel.Padding = new Padding(4, 8, 0, 0);

        panel.Controls.Add(top, 0, 0);
        panel.Controls.Add(_dailyGrid, 0, 1);
        panel.Controls.Add(_dailyTotalLabel, 0, 2);
        page.Controls.Add(panel);
        return page;
    }

    private TabPage CreateSummaryPage()
    {
        var page = new TabPage("集計");
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(12)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false
        };
        buttons.Controls.Add(CreateButton("集計を更新", RefreshSummary));

        _summaryGrid.Dock = DockStyle.Fill;
        _summaryGrid.ReadOnly = true;
        _summaryGrid.AllowUserToAddRows = false;
        _summaryGrid.AllowUserToDeleteRows = false;
        _summaryGrid.AutoGenerateColumns = false;
        _summaryGrid.RowHeadersVisible = false;
        _summaryGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _summaryGrid.Columns.Clear();
        _summaryGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "配膳場所",
            DataPropertyName = nameof(SummaryRow.Group),
            FillWeight = 160
        });
        _summaryGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "登録人数",
            DataPropertyName = nameof(SummaryRow.Registered)
        });
        _summaryGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "本日の在籍人数",
            DataPropertyName = nameof(SummaryRow.ActiveToday)
        });
        _summaryGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "本日の給食数",
            DataPropertyName = nameof(SummaryRow.ServedToday)
        });
        _summaryGrid.DataSource = _summaryRows;

        panel.Controls.Add(buttons, 0, 0);
        panel.Controls.Add(_summaryGrid, 0, 1);
        page.Controls.Add(panel);
        return page;
    }

    private Button CreateButton(string text, Action action, bool requiresAdmin = false)
    {
        var button = new Button
        {
            Text = text,
            Enabled = !requiresAdmin || !_isReadOnly,
            AutoSize = true,
            Margin = new Padding(0, 0, 8, 8),
            Padding = new Padding(10, 5, 10, 5)
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static void ConfigureColoredTabs(TabControl tabs, IReadOnlyList<Color> colors)
    {
        tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
        tabs.SizeMode = TabSizeMode.Fixed;
        tabs.ItemSize = new Size(150, 30);
        tabs.DrawItem += (_, eventArgs) =>
        {
            var page = tabs.TabPages[eventArgs.Index];
            var baseColor = colors[eventArgs.Index % colors.Count];
            var selected = eventArgs.Index == tabs.SelectedIndex;
            var backColor = selected ? Darken(baseColor, 18) : baseColor;

            using var background = new SolidBrush(backColor);
            using var selectedFont = selected ? new Font(tabs.Font, FontStyle.Bold) : null;
            eventArgs.Graphics.FillRectangle(background, eventArgs.Bounds);
            TextRenderer.DrawText(
                eventArgs.Graphics,
                page.Text,
                selectedFont ?? tabs.Font,
                eventArgs.Bounds,
                Color.FromArgb(35, 45, 55),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        };
    }

    private static Color Darken(Color color, int amount)
    {
        return Color.FromArgb(
            Math.Max(0, color.R - amount),
            Math.Max(0, color.G - amount),
            Math.Max(0, color.B - amount));
    }

    private void ConfigurePeopleGrid()
    {
        _peopleGrid.Dock = DockStyle.Fill;
        _peopleGrid.ReadOnly = true;
        _peopleGrid.AllowUserToAddRows = false;
        _peopleGrid.AllowUserToDeleteRows = false;
        _peopleGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _peopleGrid.MultiSelect = false;
        _peopleGrid.AutoGenerateColumns = false;
        _peopleGrid.RowHeadersVisible = false;
        _peopleGrid.ScrollBars = ScrollBars.Vertical;
        _peopleGrid.Columns.Clear();
        _peopleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "区分", DataPropertyName = nameof(PersonRow.Type), ReadOnly = true, Width = 60 });
        _peopleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "学年", DataPropertyName = nameof(PersonRow.Grade), ReadOnly = true, Width = 42 });
        _peopleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "組", DataPropertyName = nameof(PersonRow.ClassName), ReadOnly = true, Width = 42 });
        _peopleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "番号", DataPropertyName = nameof(PersonRow.StudentNumber), ReadOnly = true, Width = 48 });
        _peopleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "姓", DataPropertyName = nameof(PersonRow.LastName), ReadOnly = true, Width = 75 });
        _peopleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "名", DataPropertyName = nameof(PersonRow.FirstName), ReadOnly = true, Width = 75 });
        _peopleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "配膳場所", DataPropertyName = nameof(PersonRow.DeliveryPlace), ReadOnly = true, Width = 105 });
        _peopleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "喫食日", DataPropertyName = nameof(PersonRow.EatDays), ReadOnly = true, Width = 85 });
        _peopleGrid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "牛乳", DataPropertyName = nameof(PersonRow.HasMilk), ReadOnly = true, Width = 45 });
        _peopleGrid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "アレルギー", DataPropertyName = nameof(PersonRow.HasAllergySupport), ReadOnly = true, Width = 65 });
        _peopleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "開始日", DataPropertyName = nameof(PersonRow.ActiveFrom), ReadOnly = true, Width = 82 });
        _peopleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "終了日", DataPropertyName = nameof(PersonRow.ActiveTo), ReadOnly = true, Width = 82 });
        _peopleGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "備考",
            DataPropertyName = nameof(PersonRow.Memo),
            ReadOnly = true,
            MinimumWidth = 80,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _peopleGrid.DataSource = _personRows;
    }

    private void ConfigureDailyGrid()
    {
        _dailyGrid.Dock = DockStyle.Fill;
        _dailyGrid.ReadOnly = _isReadOnly;
        _dailyGrid.AllowUserToAddRows = false;
        _dailyGrid.AllowUserToDeleteRows = false;
        _dailyGrid.AutoGenerateColumns = false;
        _dailyGrid.DataSource = _dailyRows;
        _dailyGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "区分", DataPropertyName = nameof(DailyMealRow.Type), ReadOnly = true });
        _dailyGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "学年", DataPropertyName = nameof(DailyMealRow.Grade), ReadOnly = true });
        _dailyGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "組", DataPropertyName = nameof(DailyMealRow.ClassName), ReadOnly = true });
        _dailyGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "番号", DataPropertyName = nameof(DailyMealRow.StudentNumber), ReadOnly = true });
        _dailyGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "姓", DataPropertyName = nameof(DailyMealRow.LastName), ReadOnly = true, Width = 110 });
        _dailyGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "名", DataPropertyName = nameof(DailyMealRow.FirstName), ReadOnly = true, Width = 110 });
        _dailyGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "配膳場所", DataPropertyName = nameof(DailyMealRow.DeliveryPlace), ReadOnly = true });
        _dailyGrid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "牛乳", DataPropertyName = nameof(DailyMealRow.HasMilk), ReadOnly = true });
        _dailyGrid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "アレルギー", DataPropertyName = nameof(DailyMealRow.HasAllergySupport), ReadOnly = true });
        _dailyGrid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "提供", DataPropertyName = nameof(DailyMealRow.IsServed) });
        _dailyGrid.Columns.Add(new DataGridViewComboBoxColumn
        {
            HeaderText = "状態",
            DataPropertyName = nameof(DailyMealRow.Status),
            DataSource = new[] { "提供", "停止", "欠席" }
        });
        _dailyGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "理由", DataPropertyName = nameof(DailyMealRow.Reason), Width = 220 });
        _dailyGrid.CellValueChanged += (_, _) => UpdateDailyTotal();
        _dailyGrid.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (_dailyGrid.IsCurrentCellDirty)
            {
                _dailyGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        };
    }

    private void ConfigureMonthlyGrid()
    {
        _monthlyGrid.Dock = DockStyle.Fill;
        _monthlyGrid.ReadOnly = true;
        _monthlyGrid.AllowUserToAddRows = false;
        _monthlyGrid.AllowUserToDeleteRows = false;
        _monthlyGrid.AutoGenerateColumns = false;
        _monthlyGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _monthlyGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _monthlyGrid.ScrollBars = ScrollBars.Vertical;
        _monthlyGrid.RowHeadersVisible = false;
        _monthlyGrid.Columns.Clear();
        _monthlyGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "配膳場所", DataPropertyName = nameof(MonthlyMealRow.DeliveryPlace), FillWeight = 145 });
        _monthlyGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "提供数", DataPropertyName = nameof(MonthlyMealRow.Served), FillWeight = 65 });
        _monthlyGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "牛乳数", DataPropertyName = nameof(MonthlyMealRow.Milk), FillWeight = 65 });
        _monthlyGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "アレルギー対応数", DataPropertyName = nameof(MonthlyMealRow.AllergySupport), FillWeight = 105 });
        _monthlyGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "停止・欠席数", DataPropertyName = nameof(MonthlyMealRow.StoppedOrAbsent), FillWeight = 90 });
        _monthlyGrid.Columns.Add(new DataGridViewButtonColumn
        {
            HeaderText = "詳細",
            Text = "確認",
            UseColumnTextForButtonValue = true,
            FillWeight = 55
        });
        _monthlyGrid.DataSource = _monthlyRows;
        _monthlyGrid.CellContentClick += (_, eventArgs) =>
        {
            if (eventArgs.RowIndex < 0 ||
                eventArgs.ColumnIndex != _monthlyGrid.Columns.Count - 1 ||
                _monthlyGrid.Rows[eventArgs.RowIndex].DataBoundItem is not MonthlyMealRow row)
            {
                return;
            }

            ShowStoppedOrAbsentDetails(row);
        };
    }

    private void ConfigureMonthlyMatrixGrid()
    {
        _monthlyMatrixGrid.Dock = DockStyle.Fill;
        _monthlyMatrixGrid.ReadOnly = true;
        _monthlyMatrixGrid.AllowUserToAddRows = false;
        _monthlyMatrixGrid.AllowUserToDeleteRows = false;
        _monthlyMatrixGrid.AllowUserToResizeRows = false;
        _monthlyMatrixGrid.RowHeadersVisible = false;
        _monthlyMatrixGrid.AutoGenerateColumns = false;
        _monthlyMatrixGrid.SelectionMode = DataGridViewSelectionMode.CellSelect;
        _monthlyMatrixGrid.BackgroundColor = Color.White;
        _monthlyMatrixGrid.BorderStyle = BorderStyle.FixedSingle;
        _monthlyMatrixGrid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _monthlyMatrixGrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _monthlyMatrixGrid.ColumnHeadersHeight = 42;
        _monthlyMatrixGrid.RowTemplate.Height = 24;
        _monthlyMatrixGrid.CellDoubleClick += (_, eventArgs) =>
        {
            if (eventArgs.RowIndex < 0 ||
                eventArgs.ColumnIndex < 3 ||
                _monthlyMatrixGrid.Columns[eventArgs.ColumnIndex].Tag is not DateTime date)
            {
                return;
            }

            var tag = _monthlyMatrixGrid.Rows[eventArgs.RowIndex].Tag;
            if (tag is MonthlyMatrixRowTag rowTag)
            {
                ShowServedPeopleDetails(date, rowTag.DeliveryPlace, rowTag.Type);
            }
            else if (tag is MonthlySummaryRowTag.Allergy)
            {
                ShowAllergyPeopleDetails(date);
            }
        };
    }

    private void RefreshPeople()
    {
        _personRows.Clear();
        foreach (var person in _data.People.OrderBy(p => p.Type).ThenBy(p => p.Grade).ThenBy(p => p.ClassName).ThenBy(p => ToNumber(p.StudentNumber)).ThenBy(p => p.StudentNumber).ThenBy(p => p.LastName).ThenBy(p => p.FirstName))
        {
            _personRows.Add(PersonRow.FromPerson(person));
        }
    }

    private void RefreshDaily()
    {
        _dailyRows.Clear();
        var date = _mealDatePicker.Value.Date;
        var activePeople = _data.People
            .Where(p => p.ActiveFrom.Date <= date && (p.ActiveTo is null || p.ActiveTo.Value.Date >= date))
            .OrderBy(p => p.Type)
            .ThenBy(p => p.Grade)
            .ThenBy(p => p.ClassName)
            .ThenBy(p => ToNumber(p.StudentNumber))
            .ThenBy(p => p.StudentNumber)
            .ThenBy(p => p.LastName)
            .ThenBy(p => p.FirstName);

        foreach (var person in activePeople)
        {
            var record = _data.MealRecords.FirstOrDefault(r => r.PersonId == person.Id && r.Date.Date == date);
            _dailyRows.Add(DailyMealRow.From(person, record, date));
        }

        UpdateDailyTotal();
    }

    private void RefreshMonthly()
    {
        _monthlyAllRows.Clear();
        var month = new DateTime(_selectedMealYear, _selectedMealMonth, 1);
        var lastDate = month.AddMonths(1).AddDays(-1);

        for (var date = month; date <= lastDate; date = date.AddDays(1))
        {
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                continue;
            }

            var targetDate = date;
            var activePeople = _data.People
                .Where(person => person.ActiveFrom.Date <= targetDate &&
                                 (person.ActiveTo is null || person.ActiveTo.Value.Date >= targetDate))
                .ToList();

            var groups = activePeople
                .GroupBy(person => person.GetDeliveryPlace(targetDate))
                .OrderBy(group => group.Key);

            foreach (var group in groups)
            {
                var people = group.ToList();
                var servedPeople = people
                    .Where(person => GetMealStatus(person, targetDate) == MealStatus.Serve)
                    .ToList();

                _monthlyAllRows.Add(new MonthlyMealRow
                {
                    DateValue = targetDate,
                    Date = targetDate.ToString("M/d"),
                    DayOfWeek = JapaneseDayOfWeek(targetDate.DayOfWeek),
                    DeliveryPlace = string.IsNullOrWhiteSpace(group.Key) ? "未設定" : group.Key,
                    Served = servedPeople.Count,
                    Milk = servedPeople.Count(person => person.HasMilk),
                    AllergySupport = servedPeople.Count(person => person.HasAllergySupport),
                    StoppedOrAbsent = people.Count - servedPeople.Count
                });
            }
        }

        RefreshMonthlyMatrix(month);

        var served = _monthlyAllRows.Sum(row => row.Served);
        var milk = _monthlyAllRows.Sum(row => row.Milk);
        var allergy = _monthlyAllRows.Sum(row => row.AllergySupport);
        _monthlyTotalLabel.Text = $"月合計  提供: {served} / 牛乳: {milk} / アレルギー対応: {allergy}";
    }

    private void ChangeMealMonth()
    {
        if (_updatingMealMonth)
        {
            return;
        }

        var newMonth = (int)_mealMonthInput.Value;
        _selectedMealMonth = newMonth;
        _selectedMealYear = YearForFiscalMonth(_registeredFiscalYear, newMonth);
        _mealYearLabel.Text = $"{_selectedMealYear}年";
        RefreshMonthly();
    }

    private void RegisterFiscalYear()
    {
        using var dialog = new FiscalYearRegistrationForm(_registeredFiscalYear);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _registeredFiscalYear = dialog.FiscalYear;
        _data.RegisteredFiscalYear = _registeredFiscalYear;
        _repository.Save(_data);

        _selectedMealMonth = 4;
        _selectedMealYear = _registeredFiscalYear;
        _registeredFiscalYearLabel.Text = $"登録年度: {_registeredFiscalYear}年度";
        _mealYearLabel.Text = $"{_selectedMealYear}年";
        _updatingMealMonth = true;
        _mealMonthInput.Value = 4;
        _updatingMealMonth = false;
        RefreshMonthly();
    }

    private static int YearForFiscalMonth(int fiscalYear, int month)
    {
        return month >= 4 ? fiscalYear : fiscalYear + 1;
    }

    private static int CurrentFiscalYear()
    {
        var today = DateTime.Today;
        return today.Month >= 4 ? today.Year : today.Year - 1;
    }

    private void RefreshMonthlyMatrix(DateTime month)
    {
        _monthlyMatrixGrid.SuspendLayout();
        _monthlyMatrixGrid.Columns.Clear();
        _monthlyMatrixGrid.Rows.Clear();

        _monthlyMatrixGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "学級",
            Width = 75,
            Frozen = true,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
        _monthlyMatrixGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "基本数",
            Width = 55,
            Frozen = true,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
        _monthlyMatrixGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "区分",
            Width = 75,
            Frozen = true,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });

        var daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(month.Year, month.Month, day);
            _monthlyMatrixGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = $"{day}\n{JapaneseDayOfWeek(date.DayOfWeek)}",
                Width = 36,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Tag = date
            });
        }

        _monthlyMatrixGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "月合計",
            Width = 50,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });

        var groups = _data.People
            .SelectMany(person => Enumerable.Range(0, daysInMonth)
                .Select(offset => month.AddDays(offset))
                .Where(date => IsActive(person, date))
                .Select(date => new
                {
                    Person = person,
                    DeliveryPlace = NormalizeDeliveryPlace(person.GetDeliveryPlace(date))
                }))
            .GroupBy(item => new { item.DeliveryPlace, item.Person.Type })
            .OrderBy(group => DeliveryPlaceSortKey(group.Key.DeliveryPlace))
            .ThenBy(group => group.Key.DeliveryPlace)
            .ThenBy(group => (int)group.Key.Type)
            .ToList();

        foreach (var group in groups)
        {
            var values = new object[daysInMonth + 4];
            var people = group
                .Select(item => item.Person)
                .DistinctBy(person => person.Id)
                .ToList();
            var firstPerson = people[0];
            values[0] = group.Key.DeliveryPlace;
            values[1] = GetDeliveryPlaceBasicCount(month, group.Key.DeliveryPlace)?.ToString() ?? "";
            values[2] = firstPerson.TypeLabel;
            var monthTotal = 0;
            for (var day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(month.Year, month.Month, day);
                var count = people.Count(person =>
                    IsActive(person, date) &&
                    NormalizeDeliveryPlace(person.GetDeliveryPlace(date)) == group.Key.DeliveryPlace &&
                    GetMealStatus(person, date) == MealStatus.Serve);
                values[day + 2] = count;
                monthTotal += count;
            }

            values[^1] = monthTotal;
            var rowIndex = _monthlyMatrixGrid.Rows.Add(values);
            var matrixRow = _monthlyMatrixGrid.Rows[rowIndex];
            matrixRow.Tag = new MonthlyMatrixRowTag(group.Key.DeliveryPlace, group.Key.Type);
            if (firstPerson.Type != PersonType.Student)
            {
                matrixRow.DefaultCellStyle.BackColor = Color.FromArgb(232, 241, 250);
                matrixRow.DefaultCellStyle.Font = new Font(_monthlyMatrixGrid.Font, FontStyle.Bold);
            }

            StyleMonthlyMatrixRow(matrixRow, month, daysInMonth, false);
            if (firstPerson.Type == PersonType.Student)
            {
                MarkStudentMealCountChanges(matrixRow, month, daysInMonth);
            }

            if (IsStaffRoom(group.Key.DeliveryPlace))
            {
                for (var day = 1; day <= daysInMonth; day++)
                {
                    matrixRow.Cells[day + 2].ToolTipText =
                        "ダブルクリックすると喫食者を確認できます。";
                }
            }
        }

        AddMonthlyMatrixSectionHeader("日別合計", daysInMonth);
        AddMonthlyMatrixSummaryRow("生徒合計", month, daysInMonth,
            date => CountMeals(date, person => person.Type == PersonType.Student),
            Color.FromArgb(224, 239, 252));
        AddMonthlyMatrixSummaryRow("職員室合計", month, daysInMonth,
            date => CountMeals(date, person =>
                person.Type != PersonType.Tasting &&
                IsStaffRoom(person.GetDeliveryPlace(date))),
            Color.FromArgb(232, 241, 250));
        AddMonthlyMatrixSummaryRow("教室職員合計", month, daysInMonth,
            date => CountMeals(date, person =>
                person.Type != PersonType.Student &&
                person.Type != PersonType.Tasting &&
                !IsStaffRoom(person.GetDeliveryPlace(date))),
            Color.FromArgb(238, 238, 248));
        AddMonthlyMatrixSummaryRow("給食合計", month, daysInMonth,
            date => CountMeals(date, person => person.Type != PersonType.Tasting),
            Color.FromArgb(224, 239, 252));
        AddMonthlyMatrixSummaryRow("牛乳数", month, daysInMonth,
            date => _data.People.Count(person =>
                person.Type != PersonType.Tasting &&
                IsActive(person, date) &&
                person.HasMilk &&
                GetMealStatus(person, date) == MealStatus.Serve),
            Color.FromArgb(226, 243, 235));
        var allergyRow = AddMonthlyMatrixSummaryRow("アレルギー対応", month, daysInMonth,
            date => _data.People.Count(person =>
                IsActive(person, date) &&
                person.HasAllergySupport &&
                GetMealStatus(person, date) == MealStatus.Serve),
            Color.FromArgb(255, 239, 220));
        allergyRow.Tag = MonthlySummaryRowTag.Allergy;
        for (var day = 1; day <= daysInMonth; day++)
        {
            allergyRow.Cells[day + 2].ToolTipText =
                "ダブルクリックするとアレルギー対応者を確認できます。";
        }
        AddMonthlyMatrixSummaryRow("試食会 食数", month, daysInMonth,
            date => CountMeals(date, person => person.Type == PersonType.Tasting),
            Color.FromArgb(255, 235, 213));
        AddMonthlyMatrixSummaryRow("試食会 牛乳", month, daysInMonth,
            date => CountMeals(date, person =>
                person.Type == PersonType.Tasting && person.HasMilk),
            Color.FromArgb(255, 245, 220));
        AddMonthlyMatrixSummaryRow("総合計", month, daysInMonth,
            date => CountServed(date, _data.People),
            Color.FromArgb(205, 225, 245));

        _monthlyMatrixGrid.ResumeLayout();
    }

    private void AddMonthlyMatrixSectionHeader(string label, int daysInMonth)
    {
        var values = new object[daysInMonth + 4];
        values[2] = label;
        var row = _monthlyMatrixGrid.Rows[_monthlyMatrixGrid.Rows.Add(values)];
        row.Height = 28;
        row.DefaultCellStyle.BackColor = Color.FromArgb(67, 87, 105);
        row.DefaultCellStyle.ForeColor = Color.White;
        row.DefaultCellStyle.Font = new Font(_monthlyMatrixGrid.Font, FontStyle.Bold);
        row.Cells[2].Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        row.DividerHeight = 3;
    }

    private int CountMeals(DateTime date, Func<Person, bool> predicate)
    {
        return _data.People.Count(person =>
            predicate(person) &&
            IsActive(person, date) &&
            GetMealStatus(person, date) == MealStatus.Serve);
    }

    private int? GetDeliveryPlaceBasicCount(DateTime month, string deliveryPlace)
    {
        var fiscalYear = month.Month >= 4 ? month.Year : month.Year - 1;
        var item = _data.DeliveryPlaceBasicCounts.FirstOrDefault(basicCount =>
            basicCount.FiscalYear == fiscalYear &&
            NormalizeDeliveryPlace(basicCount.DeliveryPlace)
                .Equals(
                    NormalizeDeliveryPlace(deliveryPlace),
                    StringComparison.CurrentCultureIgnoreCase));
        if (item is null)
        {
            return null;
        }

        return month.Month switch
        {
            4 => item.April,
            5 => item.May,
            6 => item.June,
            7 => item.July,
            8 => item.August,
            9 => item.September,
            10 => item.October,
            11 => item.November,
            12 => item.December,
            1 => item.January,
            2 => item.February,
            _ => item.March
        };
    }

    private static bool IsStaffRoom(string deliveryPlace)
    {
        return NormalizeDeliveryPlace(deliveryPlace)
            .Equals("職員室", StringComparison.CurrentCultureIgnoreCase);
    }

    private void ShowServedPeopleDetails(
        DateTime date,
        string deliveryPlace,
        PersonType personType)
    {
        var people = _data.People
            .Where(person =>
                person.Type == personType &&
                IsActive(person, date) &&
                NormalizeDeliveryPlace(person.GetDeliveryPlace(date))
                    .Equals(
                        NormalizeDeliveryPlace(deliveryPlace),
                        StringComparison.CurrentCultureIgnoreCase) &&
                GetMealStatus(person, date) == MealStatus.Serve)
            .OrderBy(person => person.Type)
            .ThenBy(person => person.LastName)
            .ThenBy(person => person.FirstName)
            .ToList();

        var typeLabel = people.FirstOrDefault()?.TypeLabel ?? PersonTypeLabel(personType);
        using var dialog = new ServedPeopleDetailsForm(
            date,
            $"{deliveryPlace} / {typeLabel}",
            people,
            _data.MealRecords);
        dialog.ShowDialog(this);
    }

    private void ShowAllergyPeopleDetails(DateTime date)
    {
        var people = _data.People
            .Where(person =>
                IsActive(person, date) &&
                person.HasAllergySupport &&
                GetMealStatus(person, date) == MealStatus.Serve)
            .OrderBy(person => person.GetDeliveryPlace(date))
            .ThenBy(person => person.Type)
            .ThenBy(person => person.LastName)
            .ThenBy(person => person.FirstName)
            .ToList();

        using var dialog = new ServedPeopleDetailsForm(
            date,
            "アレルギー対応",
            people,
            _data.MealRecords);
        dialog.ShowDialog(this);
    }

    private static string PersonTypeLabel(PersonType personType)
    {
        return personType switch
        {
            PersonType.Staff => "職員",
            PersonType.Student => "生徒",
            PersonType.Alt => "ALT",
            PersonType.Trainee => "教育実習生",
            PersonType.Tasting => "試食会",
            PersonType.Guest => "ゲスト",
            _ => ""
        };
    }

    private DataGridViewRow AddMonthlyMatrixSummaryRow(
        string label,
        DateTime month,
        int daysInMonth,
        Func<DateTime, int> countForDate,
        Color backColor)
    {
        var values = new object[daysInMonth + 4];
        values[0] = "";
        values[2] = label;
        var total = 0;
        for (var day = 1; day <= daysInMonth; day++)
        {
            var count = countForDate(new DateTime(month.Year, month.Month, day));
            values[day + 2] = count;
            total += count;
        }

        values[^1] = total;
        var row = _monthlyMatrixGrid.Rows[_monthlyMatrixGrid.Rows.Add(values)];
        row.DefaultCellStyle.BackColor = backColor;
        row.DefaultCellStyle.Font = new Font(_monthlyMatrixGrid.Font, FontStyle.Bold);
        StyleMonthlyMatrixRow(row, month, daysInMonth, true);
        return row;
    }

    private static void MarkStudentMealCountChanges(
        DataGridViewRow row,
        DateTime month,
        int daysInMonth)
    {
        int? previousCount = null;
        DateTime? previousDate = null;
        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(month.Year, month.Month, day);
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                continue;
            }

            var cell = row.Cells[day + 2];
            var count = Convert.ToInt32(cell.Value);
            if (previousCount is not null && count != previousCount.Value)
            {
                var difference = count - previousCount.Value;
                cell.Style.BackColor = Color.FromArgb(255, 210, 145);
                cell.Style.ForeColor = Color.FromArgb(125, 55, 0);
                cell.Style.Font = new Font(
                    row.DataGridView?.Font ?? SystemFonts.DefaultFont,
                    FontStyle.Bold);
                cell.ToolTipText =
                    $"{previousDate:M/d}の{previousCount}人から{difference:+#;-#;0}人変化しています。";
            }

            previousCount = count;
            previousDate = date;
        }
    }

    private static void StyleMonthlyMatrixRow(
        DataGridViewRow row,
        DateTime month,
        int daysInMonth,
        bool isSummary)
    {
        row.Cells[0].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        row.Cells[1].Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        row.Cells[2].Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(month.Year, month.Month, day);
            var cellIndex = day + 2;
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                row.Cells[cellIndex].Style.BackColor = Color.FromArgb(255, 205, 45);
                row.Cells[cellIndex].Style.ForeColor = Color.FromArgb(80, 60, 0);
            }
            else if (!isSummary && Convert.ToInt32(row.Cells[cellIndex].Value) == 0)
            {
                row.Cells[cellIndex].Style.ForeColor = Color.Gray;
            }
        }
    }

    private static bool IsActive(Person person, DateTime date)
    {
        return person.ActiveFrom.Date <= date.Date &&
               (person.ActiveTo is null || person.ActiveTo.Value.Date >= date.Date);
    }

    private static int DeliveryPlaceSortKey(string deliveryPlace)
    {
        if (IsStaffRoom(deliveryPlace))
        {
            return 20000;
        }

        var match = System.Text.RegularExpressions.Regex.Match(
            deliveryPlace,
            @"(?<grade>\d+)年(?<class>\d+)組");
        return match.Success
            ? int.Parse(match.Groups["grade"].Value) * 100 + int.Parse(match.Groups["class"].Value)
            : 10000;
    }

    private void BuildMonthlyCalendar(DateTime month)
    {
        _monthlyCalendar.SuspendLayout();
        _monthlyCalendar.Controls.Clear();
        _monthlyCalendar.ColumnStyles.Clear();
        _monthlyCalendar.RowStyles.Clear();
        _monthlyCalendar.Dock = DockStyle.Fill;
        _monthlyCalendar.ColumnCount = 7;
        _monthlyCalendar.RowCount = 7;
        _monthlyCalendar.Padding = new Padding(0, 4, 0, 4);
        _monthlyCalendar.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

        for (var column = 0; column < 7; column++)
        {
            _monthlyCalendar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 7));
        }

        _monthlyCalendar.RowStyles.Add(new RowStyle(SizeType.Absolute, 27));
        for (var row = 1; row < 7; row++)
        {
            _monthlyCalendar.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 6));
        }

        var dayNames = new[] { "日", "月", "火", "水", "木", "金", "土" };
        for (var column = 0; column < dayNames.Length; column++)
        {
            _monthlyCalendar.Controls.Add(new Label
            {
                Text = dayNames[column],
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(Font, FontStyle.Bold),
                ForeColor = column == 0 ? Color.Firebrick : column == 6 ? Color.RoyalBlue : SystemColors.ControlText,
                BackColor = Color.FromArgb(242, 244, 247),
                Margin = Padding.Empty
            }, column, 0);
        }

        var firstColumn = (int)month.DayOfWeek;
        var daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(month.Year, month.Month, day);
            var position = firstColumn + day - 1;
            var column = position % 7;
            var row = position / 7 + 1;
            _monthlyCalendar.Controls.Add(CreateCalendarDayButton(date), column, row);
        }

        _monthlyCalendar.ResumeLayout();
    }

    private Button CreateCalendarDayButton(DateTime date)
    {
        var rows = _monthlyAllRows.Where(row => row.DateValue.Date == date.Date).ToList();
        var served = rows.Sum(row => row.Served);
        var milk = rows.Sum(row => row.Milk);
        var allergy = rows.Sum(row => row.AllergySupport);
        var isWeekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        var button = new Button
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            FlatStyle = FlatStyle.Flat,
            TextAlign = ContentAlignment.TopLeft,
            Padding = new Padding(7, 4, 3, 2),
            Font = new Font(Font.FontFamily, 9),
            Text = isWeekend
                ? $"{date.Day}\n給食なし"
                : $"{date.Day}\n給食 {served}\n牛乳 {milk}  アレルギー {allergy}",
            BackColor = isWeekend
                ? Color.FromArgb(245, 245, 245)
                : served == 0
                    ? Color.FromArgb(255, 247, 230)
                    : Color.FromArgb(232, 247, 238),
            ForeColor = date.DayOfWeek == DayOfWeek.Sunday
                ? Color.Firebrick
                : date.DayOfWeek == DayOfWeek.Saturday ? Color.RoyalBlue : Color.FromArgb(32, 45, 58),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderColor = date.Date == DateTime.Today
            ? Color.FromArgb(35, 110, 185)
            : Color.FromArgb(210, 214, 220);
        button.FlatAppearance.BorderSize = date.Date == DateTime.Today ? 2 : 1;
        button.Click += (_, _) =>
        {
            _selectedMonthlyDate = date;
            ShowMonthlyDetails(date);
            BuildMonthlyCalendar(new DateTime(date.Year, date.Month, 1));
        };
        if (date.Date == _selectedMonthlyDate.Date)
        {
            button.BackColor = Color.FromArgb(215, 234, 252);
        }

        return button;
    }

    private void ShowMonthlyDetails(DateTime date)
    {
        _monthlyRows.Clear();
        foreach (var row in _monthlyAllRows
            .Where(row => row.DateValue.Date == date.Date)
            .OrderBy(row => row.DeliveryPlace))
        {
            _monthlyRows.Add(row);
        }

        _monthlyDetailLabel.Text = $"{date:yyyy年M月d日}（{JapaneseDayOfWeek(date.DayOfWeek)}）の配膳場所別内訳";
    }

    private void ShowStoppedOrAbsentDetails(MonthlyMealRow monthlyRow)
    {
        var details = _data.People
            .Where(person =>
                person.ActiveFrom.Date <= monthlyRow.DateValue.Date &&
                (person.ActiveTo is null || person.ActiveTo.Value.Date >= monthlyRow.DateValue.Date) &&
                NormalizeDeliveryPlace(person.GetDeliveryPlace(monthlyRow.DateValue)) ==
                NormalizeDeliveryPlace(monthlyRow.DeliveryPlace))
            .Select(person =>
            {
                var record = _data.MealRecords.FirstOrDefault(item =>
                    item.PersonId == person.Id && item.Date.Date == monthlyRow.DateValue.Date);
                var status = GetMealStatus(person, monthlyRow.DateValue);
                return new MealStatusDetail
                {
                    Type = person.TypeLabel,
                    Grade = person.Grade,
                    ClassName = person.ClassName,
                    StudentNumber = person.StudentNumber,
                    Name = person.FullName,
                    Status = StatusToLabel(status),
                    Reason = record?.Reason ??
                             (!person.EatsOn(monthlyRow.DateValue.DayOfWeek) ? "喫食日ではありません" : "")
                };
            })
            .Where(detail => detail.Status != "提供")
            .OrderBy(detail => detail.Type)
            .ThenBy(detail => detail.Grade)
            .ThenBy(detail => detail.ClassName)
            .ThenBy(detail => ToNumber(detail.StudentNumber))
            .ThenBy(detail => detail.Name)
            .ToList();

        using var dialog = new MealStatusDetailsForm(
            monthlyRow.DateValue,
            monthlyRow.DeliveryPlace,
            details);
        dialog.ShowDialog(this);
    }

    private static string NormalizeDeliveryPlace(string place)
    {
        return string.IsNullOrWhiteSpace(place) ? "未設定" : place.Trim();
    }

    private MealStatus GetMealStatus(Person person, DateTime date)
    {
        var record = _data.MealRecords.FirstOrDefault(item =>
            item.PersonId == person.Id && item.Date.Date == date.Date);
        return record?.Status ?? (person.EatsOn(date.DayOfWeek) ? MealStatus.Serve : MealStatus.Stop);
    }

    private static string JapaneseDayOfWeek(DayOfWeek dayOfWeek)
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

    private void RefreshSummary()
    {
        _summaryRows.Clear();
        var today = DateTime.Today;
        var groups = _data.People
            .GroupBy(person => NormalizeDeliveryPlace(person.GetDeliveryPlace(today)))
            .OrderBy(group => DeliveryPlaceSortKey(group.Key))
            .ThenBy(group => group.Key);

        foreach (var group in groups)
        {
            var people = group.ToList();
            _summaryRows.Add(new SummaryRow
            {
                Group = group.Key,
                Registered = people.Count,
                ActiveToday = people.Count(person => IsActive(person, today)),
                ServedToday = CountServed(today, people)
            });
        }
    }

    private int CountServed(DateTime date, IReadOnlyCollection<Person> people)
    {
        return people.Count(person =>
            person.ActiveFrom.Date <= date.Date &&
            (person.ActiveTo is null || person.ActiveTo.Value.Date >= date.Date) &&
            GetMealStatus(person, date) == MealStatus.Serve);
    }

    private void ImportRoster()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "CSVファイル (*.csv)|*.csv|すべてのファイル (*.*)|*.*",
            Title = "ExcelからCSV UTF-8で保存した名簿を選択"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var imported = CsvRosterImporter.Import(dialog.FileName);
        var added = 0;
        foreach (var person in imported)
        {
            var classDeliveryPlace = CreateClassDeliveryPlace(person.Grade, person.ClassName);
            if (!string.IsNullOrWhiteSpace(classDeliveryPlace))
            {
                AddDeliveryPlaceIfMissing(classDeliveryPlace);
                if (string.IsNullOrWhiteSpace(person.DeliveryPlace1))
                {
                    person.DeliveryPlace1 = classDeliveryPlace;
                }

                if (person.DeliveryPlaceHistories.Count == 0)
                {
                    person.DeliveryPlaceHistories.Add(new DeliveryPlaceHistory
                    {
                        DeliveryPlace = classDeliveryPlace,
                        StartDate = person.ActiveFrom.Date
                    });
                }
            }

            var exists = _data.People.Any(p =>
                p.LastName == person.LastName &&
                p.FirstName == person.FirstName &&
                p.Type == person.Type &&
                p.Grade == person.Grade &&
                p.ClassName == person.ClassName &&
                p.StudentNumber == person.StudentNumber);
            if (!exists)
            {
                _data.People.Add(person);
                added++;
            }
        }

        _repository.Save(_data);
        RefreshPeople();
        RefreshDaily();
        RefreshMonthly();
        RefreshSummary();
        MessageBox.Show($"{added}人を読み込みました。", "名簿読み込み", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static string CreateClassDeliveryPlace(string grade, string className)
    {
        var trimmedGrade = grade.Trim();
        var trimmedClass = className.Trim();
        if (trimmedGrade.Length == 0 || trimmedClass.Length == 0)
        {
            return "";
        }

        return $"{trimmedGrade}年{trimmedClass}組";
    }

    private void AddPerson()
    {
        using var dialog = new PersonEditForm(_data.DeliveryPlaces);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _data.People.Add(dialog.Person);
        SaveAll(dialog.Person.Id);
    }

    private void EditSelectedPerson()
    {
        var selected = SelectedPerson();
        if (selected is null)
        {
            MessageBox.Show("編集する人を選択してください。");
            return;
        }

        using var dialog = new PersonEditForm(_data.DeliveryPlaces, selected);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        selected.Type = dialog.Person.Type;
        selected.Grade = dialog.Person.Grade;
        selected.ClassName = dialog.Person.ClassName;
        selected.StudentNumber = dialog.Person.StudentNumber;
        selected.LastName = dialog.Person.LastName;
        selected.FirstName = dialog.Person.FirstName;
        selected.Name = dialog.Person.FullName;
        selected.DeliveryPlace1 = dialog.Person.DeliveryPlace1;
        selected.DeliveryPlace2 = "";
        selected.DeliveryPlaceHistories = dialog.Person.DeliveryPlaceHistories;
        selected.EatMonday = dialog.Person.EatMonday;
        selected.EatTuesday = dialog.Person.EatTuesday;
        selected.EatWednesday = dialog.Person.EatWednesday;
        selected.EatThursday = dialog.Person.EatThursday;
        selected.EatFriday = dialog.Person.EatFriday;
        selected.HasMilk = dialog.Person.HasMilk;
        selected.HasAllergySupport = dialog.Person.HasAllergySupport;
        selected.ActiveFrom = dialog.Person.ActiveFrom;
        selected.ActiveTo = dialog.Person.ActiveTo;
        selected.Memo = dialog.Person.Memo;
        SaveAll(selected.Id);
    }

    private void ManageDeliveryPlaces()
    {
        using var dialog = new DeliveryPlaceManagerForm(_data.DeliveryPlaces, _data.People);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _data.DeliveryPlaces = dialog.DeliveryPlaces;
        _repository.Save(_data);
        RefreshPeople();
        RefreshDaily();
        RefreshMonthly();
    }

    private void ManageDeliveryPlaceBasicCounts()
    {
        using var dialog = new DeliveryPlaceBasicCountForm(
            _data.DeliveryPlaceBasicCounts,
            _data.DeliveryPlaces,
            _data.People,
            _registeredFiscalYear);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _data.DeliveryPlaceBasicCounts = dialog.DeliveryPlaceBasicCounts;
        _repository.Save(_data);
    }

    private void DeleteSelectedPerson()
    {
        var selected = SelectedPerson();
        if (selected is null)
        {
            MessageBox.Show("削除する人を選択してください。");
            return;
        }

        var result = MessageBox.Show($"{selected.FullName}を削除しますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (result != DialogResult.Yes)
        {
            return;
        }

        _data.People.Remove(selected);
        _data.MealRecords.RemoveAll(r => r.PersonId == selected.Id);
        SaveAll();
    }

    private void DeleteAllPeople()
    {
        if (_data.People.Count == 0)
        {
            MessageBox.Show("削除する名簿データがありません。", "名簿を全員削除",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = MessageBox.Show(
            $"登録されている {_data.People.Count} 人を全員削除します。\n" +
            "給食記録もすべて削除されます。この操作は元に戻せません。\n\n本当に削除しますか？",
            "名簿を全員削除",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);
        if (result != DialogResult.Yes)
        {
            return;
        }

        _data.People.Clear();
        _data.MealRecords.Clear();
        SaveAll();
        MessageBox.Show("名簿を全員削除しました。", "名簿を全員削除",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private Person? SelectedPerson()
    {
        if (_peopleGrid.CurrentRow?.DataBoundItem is not PersonRow row)
        {
            return null;
        }

        return _data.People.FirstOrDefault(p => p.Id == row.Id);
    }

    private void MarkAllServed()
    {
        foreach (var row in _dailyRows)
        {
            row.Status = "提供";
            row.IsServed = true;
            row.Reason = "";
        }

        _dailyGrid.Refresh();
        UpdateDailyTotal();
    }

    private void SaveDaily()
    {
        var date = _mealDatePicker.Value.Date;
        _data.MealRecords.RemoveAll(r => r.Date.Date == date);

        foreach (var row in _dailyRows)
        {
            _data.MealRecords.Add(new MealRecord
            {
                PersonId = row.Id,
                Date = date,
                Status = LabelToStatus(row.IsServed ? "提供" : row.Status),
                Reason = row.Reason
            });
        }

        _repository.Save(_data);
        RefreshSummary();
        MessageBox.Show("保存しました。", "日別管理", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void SaveAll(Guid? selectedPersonId = null)
    {
        _repository.Save(_data);
        RefreshPeople();
        RestorePeopleSelection(selectedPersonId);
        RefreshDaily();
        RefreshMonthly();
        RefreshSummary();
    }

    private void RestorePeopleSelection(Guid? personId)
    {
        if (personId is null)
        {
            return;
        }

        foreach (DataGridViewRow gridRow in _peopleGrid.Rows)
        {
            if (gridRow.DataBoundItem is not PersonRow row || row.Id != personId)
            {
                continue;
            }

            _peopleGrid.ClearSelection();
            gridRow.Selected = true;
            _peopleGrid.CurrentCell = gridRow.Cells[0];
            _peopleGrid.FirstDisplayedScrollingRowIndex = gridRow.Index;
            return;
        }
    }

    private void UpdateDailyTotal()
    {
        var served = _dailyRows.Count(r => r.IsServed || r.Status == "提供");
        var stopped = _dailyRows.Count - served;
        _dailyTotalLabel.Text = $"提供数: {served} / 停止・欠席: {stopped} / 登録: {_dailyRows.Count}";
    }

    private static MealStatus LabelToStatus(string label)
    {
        return label switch
        {
            "停止" => MealStatus.Stop,
            "欠席" => MealStatus.Absent,
            _ => MealStatus.Serve
        };
    }

    private static string StatusToLabel(MealStatus status)
    {
        return status switch
        {
            MealStatus.Stop => "停止",
            MealStatus.Absent => "欠席",
            _ => "提供"
        };
    }

    private static int ToNumber(string value)
    {
        return int.TryParse(value, out var number) ? number : int.MaxValue;
    }

    private sealed class PersonRow
    {
        public Guid Id { get; init; }
        public string Type { get; init; } = "";
        public string Grade { get; init; } = "";
        public string ClassName { get; init; } = "";
        public string StudentNumber { get; init; } = "";
        public string LastName { get; init; } = "";
        public string FirstName { get; init; } = "";
        public string DeliveryPlace { get; init; } = "";
        public string EatDays { get; init; } = "";
        public bool HasMilk { get; init; }
        public bool HasAllergySupport { get; init; }
        public string ActiveFrom { get; init; } = "";
        public string ActiveTo { get; init; } = "";
        public string Memo { get; init; } = "";

        public static PersonRow FromPerson(Person person)
        {
            return new PersonRow
            {
                Id = person.Id,
                Type = person.TypeLabel,
                Grade = person.Grade,
                ClassName = person.ClassName,
                StudentNumber = person.StudentNumber,
                LastName = person.LastName,
                FirstName = person.FirstName,
                DeliveryPlace = person.GetDeliveryPlace(DateTime.Today),
                EatDays = FormatEatDays(person),
                HasMilk = person.HasMilk,
                HasAllergySupport = person.HasAllergySupport,
                ActiveFrom = person.ActiveFrom.ToShortDateString(),
                ActiveTo = person.ActiveTo?.ToShortDateString() ?? "",
                Memo = person.Memo
            };
        }
    }

    private sealed class DailyMealRow
    {
        public Guid Id { get; init; }
        public string Type { get; init; } = "";
        public string Grade { get; init; } = "";
        public string ClassName { get; init; } = "";
        public string StudentNumber { get; init; } = "";
        public string LastName { get; init; } = "";
        public string FirstName { get; init; } = "";
        public string DeliveryPlace { get; init; } = "";
        public bool HasMilk { get; init; }
        public bool HasAllergySupport { get; init; }
        public bool IsServed { get; set; } = true;
        public string Status { get; set; } = "提供";
        public string Reason { get; set; } = "";

        public static DailyMealRow From(Person person, MealRecord? record, DateTime date)
        {
            var status = record?.Status ?? (person.EatsOn(date.DayOfWeek) ? MealStatus.Serve : MealStatus.Stop);
            return new DailyMealRow
            {
                Id = person.Id,
                Type = person.TypeLabel,
                Grade = person.Grade,
                ClassName = person.ClassName,
                StudentNumber = person.StudentNumber,
                LastName = person.LastName,
                FirstName = person.FirstName,
                DeliveryPlace = person.GetDeliveryPlace(date),
                HasMilk = person.HasMilk,
                HasAllergySupport = person.HasAllergySupport,
                IsServed = status == MealStatus.Serve,
                Status = StatusToLabel(status),
                Reason = record?.Reason ?? (person.EatsOn(date.DayOfWeek) ? "" : "喫食日ではありません")
            };
        }
    }

    private sealed class MonthlyMealRow
    {
        public DateTime DateValue { get; init; }
        public string Date { get; init; } = "";
        public string DayOfWeek { get; init; } = "";
        public string DeliveryPlace { get; init; } = "";
        public int Served { get; init; }
        public int Milk { get; init; }
        public int AllergySupport { get; init; }
        public int StoppedOrAbsent { get; init; }
    }

    private sealed record MonthlyMatrixRowTag(string DeliveryPlace, PersonType Type);

    private enum MonthlySummaryRowTag
    {
        Allergy
    }

    public sealed class MealStatusDetail
    {
        public string Type { get; init; } = "";
        public string Grade { get; init; } = "";
        public string ClassName { get; init; } = "";
        public string StudentNumber { get; init; } = "";
        public string Name { get; init; } = "";
        public string Status { get; init; } = "";
        public string Reason { get; init; } = "";
    }

    private static string FormatEatDays(Person person)
    {
        var days = new List<string>();
        if (person.EatMonday) days.Add("月");
        if (person.EatTuesday) days.Add("火");
        if (person.EatWednesday) days.Add("水");
        if (person.EatThursday) days.Add("木");
        if (person.EatFriday) days.Add("金");
        return string.Join("", days);
    }

    private sealed class SummaryRow
    {
        public string Group { get; init; } = "";
        public int Registered { get; init; }
        public int ActiveToday { get; init; }
        public int ServedToday { get; init; }
    }
}
