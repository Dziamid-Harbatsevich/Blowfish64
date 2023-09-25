using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;

namespace Blowfish64Lib.Utils;

public static class Filesystem
{
    internal const string DEFAULT_FILE_EXTENSION = ".txt";

    public static string NewOrDefaultFolder(string dirName)
    {
        var dirCurrent = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        var dirNew = Path.Combine(dirCurrent, dirName);
        if (!Directory.Exists(dirNew))
        {
            Directory.CreateDirectory(dirNew);
        }

        return Directory.Exists(dirNew) ? dirNew : dirCurrent;
    }

    public static string? GetContentFromFile(string dir)
    {
        string initialDir = NewOrDefaultFolder(dir);

        var dialog = new OpenFileDialog();
        dialog.InitialDirectory = initialDir;
        dialog.FileName = "Document"; // Default file name
        dialog.DefaultExt = DEFAULT_FILE_EXTENSION; // Default file extension
        dialog.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

        if (dialog.ShowDialog() == true)
        {
            FileInfo fileInfo = new FileInfo(dialog.FileName);
            StreamReader reader = new StreamReader(fileInfo.Open(FileMode.Open, FileAccess.Read), Encoding.GetEncoding(1251));
            string result = reader.ReadToEnd();
            reader.Close();

            if (result.ToString().Length > 0)
            {
                return result.ToString();
            }
            else
            {
                MessageBox.Show("Содержимое отсутствует.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            return result;
        }

        return null;

    }

    public static void PutContentToFile(string dir, string content, string fPrefix = "")
    {
        string initialDir = Filesystem.NewOrDefaultFolder(dir);

        SaveFileDialog dialog = new SaveFileDialog();
        dialog.InitialDirectory = initialDir;
        dialog.FileName = fPrefix + DateTime.Now.ToFileTimeUtc();   // Default file name
        dialog.DefaultExt = DEFAULT_FILE_EXTENSION;     // Default file extension
        dialog.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

        if (dialog.ShowDialog() == true)
            File.WriteAllText(dialog.FileName, content, Encoding.Default);
    }
}