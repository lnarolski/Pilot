using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Xamarin.Essentials;

namespace Pilot
{
    enum DatabaseState
    {
        DATABASE_OPENED,
        DATABASE_CLOSED,
        DATABASE_ERROR,
        DATABASE_NULL,
        DATABASE_OK
    }

    public class ShortcutsTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Image { get; set; }
        public string Text { get; set; }
        public string WWWAddress { get; set; }
    }

    public class ConfigTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string LastIpAddress { get; set; }
        public string LastPort { get; set; }
        public string LastPassword { get; set; }
    }
    static class DatabaseClass
    {
        public static DatabaseState DatabaseState;
        private static SQLiteConnection db;
        public static string exceptionText { get; private set; }
        public static void OpenDB() //Otwieranie bazy danych
        {
            try
            {
                string path;

                if (DeviceInfo.Platform == DevicePlatform.UWP) //Wybór ścieżki dla pliku bazy danych zależnei od platformy
                {
                    path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                }
                else
                {
                    path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                }

                if (!File.Exists(Path.Combine(path, "db.db3"))) //Utworzenie lub otwarcie pliku bazy danych
                {
                    db = new SQLiteConnection(Path.Combine(path, "db.db3"));
                    db.CreateTable<ShortcutsTable>();
                    db.CreateTable<ConfigTable>();

                    var ConfigTable = new ConfigTable();
                    ConfigTable.LastIpAddress = "";
                    ConfigTable.LastPort = "1234";
                    ConfigTable.LastPassword = "";
                    db.Insert(ConfigTable);
                }
                else
                {
                    db = new SQLiteConnection(Path.Combine(path, "db.db3"));
                }

                DatabaseState = DatabaseState.DATABASE_OPENED;
                return;
            }
            catch (Exception ex)
            {
                DatabaseState = DatabaseState.DATABASE_ERROR;
                exceptionText = ex.ToString();
                return;
            }
        }

        public static void CloseDB() //Zamykanie bazy danych
        {
            if (db != null)
            {
                try
                {
                    db.Close();
                    DatabaseState = DatabaseState.DATABASE_CLOSED;
                    return;
                }
                catch (Exception ex)
                {
                    DatabaseState = DatabaseState.DATABASE_ERROR;
                    exceptionText = ex.ToString();
                    return;
                }
            }
            DatabaseState = DatabaseState.DATABASE_NULL;
        }

        public static void UpdateConfig(string LastIpAddress, string LastPort, string LastPassword) //Aktualizacja konfiguracji
        {
            try
            {
                OpenDB();
                if (DatabaseState == DatabaseState.DATABASE_OPENED)
                {
                    db.Execute("UPDATE ConfigTable SET LastIpAddress = ?, LastPort = ?, LastPassword = ? WHERE Id = 1", LastIpAddress, LastPort, LastPassword);
                    db.Close();

                    DatabaseState = DatabaseState.DATABASE_OK;
                }
                else
                {
                    DatabaseState = DatabaseState.DATABASE_ERROR;
                }
            }
            catch (Exception ex)
            {
                exceptionText = ex.ToString();
                DatabaseState = DatabaseState.DATABASE_ERROR;
                return;
            }
        }

        public static string GetLastIPAddress() //Odczyt ostatniego adresu IP, z którym doszło do pomyślnego utworzenia połączenia
        {
            try
            {
                OpenDB();
                if (DatabaseState == DatabaseState.DATABASE_OPENED)
                {
                    var existingItem = db.Get<ConfigTable>(1);
                    db.Close();

                    DatabaseState = DatabaseState.DATABASE_OK;
                    return existingItem.LastIpAddress;
                }
                else
                {
                    DatabaseState = DatabaseState.DATABASE_ERROR;
                    return "";
                }
            }
            catch (Exception ex)
            {
                exceptionText = ex.ToString();
                DatabaseState = DatabaseState.DATABASE_ERROR;
                return "";
            }
        }
        public static string GetLastPort() //Odczyt ostatniego numeru portu, z którym doszło do pomyślnego utworzenia połączenia
        {
            try
            {
                OpenDB();
                if (DatabaseState == DatabaseState.DATABASE_OPENED)
                {
                    var existingItem = db.Get<ConfigTable>(1);
                    db.Close();

                    DatabaseState = DatabaseState.DATABASE_OK;
                    return existingItem.LastPort;
                }
                else
                {
                    DatabaseState = DatabaseState.DATABASE_ERROR;
                    return "";
                }
            }
            catch (Exception ex)
            {
                exceptionText = ex.ToString();
                DatabaseState = DatabaseState.DATABASE_ERROR;
                return "";
            }
        }

        public static string GetLastPassword() //Odczyt ostatniego hasła, z którym doszło do pomyślnego utworzenia połączenia
        {
            try
            {
                OpenDB();
                if (DatabaseState == DatabaseState.DATABASE_OPENED)
                {
                    var existingItem = db.Get<ConfigTable>(1);
                    db.Close();

                    DatabaseState = DatabaseState.DATABASE_OK;
                    return existingItem.LastPassword;
                }
                else
                {
                    DatabaseState = DatabaseState.DATABASE_ERROR;
                    return "";
                }
            }
            catch (Exception ex)
            {
                exceptionText = ex.ToString();
                DatabaseState = DatabaseState.DATABASE_ERROR;
                return "";
            }
        }

        public static void CreateShortcutsList(ObservableCollection<ShortcutCell> observableCollection) //Odczyt tabeli skrótów z bazy danych i utworzenie listy dla kontrolki ListView
        {
            OpenDB();
            if (DatabaseState == DatabaseState.DATABASE_OPENED)
            {
                IEnumerable<ShortcutsTable> shortcuts = db.Table<ShortcutsTable>();
                observableCollection.Clear();

                var Enumerator = shortcuts.GetEnumerator();
                if (Enumerator != null)
                {
                    foreach (var item in shortcuts)
                    {
                        observableCollection.Add(new ShortcutCell()
                        {
                            Id = item.Id,
                            Image = item.Image,
                            Text = item.Text,
                            WWWAddress = item.WWWAddress,
                            ButtonVisible = true,
                        });
                    }
                }

                db.Close();
                DatabaseState = DatabaseState.DATABASE_OK;
                return;
            }
            else
            {
                DatabaseState = DatabaseState.DATABASE_ERROR;
                return;
            }
        }

        public static ShortcutCell GetShortcutFromDatabase(int id) //Odczyt rekordu ze skrótem o zadanym numerze Id
        {
            if (id < 1)
            {
                return new ShortcutCell();
            }

            try
            {
                OpenDB();
                if (DatabaseState == DatabaseState.DATABASE_OPENED)
                {
                    var existingItem = db.Get<ShortcutsTable>(id);
                    db.Close();

                    ShortcutCell shortcutCell = new ShortcutCell()
                    {
                        Id = existingItem.Id,
                        Text = existingItem.Text,
                        WWWAddress = existingItem.WWWAddress,
                        Image = existingItem.Image,
                        ButtonVisible = true,
                    };

                    DatabaseState = DatabaseState.DATABASE_OK;
                    return shortcutCell;
                }
                else
                {
                    DatabaseState = DatabaseState.DATABASE_ERROR;
                    return new ShortcutCell();
                }
            }
            catch (Exception ex)
            {
                exceptionText = ex.ToString();
                DatabaseState = DatabaseState.DATABASE_ERROR;
                return new ShortcutCell();
            }
        }

        public static void AddNewShortcut(ShortcutCell shortcutCell) //Dodanie nowego skrótu do bazy danych
        {
            try
            {
                OpenDB();
                if (DatabaseState == DatabaseState.DATABASE_OPENED)
                {
                    var Shortcut = new ShortcutsTable()
                    {
                        Image = shortcutCell.Image,
                        Text = shortcutCell.Text,
                        WWWAddress = shortcutCell.WWWAddress,
                    };

                    if (db.Insert(Shortcut) != 1)
                    {
                        DatabaseState = DatabaseState.DATABASE_ERROR;
                        db.Close();
                        return;
                    }
                    db.Close();

                    DatabaseState = DatabaseState.DATABASE_OK;
                    return;
                }
                else
                {
                    DatabaseState = DatabaseState.DATABASE_ERROR;
                    return;
                }
            }
            catch (Exception ex)
            {
                exceptionText = ex.ToString();
                DatabaseState = DatabaseState.DATABASE_ERROR;
            }
        }

        public static void EditShortcut(ShortcutCell shortcutCell) //Modyfikacja skrótu znaajdującego się bazie danych
        {
            try
            {
                OpenDB();
                if (DatabaseState == DatabaseState.DATABASE_OPENED)
                {
                    db.Execute("UPDATE ShortcutsTable SET Text = ?, WWWAddress = ?, Image = ? WHERE Id = ?", shortcutCell.Text, shortcutCell.WWWAddress, shortcutCell.Image, shortcutCell.Id);
                    db.Close();

                    DatabaseState = DatabaseState.DATABASE_OK;
                }
                else
                {
                    DatabaseState = DatabaseState.DATABASE_ERROR;
                }
            }
            catch (Exception ex)
            {
                exceptionText = ex.ToString();
                DatabaseState = DatabaseState.DATABASE_ERROR;
            }
        }

        public static void DeleteShortcut(ShortcutCell shortcutCell) //Usunięcie skrótu z bazy danych
        {
            try
            {
                OpenDB();
                if (DatabaseState == DatabaseState.DATABASE_OPENED)
                {
                    db.Execute("DELETE FROM ShortcutsTable WHERE Id = ?", shortcutCell.Id);
                    db.Close();

                    DatabaseState = DatabaseState.DATABASE_OK;
                }
                else
                {
                    DatabaseState = DatabaseState.DATABASE_ERROR;
                }
            }
            catch (Exception ex)
            {
                exceptionText = ex.ToString();
                DatabaseState = DatabaseState.DATABASE_ERROR;
            }
        }
    }
}
