namespace KyushokuKanriSystem;

public sealed class PersonEditForm : Form
{
    private static readonly (PersonType Type, string Label)[] TypeOptions =
    [
        (PersonType.Staff, "職員"),
        (PersonType.Student, "生徒"),
        (PersonType.Alt, "ALT"),
        (PersonType.Trainee, "教育実習生"),
        (PersonType.Tasting, "試食会"),
        (PersonType.Guest, "ゲスト")
    ];

    private readonly TextBox _grade = new();
    private readonly TextBox _className = new();
    private readonly TextBox _studentNumber = new();
    private readonly ComboBox _type = new();
    private readonly TextBox _lastName = new();
    private readonly TextBox _firstName = new();
    private readonly ComboBox _deliveryPlace = new();
    private readonly CheckBox _eatMonday = new() { Text = "月", AutoSize = true };
    private readonly CheckBox _eatTuesday = new() { Text = "火", AutoSize = true };
    private readonly CheckBox _eatWednesday = new() { Text = "水", AutoSize = true };
    private readonly CheckBox _eatThursday = new() { Text = "木", AutoSize = true };
    private readonly CheckBox _eatFriday = new() { Text = "金", AutoSize = true };
    private readonly CheckBox _hasMilk = new() { Text = "牛乳あり", AutoSize = true };
    private readonly CheckBox _hasAllergySupport = new() { Text = "アレルギー対応あり", AutoSize = true };
    private readonly DateTimePicker _activeFrom = new();
    private readonly CheckBox _hasActiveTo = new() { Text = "設定する", AutoSize = true };
    private readonly DateTimePicker _activeTo = new();
    private readonly TextBox _memo = new();
    private readonly IReadOnlyCollection<string> _deliveryPlaces;

    public Person Person { get; private set; }

    public PersonEditForm(IReadOnlyCollection<string> deliveryPlaces, Person? person = null)
    {
        _deliveryPlaces = deliveryPlaces;
        Person = person is null ? new Person() : ClonePerson(person);

        Text = person is null ? "1人追加" : "編集";
        Width = 640;
        Height = 640;
        MinimumSize = new Size(600, 600);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;

        Controls.Add(CreateLayout());
        LoadPerson();
    }

    private static Person ClonePerson(Person person)
    {
        return new Person
        {
            Id = person.Id,
            Type = person.Type,
            Grade = person.Grade,
            ClassName = person.ClassName,
            StudentNumber = person.StudentNumber,
            LastName = person.LastName,
            FirstName = person.FirstName,
            Name = person.FullName,
            DeliveryPlace1 = person.DeliveryPlace1,
            DeliveryPlace2 = "",
            DeliveryPlaceHistories = person.DeliveryPlaceHistories
                .Select(history => new DeliveryPlaceHistory
                {
                    Id = history.Id,
                    DeliveryPlace = history.DeliveryPlace,
                    StartDate = history.StartDate,
                    EndDate = history.EndDate
                })
                .ToList(),
            EatMonday = person.EatMonday,
            EatTuesday = person.EatTuesday,
            EatWednesday = person.EatWednesday,
            EatThursday = person.EatThursday,
            EatFriday = person.EatFriday,
            HasMilk = person.HasMilk,
            HasAllergySupport = person.HasAllergySupport,
            ActiveFrom = person.ActiveFrom,
            ActiveTo = person.ActiveTo,
            Memo = person.Memo
        };
    }

