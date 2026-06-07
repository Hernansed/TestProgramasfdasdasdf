using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using curs.Services;
using System;

namespace curs
{
    public partial class SettingsWindow : Window
    {
        private readonly ThemeService _themeService = ThemeService.Instance;
        private Canvas? _colorPickerCanvas;
        private Color _selectedColor;
        private readonly Color[] _presetColors = new[]
        {
            Color.FromArgb(255, 0, 120, 215),    // Blue
            Color.FromArgb(255, 255, 51, 0),     // Red
            Color.FromArgb(255, 0, 153, 76),     // Green
            Color.FromArgb(255, 255, 153, 0),    // Orange
            Color.FromArgb(255, 153, 51, 255)    // Purple
        };

        public SettingsWindow()
        {
            InitializeComponent();
            InitializeThemeSettings();
            InitializeColorPicker();
        }

        private void InitializeThemeSettings()
        {
            var lightRadio = this.FindControl<RadioButton>("LightThemeRadio");
            var darkRadio = this.FindControl<RadioButton>("DarkThemeRadio");
            var defaultRadio = this.FindControl<RadioButton>("DefaultThemeRadio");

            // Set current theme
            var currentTheme = _themeService.CurrentTheme;
            if (lightRadio != null) lightRadio.IsChecked = currentTheme == "Light";
            if (darkRadio != null) darkRadio.IsChecked = currentTheme == "Dark";
            if (defaultRadio != null) defaultRadio.IsChecked = currentTheme == "Default";

            // Add click handlers
            if (lightRadio != null) lightRadio.Click += (s, e) => _themeService.CurrentTheme = "Light";
            if (darkRadio != null) darkRadio.Click += (s, e) => _themeService.CurrentTheme = "Dark";
            if (defaultRadio != null) defaultRadio.Click += (s, e) => _themeService.CurrentTheme = "Default";
        }

        private void InitializeColorPicker()
        {
            _colorPickerCanvas = this.FindControl<Canvas>("ColorPickerCanvas");
            _selectedColor = _themeService.AccentColor;

            if (_colorPickerCanvas != null)
            {
                _colorPickerCanvas.Width = 350;
                _colorPickerCanvas.Height = 250;
                DrawColorGradient();
                _colorPickerCanvas.PointerPressed += ColorPickerCanvas_PointerPressed;
                _colorPickerCanvas.PointerMoved += ColorPickerCanvas_PointerMoved;
            }

            // Initialize preset color buttons
            InitializePresetButtons();

            // Apply color button
            var applyColorBtn = this.FindControl<Button>("ApplyColorButton");
            if (applyColorBtn != null)
                applyColorBtn.Click += (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine($"[SettingsWindow] Applying color: {_selectedColor}");
                    _themeService.AccentColor = _selectedColor;
                    System.Diagnostics.Debug.WriteLine($"[SettingsWindow] Color applied. Current theme accent: {_themeService.AccentColor}");
                };

            // OK and Cancel buttons
            var okBtn = this.FindControl<Button>("OkButton");
            var cancelBtn = this.FindControl<Button>("CancelButton");

            if (okBtn != null) okBtn.Click += (s, e) => this.Close();
            if (cancelBtn != null) cancelBtn.Click += (s, e) => this.Close();

            UpdateColorDisplay();
        }

