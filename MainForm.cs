using System.ComponentModel;

namespace KyushokuKanriSystem;

public sealed class MainForm : Form
{
    private readonly AppRepository _repository = new();
    private readonly AppData _data;
    private readonly BindingList<PersonRow> _personRows = [];
    private readonly BindingList<DailyMealRow> _dailyRows = [];
    private readonly BindingList<MonthlyMealRow> _monthlyRows = [];
    private readonly BindingList<SummaryRow> _summaryRows = [];
    private readonly AppUser? _currentUser;

    private readonly DataGridView _peopleGrid = new();
    private readonly DataGridView _dailyGrid = new();
    private readonly DataGridView _monthlyGrid = new();
    private readonly DataGridView _summaryGrid = new();
    private readonly DateTimePicker _mealDatePicker = new();
    private readonly DateTimePicker _mealMonthPicker = new();
    private readonly Label _dailyTotalLabel = new();
    private readonly Label _monthlyTotalLabel = new();

    public MainForm(AppUser? currentUser = null)
    {
        _currentUser = currentUser;
        _data = _repository.Load();
        NormalizePeople();
        NormalizeDeliveryPlaces();
        NormalizeDeliveryPlaceHistories();
        Text = _currentUser is null
            ? "給食管理システム"
            : $"給食管理システム - {_currentUser.DisplayName}";
        Width = 1120;
        Height = 760;
        MinimumSize = new Size(980, 640);
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;

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
        tabs.TabPages.Add(CreateSummaryPage());
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
        _mealMonthPicker.Format = DateTimePickerFormat.Custom;
        _mealMonthPicker.CustomFormat = "yyyy年MM月";
        _mealMonthPicker.ShowUpDown = true;
        _mealMonthPicker.Width = 130;
        _mealMonthPicker.ValueChanged += (_, _) => RefreshMonthly();
        top.Controls.Add(new Label { Text = "対象月", AutoSize = true, Padding = new Padding(0, 8, 6, 0) });
        top.Controls.Add(_mealMonthPicker);
        top.Controls.Add(CreateButton("更新", RefreshMonthly));

        ConfigureMonthlyGrid();
        _monthlyTotalLabel.AutoSize = true;
        _monthlyTotalLabel.Padding = new Padding(4, 8, 0, 0);

        panel.Controls.Add(top, 0, 0);
        panel.Controls.Add(_monthlyGrid, 0, 1);
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
        buttons.Controls.Add(CreateButton("CSV名簿を読み込み", ImportRoster));
        buttons.Controls.Add(CreateButton("1人追加", AddPerson));
        buttons.Controls.Add(CreateButton("選択を編集", EditSelectedPerson));
        buttons.Controls.Add(CreateButton("選択を削除", DeleteSelectedPerson));
        buttons.Controls.Add(CreateButton("配膳場所管理", ManageDeliveryPlaces));

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
        top.Controls.Add(CreateButton("全員提供", MarkAllServed));
        top.Controls.Add(CreateButton("保存", SaveDaily));

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
        _summaryGrid.AutoGenerateColumns = true;
        _summaryGrid.DataSource = _summaryRows;

        panel.Controls.Add(buttons, 0, 0);
        panel.Controls.Add(_summaryGrid, 0, 1);
        page.Controls.Add(panel);
        return page;
    }

