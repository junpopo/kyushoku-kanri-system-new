namespace KyushokuKanriSystem;

public sealed class ServedPeopleDetailsForm : Form
{
    public ServedPeopleDetailsForm(
        DateTime date,
        string deliveryPlace,
        IReadOnlyCollection<Person> people)
    {
        Text = "喫食者の詳細";
        Width = 720;
        Height = 480;
        MinimumSize = new Size(620, 360);
        StartPosition = FormStartPosition.CenterParent;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var title = new Label
        {
            Text = $"{date:yyyy年M月d日}  {deliveryPlace}  喫食者: {people.Count}人",
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        };

        var rows = people.Select(person => new ServedPersonRow
        {
            Type = person.TypeLabel,
            Name = person.FullName,
            HasMilk = person.HasMilk,
            HasAllergySupport = person.HasAllergySupport,
            Memo = person.Memo
        }).ToList();

        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoGenerateColumns = false,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            DataSource = rows
        };
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "区分", DataPropertyName = nameof(ServedPersonRow.Type), FillWeight = 70 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "氏名", DataPropertyName = nameof(ServedPersonRow.Name), FillWeight = 120 });
        grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "牛乳", DataPropertyName = nameof(ServedPersonRow.HasMilk), FillWeight = 45 });
        grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "アレルギー対応", DataPropertyName = nameof(ServedPersonRow.HasAllergySupport), FillWeight = 80 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "備考", DataPropertyName = nameof(ServedPersonRow.Memo), FillWeight = 160 });

        var closePanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 8, 0, 0)
        };
        var close = new Button
        {
            Text = "閉じる",
            DialogResult = DialogResult.OK,
            AutoSize = true,
            Padding = new Padding(16, 5, 16, 5)
        };
        closePanel.Controls.Add(close);
        AcceptButton = close;
        CancelButton = close;

        root.Controls.Add(title, 0, 0);
        root.Controls.Add(grid, 0, 1);
        root.Controls.Add(closePanel, 0, 2);
        Controls.Add(root);
    }

    private sealed class ServedPersonRow
    {
        public string Type { get; init; } = "";
        public string Name { get; init; } = "";
        public bool HasMilk { get; init; }
        public bool HasAllergySupport { get; init; }
        public string Memo { get; init; } = "";
    }
}
