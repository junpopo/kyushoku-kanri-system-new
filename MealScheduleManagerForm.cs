using System.ComponentModel;

namespace KyushokuKanriSystem;

public sealed class MealScheduleManagerForm : Form
{
    private readonly BindingList<ScheduleRow> _rows = [];
    private readonly IReadOnlyCollection<Person> _people;
    private readonly DataGridView _grid = new();
    private readonly DateTimePicker _effectiveDate = new();
    private readonly ComboBox _periodType = new();
    private readonly DateTimePicker _endDate = new();
    private readonly ComboBox _scope = new();
    private readonly ComboBox _grade = new();
    private readonly ComboBox _person = new();
    private readonly ComboBox _action = new();
    private readonly TextBox _reason = new();

    public List<MealScheduleChange> Changes { get; private set; }
    public event EventHandler? ChangesSaved;

    public MealScheduleManagerForm(
        IEnumerable<MealScheduleChange> changes,
        IReadOnlyCollection<Person> people)
    {
        _people = people;
        Changes = changes.Select(Clone).ToList();

        Text = "給食開始・停止・再開管理";
        Width = 900;
        Height = 560;
        MinimumSize = new Size(760, 460);
        StartPosition = FormStartPosition.CenterParent;
        ControlBox = false;

        Controls.Add(CreateLayout());
        ConfigureInputs();
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

        var editButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false
        };
        editButtons.Controls.Add(CreateButton("追加", AddChange));
        editButtons.Controls.Add(CreateButton("修正", UpdateChange));
        editButtons.Controls.Add(CreateButton("削除", DeleteChange));
        root.Controls.Add(editButtons, 0, 2);

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
        save.Click += (_, _) => ChangesSaved?.Invoke(this, EventArgs.Empty);
        var close = new Button
        {
            Text = "閉じる",
            AutoSize = true,
            Padding = new Padding(12, 5, 12, 5)
        };
        close.Click += (_, _) => Close();
        closeButtons.Controls.Add(save);
        closeButtons.Controls.Add(close);
        AcceptButton = save;
        root.Controls.Add(closeButtons, 0, 3);
        return root;
    }

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AutoGenerateColumns = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "開始日", DataPropertyName = nameof(ScheduleRow.StartDate), FillWeight = 75 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "終了日", DataPropertyName = nameof(ScheduleRow.EndDate), FillWeight = 75 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "対象", DataPropertyName = nameof(ScheduleRow.Scope), FillWeight = 65 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "学年・組・番号・個人", DataPropertyName = nameof(ScheduleRow.Target), FillWeight = 175 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "変更", DataPropertyName = nameof(ScheduleRow.Action), FillWeight = 70 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "理由", DataPropertyName = nameof(ScheduleRow.Reason), FillWeight = 180 });
        _grid.DataSource = _rows;
        _grid.SelectionChanged += (_, _) => LoadSelected();
    }

    private Control CreateEditArea()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 4,
            RowCount = 4,
            Padding = new Padding(0, 12, 0, 0)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        AddField(panel, 0, 0, "開始日", _effectiveDate);
        AddField(panel, 0, 2, "適用方法", _periodType);
        AddField(panel, 1, 0, "終了日", _endDate);
        AddField(panel, 1, 2, "対象", _scope);
        AddField(panel, 2, 0, "学年", _grade);
        AddField(panel, 2, 2, "個人", _person);
        AddField(panel, 3, 0, "変更", _action);
        AddField(panel, 3, 2, "理由", _reason);
        return panel;
    }

    private void ConfigureInputs()
    {
        _effectiveDate.Format = DateTimePickerFormat.Short;
        _effectiveDate.ValueChanged += (_, _) =>
        {
            if (_periodType.SelectedIndex == 0 || _endDate.Value.Date < _effectiveDate.Value.Date)
            {
                _endDate.Value = _effectiveDate.Value.Date;
            }
        };

        _periodType.DropDownStyle = ComboBoxStyle.DropDownList;
        _periodType.Items.AddRange(["1日だけ", "期間指定", "終了日なし"]);
        _periodType.SelectedIndex = 0;
        _periodType.SelectedIndexChanged += (_, _) => UpdatePeriodInputs();

        _endDate.Format = DateTimePickerFormat.Short;
        _endDate.Value = _effectiveDate.Value.Date;

        _scope.DropDownStyle = ComboBoxStyle.DropDownList;
        _scope.Items.AddRange(["全体", "学年", "個人"]);
        _scope.SelectedIndex = 2;
        _scope.SelectedIndexChanged += (_, _) => UpdateTargetInputs();

        _grade.DropDownStyle = ComboBoxStyle.DropDownList;
        _grade.Items.AddRange(_people
            .Where(person => person.Type == PersonType.Student && !string.IsNullOrWhiteSpace(person.Grade))
            .Select(person => person.Grade.Trim())
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .OrderBy(SortNumber)
            .ThenBy(value => value)
            .Cast<object>()
            .ToArray());

        _person.DropDownStyle = ComboBoxStyle.DropDownList;
        _person.DisplayMember = nameof(PersonOption.Label);
        _person.ValueMember = nameof(PersonOption.Id);
        _person.DataSource = _people
            .OrderBy(person => person.Type)
            .ThenBy(person => SortNumber(person.Grade))
            .ThenBy(person => person.Grade)
            .ThenBy(person => SortNumber(person.ClassName))
            .ThenBy(person => person.ClassName)
            .ThenBy(person => SortNumber(person.StudentNumber))
            .ThenBy(person => person.FullName)
            .Select(person => new PersonOption(person.Id, PersonLabel(person)))
            .ToList();

        _action.DropDownStyle = ComboBoxStyle.DropDownList;
        _action.Items.AddRange(["開始", "停止", "再開"]);
        _action.SelectedIndex = 1;
        UpdatePeriodInputs();
        UpdateTargetInputs();
    }

    private void AddChange()
    {
        if (!TryBuildChange(out var change))
        {
            return;
        }

        Changes.Add(change);
        RefreshRows(change.Id);
    }

    private void UpdateChange()
    {
        var selected = SelectedChange();
        if (selected is null || !TryBuildChange(out var replacement))
        {
            return;
        }

        selected.EffectiveDate = replacement.EffectiveDate;
        selected.EndDate = replacement.EndDate;
        selected.Scope = replacement.Scope;
        selected.Grade = replacement.Grade;
        selected.PersonId = replacement.PersonId;
        selected.Action = replacement.Action;
        selected.Reason = replacement.Reason;
        RefreshRows(selected.Id);
    }

    private void DeleteChange()
    {
        var selected = SelectedChange();
        if (selected is null)
        {
            MessageBox.Show("削除する設定を選択してください。");
            return;
        }

        Changes.Remove(selected);
        RefreshRows();
    }

    private bool TryBuildChange(out MealScheduleChange change)
    {
        change = new MealScheduleChange
        {
            EffectiveDate = _effectiveDate.Value.Date,
            EndDate = EndDateFromInputs(),
            Scope = ScopeFromIndex(_scope.SelectedIndex),
            Action = ActionFromIndex(_action.SelectedIndex),
            Reason = _reason.Text.Trim()
        };

        if (change.EndDate is DateTime endDate && endDate.Date < change.EffectiveDate.Date)
        {
            MessageBox.Show("終了日は開始日以降の日付を指定してください。");
            return false;
        }

        if (change.Scope == MealScheduleScope.Grade)
        {
            if (_grade.SelectedItem is not string grade)
            {
                MessageBox.Show("学年を選択してください。");
                return false;
            }

            change.Grade = grade;
        }
        else if (change.Scope == MealScheduleScope.Person)
        {
            if (_person.SelectedItem is not PersonOption person)
            {
                MessageBox.Show("個人を選択してください。");
                return false;
            }

            change.PersonId = person.Id;
        }

        return true;
    }

    private void RefreshRows(Guid? selectedId = null)
    {
        _rows.Clear();
        foreach (var change in Changes
            .OrderBy(change => change.EffectiveDate)
            .ThenBy(change => ScopePriority(change.Scope)))
        {
            _rows.Add(new ScheduleRow
            {
                Id = change.Id,
                StartDate = change.EffectiveDate.ToShortDateString(),
                EndDate = EndDateLabel(change),
                Scope = ScopeLabel(change.Scope),
                Target = TargetLabel(change),
                Action = ActionLabel(change.Action),
                Reason = change.Reason
            });
        }

        if (selectedId is null)
        {
            return;
        }

        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (row.DataBoundItem is ScheduleRow item && item.Id == selectedId)
            {
                row.Selected = true;
                _grid.CurrentCell = row.Cells[0];
                break;
            }
        }
    }

    private void LoadSelected()
    {
        var change = SelectedChange();
        if (change is null)
        {
            return;
        }

        _effectiveDate.Value = change.EffectiveDate;
        _periodType.SelectedIndex = change.EndDate switch
        {
            null => 2,
            DateTime endDate when endDate.Date == change.EffectiveDate.Date => 0,
            _ => 1
        };
        _endDate.Value = change.EndDate ?? change.EffectiveDate;
        _scope.SelectedIndex = change.Scope switch
        {
            MealScheduleScope.All => 0,
            MealScheduleScope.Grade => 1,
            _ => 2
        };
        if (change.Scope == MealScheduleScope.Grade)
        {
            _grade.SelectedItem = change.Grade;
        }
        else if (change.Scope == MealScheduleScope.Person)
        {
            if (change.PersonId is Guid personId)
            {
                _person.SelectedValue = personId;
            }
        }

        _action.SelectedIndex = change.Action switch
        {
            MealScheduleAction.Start => 0,
            MealScheduleAction.Stop => 1,
            _ => 2
        };
        _reason.Text = change.Reason;
    }

    private void UpdatePeriodInputs()
    {
        _endDate.Enabled = _periodType.SelectedIndex == 1;
        if (_periodType.SelectedIndex == 0)
        {
            _endDate.Value = _effectiveDate.Value.Date;
        }
    }

    private DateTime? EndDateFromInputs()
    {
        return _periodType.SelectedIndex switch
        {
            0 => _effectiveDate.Value.Date,
            1 => _endDate.Value.Date,
            _ => null
        };
    }

    private void UpdateTargetInputs()
    {
        _grade.Enabled = _scope.SelectedIndex == 1;
        _person.Enabled = _scope.SelectedIndex == 2;
    }

    private MealScheduleChange? SelectedChange()
    {
        return _grid.CurrentRow?.DataBoundItem is ScheduleRow row
            ? Changes.FirstOrDefault(change => change.Id == row.Id)
            : null;
    }

    private string TargetLabel(MealScheduleChange change)
    {
        return change.Scope switch
        {
            MealScheduleScope.All => "全員",
            MealScheduleScope.Grade => $"{change.Grade}年",
            _ => _people.FirstOrDefault(person => person.Id == change.PersonId) is { } person
                ? PersonLabel(person)
                : "削除済み"
        };
    }

    private static MealScheduleScope ScopeFromIndex(int index)
    {
        return index switch
        {
            0 => MealScheduleScope.All,
            1 => MealScheduleScope.Grade,
            _ => MealScheduleScope.Person
        };
    }

    private static MealScheduleAction ActionFromIndex(int index)
    {
        return index switch
        {
            0 => MealScheduleAction.Start,
            2 => MealScheduleAction.Resume,
            _ => MealScheduleAction.Stop
        };
    }

    private static string ScopeLabel(MealScheduleScope scope) => scope switch
    {
        MealScheduleScope.All => "全体",
        MealScheduleScope.Grade => "学年",
        _ => "個人"
    };

    private static string ActionLabel(MealScheduleAction action) => action switch
    {
        MealScheduleAction.Start => "開始",
        MealScheduleAction.Resume => "再開",
        _ => "停止"
    };

    private static string EndDateLabel(MealScheduleChange change)
    {
        if (change.EndDate is null)
        {
            return "終了日なし";
        }

        return change.EndDate.Value.Date == change.EffectiveDate.Date
            ? "同日"
            : change.EndDate.Value.ToShortDateString();
    }

    private static int ScopePriority(MealScheduleScope scope) => scope switch
    {
        MealScheduleScope.All => 0,
        MealScheduleScope.Grade => 1,
        _ => 2
    };

    private static string PersonLabel(Person person)
    {
        if (person.Type != PersonType.Student)
        {
            return $"{person.TypeLabel}　{person.FullName}".Trim();
        }

        var grade = string.IsNullOrWhiteSpace(person.Grade) ? "未設定" : $"{person.Grade}年";
        var className = string.IsNullOrWhiteSpace(person.ClassName) ? "未設定" : $"{person.ClassName}組";
        var number = string.IsNullOrWhiteSpace(person.StudentNumber) ? "未設定" : $"{person.StudentNumber}番";
        return $"生徒　{grade}　{className}　{number}　{person.FullName}";
    }

    private static void AddField(
        TableLayoutPanel panel,
        int row,
        int column,
        string label,
        Control input)
    {
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            Padding = new Padding(0, 7, 0, 0)
        }, column, row);
        input.Dock = DockStyle.Fill;
        input.Margin = new Padding(0, 2, 8, 6);
        panel.Controls.Add(input, column + 1, row);
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

    private static MealScheduleChange Clone(MealScheduleChange change)
    {
        return new MealScheduleChange
        {
            Id = change.Id,
            EffectiveDate = change.EffectiveDate,
            EndDate = change.EndDate,
            Scope = change.Scope,
            Grade = change.Grade,
            PersonId = change.PersonId,
            Action = change.Action,
            Reason = change.Reason
        };
    }

    private static int SortNumber(string value)
    {
        return int.TryParse(value, out var number) ? number : int.MaxValue;
    }

    private sealed record PersonOption(Guid Id, string Label);

    private sealed class ScheduleRow
    {
        public Guid Id { get; init; }
        public string StartDate { get; init; } = "";
        public string EndDate { get; init; } = "";
        public string Scope { get; init; } = "";
        public string Target { get; init; } = "";
        public string Action { get; init; } = "";
        public string Reason { get; init; } = "";
    }
}