    private Button CreateButton(string text, Action action)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            Margin = new Padding(0, 0, 8, 8),
            Padding = new Padding(10, 5, 10, 5)
        };
        button.Click += (_, _) => action();
        return button;
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
        _peopleGrid.Columns.Clear();
        _peopleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "区分", DataPropertyName = nameof(PersonRow.Type), ReadOnly = true });
        _peopleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "学年", DataPropertyName = nameof(PersonRow.Grade), ReadOnly = true });
        _peopleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "組", DataPropertyName = nameof(PersonRow.ClassName), ReadOnly = true });
        _peopleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "番号", DataPropertyName = nameof(PersonRow.StudentNumber), ReadOnly = true });
        _peopleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "姓", DataPropertyName = nameof(PersonRow.LastName), ReadOnly = true });
        _peopleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "名", DataPropertyName = nameof(PersonRow.FirstName), ReadOnly = true });
        _peopleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "現在の配膳場所", DataPropertyName = nameof(PersonRow.DeliveryPlace), ReadOnly = true });
        _peopleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "喫食日", DataPropertyName = nameof(PersonRow.EatDays), ReadOnly = true });
        _peopleGrid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "牛乳", DataPropertyName = nameof(PersonRow.HasMilk), ReadOnly = true });
        _peopleGrid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "アレルギー", DataPropertyName = nameof(PersonRow.HasAllergySupport), ReadOnly = true });
        _peopleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "開始日", DataPropertyName = nameof(PersonRow.ActiveFrom), ReadOnly = true });
        _peopleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "終了日", DataPropertyName = nameof(PersonRow.ActiveTo), ReadOnly = true });
        _peopleGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "備考", DataPropertyName = nameof(PersonRow.Memo), ReadOnly = true });
        _peopleGrid.DataSource = _personRows;
    }

    private void ConfigureDailyGrid()
    {
        _dailyGrid.Dock = DockStyle.Fill;
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
        _monthlyGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "日付", DataPropertyName = nameof(MonthlyMealRow.Date), FillWeight = 70 });
        _monthlyGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "曜日", DataPropertyName = nameof(MonthlyMealRow.DayOfWeek), FillWeight = 45 });
        _monthlyGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "配膳場所", DataPropertyName = nameof(MonthlyMealRow.DeliveryPlace), FillWeight = 145 });
        _monthlyGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "提供数", DataPropertyName = nameof(MonthlyMealRow.Served), FillWeight = 65 });
        _monthlyGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "牛乳数", DataPropertyName = nameof(MonthlyMealRow.Milk), FillWeight = 65 });
        _monthlyGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "アレルギー対応数", DataPropertyName = nameof(MonthlyMealRow.AllergySupport), FillWeight = 105 });
        _monthlyGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "停止・欠席数", DataPropertyName = nameof(MonthlyMealRow.StoppedOrAbsent), FillWeight = 90 });
        _monthlyGrid.DataSource = _monthlyRows;
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
        _monthlyRows.Clear();
        var month = new DateTime(_mealMonthPicker.Value.Year, _mealMonthPicker.Value.Month, 1);
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

                _monthlyRows.Add(new MonthlyMealRow
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

        var served = _monthlyRows.Sum(row => row.Served);
        var milk = _monthlyRows.Sum(row => row.Milk);
        var allergy = _monthlyRows.Sum(row => row.AllergySupport);
        _monthlyTotalLabel.Text = $"月合計  提供: {served} / 牛乳: {milk} / アレルギー対応: {allergy}";
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
        var groups = _data.People
            .GroupBy(p => p.GroupLabel)
            .OrderBy(g => g.Key);

        foreach (var group in groups)
        {
            var people = group.ToList();
            _summaryRows.Add(new SummaryRow
            {
                Group = group.Key,
                Registered = people.Count,
                ActiveToday = people.Count(p => p.ActiveFrom.Date <= DateTime.Today && (p.ActiveTo is null || p.ActiveTo.Value.Date >= DateTime.Today)),
                ServedToday = CountServed(DateTime.Today, people)
            });
        }
    }

    private int CountServed(DateTime date, IReadOnlyCollection<Person> people)
    {
        return people.Count(person =>
        {
            var record = _data.MealRecords.FirstOrDefault(r => r.PersonId == person.Id && r.Date.Date == date.Date);
            return record is null || record.Status == MealStatus.Serve;
        });
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
        SaveAll();
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
        SaveAll();
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

    private void SaveAll()
    {
        _repository.Save(_data);
        RefreshPeople();
        RefreshDaily();
        RefreshMonthly();
        RefreshSummary();
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
