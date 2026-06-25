using System.ComponentModel;

namespace KyushokuKanriSystem;

public sealed class DeliveryPlaceHistoryForm : Form
{
    private readonly BindingList<HistoryRow> _rows = [];
    private readonly ComboBox _deliveryPlace = new();
    private readonly DateTimePicker _startDate = new();
    private readonly CheckBox _hasEndDate = new() { Text = "終了日あり", AutoSize = true };
    private readonly DateTimePicker _endDate = new();
    private readonly DataGridView _grid = new();

    public List<DeliveryPlaceHistory> Histories { get; private set; }

    public DeliveryPlaceHistoryForm(IEnumerable<DeliveryPlaceHistory> histories, IReadOnlyCollection<string> deliveryPlaces)
    {
        Histories = histories
            .Select(history => new DeliveryPlaceHistory
            {
                Id = history.Id,
                DeliveryPlace = history.DeliveryPlace,
                StartDate = history.StartDate,
                EndDate = history.EndDate
            })
            .ToList();

        Text = "配膳場所履歴";
        Width = 620;
        Height = 500;
        StartPosition = FormStartPosition.CenterParent;
        ControlBox = false;
        FormBorderStyle = FormBorderStyle.Sizable;

        _deliveryPlace.DropDownStyle = ComboBoxStyle.DropDownList;
        _deliveryPlace.Items.AddRange(deliveryPlaces.OrderBy(place => place).Cast<object>().ToArray());
        _startDate.Format = DateTimePickerFormat.Short;
        _endDate.Format = DateTimePickerFormat.Short;
        _endDate.Enabled = false;
        _hasEndDate.CheckedChanged += (_, _) => _endDate.Enabled = _hasEndDate.Checked;

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
            Padding = new Padding(16)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        ConfigureGrid();
        root.Controls.Add(_grid, 0, 0);
        root.Controls.Add(CreateEditArea(), 0, 1);
        root.Controls.Add(CreateEditButtons(), 0, 2);
        root.Controls.Add(CreateCloseButtons(), 0, 3);
        return root;
    }

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.AutoGenerateColumns = false;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "配膳場所", DataPropertyName = nameof(HistoryRow.DeliveryPlace), ReadOnly = true, Width = 180 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "開始日", DataPropertyName = nameof(HistoryRow.StartDate), ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "終了日", DataPropertyName = nameof(HistoryRow.EndDate), ReadOnly = true });
        _grid.DataSource = _rows;
        _grid.SelectionChanged += (_, _) => LoadSelected();
    }

    private Control CreateEditArea()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(0, 12, 0, 0)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddRow(panel, 0, "配膳場所", _deliveryPlace);
        AddRow(panel, 1, "開始日", _startDate);
        AddRow(panel, 2, "終了日", CreateEndDatePanel());
        return panel;
    }

    private Control CreateEndDatePanel()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false };
        panel.Controls.Add(_hasEndDate);
        panel.Controls.Add(_endDate);
        return panel;
    }

    private FlowLayoutPanel CreateEditButtons()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false };
        panel.Controls.Add(CreateButton("追加", AddHistory));
        panel.Controls.Add(CreateButton("修正", UpdateHistory));
        panel.Controls.Add(CreateButton("削除", DeleteHistory));
        return panel;
    }

    private FlowLayoutPanel CreateCloseButtons()
    {
        var panel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            AutoSize = true,
            Padding = new Padding(0, 8, 0, 0)
        };
        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, AutoSize = true, Padding = new Padding(16, 6, 16, 6) };
        var cancel = new Button { Text = "キャンセル", DialogResult = DialogResult.Cancel, AutoSize = true, Padding = new Padding(16, 6, 16, 6) };
        panel.Controls.Add(ok);
        panel.Controls.Add(cancel);
        AcceptButton = ok;
        CancelButton = cancel;
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

    private static void AddRow(TableLayoutPanel panel, int row, string labelText, Control input)
    {
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.Controls.Add(new Label { Text = labelText, AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, row);
        input.Dock = DockStyle.Fill;
        input.Margin = new Padding(0, 2, 0, 6);
        panel.Controls.Add(input, 1, row);
    }

    private void AddHistory()
    {
        if (!TryCreateHistory(out var history))
        {
            return;
        }

        Histories.Add(history);
        RefreshRows(history.Id);
    }

    private void UpdateHistory()
    {
        var selected = SelectedHistory();
        if (selected is null || !TryCreateHistory(out var updated))
        {
            return;
        }

        selected.DeliveryPlace = updated.DeliveryPlace;
        selected.StartDate = updated.StartDate;
        selected.EndDate = updated.EndDate;
        RefreshRows(selected.Id);
    }

    private void DeleteHistory()
    {
        var selected = SelectedHistory();
        if (selected is null)
        {
            MessageBox.Show("削除する履歴を選択してください。");
            return;
        }

        Histories.Remove(selected);
        RefreshRows();
    }

    private bool TryCreateHistory(out DeliveryPlaceHistory history)
    {
        history = new DeliveryPlaceHistory();
        if (string.IsNullOrWhiteSpace(_deliveryPlace.Text))
        {
            MessageBox.Show("配膳場所を選択してください。");
            return false;
        }

        var endDate = _hasEndDate.Checked ? _endDate.Value.Date : (DateTime?)null;
        if (endDate is not null && endDate.Value.Date < _startDate.Value.Date)
        {
            MessageBox.Show("終了日は開始日以降にしてください。");
            return false;
        }

        history.DeliveryPlace = _deliveryPlace.Text.Trim();
        history.StartDate = _startDate.Value.Date;
        history.EndDate = endDate;
        return true;
    }

    private DeliveryPlaceHistory? SelectedHistory()
    {
        if (_grid.CurrentRow?.DataBoundItem is not HistoryRow row)
        {
            return null;
        }

        return Histories.FirstOrDefault(history => history.Id == row.Id);
    }

    private void LoadSelected()
    {
        var selected = SelectedHistory();
        if (selected is null)
        {
            return;
        }

        SelectComboText(_deliveryPlace, selected.DeliveryPlace);
        _startDate.Value = selected.StartDate;
        _hasEndDate.Checked = selected.EndDate is not null;
        _endDate.Value = selected.EndDate ?? DateTime.Today;
    }

    private void RefreshRows(Guid? selectedId = null)
    {
        _rows.Clear();
        foreach (var history in Histories.OrderBy(history => history.StartDate))
        {
            _rows.Add(HistoryRow.From(history));
        }

        if (selectedId is null)
        {
            return;
        }

        foreach (DataGridViewRow gridRow in _grid.Rows)
        {
            if (gridRow.DataBoundItem is HistoryRow row && row.Id == selectedId)
            {
                gridRow.Selected = true;
                _grid.CurrentCell = gridRow.Cells[0];
                break;
            }
        }
    }

    private static void SelectComboText(ComboBox comboBox, string value)
    {
        var index = comboBox.FindStringExact(value);
        if (index >= 0)
        {
            comboBox.SelectedIndex = index;
        }
    }

    private sealed class HistoryRow
    {
        public Guid Id { get; init; }
        public string DeliveryPlace { get; init; } = "";
        public string StartDate { get; init; } = "";
        public string EndDate { get; init; } = "";

        public static HistoryRow From(DeliveryPlaceHistory history)
        {
            return new HistoryRow
            {
                Id = history.Id,
                DeliveryPlace = history.DeliveryPlace,
                StartDate = history.StartDate.ToShortDateString(),
                EndDate = history.EndDate?.ToShortDateString() ?? ""
            };
        }
    }
}
