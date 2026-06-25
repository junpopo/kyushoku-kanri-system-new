using System.ComponentModel;

namespace KyushokuKanriSystem;

public sealed class SchoolClassManagerForm : Form
{
    private readonly BindingList<SchoolClass> _rows;
    private readonly IReadOnlyCollection<Person> _people;
    private readonly int _fiscalYear;
    private readonly DataGridView _grid = new();

    public List<SchoolClass> SchoolClasses { get; private set; } = [];

    public SchoolClassManagerForm(
        IEnumerable<SchoolClass> schoolClasses,
        IReadOnlyCollection<Person> people,
        int fiscalYear)
    {
        _people = people;
        _fiscalYear = fiscalYear;
        _rows = new BindingList<SchoolClass>(schoolClasses
            .Where(item => item.FiscalYear == fiscalYear)
            .Select(Clone)
            .OrderBy(item => SortNumber(item.Grade))
            .ThenBy(item => item.Grade)
            .ThenBy(item => SortNumber(item.ClassName))
            .ThenBy(item => item.ClassName)
            .ToList());

        Text = "学年・クラス管理";
        Width = 520;
        Height = 500;
        MinimumSize = new Size(460, 380);
        StartPosition = FormStartPosition.CenterParent;
        ControlBox = false;

        Controls.Add(CreateLayout());
        AddClassesFromRoster();
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
            Text = $"年度: {_fiscalYear}年度",
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold),
            Padding = new Padding(0, 7, 12, 0)
        });
        tools.Controls.Add(CreateButton("名簿から追加", AddClassesFromRoster));
        tools.Controls.Add(CreateButton("選択行を削除", DeleteSelectedRow));

        ConfigureGrid();

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft
        };
        var save = new Button { Text = "保存", AutoSize = true, Padding = new Padding(16, 5, 16, 5) };
        save.Click += (_, _) => SaveAndClose();
        var cancel = new Button
        {
            Text = "キャンセル",
            DialogResult = DialogResult.Cancel,
            AutoSize = true,
            Padding = new Padding(12, 5, 12, 5)
        };
        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);
        AcceptButton = save;
        CancelButton = cancel;

        root.Controls.Add(tools, 0, 0);
        root.Controls.Add(_grid, 0, 1);
        root.Controls.Add(buttons, 0, 2);
        return root;
    }

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.AutoGenerateColumns = false;
        _grid.AllowUserToAddRows = true;
        _grid.AllowUserToDeleteRows = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "学年",
            DataPropertyName = nameof(SchoolClass.Grade)
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "組",
            DataPropertyName = nameof(SchoolClass.ClassName)
        });
        _grid.DataSource = _rows;
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

    private void AddClassesFromRoster()
    {
        _grid.EndEdit();
        foreach (var group in _people
            .Where(person =>
                person.Type == PersonType.Student &&
                !string.IsNullOrWhiteSpace(person.Grade) &&
                !string.IsNullOrWhiteSpace(person.ClassName))
            .GroupBy(person => new
            {
                Grade = person.Grade.Trim(),
                ClassName = person.ClassName.Trim()
            })
            .OrderBy(group => SortNumber(group.Key.Grade))
            .ThenBy(group => group.Key.Grade)
            .ThenBy(group => SortNumber(group.Key.ClassName))
            .ThenBy(group => group.Key.ClassName))
        {
            if (_rows.Any(item =>
                item.Grade.Equals(group.Key.Grade, StringComparison.CurrentCultureIgnoreCase) &&
                item.ClassName.Equals(group.Key.ClassName, StringComparison.CurrentCultureIgnoreCase)))
            {
                continue;
            }

            _rows.Add(new SchoolClass
            {
                FiscalYear = _fiscalYear,
                Grade = group.Key.Grade,
                ClassName = group.Key.ClassName
            });
        }
    }

    private void DeleteSelectedRow()
    {
        if (_grid.CurrentRow?.DataBoundItem is not SchoolClass selected)
        {
            MessageBox.Show("削除するクラスを選択してください。");
            return;
        }

        var usedCount = _people.Count(person =>
            person.Type == PersonType.Student &&
            person.Grade.Equals(selected.Grade, StringComparison.CurrentCultureIgnoreCase) &&
            person.ClassName.Equals(selected.ClassName, StringComparison.CurrentCultureIgnoreCase));
        if (usedCount > 0)
        {
            MessageBox.Show(
                $"{selected.Grade}年{selected.ClassName}組は名簿で{usedCount}人が使用中のため削除できません。",
                "削除不可",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
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
            .Where(item =>
                !string.IsNullOrWhiteSpace(item.Grade) ||
                !string.IsNullOrWhiteSpace(item.ClassName))
            .Select(item => new SchoolClass
            {
                FiscalYear = _fiscalYear,
                Grade = item.Grade.Trim(),
                ClassName = item.ClassName.Trim()
            })
            .ToList();
        if (normalized.Any(item => item.Grade.Length == 0 || item.ClassName.Length == 0))
        {
            MessageBox.Show("学年と組を入力してください。", "入力確認",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var duplicate = normalized
            .GroupBy(
                item => $"{item.Grade}\u001f{item.ClassName}",
                StringComparer.CurrentCultureIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            var item = duplicate.First();
            MessageBox.Show($"{item.Grade}年{item.ClassName}組が重複しています。", "入力確認",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SchoolClasses = normalized
            .OrderBy(item => SortNumber(item.Grade))
            .ThenBy(item => item.Grade)
            .ThenBy(item => SortNumber(item.ClassName))
            .ThenBy(item => item.ClassName)
            .ToList();
        DialogResult = DialogResult.OK;
        Close();
    }

    private static SchoolClass Clone(SchoolClass item)
    {
        return new SchoolClass
        {
            FiscalYear = item.FiscalYear,
            Grade = item.Grade,
            ClassName = item.ClassName
        };
    }

    private static int SortNumber(string value)
    {
        return int.TryParse(value, out var number) ? number : int.MaxValue;
    }
}