    private Control CreateLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(10)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(CreateBasicSection(), 0, 0);
        root.Controls.Add(CreateMealSection(), 0, 1);
        root.Controls.Add(CreateDateSection(), 0, 2);
        root.Controls.Add(CreateMemoSection(), 0, 3);
        root.Controls.Add(CreateButtonBar(), 0, 4);
        return root;
    }

    private GroupBox CreateBasicSection()
    {
        var group = CreateGroup("基本情報");
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 6,
            RowCount = 3,
            Padding = new Padding(0, 2, 0, 0)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 55));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 35));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 45));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));

        _type.DropDownStyle = ComboBoxStyle.DropDownList;
        _type.Items.AddRange(TypeOptions.Select(option => option.Label).ToArray());

        AddCompactLabel(panel, "区分", 0, 0);
        AddCompactInput(panel, _type, 1, 0);
        panel.SetColumnSpan(_type, 5);

        AddCompactLabel(panel, "学年", 0, 1);
        AddCompactInput(panel, _grade, 1, 1);
        AddCompactLabel(panel, "組", 2, 1);
        AddCompactInput(panel, _className, 3, 1);
        AddCompactLabel(panel, "番号", 4, 1);
        AddCompactInput(panel, _studentNumber, 5, 1);

        AddCompactLabel(panel, "姓", 0, 2);
        AddCompactInput(panel, _lastName, 1, 2);
        panel.SetColumnSpan(_lastName, 2);
        AddCompactLabel(panel, "名", 3, 2);
        AddCompactInput(panel, _firstName, 4, 2);
        panel.SetColumnSpan(_firstName, 2);

        group.Controls.Add(panel);
        return group;
    }

    private GroupBox CreateMealSection()
    {
        var group = CreateGroup("配膳・食事設定");
        var panel = CreateGrid();

        ConfigureDeliveryPlaceCombo(_deliveryPlace, _deliveryPlaces);

        AddRow(panel, 0, "現在の配膳場所", CreateDeliveryPlacePanel());
        AddRow(panel, 1, "喫食日", CreateWeekdayPanel());
        AddRow(panel, 2, "牛乳", _hasMilk);
        AddRow(panel, 3, "アレルギー", _hasAllergySupport);

        group.Controls.Add(panel);
        return group;
    }

    private Control CreateDeliveryPlacePanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 1
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var historyButton = new Button
        {
            Text = "履歴管理",
            AutoSize = true,
            Margin = new Padding(8, 2, 0, 6),
            Padding = new Padding(10, 4, 10, 4)
        };
        historyButton.Click += (_, _) => ManageDeliveryPlaceHistories();

        panel.Controls.Add(_deliveryPlace, 0, 0);
        panel.Controls.Add(historyButton, 1, 0);
        return panel;
    }

    private GroupBox CreateDateSection()
    {
        var group = CreateGroup("期間");
        var panel = CreateGrid();

        _activeFrom.Format = DateTimePickerFormat.Short;
        _activeTo.Format = DateTimePickerFormat.Short;
        _activeTo.Enabled = false;
        _hasActiveTo.CheckedChanged += (_, _) => _activeTo.Enabled = _hasActiveTo.Checked;

        AddRow(panel, 0, "開始日", _activeFrom);
        AddRow(panel, 1, "終了日", CreateEndDatePanel());

        group.Controls.Add(panel);
        return group;
    }

    private GroupBox CreateMemoSection()
    {
        var group = CreateGroup("備考");
        group.AutoSize = false;
        group.MinimumSize = new Size(0, 105);
        _memo.Dock = DockStyle.Fill;
        _memo.Multiline = true;
        _memo.AcceptsReturn = true;
        _memo.WordWrap = true;
        _memo.ScrollBars = ScrollBars.Vertical;
        group.Controls.Add(_memo);
        return group;
    }

    private FlowLayoutPanel CreateButtonBar()
    {
        var buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            AutoSize = true,
            Padding = new Padding(0, 12, 0, 0)
        };
        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, AutoSize = true, Padding = new Padding(16, 6, 16, 6) };
        var cancel = new Button { Text = "キャンセル", DialogResult = DialogResult.Cancel, AutoSize = true, Padding = new Padding(16, 6, 16, 6) };
        ok.Click += (_, _) =>
        {
            if (!Apply())
            {
                DialogResult = DialogResult.None;
            }
        };
        buttons.Controls.Add(ok);
        buttons.Controls.Add(cancel);
        AcceptButton = ok;
        CancelButton = cancel;
        return buttons;
    }

    private static GroupBox CreateGroup(string text)
    {
        return new GroupBox
        {
            Text = text,
            Dock = DockStyle.Fill,
            AutoSize = true,
            Padding = new Padding(9),
            Margin = new Padding(0, 0, 0, 7)
        };
    }

    private static TableLayoutPanel CreateGrid()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            Padding = new Padding(0, 4, 0, 0)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        return panel;
    }

    private Control CreateWeekdayPanel()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false };
        panel.Controls.AddRange([_eatMonday, _eatTuesday, _eatWednesday, _eatThursday, _eatFriday]);
        return panel;
    }

    private Control CreateEndDatePanel()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false };
        panel.Controls.Add(_hasActiveTo);
        panel.Controls.Add(_activeTo);
        return panel;
    }

    private static void ConfigureDeliveryPlaceCombo(ComboBox comboBox, IReadOnlyCollection<string> deliveryPlaces)
    {
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox.Items.AddRange(deliveryPlaces.OrderBy(place => place).Cast<object>().ToArray());
    }

    private static void AddRow(TableLayoutPanel panel, int row, string labelText, Control input)
    {
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.Controls.Add(new Label { Text = labelText, AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, row);
        input.Dock = DockStyle.Fill;
        input.Margin = new Padding(0, 1, 0, 4);
        panel.Controls.Add(input, 1, row);
    }

    private static void AddCompactLabel(TableLayoutPanel panel, string text, int column, int row)
    {
        panel.Controls.Add(new Label
        {
            Text = text,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 5, 4, 3)
        }, column, row);
    }

    private static void AddCompactInput(TableLayoutPanel panel, Control input, int column, int row)
    {
        input.Dock = DockStyle.Fill;
        input.Margin = new Padding(0, 1, 8, 4);
        panel.Controls.Add(input, column, row);
    }

    private void LoadPerson()
    {
        _grade.Text = Person.Grade;
        _className.Text = Person.ClassName;
        _studentNumber.Text = Person.StudentNumber;
        _type.SelectedIndex = Array.FindIndex(TypeOptions, option => option.Type == Person.Type);
        if (_type.SelectedIndex < 0)
        {
            _type.SelectedIndex = 1;
        }

        _lastName.Text = Person.LastName;
        _firstName.Text = Person.FirstName;
        EnsureInitialHistory();
        SelectComboText(_deliveryPlace, Person.GetDeliveryPlace(DateTime.Today));
        _eatMonday.Checked = Person.EatMonday;
        _eatTuesday.Checked = Person.EatTuesday;
        _eatWednesday.Checked = Person.EatWednesday;
        _eatThursday.Checked = Person.EatThursday;
        _eatFriday.Checked = Person.EatFriday;
        _hasMilk.Checked = Person.HasMilk;
        _hasAllergySupport.Checked = Person.HasAllergySupport;
        _activeFrom.Value = Person.ActiveFrom;
        _hasActiveTo.Checked = Person.ActiveTo is not null;
        _activeTo.Value = Person.ActiveTo ?? DateTime.Today;
        _memo.Text = Person.Memo;
    }

    private void EnsureInitialHistory()
    {
        if (Person.DeliveryPlaceHistories.Count > 0 || string.IsNullOrWhiteSpace(Person.DeliveryPlace1))
        {
            return;
        }

        Person.DeliveryPlaceHistories.Add(new DeliveryPlaceHistory
        {
            DeliveryPlace = Person.DeliveryPlace1,
            StartDate = Person.ActiveFrom.Date
        });
    }

    private void ManageDeliveryPlaceHistories()
    {
        using var dialog = new DeliveryPlaceHistoryForm(Person.DeliveryPlaceHistories, _deliveryPlaces);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        Person.DeliveryPlaceHistories = dialog.Histories;
        SelectComboText(_deliveryPlace, Person.GetDeliveryPlace(DateTime.Today));
    }

    private bool Apply()
    {
        var selectedType = TypeOptions[Math.Max(0, _type.SelectedIndex)].Type;
        if (selectedType != PersonType.Staff &&
            (string.IsNullOrWhiteSpace(_grade.Text) ||
             string.IsNullOrWhiteSpace(_className.Text) ||
             string.IsNullOrWhiteSpace(_studentNumber.Text)))
        {
            MessageBox.Show("職員以外の場合は、学年・組・番号を入力してください。");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_lastName.Text + _firstName.Text))
        {
            MessageBox.Show("姓または名を入力してください。");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_deliveryPlace.Text) && Person.DeliveryPlaceHistories.Count == 0)
        {
            MessageBox.Show("配膳場所を選択してください。");
            return false;
        }

        if (!(_eatMonday.Checked || _eatTuesday.Checked || _eatWednesday.Checked || _eatThursday.Checked || _eatFriday.Checked))
        {
            MessageBox.Show("喫食日を1つ以上選択してください。");
            return false;
        }

        Person.Type = selectedType;
        Person.Grade = _grade.Text.Trim();
        Person.ClassName = _className.Text.Trim();
        Person.StudentNumber = _studentNumber.Text.Trim();
        Person.LastName = _lastName.Text.Trim();
        Person.FirstName = _firstName.Text.Trim();
        Person.Name = $"{Person.LastName} {Person.FirstName}".Trim();
        Person.DeliveryPlace1 = _deliveryPlace.Text.Trim();
        Person.DeliveryPlace2 = "";
        if (Person.DeliveryPlaceHistories.Count == 0)
        {
            Person.DeliveryPlaceHistories.Add(new DeliveryPlaceHistory
            {
                DeliveryPlace = Person.DeliveryPlace1,
                StartDate = _activeFrom.Value.Date
            });
        }

        Person.EatMonday = _eatMonday.Checked;
        Person.EatTuesday = _eatTuesday.Checked;
        Person.EatWednesday = _eatWednesday.Checked;
        Person.EatThursday = _eatThursday.Checked;
        Person.EatFriday = _eatFriday.Checked;
        Person.HasMilk = _hasMilk.Checked;
        Person.HasAllergySupport = _hasAllergySupport.Checked;
        Person.ActiveFrom = _activeFrom.Value.Date;
        Person.ActiveTo = _hasActiveTo.Checked ? _activeTo.Value.Date : null;
        Person.Memo = _memo.Text.Trim();
        return true;
    }

    private static void SelectComboText(ComboBox comboBox, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
            return;
        }

        var index = comboBox.FindStringExact(value);
        if (index >= 0)
        {
            comboBox.SelectedIndex = index;
        }
    }
}
