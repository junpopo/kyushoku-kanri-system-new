using System.ComponentModel;

namespace KyushokuKanriSystem;

public sealed class DeliveryPlaceBasicCountForm : Form
{
    private readonly List<DeliveryPlaceBasicCount> _allCounts;
    private readonly IReadOnlyCollection<string> _deliveryPlaces;
    private readonly IReadOnlyCollection<Person> _people;
    private readonly BindingList<DeliveryPlaceBasicCount> _rows = [];
    private readonly DataGridView _grid = new();
    private int _loadedFiscalYear;

    public List<DeliveryPlaceBasicCount> DeliveryPlaceBasicCounts { get; private set; } = [];

    public DeliveryPlaceBasicCountForm(
        IEnumerable<DeliveryPlaceBasicCount> basicCounts,
        IReadOnlyCollection<string> deliveryPlaces,
        IReadOnlyCollection<Person> people,
        int initialFiscalYear)
    {
        _deliveryPlaces = deliveryPlaces;
        _people = people;
        _loadedFiscalYear = initialFiscalYear;
        _allCounts = basicCounts.Select(item =>
        {
            var copy = item.FiscalYear != 0
                ? Clone(item)
                : CreateForecastRow(
                    _loadedFiscalYear,
                    item.DeliveryPlace,
                    "生徒",
                    item.BasicCount);
            if (string.IsNullOrWhiteSpace(copy.Category))
            {
                copy.Category = "生徒";
            }

            return copy;
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
            Text = $"年度: {_loadedFiscalYear}年度",
            AutoSize = true,
            Padding = new Padding(0, 7, 12, 0),
            Font = new Font(Font, FontStyle.Bold)
        });
        tools.Controls.Add(CreateButton("4月名簿から12か月作成", CreateForecastFromAprilRoster));
        tools.Controls.Add(CreateButton("配膳場所を追加", AddMissingDeliveryPlaces));

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
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "配膳場所",
            DataPropertyName = nameof(DeliveryPlaceBasicCount.DeliveryPlace),
            Width = 145,
            Frozen = true,
            ReadOnly = true
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "区分",
            DataPropertyName = nameof(DeliveryPlaceBasicCount.Category),
            Width = 70,
            Frozen = true,
            ReadOnly = true
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
        _grid.CellEndEdit += (_, eventArgs) =>
        {
            if (eventArgs.ColumnIndex < 2 ||
                eventArgs.ColumnIndex > 13 ||
                eventArgs.RowIndex < 0 ||
                _grid.Rows[eventArgs.RowIndex].DataBoundItem is not DeliveryPlaceBasicCount item)
            {
                return;
            }

            var editedMonthIndex = eventArgs.ColumnIndex - 2;
            var value = GetMonthValue(item, editedMonthIndex);
            for (var monthIndex = editedMonthIndex + 1; monthIndex < 12; monthIndex++)
            {
                SetMonthValue(item, monthIndex, value);
                _grid.Rows[eventArgs.RowIndex].Cells[monthIndex + 2].Value = value;
            }

            _grid.InvalidateRow(eventArgs.RowIndex);
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
            .OrderBy(item => DeliveryPlaceSortKey(item.DeliveryPlace))
            .ThenBy(item => item.DeliveryPlace)
            .ThenBy(item => CategorySortKey(item.Category)))
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
        var fiscalYear = _loadedFiscalYear;
        var aprilDate = new DateTime(fiscalYear, 4, 1);
        var counts = _people
            .Where(person =>
                IsActive(person, aprilDate) &&
                person.Type != PersonType.Tasting)
            .GroupBy(person => new
            {
                Place = NormalizePlace(person.GetDeliveryPlace(aprilDate)),
                Category = BasicCategory(person)
            })
            .ToDictionary(
                group => $"{group.Key.Place}\u001f{group.Key.Category}",
                group => group.Count(),
                StringComparer.CurrentCultureIgnoreCase);
        var places = KnownPlaces(aprilDate).ToList();

        _rows.Clear();
        foreach (var place in places)
        {
            foreach (var category in new[] { "生徒", "職員" })
            {
                var aprilCount = counts.GetValueOrDefault($"{place}\u001f{category}");
                _rows.Add(CreateForecastRow(fiscalYear, place, category, aprilCount));
            }
        }
    }

