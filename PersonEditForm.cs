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
    private readonly TextBox _deliveryPlace1 = new();
    private readonly TextBox _deliveryPlace2 = new();
    private readonly CheckBox _eatMonday = new() { Text = "月" };
    private readonly CheckBox _eatTuesday = new() { Text = "火" };
    private readonly CheckBox _eatWednesday = new() { Text = "水" };
    private readonly CheckBox _eatThursday = new() { Text = "木" };
    private readonly CheckBox _eatFriday = new() { Text = "金" };
    private readonly CheckBox _hasMilk = new() { Text = "牛乳あり" };
    private readonly CheckBox _hasAllergySupport = new() { Text = "アレルギー対応あり" };
    private readonly DateTimePicker _activeFrom = new();
    private readonly CheckBox _hasActiveTo = new();
    private readonly DateTimePicker _activeTo = new();
    private readonly TextBox _memo = new();

    public Person Person { get; private set; }

    public PersonEditForm(Person? person = null)
    {
        Person = person is null ? new Person() : new Person
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
            DeliveryPlace2 = person.DeliveryPlace2,
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

        Text = person is null ? "1人追加" : "編集";
        Width = 500;
        Height = 700;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        Controls.Add(CreateLayout());
        LoadPerson();
    }

    private Control CreateLayout()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 15,
            Padding = new Padding(16)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _type.DropDownStyle = ComboBoxStyle.DropDownList;
        _type.Items.AddRange(TypeOptions.Select(option => option.Label).ToArray());
        _activeFrom.Format = DateTimePickerFormat.Short;
        _activeTo.Format = DateTimePickerFormat.Short;
        _activeTo.Enabled = false;
        _hasActiveTo.CheckedChanged += (_, _) => _activeTo.Enabled = _hasActiveTo.Checked;

        AddRow(panel, 0, "学年", _grade);
        AddRow(panel, 1, "組", _className);
        AddRow(panel, 2, "番号", _studentNumber);
        AddRow(panel, 3, "区分", _type);
        AddRow(panel, 4, "姓", _lastName);
        AddRow(panel, 5, "名", _firstName);
        AddRow(panel, 6, "配膳場所1", _deliveryPlace1);
        AddRow(panel, 7, "配膳場所2", _deliveryPlace2);
        AddRow(panel, 8, "喫食日", CreateWeekdayPanel());
        AddRow(panel, 9, "牛乳", _hasMilk);
        AddRow(panel, 10, "アレルギー", _hasAllergySupport);
        AddRow(panel, 11, "開始日", _activeFrom);
        AddRow(panel, 12, "終了日", CreateEndDatePanel());
        AddRow(panel, 13, "備考", _memo);

        var buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            AutoSize = true
        };
        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, AutoSize = true };
        var cancel = new Button { Text = "キャンセル", DialogResult = DialogResult.Cancel, AutoSize = true };
        ok.Click += (_, _) =>
        {
            if (!Apply())
            {
                DialogResult = DialogResult.None;
            }
        };
        buttons.Controls.Add(ok);
        buttons.Controls.Add(cancel);
        panel.Controls.Add(buttons, 0, 14);
        panel.SetColumnSpan(buttons, 2);

        AcceptButton = ok;
        CancelButton = cancel;
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

    private static void AddRow(TableLayoutPanel panel, int row, string labelText, Control input)
    {
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.Controls.Add(new Label { Text = labelText, AutoSize = true, Padding = new Padding(0, 7, 0, 0) }, 0, row);
        input.Dock = DockStyle.Fill;
        panel.Controls.Add(input, 1, row);
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
        _deliveryPlace1.Text = Person.DeliveryPlace1;
        _deliveryPlace2.Text = Person.DeliveryPlace2;
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

        if (string.IsNullOrWhiteSpace(_deliveryPlace1.Text) || string.IsNullOrWhiteSpace(_deliveryPlace2.Text))
        {
            MessageBox.Show("配膳場所1と配膳場所2を入力してください。");
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
        Person.DeliveryPlace1 = _deliveryPlace1.Text.Trim();
        Person.DeliveryPlace2 = _deliveryPlace2.Text.Trim();
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
}
