using System.ComponentModel;

namespace KyushokuKanriSystem;

public sealed class DeliveryPlaceBasicCountForm : Form
{
    private readonly BindingList<DeliveryPlaceBasicCount> _rows;
    private readonly IReadOnlyCollection<string> _deliveryPlaces;
    private readonly IReadOnlyCollection<Person> _people;
    private readonly DataGridView _grid = new();

    public List<DeliveryPlaceBasicCount> DeliveryPlaceBasicCounts { get; private set; } = [];

    public DeliveryPlaceBasicCountForm(
        IEnumerable<DeliveryPlaceBasicCount> basicCounts,
        IReadOnlyCollection<string> deliveryPlaces,
        IReadOnlyCollection<Person> people)
    {
        _deliveryPlaces = deliveryPlaces;
        _people = people;
        _rows = new BindingList<DeliveryPlaceBasicCount>(basicCounts
            .Select(item => new DeliveryPlaceBasicCount
            {
                DeliveryPlace = item.DeliveryPlace,
                BasicCount = item.BasicCount
            })
            .OrderBy(item => item.DeliveryPlace)
            .ToList());

        Text = "配膳別基本数";
        Width = 520;
        Height = 500;
        MinimumSize = new Size(460, 380);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;
        MinimizeBox = false;

        Controls.Add(CreateLayout());
        AddDeliveryPlaces();
    }

    private Control CreateLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(16)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var tools = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false
        };
        tools.Controls.Add(CreateButton("配膳場所を追加", AddDeliveryPlaces));
        tools.Controls.Add(CreateButton("選択行を削除", DeleteSelectedRow));

        ConfigureGrid();

        var closeButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft
        };
        var save = new Button
        {
            Text = "保存",
            AutoSize = true,
            Padding = new Padding(16, 5, 16, 5)
        };
        save.Click += (_, _) => SaveAndClose();
        var cancel = new Button
        {
            Text = "キャンセル",
            DialogResult = DialogResult.Cancel,
            AutoSize = true,
            Padding = new Padding(12, 5, 12, 5)
        };
        closeButtons.Controls.Add(save);
        closeButtons.Controls.Add(cancel);

        AcceptButton = save;
        CancelButton = cancel;

        root.Controls.Add(tools, 0, 0);
        root.Controls.Add(_grid, 0, 1);
        root.Controls.Add(closeButtons, 0, 2);
        return root;
    }

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.AutoGenerateColumns = false;
        _grid.AllowUserToAddRows = true;
        _grid.AllowUserToDeleteRows = true;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "配膳場所",
            DataPropertyName = nameof(DeliveryPlaceBasicCount.DeliveryPlace),
            FillWeight = 180
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "基本数",
            DataPropertyName = nameof(DeliveryPlaceBasicCount.BasicCount),
            FillWeight = 80
        });
        _grid.DataSource = _rows;
        _grid.DataError += (_, eventArgs) =>
        {
            MessageBox.Show("基本数には0以上の整数を入力してください。", "入力確認",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            eventArgs.ThrowException = false;
        };
    }

    private static Button CreateButton(string text, Action action)
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

    private void AddDeliveryPlaces()
    {
        _grid.EndEdit();
        var today = DateTime.Today;
        var places = _deliveryPlaces
            .Concat(_people.Select(person => person.GetDeliveryPlace(today)))
            .Where(place => !string.IsNullOrWhiteSpace(place))
            .Select(place => place.Trim())
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .OrderBy(place => place);

        foreach (var place in places)
        {
            if (_rows.Any(item =>
                item.DeliveryPlace.Trim().Equals(place, StringComparison.CurrentCultureIgnoreCase)))
            {
                continue;
            }

            _rows.Add(new DeliveryPlaceBasicCount
            {
                DeliveryPlace = place,
                BasicCount = _people.Count(person =>
                    person.ActiveFrom.Date <= today &&
                    (person.ActiveTo is null || person.ActiveTo.Value.Date >= today) &&
                    person.GetDeliveryPlace(today).Trim()
                        .Equals(place, StringComparison.CurrentCultureIgnoreCase))
            });
        }
    }

    private void DeleteSelectedRow()
    {
        if (_grid.CurrentRow?.DataBoundItem is not DeliveryPlaceBasicCount selected)
        {
            MessageBox.Show("削除する行を選択してください。");
            return;
        }

        _rows.Remove(selected);
    }

    private void SaveAndClose()
    {
        if (!_grid.EndEdit())
        {
            return;
        }

        var normalized = _rows
            .Where(item => !string.IsNullOrWhiteSpace(item.DeliveryPlace))
            .Select(item => new DeliveryPlaceBasicCount
            {
                DeliveryPlace = item.DeliveryPlace.Trim(),
                BasicCount = item.BasicCount
            })
            .ToList();

        if (normalized.Any(item => item.BasicCount < 0))
        {
            MessageBox.Show("基本数には0以上の整数を入力してください。", "入力確認",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var duplicate = normalized
            .GroupBy(item => item.DeliveryPlace, StringComparer.CurrentCultureIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            MessageBox.Show($"{duplicate.Key}が重複しています。", "入力確認",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DeliveryPlaceBasicCounts = normalized
            .OrderBy(item => item.DeliveryPlace)
            .ToList();
        DialogResult = DialogResult.OK;
        Close();
    }
}
