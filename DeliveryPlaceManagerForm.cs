namespace KyushokuKanriSystem;

public sealed class DeliveryPlaceManagerForm : Form
{
    private readonly ListBox _placeList = new();
    private readonly TextBox _placeName = new();
    private readonly IReadOnlyCollection<Person> _people;

    public List<string> DeliveryPlaces { get; private set; }

    public DeliveryPlaceManagerForm(IEnumerable<string> deliveryPlaces, IReadOnlyCollection<Person> people)
    {
        _people = people;
        DeliveryPlaces = deliveryPlaces
            .Where(place => !string.IsNullOrWhiteSpace(place))
            .Select(place => place.Trim())
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .OrderBy(place => place)
            .ToList();

        Text = "配膳場所管理";
        Width = 460;
        Height = 440;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        Controls.Add(CreateLayout());
        RefreshList();
    }

    private Control CreateLayout()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(16)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _placeList.Dock = DockStyle.Fill;
        _placeList.SelectedIndexChanged += (_, _) =>
        {
            if (_placeList.SelectedItem is string selected)
            {
                _placeName.Text = selected;
            }
        };

        _placeName.Dock = DockStyle.Fill;

        var editButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false
        };
        editButtons.Controls.Add(CreateButton("追加", AddPlace));
        editButtons.Controls.Add(CreateButton("修正", UpdatePlace));
        editButtons.Controls.Add(CreateButton("削除", DeletePlace));

        var closeButtons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            AutoSize = true
        };
        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, AutoSize = true };
        var cancel = new Button { Text = "キャンセル", DialogResult = DialogResult.Cancel, AutoSize = true };
        closeButtons.Controls.Add(ok);
        closeButtons.Controls.Add(cancel);
        AcceptButton = ok;
        CancelButton = cancel;

        panel.Controls.Add(_placeList, 0, 0);
        panel.Controls.Add(_placeName, 0, 1);
        panel.Controls.Add(editButtons, 0, 2);
        panel.Controls.Add(closeButtons, 0, 3);
        return panel;
    }

    private static Button CreateButton(string text, Action action)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            Margin = new Padding(0, 8, 8, 8),
            Padding = new Padding(10, 5, 10, 5)
        };
        button.Click += (_, _) => action();
        return button;
    }

    private void AddPlace()
    {
        var value = _placeName.Text.Trim();
        if (value.Length == 0)
        {
            MessageBox.Show("配膳場所を入力してください。");
            return;
        }

        if (DeliveryPlaces.Contains(value, StringComparer.CurrentCultureIgnoreCase))
        {
            MessageBox.Show("同じ配膳場所が既にあります。");
            return;
        }

        DeliveryPlaces.Add(value);
        RefreshList(value);
    }

    private void UpdatePlace()
    {
        if (_placeList.SelectedItem is not string oldValue)
        {
            MessageBox.Show("修正する配膳場所を選択してください。");
            return;
        }

        var newValue = _placeName.Text.Trim();
        if (newValue.Length == 0)
        {
            MessageBox.Show("配膳場所を入力してください。");
            return;
        }

        if (!oldValue.Equals(newValue, StringComparison.CurrentCultureIgnoreCase) &&
            DeliveryPlaces.Contains(newValue, StringComparer.CurrentCultureIgnoreCase))
        {
            MessageBox.Show("同じ配膳場所が既にあります。");
            return;
        }

        var index = DeliveryPlaces.FindIndex(place => place.Equals(oldValue, StringComparison.CurrentCultureIgnoreCase));
        if (index < 0)
        {
            return;
        }

        DeliveryPlaces[index] = newValue;
        foreach (var person in _people)
        {
            if (person.DeliveryPlace1.Equals(oldValue, StringComparison.CurrentCultureIgnoreCase))
            {
                person.DeliveryPlace1 = newValue;
            }

            if (person.DeliveryPlace2.Equals(oldValue, StringComparison.CurrentCultureIgnoreCase))
            {
                person.DeliveryPlace2 = newValue;
            }
        }

        RefreshList(newValue);
    }

    private void DeletePlace()
    {
        if (_placeList.SelectedItem is not string selected)
        {
            MessageBox.Show("削除する配膳場所を選択してください。");
            return;
        }

        var usedCount = _people.Count(person =>
            person.DeliveryPlace1.Equals(selected, StringComparison.CurrentCultureIgnoreCase) ||
            person.DeliveryPlace2.Equals(selected, StringComparison.CurrentCultureIgnoreCase));

        var message = usedCount > 0
            ? $"{selected} は {usedCount}人に設定されています。削除すると、その人の配膳場所は空になります。削除しますか？"
            : $"{selected} を削除しますか？";
        if (MessageBox.Show(message, "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
        {
            return;
        }

        DeliveryPlaces.RemoveAll(place => place.Equals(selected, StringComparison.CurrentCultureIgnoreCase));
        foreach (var person in _people)
        {
            if (person.DeliveryPlace1.Equals(selected, StringComparison.CurrentCultureIgnoreCase))
            {
                person.DeliveryPlace1 = "";
            }

            if (person.DeliveryPlace2.Equals(selected, StringComparison.CurrentCultureIgnoreCase))
            {
                person.DeliveryPlace2 = "";
            }
        }

        _placeName.Clear();
        RefreshList();
    }

    private void RefreshList(string? selectedValue = null)
    {
        DeliveryPlaces = DeliveryPlaces
            .Where(place => !string.IsNullOrWhiteSpace(place))
            .Select(place => place.Trim())
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .OrderBy(place => place)
            .ToList();

        _placeList.DataSource = null;
        _placeList.DataSource = DeliveryPlaces;
        if (selectedValue is not null)
        {
            _placeList.SelectedItem = selectedValue;
        }
    }
}
