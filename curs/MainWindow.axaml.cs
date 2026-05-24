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
        private ObservableCollection<string> _items = new();
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
            InitializeControls();
            Database.Initialize();
            SetCurrentUser(user);
            LoadTests();
        }

        private void InitializeControls()
        {
            _inputTextBox = this.FindControl<TextBox>("InputTextBox");
            _itemsListBox = this.FindControl<ListBox>("ItemsListBox");
            _userLabel = this.FindControl<TextBlock>("UserLabel");

            if (_itemsListBox != null)
                _itemsListBox.ItemsSource = _items;

            var addButton = this.FindControl<Button>("AddButton");
            if (addButton != null)
                addButton.Click += AddButton_Click;

            var clearButton = this.FindControl<Button>("ClearButton");
            if (clearButton != null)
                clearButton.Click += ClearButton_Click;
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
            cmd.CommandText = "SELECT Title FROM Tests ORDER BY Id DESC";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                _items.Add(reader.GetString(0));
            }
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
