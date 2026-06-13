using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using curs.Services;

namespace curs
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // Initialize theme service
            _ = ThemeService.Instance;

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Start with TestProgram (contains login/registration views)
                desktop.MainWindow = new TestProgram();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}