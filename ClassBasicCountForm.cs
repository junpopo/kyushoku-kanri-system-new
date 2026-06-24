using System.ComponentModel;

namespace KyushokuKanriSystem;

public sealed class ClassBasicCountForm : Form
{
    private readonly BindingList<ClassBasicCount> _rows;
    private readonly IReadOnlyCollection<Person> _people;
    private readonly DataGridView _grid = new();

    public List<ClassBasicCount> ClassBasicCounts { get; private set; } = [];

    public ClassBasicCountForm(
        IEnumerable<ClassBasicCount> classBasicCounts,
        IReadOnlyCollection<Person> people)
    {
        _people = people;
        _rows = new BindingList<ClassBasicCount>(classBasicCounts
            .Select(item => new ClassBasicCount
            {
                Grade = item.Grade,
                ClassName = item.ClassName,
                BasicCount = item.BasicCount
            })
            .OrderBy(item => SortNumber(item.Grade))
            .ThenBy(item => item.Grade)
            .ThenBy(item => SortNumber(item.ClassName))
            .ThenBy(item => item.ClassName)
            .ToList());

        Text = "クラス別基本数";
        Width = 520;
        Height = 500;
        MinimumSize = new Size(460, 380);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;
        MinimizeBox = false;

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
        tools.Controls.Add(CreateButton("名簿からクラスを追加", AddClassesFromRoster));
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
            HeaderText = "学年",
            DataPropertyName = nameof(ClassBasicCount.Grade),
            FillWeight = 70
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "組",
            DataPropertyName = nameof(ClassBasicCount.ClassName),
            FillWeight = 70
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "基本数",
            DataPropertyName = nameof(ClassBasicCount.BasicCount),
            FillWeight = 100
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

    private void AddClassesFromRoster()
    {
        _grid.EndEdit();
        var classes = _people
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
            .ThenBy(group => group.Key.ClassName);

        foreach (var classGroup in classes)
        {
            var alreadyExists = _rows.Any(item =>
                item.Grade.Trim().Equals(classGroup.Key.Grade, StringComparison.CurrentCultureIgnoreCase) &&
                item.ClassName.Trim().Equals(classGroup.Key.ClassName, StringComparison.CurrentCultureIgnoreCase));
            if (alreadyExists)
            {
                continue;
            }

            _rows.Add(new ClassBasicCount
            {
                Grade = classGroup.Key.Grade,
                ClassName = classGroup.Key.ClassName,
                BasicCount = classGroup.Count()
            });
        }
    }

    private void DeleteSelectedRow()
    {
        if (_grid.CurrentRow?.DataBoundItem is not ClassBasicCount selected)
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
            .Where(item =>
                !string.IsNullOrWhiteSpace(item.Grade) ||
                !string.IsNullOrWhiteSpace(item.ClassName))
            .Select(item => new ClassBasicCount
            {
                Grade = item.Grade.Trim(),
                ClassName = item.ClassName.Trim(),
                BasicCount = item.BasicCount
            })
            .ToList();

        if (normalized.Any(item =>
            item.Grade.Length == 0 ||
            item.ClassName.Length == 0))
        {
            MessageBox.Show("学年と組を入力してください。", "入力確認",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (normalized.Any(item => item.BasicCount < 0))
        {
            MessageBox.Show("基本数には0以上の整数を入力してください。", "入力確認",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var duplicate = normalized
            .GroupBy(item => $"{item.Grade}\u001f{item.ClassName}",
                StringComparer.CurrentCultureIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            var item = duplicate.First();
            MessageBox.Show($"{item.Grade}年{item.ClassName}組が重複しています。", "入力確認",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        ClassBasicCounts = normalized
            .OrderBy(item => SortNumber(item.Grade))
            .ThenBy(item => item.Grade)
            .ThenBy(item => SortNumber(item.ClassName))
            .ThenBy(item => item.ClassName)
            .ToList();
        DialogResult = DialogResult.OK;
        Close();
    }

    private static int SortNumber(string value)
    {
        return int.TryParse(value, out var number) ? number : int.MaxValue;
    }
}
