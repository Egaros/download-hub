using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace Helper
{
    public class HelperMethods
    {
        public static async Task MessageUser(string message)
        {
            MessageDialog mess = new MessageDialog(message);
            mess.Options = MessageDialogOptions.AcceptUserInputAfterDelay;
            mess.ShowAsync();
            await Task.Delay(3000);
            //dialog.Hide();
        }

        public static bool defaultPathExists()
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (settings.Values.ContainsKey("defaultPath"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string normalizeName(string name)
        {
            name = name.Replace(":", " ");
            name = name.Replace("<", " ");
            name = name.Replace(">", " ");
            name = name.Replace(",", " ");
            name = name.Replace(".", " ");
            name = name.Replace("-", " ");
            name = name.Replace("|", " ");
            name = name.Replace("\\", " ");
            name = name.Replace("?", " ");
            name = name.Replace("!", " ");
            name = name.Replace("@", " ");
            name = name.Replace("#", " ");
            name = name.Replace("$", " ");
            name = name.Replace("%", " ");
            name = name.Replace("^", " ");
            name = name.Replace("&", " ");
            name = name.Replace("*", " ");
            name = name.Replace("(", " ");
            name = name.Replace(")", " ");
            name = name.Replace("+", " ");
            name = name.Replace("=", " ");
            name = name.Replace("_", " ");
            name = name.Replace(";", " ");
            name = name.Replace("\"", " ");
            name = name.Replace("'", " ");
            name = name.Replace("[", " ");
            name = name.Replace("{", " ");
            name = name.Replace("}", " ");
            name = name.Replace("]", " ");
            name = name.Replace("`", " ");
            name = name.Replace("~", " ");
            name = name.Replace("/", " ");

            return name;
        }

        public static SQLiteConnection LoadDatabase()
        {            
            SQLiteConnection conn = new SQLiteConnection("myDB");
            string sql = @"CREATE TABLE IF NOT EXISTS ModifiedAllDownloads(Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                                                                 FileName VARCHAR(1000),
                                                                 FileLocation VARCHAR(1000),
                                                                 FileType VARCHAR(10));";
            using (var statement = conn.Prepare(sql))
            {
                statement.Step();
            }

            return conn;
        }

        public static void addData(SQLiteConnection conn, string name, string location, string type)
        {
            try
            {
                using (var stmt = conn.Prepare("INSERT INTO ModifiedAllDownloads(FileName,FileLocation,FileType) VALUES(?, ?, ?)"))
                {
                    stmt.Bind(1, name);
                    stmt.Bind(2, location);
                    stmt.Bind(3, type);
                    stmt.Step();
                }
            }
            catch (Exception)
            {

            }
        }

        public static void removeData(SQLiteConnection conn, string name)
        {
            using (var stmt = conn.Prepare("DELETE FROM ModifiedAllDownloads WHERE FileName = ?"))
            {
                stmt.Bind(1, name);
                stmt.Step();
            }
        }

        public static List<AllDownloads.CompletedDownload> getData(SQLiteConnection conn)
        {
            List<AllDownloads.CompletedDownload> list = new List<AllDownloads.CompletedDownload>();
            using (var stmt = conn.Prepare("SELECT * FROM ModifiedAllDownloads"))
            {
                while (SQLiteResult.ROW == stmt.Step())
                {
                    AllDownloads.CompletedDownload download = new AllDownloads.CompletedDownload(stmt[1].ToString(), stmt[2].ToString(), stmt[3].ToString());
                    list.Add(download);
                }
            }
            return list;
        }

        public static bool checkEntry(SQLiteConnection conn, string fileName, string fileLocation)
        {
            bool flag = false;
            using (var stmt = conn.Prepare("SELECT * FROM ModifiedAllDownloads WHERE FileName=? & FileLocation=?"))
            {
                stmt.Bind(1, fileName);
                stmt.Bind(2, fileLocation);
                if (SQLiteResult.DONE == stmt.Step())
                {
                    if (stmt.DataCount != 0)
                        //if(!stmt[1].ToString())
                        //AllDownloads.CompletedDownload download = new AllDownloads.CompletedDownload(stmt[1].ToString(), stmt[2].ToString());
                        flag = true;
                }
            }
            return flag;
        }
    }
}