        private void DrawColorGradient()
        {
            if (_colorPickerCanvas == null) return;

            _colorPickerCanvas.Children.Clear();

            // Create color gradient with hue on X and brightness on Y
            int width = 350;
            int height = 250;
            int hueSteps = 18;
            int brightnessSteps = 15;
            
            double cellWidth = (double)width / hueSteps;
            double cellHeight = (double)height / brightnessSteps;

            // Draw hue spectrum with brightness gradient
            for (int h = 0; h < hueSteps; h++)
            {
                for (int b = 0; b < brightnessSteps; b++)
                {
                    int hue = (int)((h / (double)hueSteps) * 360);
                    int brightness = 100 - (int)((b / (double)brightnessSteps) * 100);
                    
                    var color = HsvToRgb(hue, 100, brightness);
                    
                    var rect = new Rectangle
                    {
                        Width = cellWidth + 1,
                        Height = cellHeight + 1,
                        Fill = new SolidColorBrush(color),
                        StrokeThickness = 0
                    };
                    
                    Canvas.SetLeft(rect, h * cellWidth);
                    Canvas.SetTop(rect, b * cellHeight);
                    _colorPickerCanvas.Children.Add(rect);
                }
            }
        }

        private void InitializePresetButtons()
        {
            var colorButtons = new[] { "Color1", "Color2", "Color3", "Color4", "Color5" };
            for (int i = 0; i < colorButtons.Length && i < _presetColors.Length; i++)
            {
                var btn = this.FindControl<Button>(colorButtons[i]);
                if (btn != null)
                {
                    var color = _presetColors[i];
                    btn.Background = new SolidColorBrush(color);
                    btn.Click += (s, e) =>
                    {
                        _selectedColor = color;
                        System.Diagnostics.Debug.WriteLine($"[PresetColor] Selected preset color: {color}");
                        UpdateColorDisplay();
                    };
                }
            }
        }

        private void ColorPickerCanvas_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            SelectColor(e.GetPosition(_colorPickerCanvas));
        }

        private void ColorPickerCanvas_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (e.GetCurrentPoint(_colorPickerCanvas).Properties.IsLeftButtonPressed)
            {
                SelectColor(e.GetPosition(_colorPickerCanvas));
            }
        }

        private void SelectColor(Point? position)
        {
            if (position == null || _colorPickerCanvas == null) return;

            var x = position.Value.X;
            var y = position.Value.Y;
            var width = 350;
            var height = 250;

            // Clamp to canvas bounds
            x = Math.Max(0, Math.Min(width, x));
            y = Math.Max(0, Math.Min(height, y));

            // Map position to HSV
            var hue = (int)(x / width * 360);
            var brightness = (int)(100 - (y / height * 100));
            var saturation = 100;

            _selectedColor = HsvToRgb(hue, saturation, brightness);
            System.Diagnostics.Debug.WriteLine($"[ColorPicker] Selected color from palette: {_selectedColor} (H:{hue} S:{saturation} B:{brightness})");
            UpdateColorDisplay();
        }

        private void UpdateColorDisplay()
        {
            var hexText = this.FindControl<TextBlock>("ColorHexText");
            if (hexText != null)
            {
                hexText.Text = $"Цвет:\n#{_selectedColor.R:X2}{_selectedColor.G:X2}{_selectedColor.B:X2}";
            }

            var preview = this.FindControl<Rectangle>("ColorPreview");
            if (preview != null)
            {
                preview.Fill = new SolidColorBrush(_selectedColor);
                System.Diagnostics.Debug.WriteLine($"[ColorDisplay] Updated preview with color: {_selectedColor}");
            }
        }

        private static Color HsvToRgb(int hue, int saturation, int value)
        {
            var h = hue / 60.0;
            var s = saturation / 100.0;
            var v = value / 100.0;

            var c = v * s;
            var x = c * (1 - ((h % 2) - 1));
            var m = v - c;

            double r, g, b;

            if (h >= 0 && h < 1) { r = c; g = x; b = 0; }
            else if (h >= 1 && h < 2) { r = x; g = c; b = 0; }
            else if (h >= 2 && h < 3) { r = 0; g = c; b = x; }
            else if (h >= 3 && h < 4) { r = 0; g = x; b = c; }
            else if (h >= 4 && h < 5) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }

            return Color.FromArgb(
                255,
                (byte)((r + m) * 255),
                (byte)((g + m) * 255),
                (byte)((b + m) * 255)
            );
        }
    }
}
