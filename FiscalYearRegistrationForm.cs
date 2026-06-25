namespace KyushokuKanriSystem;

public sealed class FiscalYearRegistrationForm : Form
{
    private readonly NumericUpDown _fiscalYear = new();

    public int FiscalYear { get; private set; }

    public FiscalYearRegistrationForm(int fiscalYear)
    {
        FiscalYear = fiscalYear;
        Text = "年度登録";
        ClientSize = new Size(300, 135);
        StartPosition = FormStartPosition.CenterParent;
        ControlBox = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        Controls.Add(CreateLayout(fiscalYear));
    }

    private Control CreateLayout(int fiscalYear)
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(16)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var label = new Label
        {
            Text = "給食を管理する年度を登録してください。",
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 8)
        };
        var yearPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false
        };
        _fiscalYear.Minimum = 2000;
        _fiscalYear.Maximum = 2100;
        _fiscalYear.Value = fiscalYear;
        _fiscalYear.Width = 90;
        yearPanel.Controls.Add(_fiscalYear);
        yearPanel.Controls.Add(new Label
        {
            Text = "年度（4月～翌年3月）",
            AutoSize = true,
            Padding = new Padding(4, 5, 0, 0)
        });

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            Margin = new Padding(0, 10, 0, 0)
        };
        var register = new Button
        {
            Text = "登録",
            AutoSize = true,
            Padding = new Padding(14, 4, 14, 4)
        };
        register.Click += (_, _) =>
        {
            FiscalYear = (int)_fiscalYear.Value;
            DialogResult = DialogResult.OK;
            Close();
        };
        var cancel = new Button
        {
            Text = "キャンセル",
            DialogResult = DialogResult.Cancel,
            AutoSize = true,
            Padding = new Padding(10, 4, 10, 4)
        };
        buttons.Controls.Add(register);
        buttons.Controls.Add(cancel);
        AcceptButton = register;
        CancelButton = cancel;

        root.Controls.Add(label, 0, 0);
        root.Controls.Add(yearPanel, 0, 1);
        root.Controls.Add(buttons, 0, 2);
        return root;
    }
}
