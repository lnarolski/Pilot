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
    }
    static class DatabaseClass
    {
        public static DatabaseState DatabaseState;
        private static SQLiteConnection db;
        public static string exceptionText { get; private set; }
        public static void OpenDB()
        {
            try
            {
                string path;

                if (DeviceInfo.Platform == DevicePlatform.UWP)
                {
                    path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                }
                else
                {
                    path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                }

                if (!File.Exists(Path.Combine(path, "db.db3")))
                {
                    db = new SQLiteConnection(Path.Combine(path, "db.db3"));
                    db.CreateTable<ShortcutsTable>();
                    db.CreateTable<ConfigTable>();

                    var ConfigTable = new ConfigTable();
                    ConfigTable.LastIpAddress = "";
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

        public static void CloseDB()
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

        public static void UpdateLastIPAddress(string LastIpAddress)
        {
            try
            {
                OpenDB();
                if (DatabaseState == DatabaseState.DATABASE_OPENED)
                {
                    db.Execute("UPDATE ConfigTable SET LastIpAddress = ? WHERE Id = 1", LastIpAddress);
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

        public static string GetLastIPAddress()
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

        public static void CreateShortcutsList(ObservableCollection<ShortcutCell> observableCollection)
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

        public static ShortcutCell GetShortcutFromDatabase(int id)
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

        public static void AddNewShortcut(ShortcutCell shortcutCell)
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

        public static void EditShortcut(ShortcutCell shortcutCell)
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

        public static void DeleteShortcut(ShortcutCell shortcutCell)
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
