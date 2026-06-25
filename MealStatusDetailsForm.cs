namespace KyushokuKanriSystem;

public sealed class MealStatusDetailsForm : Form
{
    public MealStatusDetailsForm(
        DateTime date,
        string deliveryPlace,
        IReadOnlyCollection<MainForm.MealStatusDetail> details,
        string detailLabel = "停止・欠席")
    {
        Text = $"{detailLabel}の詳細";
        Width = 850;
        Height = 480;
        MinimumSize = new Size(700, 380);
        StartPosition = FormStartPosition.CenterParent;
        ControlBox = false;

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
            Text = $"{date:yyyy年M月d日}  {deliveryPlace}  {detailLabel}: {details.Count}人",
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        };

        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoGenerateColumns = false,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            DataSource = details.ToList()
        };
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "区分", DataPropertyName = nameof(MainForm.MealStatusDetail.Type), FillWeight = 70 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "学年", DataPropertyName = nameof(MainForm.MealStatusDetail.Grade), FillWeight = 45 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "組", DataPropertyName = nameof(MainForm.MealStatusDetail.ClassName), FillWeight = 45 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "番号", DataPropertyName = nameof(MainForm.MealStatusDetail.StudentNumber), FillWeight = 50 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "氏名", DataPropertyName = nameof(MainForm.MealStatusDetail.Name), FillWeight = 110 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "配膳場所", DataPropertyName = nameof(MainForm.MealStatusDetail.DeliveryPlace), FillWeight = 95 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "状態", DataPropertyName = nameof(MainForm.MealStatusDetail.Status), FillWeight = 60 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "理由", DataPropertyName = nameof(MainForm.MealStatusDetail.Reason), FillWeight = 170 });

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
}
