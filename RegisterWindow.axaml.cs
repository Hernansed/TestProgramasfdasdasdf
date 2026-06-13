using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using curs.Services;

namespace curs
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
            var reg = this.FindControl<Button>("RegisterButton");
            var cancel = this.FindControl<Button>("CancelButton");
            if (reg != null) reg.Click += RegisterButton_Click;
            if (cancel != null) cancel.Click += (_, __) => this.Close();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void RegisterButton_Click(object? sender, RoutedEventArgs e)
        {
            var userBox = this.FindControl<TextBox>("UsernameBox");
            var emailBox = this.FindControl<TextBox>("EmailBox");
            var nameBox = this.FindControl<TextBox>("NameBox");
            var passBox = this.FindControl<TextBox>("PasswordBox");
            var status = this.FindControl<TextBlock>("StatusText");

            var username = userBox?.Text ?? string.Empty;
            var email = emailBox?.Text ?? string.Empty;
            var name = nameBox?.Text ?? string.Empty;
            var password = passBox?.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
            {
                if (status != null) status.Text = "Заполните имя, почту и пароль";
                return;
            }

            if (AuthService.EmailExists(email))
            {
                if (status != null) status.Text = "Эта почта уже зарегистрирована";
                return;
            }

            if (AuthService.UsernameExists(username))
            {
                if (status != null) status.Text = "Имя пользователя уже занято";
                return;
            }

            var ok = AuthService.Register(username, password, email, name);
            if (ok)
            {
                if (status != null) status.Text = "Пользователь создан";
                this.Close();
            }
            else
            {
                if (status != null) status.Text = "Не удалось создать пользователя";
            }
        }
    }
}
