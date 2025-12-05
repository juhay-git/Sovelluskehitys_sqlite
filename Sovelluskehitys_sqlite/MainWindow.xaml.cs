using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using Microsoft.Data.Sqlite;
using System;
using System.IO;



namespace Sovelluskehitys_sqlite
{
    public partial class MainWindow : Window
    {
        private readonly string _dbPath;
        private readonly string _connectionString;
        private readonly ObservableCollection<Person> _people = new();

        public MainWindow()
        {
            InitializeComponent();

            // DB file in the same folder as the .exe
            _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "people.db");
            _connectionString = $"Data Source={_dbPath}";

            PeopleGrid.ItemsSource = _people;
        }

        // Button: Initialize DB
        private void InitializeDb_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                // Hard-coded CREATE TABLE query
                var createCmd = connection.CreateCommand();
                createCmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS People (
                        Id   INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Age  INTEGER NOT NULL
                    );";
                createCmd.ExecuteNonQuery();

                StatusText.Text = "Database initialized.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        // Button: Load
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            _people.Clear();

            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                // Hard-coded SELECT query
                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT Id, Name, Age FROM People ORDER BY Id;";

                using var reader = selectCmd.ExecuteReader();
                while (reader.Read())
                {
                    _people.Add(new Person
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Age = reader.GetInt32(2)
                    });
                }

                StatusText.Text = $"Loaded {_people.Count} row(s).";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        // Button: Add Sample
        private void AddSample_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                // Hard-coded INSERT query
                var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO People (Name, Age) 
                    VALUES ('John Doe', 30);";
                insertCmd.ExecuteNonQuery();

                StatusText.Text = "Inserted sample row (John Doe, 30).";

                // Reload data
                Load_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        // Button: Delete Selected
        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (PeopleGrid.SelectedItem is not Person selected)
            {
                StatusText.Text = "No row selected.";
                return;
            }

            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                // Hard-coded DELETE query with parameter
                var deleteCmd = connection.CreateCommand();
                deleteCmd.CommandText = "DELETE FROM People WHERE Id = $id;";
                deleteCmd.Parameters.AddWithValue("$id", selected.Id);
                deleteCmd.ExecuteNonQuery();

                StatusText.Text = $"Deleted Id={selected.Id}.";

                // Reload data
                Load_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }
    }

    // Simple POCO for binding
    public class Person
    {
        public int Id { get; set; }     // Primary key
        public string Name { get; set; }
        public int Age { get; set; }
    }
}

