namespace KyushokuKanriSystem;

public sealed class LoginForm : Form
{
    private readonly IReadOnlyCollection<AppUser> _users;
    private readonly TextBox _loginId = new();
    private readonly TextBox _password = new();
    private readonly ComboBox _loginType = new();
    private readonly Label _message = new();

    public AppUser? LoggedInUser { get; private set; }

    public LoginForm(IReadOnlyCollection<AppUser> users)
    {
        _users = users;
        Text = "ログイン";
        Width = 420;
        Height = 300;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        Controls.Add(CreateLayout());
    }

    private Control CreateLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(24)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var title = new Label
        {
            Text = "給食管理システム",
            Dock = DockStyle.Fill,
            Font = new Font(Font.FontFamily, 15, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 18)
        };

        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 4
        };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _loginId.Dock = DockStyle.Fill;
        _password.Dock = DockStyle.Fill;
        _password.UseSystemPasswordChar = true;
        _loginType.Dock = DockStyle.Fill;
        _loginType.DropDownStyle = ComboBoxStyle.DropDownList;
        _loginType.Items.AddRange(["管理者", "一般利用者（閲覧のみ）"]);
        _loginType.SelectedIndex = 0;
        _message.ForeColor = Color.Firebrick;
        _message.AutoSize = true;

        AddRow(fields, 0, "ログイン区分", _loginType);
        AddRow(fields, 1, "ログインID", _loginId);
        AddRow(fields, 2, "パスワード", _password);
        fields.Controls.Add(_message, 1, 3);
        _loginType.SelectedIndexChanged += (_, _) => UpdateCredentialFields(fields);
        UpdateCredentialFields(fields);

        var buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            AutoSize = true
        };
        var login = new Button { Text = "ログイン", AutoSize = true, Padding = new Padding(16, 6, 16, 6) };
        var cancel = new Button { Text = "終了", DialogResult = DialogResult.Cancel, AutoSize = true, Padding = new Padding(16, 6, 16, 6) };
        login.Click += (_, _) => TryLogin();
        buttons.Controls.Add(login);
        buttons.Controls.Add(cancel);

        AcceptButton = login;
        CancelButton = cancel;

        root.Controls.Add(title, 0, 0);
        root.Controls.Add(fields, 0, 1);
        root.Controls.Add(buttons, 0, 2);
        return root;
    }

    private static void AddRow(TableLayoutPanel panel, int row, string labelText, Control input)
    {
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.Controls.Add(new Label { Text = labelText, AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, row);
        input.Margin = new Padding(0, 2, 0, 8);
        panel.Controls.Add(input, 1, row);
    }

    private void TryLogin()
    {
        if (_loginType.SelectedIndex == 1)
        {
            LoggedInUser = _users.FirstOrDefault(user => user.IsActive && user.Role == UserRole.User)
                ?? new AppUser
                {
                    DisplayName = "閲覧利用者",
                    Role = UserRole.User,
                    IsActive = true
                };
            DialogResult = DialogResult.OK;
            Close();
            return;
        }

        var loginId = _loginId.Text.Trim();
        var user = _users.FirstOrDefault(user =>
            user.IsActive &&
            user.LoginId.Equals(loginId, StringComparison.OrdinalIgnoreCase));

        if (user is null || !PasswordHasher.Verify(_password.Text, user.PasswordHash))
        {
            _message.Text = "ログインIDまたはパスワードが違います。";
            _password.Clear();
            _password.Focus();
            return;
        }

        var expectedRole = _loginType.SelectedIndex == 0 ? UserRole.Admin : UserRole.User;
        if (user.Role != expectedRole)
        {
            _message.Text = expectedRole == UserRole.Admin
                ? "この利用者は管理者ではありません。"
                : "管理者アカウントは「管理者」を選択してください。";
            return;
        }

        LoggedInUser = user;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void UpdateCredentialFields(TableLayoutPanel fields)
    {
        var showCredentials = _loginType.SelectedIndex == 0;
        for (var row = 1; row <= 2; row++)
        {
            fields.RowStyles[row].SizeType = showCredentials ? SizeType.AutoSize : SizeType.Absolute;
            fields.RowStyles[row].Height = showCredentials ? 0 : 0;
            for (var column = 0; column < fields.ColumnCount; column++)
            {
                var control = fields.GetControlFromPosition(column, row);
                if (control is not null)
                {
                    control.Visible = showCredentials;
                }
            }
        }

        _message.Text = showCredentials ? "" : "一般利用者は閲覧のみです。";
    }
}