    private void AddMissingDeliveryPlaces()
    {
        _grid.EndEdit();
        var fiscalYear = _loadedFiscalYear;
        var aprilDate = new DateTime(fiscalYear, 4, 1);
        var counts = _people
            .Where(person =>
                IsActive(person, aprilDate) &&
                person.Type != PersonType.Tasting)
            .GroupBy(person => new
            {
                Place = NormalizePlace(person.GetDeliveryPlace(aprilDate)),
                Category = BasicCategory(person)
            })
            .ToDictionary(
                group => $"{group.Key.Place}\u001f{group.Key.Category}",
                group => group.Count(),
                StringComparer.CurrentCultureIgnoreCase);

        foreach (var place in KnownPlaces(aprilDate))
        {
            foreach (var category in new[] { "生徒", "職員" })
            {
                if (_rows.Any(item =>
                    item.DeliveryPlace.Trim().Equals(place, StringComparison.CurrentCultureIgnoreCase) &&
                    item.Category.Equals(category, StringComparison.CurrentCultureIgnoreCase)))
                {
                    continue;
                }

                _rows.Add(CreateForecastRow(
                    fiscalYear,
                    place,
                    category,
                    counts.GetValueOrDefault($"{place}\u001f{category}")));
            }
        }
    }

    private IEnumerable<string> KnownPlaces(DateTime aprilDate)
    {
        return _deliveryPlaces
            .Concat(_people.Select(person => person.GetDeliveryPlace(aprilDate)))
            .Where(place => !string.IsNullOrWhiteSpace(place))
            .Select(NormalizePlace)
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .OrderBy(DeliveryPlaceSortKey)
            .ThenBy(place => place);
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
                copy.Category = copy.Category.Trim();
                return copy;
            })
            .ToList();

        if (normalized.Any(item => item.Category is not ("生徒" or "職員")))
        {
            MessageBox.Show("区分は生徒または職員を選択してください。", "入力確認",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (normalized.Any(HasNegativeCount))
        {
            MessageBox.Show("基本数には0以上の整数を入力してください。", "入力確認",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        var duplicate = normalized
            .GroupBy(
                item => $"{item.DeliveryPlace}\u001f{item.Category}",
                StringComparer.CurrentCultureIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            var item = duplicate.First();
            MessageBox.Show($"{item.DeliveryPlace}の{item.Category}が重複しています。", "入力確認",
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
            .ThenBy(item => DeliveryPlaceSortKey(item.DeliveryPlace))
            .ThenBy(item => item.DeliveryPlace)
            .ThenBy(item => CategorySortKey(item.Category))
            .Select(Clone)
            .ToList();
        DialogResult = DialogResult.OK;
        Close();
    }

    private static DeliveryPlaceBasicCount CreateForecastRow(
        int fiscalYear,
        string deliveryPlace,
        string category,
        int count)
    {
        return new DeliveryPlaceBasicCount
        {
            FiscalYear = fiscalYear,
            DeliveryPlace = deliveryPlace,
            Category = category,
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
            Category = item.Category,
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

    private static int GetMonthValue(DeliveryPlaceBasicCount item, int monthIndex)
    {
        return monthIndex switch
        {
            0 => item.April,
            1 => item.May,
            2 => item.June,
            3 => item.July,
            4 => item.August,
            5 => item.September,
            6 => item.October,
            7 => item.November,
            8 => item.December,
            9 => item.January,
            10 => item.February,
            _ => item.March
        };
    }

    private static void SetMonthValue(
        DeliveryPlaceBasicCount item,
        int monthIndex,
        int value)
    {
        switch (monthIndex)
        {
            case 0: item.April = value; break;
            case 1: item.May = value; break;
            case 2: item.June = value; break;
            case 3: item.July = value; break;
            case 4: item.August = value; break;
            case 5: item.September = value; break;
            case 6: item.October = value; break;
            case 7: item.November = value; break;
            case 8: item.December = value; break;
            case 9: item.January = value; break;
            case 10: item.February = value; break;
            default: item.March = value; break;
        }
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

    private static string BasicCategory(Person person)
    {
        return person.Type == PersonType.Student ? "生徒" : "職員";
    }

    private static int CategorySortKey(string category)
    {
        return category == "生徒" ? 0 : 1;
    }

    private static int DeliveryPlaceSortKey(string deliveryPlace)
    {
        if (NormalizePlace(deliveryPlace)
            .Equals("職員室", StringComparison.CurrentCultureIgnoreCase))
        {
            return 20000;
        }

        var match = System.Text.RegularExpressions.Regex.Match(
            deliveryPlace,
            @"(?<grade>\d+)年(?<class>\d+)組");
        return match.Success
            ? int.Parse(match.Groups["grade"].Value) * 100 +
              int.Parse(match.Groups["class"].Value)
            : 10000;
    }

}
