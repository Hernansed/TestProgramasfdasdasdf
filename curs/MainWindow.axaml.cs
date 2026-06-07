using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;
using curs.Models;
using curs.Data;

namespace curs
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<TestItem> _items = new();
        private TextBox? _inputTextBox;
        private ListBox? _itemsListBox;
        private TextBlock? _userLabel;
        private User? _currentUser;

        public MainWindow()
        {
            InitializeComponent();
            InitializeControls();
            Database.Initialize();
        }

        public MainWindow(User user)
        {
            InitializeComponent();
            Database.Initialize();
            SetCurrentUser(user);
            InitializeControls();
            LoadTests();
        }

        private void InitializeControls()
        {
            _inputTextBox = this.FindControl<TextBox>("InputTextBox");
            _itemsListBox = this.FindControl<ListBox>("ItemsListBox");
            _userLabel = this.FindControl<TextBlock>("UserLabel");
            var logoutBtn = this.FindControl<Button>("LogoutButton");
            var settingsBtn = this.FindControl<Button>("SettingsButton");

            if (logoutBtn != null)
                logoutBtn.Click += LogoutButton_Click;

            if (settingsBtn != null)
                settingsBtn.Click += SettingsButton_Click;

            if (_itemsListBox != null)
                _itemsListBox.ItemsSource = _items;

            var takeBtn = this.FindControl<Button>("TakeTestButton");
            if (takeBtn != null)
                takeBtn.Click += TakeTestButton_Click;

            var addButton = this.FindControl<Button>("AddButton");
            var clearButton = this.FindControl<Button>("ClearButton");

            // Only allow adding/clearing tests for admin users. Hide for regular users.
            if (_currentUser != null && _currentUser.Role != "admin")
            {
                if (addButton != null) addButton.IsVisible = false;
                if (clearButton != null) clearButton.IsVisible = false;
            }
            else
            {
                if (addButton != null)
                    addButton.Click += AddButton_Click;
                if (clearButton != null)
                    clearButton.Click += ClearButton_Click;
            }
        }

        private void LogoutButton_Click(object? sender, RoutedEventArgs e)
        {
            var login = new LoginWindow();
            login.Show();
            this.Close();
        }

        private void SettingsButton_Click(object? sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow();
            settings.ShowDialog(this);
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
            if (_userLabel != null)
                _userLabel.Text = $"Пользователь: {user.Username} ({user.Role})";
        }

        private void LoadTests()
        {
            _items.Clear();
            using var conn = new SqliteConnection($"Data Source={Database.DbPath}");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Title, Content FROM Tests ORDER BY Id DESC";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var item = new TestItem { Id = reader.GetInt32(0), Title = reader.GetString(1), Content = reader.GetString(2) };
                _items.Add(item);
            }
        }

        private async void TakeTestButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_itemsListBox == null) return;
            var sel = _itemsListBox.SelectedItem as TestItem;
            if (sel == null) return;

            var win = new TestTakingWindow(sel.Id, _currentUser);
            await win.ShowDialog(this);
        }

        private void AddButton_Click(object? sender, RoutedEventArgs e)
        {
            var text = _inputTextBox?.Text ?? string.Empty;
            if (string.IsNullOrWhiteSpace(text))
                return;

            using var conn = new SqliteConnection($"Data Source={Database.DbPath}");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Tests (Title, Content) VALUES ($t, $c);";
            cmd.Parameters.AddWithValue("$t", text);
            cmd.Parameters.AddWithValue("$c", string.Empty);
            cmd.ExecuteNonQuery();

            if (_inputTextBox != null)
                _inputTextBox.Text = string.Empty;

            LoadTests();
        }

        private void ClearButton_Click(object? sender, RoutedEventArgs e)
        {
            using var conn = new SqliteConnection($"Data Source={Database.DbPath}");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Tests;";
            cmd.ExecuteNonQuery();
            _items.Clear();
        }
    }
}
