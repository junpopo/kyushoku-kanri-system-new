namespace KyushokuKanriSystem;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var repository = new AppRepository();
        var data = repository.Load();
        EnsureDefaultAdmin(data, repository);

        using var loginForm = new LoginForm(data.Users);
        if (loginForm.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        Application.Run(new MainForm(loginForm.LoggedInUser));
    }

    private static void EnsureDefaultAdmin(AppData data, AppRepository repository)
    {
        if (data.Users.Count > 0)
        {
            return;
        }

        data.Users.Add(new AppUser
        {
            LoginId = "admin",
            DisplayName = "管理者",
            PasswordHash = PasswordHasher.Hash("admin"),
            Role = UserRole.Admin,
            IsActive = true
        });
        repository.Save(data);
    }
}
