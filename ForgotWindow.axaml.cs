using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using curs.Services;

namespace curs
{
    public partial class ForgotWindow : Window
    {
        public ForgotWindow()
        {
            InitializeComponent();
            var btn = this.FindControl<Button>("SendButton");
            if (btn != null) btn.Click += SendButton_Click;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void SendButton_Click(object? sender, RoutedEventArgs e)
        {
            var emailBox = this.FindControl<TextBox>("EmailBox");
            var status = this.FindControl<TextBlock>("StatusText");
            var email = emailBox?.Text ?? string.Empty;
            if (string.IsNullOrWhiteSpace(email))
            {
                if (status != null) status.Text = "Введите почту";
                return;
            }

            var newPass = AuthService.ResetPassword(email);
            if (newPass == null)
            {
                if (status != null) status.Text = "Почта не найдена";
                return;
            }

            // SMTP/email removed — show new password to the user
            if (status != null) status.Text = $"Новый пароль: {newPass}";
        }
    }
}
