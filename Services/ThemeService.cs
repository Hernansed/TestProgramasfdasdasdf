using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.IO;
using System.Text.Json;

namespace curs.Services
{
    public class ThemeService
    {
        private static ThemeService? _instance;
        private readonly string _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "curs_theme.json");
        
        public event Action<string>? ThemeChanged;
        public event Action<Color>? AccentColorChanged;

        private string _currentTheme = "Light";
        private Color _accentColor = Color.FromArgb(255, 0, 120, 215); // Default Blue

        public static ThemeService Instance => _instance ??= new ThemeService();

        public string CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    ApplyTheme();
                    ThemeChanged?.Invoke(value);
                    SaveSettings();
                }
            }
        }

        public Color AccentColor
        {
            get => _accentColor;
            set
            {
                System.Diagnostics.Debug.WriteLine($"[ThemeService] AccentColor setter called: old={_accentColor}, new={value}");
                if (_accentColor != value)
                {
                    _accentColor = value;
                    System.Diagnostics.Debug.WriteLine($"[ThemeService] Applying new accent color: {_accentColor}");
                    ApplyAccentColor();
                    AccentColorChanged?.Invoke(value);
                    SaveSettings();
                }
            }
        }

        public ThemeService()
        {
            LoadSettings();
        }

        private void ApplyTheme()
        {
            if (Application.Current == null) return;

            Application.Current.RequestedThemeVariant = _currentTheme switch
            {
                "Dark" => ThemeVariant.Dark,
                "Light" => ThemeVariant.Light,
                _ => ThemeVariant.Default
            };
        }

        private void ApplyAccentColor()
        {
            System.Diagnostics.Debug.WriteLine($"[ThemeService] ApplyAccentColor called for color: {_accentColor}");
            
            if (Application.Current == null)
            {
                System.Diagnostics.Debug.WriteLine("[ThemeService] ERROR: Application.Current is NULL!");
                return;
            }

            try
            {
                // Try to update application resources with multiple keys for compatibility
                if (Application.Current.Resources != null)
                {
                    System.Diagnostics.Debug.WriteLine("[ThemeService] Updating Application.Resources");
                    
                    // Create a brush from the color
                    var brush = new SolidColorBrush(_accentColor);
                    
                    // Update standard Avalonia color resources
                    Application.Current.Resources["AccentColor"] = _accentColor;
                    Application.Current.Resources["SystemAccentColor"] = _accentColor;
                    Application.Current.Resources["SystemAccentColorBrush"] = brush;
                    
                    // Also update variants
                    Application.Current.Resources["SystemAccentColorLight1"] = Color.FromArgb(200, _accentColor.R, _accentColor.G, _accentColor.B);
                    Application.Current.Resources["SystemAccentColorLight2"] = Color.FromArgb(150, _accentColor.R, _accentColor.G, _accentColor.B);
                    Application.Current.Resources["SystemAccentColorLight3"] = Color.FromArgb(100, _accentColor.R, _accentColor.G, _accentColor.B);
                    Application.Current.Resources["SystemAccentColorDark1"] = Color.FromArgb(255, _accentColor.R, _accentColor.G, _accentColor.B);
                    
                    System.Diagnostics.Debug.WriteLine("[ThemeService] Application.Resources updated successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[ThemeService] ERROR: Application.Current.Resources is NULL!");
                }

                // Apply color to all visible windows
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    System.Diagnostics.Debug.WriteLine($"[ThemeService] Updating {desktop.Windows.Count} windows");
                    foreach (var window in desktop.Windows)
                    {
                        UpdateWindowColors(window);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[ThemeService] ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ThemeService] ERROR applying accent color: {ex.Message}");
            }
        }

        private void UpdateWindowColors(Window window)
        {
            if (window == null) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"[ThemeService] UpdateWindowColors for window: {window.Title}");
                
                // Apply accent color to window background
                var accentBrush = new SolidColorBrush(_accentColor);
                
                // Create a lighter version for background (using HSV)
                byte r = _accentColor.R;
                byte g = _accentColor.G;
                byte b = _accentColor.B;
                
                // Make it lighter for background
                byte bgR = (byte)((r + 255) / 2);
                byte bgG = (byte)((g + 255) / 2);
                byte bgB = (byte)((b + 255) / 2);
                var bgColor = new Color(255, bgR, bgG, bgB);
                var bgBrush = new SolidColorBrush(bgColor);
                
                // Apply to window background
                window.Background = bgBrush;
                System.Diagnostics.Debug.WriteLine($"[ThemeService] Applied background color to window");
                
                // Update window resources
                if (window.Resources != null)
                {
                    window.Resources["AccentColor"] = _accentColor;
                    window.Resources["SystemAccentColor"] = _accentColor;
                }

                // Recursively apply to all controls in the window
                ApplyColorToControlRecursive(window, accentBrush, bgBrush);
            }
            catch (Exception ex) 
            { 
                System.Diagnostics.Debug.WriteLine($"[ThemeService] Error in UpdateWindowColors: {ex.Message}");
            }
        }

        private void ApplyColorToControlRecursive(Control? control, SolidColorBrush? accentBrush = null, SolidColorBrush? bgBrush = null)
        {
            if (control == null) return;

            try
            {
                if (accentBrush == null)
                    accentBrush = new SolidColorBrush(_accentColor);
                if (bgBrush == null)
                    bgBrush = new SolidColorBrush(_accentColor);

                // Apply accent color to buttons
                if (control is Button btn)
                {
                    btn.Background = accentBrush;
                    btn.Foreground = new SolidColorBrush(Colors.White);
                    System.Diagnostics.Debug.WriteLine($"[ThemeService] Applied color to button: {btn.Content}");
                }
                
                // Apply background color to panels and containers
                if (control is Panel panel && !(control is Canvas))
                {
                    if (panel.Background == null || panel.Background is SolidColorBrush)
                    {
                        panel.Background = bgBrush;
                        System.Diagnostics.Debug.WriteLine($"[ThemeService] Applied background color to panel");
                    }
                }

                // Recursively apply to children
                if (control is Panel panel2)
                {
                    foreach (var child in panel2.Children)
                    {
                        if (child is Control childControl)
                        {
                            ApplyColorToControlRecursive(childControl, accentBrush, bgBrush);
                        }
                    }
                }
                
                // Also check for ItemsControl children
                if (control is ItemsControl itemsControl)
                {
                    foreach (var item in itemsControl.Items)
                    {
                        if (item is Control itemChild)
                        {
                            ApplyColorToControlRecursive(itemChild, accentBrush, bgBrush);
                        }
                    }
                }
            }
            catch { }
        }

        private void SaveSettings()
        {
            try
            {
                var settings = new
                {
                    Theme = _currentTheme,
                    AccentColor = _accentColor.ToString()
                };

                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configPath, json);
            }
            catch { /* Silently fail */ }
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    var settings = JsonSerializer.Deserialize<JsonElement>(json);

                    if (settings.TryGetProperty("Theme", out var theme))
                        _currentTheme = theme.GetString() ?? "Light";

                    if (settings.TryGetProperty("AccentColor", out var color))
                    {
                        if (Color.TryParse(color.GetString(), out var parsedColor))
                            _accentColor = parsedColor;
                    }
                }
            }
            catch { /* Silently fail, use defaults */ }

            ApplyTheme();
            ApplyAccentColor();
        }
    }
}
