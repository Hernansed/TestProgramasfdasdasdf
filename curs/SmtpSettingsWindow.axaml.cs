using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using curs.Services;

namespace curs
{
    public partial class SmtpSettingsWindow : Window
    {
        public SmtpSettingsWindow()
        {
            InitializeComponent();
            var save = this.FindControl<Button>("SaveButton");
            var cancel = this.FindControl<Button>("CancelButton");
            if (save != null) save.Click += SaveButton_Click;
            if (cancel != null) cancel.Click += (_, __) => this.Close();

            // populate current
            var host = this.FindControl<TextBox>("HostBox");
            var port = this.FindControl<TextBox>("PortBox");
            var user = this.FindControl<TextBox>("UserBox");
            var pass = this.FindControl<TextBox>("PassBox");
            var from = this.FindControl<TextBox>("FromBox");
            if (host != null) host.Text = EmailService.Host ?? string.Empty;
            if (port != null) port.Text = EmailService.Port.ToString();
            if (user != null) user.Text = EmailService.Username ?? string.Empty;
            if (pass != null) pass.Text = EmailService.Password ?? string.Empty;
            if (from != null) from.Text = EmailService.From ?? string.Empty;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            var host = this.FindControl<TextBox>("HostBox")?.Text ?? string.Empty;
            var portText = this.FindControl<TextBox>("PortBox")?.Text ?? string.Empty;
            var user = this.FindControl<TextBox>("UserBox")?.Text ?? string.Empty;
            var pass = this.FindControl<TextBox>("PassBox")?.Text ?? string.Empty;
            var from = this.FindControl<TextBox>("FromBox")?.Text ?? string.Empty;
            var status = this.FindControl<TextBlock>("StatusText");

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(portText) || string.IsNullOrWhiteSpace(from))
            {
                if (status != null) status.Text = "Заполните host, port и from";
                return;
            }

            if (!int.TryParse(portText, out var port))
            {
                if (status != null) status.Text = "Порт должен быть числом";
                return;
            }

            EmailService.Configure(host, port, user, pass, from);
            EmailService.SaveSettings();
            if (status != null) status.Text = "Сохранено";
            this.Close();
        }
    }
}
