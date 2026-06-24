using System.ComponentModel;

namespace KyushokuKanriSystem;

public sealed class DeliveryPlaceBasicCountForm : Form
{
    private readonly List<DeliveryPlaceBasicCount> _allCounts;
    private readonly IReadOnlyCollection<string> _deliveryPlaces;
    private readonly IReadOnlyCollection<Person> _people;
    private readonly BindingList<DeliveryPlaceBasicCount> _rows = [];
    private readonly DataGridView _grid = new();
    private readonly NumericUpDown _fiscalYear = new();
    private int _loadedFiscalYear;
    private bool _loadingYear;

    public List<DeliveryPlaceBasicCount> DeliveryPlaceBasicCounts { get; private set; } = [];

    public DeliveryPlaceBasicCountForm(
        IEnumerable<DeliveryPlaceBasicCount> basicCounts,
        IReadOnlyCollection<string> deliveryPlaces,
        IReadOnlyCollection<Person> people)
    {
        _deliveryPlaces = deliveryPlaces;
        _people = people;
        _loadedFiscalYear = CurrentFiscalYear();
        _allCounts = basicCounts.Select(item =>
        {
            if (item.FiscalYear != 0)
            {
                return Clone(item);
            }

            return CreateForecastRow(
                _loadedFiscalYear,
                item.DeliveryPlace,
                item.BasicCount);
        }).ToList();

        Text = "配膳別基本数（月別）";
        Width = 1120;
        Height = 540;
        MinimumSize = new Size(900, 420);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = false;

        Controls.Add(CreateLayout());
        LoadFiscalYear(_loadedFiscalYear);
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
        tools.Controls.Add(new Label
        {
            Text = "年度",
            AutoSize = true,
            Padding = new Padding(0, 7, 4, 0)
        });
        _fiscalYear.Minimum = 2000;
        _fiscalYear.Maximum = 2100;
        _fiscalYear.Value = _loadedFiscalYear;
        _fiscalYear.Width = 75;
        _fiscalYear.ValueChanged += (_, _) =>
        {
            if (_loadingYear)
            {
                return;
            }

            if (!StoreCurrentYear())
            {
                _loadingYear = true;
                _fiscalYear.Value = _loadedFiscalYear;
                _loadingYear = false;
                return;
            }

            LoadFiscalYear((int)_fiscalYear.Value);
        };
        tools.Controls.Add(_fiscalYear);
        tools.Controls.Add(CreateButton("4月名簿から12か月作成", CreateForecastFromAprilRoster));
        tools.Controls.Add(CreateButton("配膳場所を追加", AddMissingDeliveryPlaces));
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
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "配膳場所",
            DataPropertyName = nameof(DeliveryPlaceBasicCount.DeliveryPlace),
            Width = 145,
            Frozen = true
        });
        AddMonthColumn("4月", nameof(DeliveryPlaceBasicCount.April));
        AddMonthColumn("5月", nameof(DeliveryPlaceBasicCount.May));
        AddMonthColumn("6月", nameof(DeliveryPlaceBasicCount.June));
        AddMonthColumn("7月", nameof(DeliveryPlaceBasicCount.July));
        AddMonthColumn("8月", nameof(DeliveryPlaceBasicCount.August));
        AddMonthColumn("9月", nameof(DeliveryPlaceBasicCount.September));
        AddMonthColumn("10月", nameof(DeliveryPlaceBasicCount.October));
        AddMonthColumn("11月", nameof(DeliveryPlaceBasicCount.November));
        AddMonthColumn("12月", nameof(DeliveryPlaceBasicCount.December));
        AddMonthColumn("1月", nameof(DeliveryPlaceBasicCount.January));
        AddMonthColumn("2月", nameof(DeliveryPlaceBasicCount.February));
        AddMonthColumn("3月", nameof(DeliveryPlaceBasicCount.March));
        _grid.DataSource = _rows;
        _grid.DataError += (_, eventArgs) =>
        {
            MessageBox.Show("基本数には0以上の整数を入力してください。", "入力確認",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            eventArgs.ThrowException = false;
        };
    }

    private void AddMonthColumn(string header, string propertyName)
    {
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = header,
            DataPropertyName = propertyName,
            Width = 65,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleRight,
                Format = "N0"
            }
        });
    }

    private static Button CreateButton(string text, Action action)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            Margin = new Padding(8, 0, 0, 8),
            Padding = new Padding(10, 5, 10, 5)
        };
        button.Click += (_, _) => action();
        return button;
    }

    private void LoadFiscalYear(int fiscalYear)
    {
        _loadedFiscalYear = fiscalYear;
        _rows.Clear();
        foreach (var item in _allCounts
            .Where(item => item.FiscalYear == fiscalYear)
            .OrderBy(item => item.DeliveryPlace))
        {
            _rows.Add(Clone(item));
        }

        if (_rows.Count == 0)
        {
            CreateForecastFromAprilRoster();
        }
        else
        {
            AddMissingDeliveryPlaces();
        }
    }

    private void CreateForecastFromAprilRoster()
    {
        _grid.EndEdit();
        var fiscalYear = (int)_fiscalYear.Value;
        var aprilDate = new DateTime(fiscalYear, 4, 1);
        var counts = _people
            .Where(person => IsActive(person, aprilDate))
            .GroupBy(person => NormalizePlace(person.GetDeliveryPlace(aprilDate)))
            .ToDictionary(
                group => group.Key,
                group => group.Count(),
                StringComparer.CurrentCultureIgnoreCase);
        var places = KnownPlaces(aprilDate).ToList();

        _rows.Clear();
        foreach (var place in places)
        {
            var aprilCount = counts.GetValueOrDefault(place);
            _rows.Add(CreateForecastRow(fiscalYear, place, aprilCount));
        }
    }

    private void AddMissingDeliveryPlaces()
    {
        _grid.EndEdit();
        var fiscalYear = (int)_fiscalYear.Value;
        var aprilDate = new DateTime(fiscalYear, 4, 1);
        var counts = _people
            .Where(person => IsActive(person, aprilDate))
            .GroupBy(person => NormalizePlace(person.GetDeliveryPlace(aprilDate)))
            .ToDictionary(
                group => group.Key,
                group => group.Count(),
                StringComparer.CurrentCultureIgnoreCase);

        foreach (var place in KnownPlaces(aprilDate))
        {
            if (_rows.Any(item =>
                item.DeliveryPlace.Trim().Equals(place, StringComparison.CurrentCultureIgnoreCase)))
            {
                continue;
            }

            _rows.Add(CreateForecastRow(fiscalYear, place, counts.GetValueOrDefault(place)));
        }
    }

    private IEnumerable<string> KnownPlaces(DateTime aprilDate)
    {
        return _deliveryPlaces
            .Concat(_people.Select(person => person.GetDeliveryPlace(aprilDate)))
            .Where(place => !string.IsNullOrWhiteSpace(place))
            .Select(NormalizePlace)
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .OrderBy(place => place);
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

    private bool StoreCurrentYear()
    {
        if (!_grid.EndEdit())
        {
            return false;
        }

        var normalized = _rows
            .Where(item => !string.IsNullOrWhiteSpace(item.DeliveryPlace))
            .Select(item =>
            {
                var copy = Clone(item);
                copy.FiscalYear = _loadedFiscalYear;
                copy.DeliveryPlace = copy.DeliveryPlace.Trim();
                return copy;
            })
            .ToList();

        if (normalized.Any(HasNegativeCount))
        {
            MessageBox.Show("基本数には0以上の整数を入力してください。", "入力確認",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        var duplicate = normalized
            .GroupBy(item => item.DeliveryPlace, StringComparer.CurrentCultureIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            MessageBox.Show($"{duplicate.Key}が重複しています。", "入力確認",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        _allCounts.RemoveAll(item => item.FiscalYear == _loadedFiscalYear);
        _allCounts.AddRange(normalized);
        return true;
    }

    private void SaveAndClose()
    {
        if (!StoreCurrentYear())
        {
            return;
        }

        DeliveryPlaceBasicCounts = _allCounts
            .OrderBy(item => item.FiscalYear)
            .ThenBy(item => item.DeliveryPlace)
            .Select(Clone)
            .ToList();
        DialogResult = DialogResult.OK;
        Close();
    }

    private static DeliveryPlaceBasicCount CreateForecastRow(
        int fiscalYear,
        string deliveryPlace,
        int count)
    {
        return new DeliveryPlaceBasicCount
        {
            FiscalYear = fiscalYear,
            DeliveryPlace = deliveryPlace,
            April = count,
            May = count,
            June = count,
            July = count,
            August = count,
            September = count,
            October = count,
            November = count,
            December = count,
            January = count,
            February = count,
            March = count
        };
    }

    private static DeliveryPlaceBasicCount Clone(DeliveryPlaceBasicCount item)
    {
        return new DeliveryPlaceBasicCount
        {
            FiscalYear = item.FiscalYear,
            DeliveryPlace = item.DeliveryPlace,
            April = item.April,
            May = item.May,
            June = item.June,
            July = item.July,
            August = item.August,
            September = item.September,
            October = item.October,
            November = item.November,
            December = item.December,
            January = item.January,
            February = item.February,
            March = item.March
        };
    }

    private static bool HasNegativeCount(DeliveryPlaceBasicCount item)
    {
        return item.April < 0 || item.May < 0 || item.June < 0 ||
               item.July < 0 || item.August < 0 || item.September < 0 ||
               item.October < 0 || item.November < 0 || item.December < 0 ||
               item.January < 0 || item.February < 0 || item.March < 0;
    }

    private static bool IsActive(Person person, DateTime date)
    {
        return person.ActiveFrom.Date <= date.Date &&
               (person.ActiveTo is null || person.ActiveTo.Value.Date >= date.Date);
    }

    private static string NormalizePlace(string place)
    {
        return string.IsNullOrWhiteSpace(place) ? "未設定" : place.Trim();
    }

    private static int CurrentFiscalYear()
    {
        var today = DateTime.Today;
        return today.Month >= 4 ? today.Year : today.Year - 1;
    }
}
