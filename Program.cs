namespace KyushokuKanriSystem;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var repository = new AppRepository();
        var data = repository.Load();
        EnsureDefaultUsers(data, repository);

        using var loginForm = new LoginForm(data.Users);
        if (loginForm.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        Application.Run(new MainForm(loginForm.LoggedInUser));
    }

    private static void EnsureDefaultUsers(AppData data, AppRepository repository)
    {
        var changed = false;
        if (!data.Users.Any(user => user.Role == UserRole.Admin))
        {
            data.Users.Add(new AppUser
            {
                LoginId = "admin",
                DisplayName = "管理者",
                PasswordHash = PasswordHasher.Hash("admin"),
                Role = UserRole.Admin,
                IsActive = true
            });
            changed = true;
        }

        if (!data.Users.Any(user => user.Role == UserRole.User))
        {
            data.Users.Add(new AppUser
            {
                LoginId = "viewer",
                DisplayName = "閲覧利用者",
                PasswordHash = PasswordHasher.Hash("viewer"),
                Role = UserRole.User,
                IsActive = true
            });
            changed = true;
        }

        if (changed)
        {
            repository.Save(data);
        }
    }
}
