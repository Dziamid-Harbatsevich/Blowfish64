using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using Blowfish64.Commands;
using Blowfish64Lib.Utils;
using Blowfish64Lib;
using Blowfish64.Windows;
using Blowfish64.Entities;
using System.ComponentModel;

namespace Blowfish64;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    internal const string KEYS_DIR_NAME = "keys";
    internal const string KEY_FILE_PREFIX = "key_";
    internal const string PLAIN_DIR_NAME = "plain";
    internal const string PLAIN_FILE_PREFIX = "plain_";
    internal const string ENCRYPTED_DIR_NAME = "encrypted";
    internal const string ENCRYPTED_FILE_PREFIX = "encrypted_";

    private _Key _key;
    private bool isKeySet;
    public bool IsKeySet
    {
        get { return _key != null; }
        set
        {
            isKeySet = value;
            EncryptButton.IsEnabled = value;
            DecryptButton.IsEnabled = value;
        }
    }
    public string PlaintText { get; set; }
    public string EncryptedText { get; set; }
    private BlowfishContext? Blowfish { get; set; }

    public MainWindow()
    {
        InitializeComponent();

        _key = new();
        KeyTextBox.DataContext = _key.KeyValue;
        PlainTextBox.DataContext = PlaintText;
        EncryptedTextBox.DataContext = EncryptedText;
        KeyTextBox.MaxLength = _Key.MAX_CHAR_KEY_LENGTH;
        myUpDownControl.MaxLength = _Key.MAX_CHAR_KEY_LENGTH;
        DataContext = this;
        IsKeySet = false;
    }

    #region File->Commands

    private RelayCommand openPlainFromFileCommand;
    public ICommand OpenPlainFromFileCommand => openPlainFromFileCommand ??= new RelayCommand(OpenPlainFromFileExecute);

    private void OpenPlainFromFileExecute(object commandParameter)
    {
        string? content = Filesystem.GetContentFromFile(PLAIN_DIR_NAME);
        if (content != null)
        {
            PlainTextBox.Text = content;
        }
    }

    private RelayCommand openEncryptedFromFileCommand;
    public ICommand OpenEncryptedFromFileCommand => openEncryptedFromFileCommand ??= new RelayCommand(OpenEncryptedFromFileExecute);

    private void OpenEncryptedFromFileExecute(object commandParameter)
    {
        string? content = Filesystem.GetContentFromFile(ENCRYPTED_DIR_NAME);
        if (content != null)
        {
            EncryptedTextBox.Text = content;
        }
    }

    private RelayCommand savePlainToFileDialogCommand;
    public ICommand SavePlainToFileDialogCommand => savePlainToFileDialogCommand ??= new RelayCommand(SavePlainToFileDialogExecute);

    private void SavePlainToFileDialogExecute(object commandParameter)
    {
        if (PlainTextBox.Text.Length > 0)
        {
            Filesystem.PutContentToFile(PLAIN_DIR_NAME, PlainTextBox.Text, PLAIN_FILE_PREFIX);
        }
    }

    private RelayCommand saveEncryptedToFileDialogCommand;
    public ICommand SaveEncryptedToFileDialogCommand => saveEncryptedToFileDialogCommand ??= new RelayCommand(SaveEncryptedToFileDialogExecute);

    private void SaveEncryptedToFileDialogExecute(object commandParameter)
    {
        if (EncryptedTextBox.Text.Length > 0)
        {
            Filesystem.PutContentToFile(ENCRYPTED_DIR_NAME, EncryptedTextBox.Text, ENCRYPTED_FILE_PREFIX);
        }
    }

    private RelayCommand importKeyCommand;
    public ICommand ImportKeyCommand => importKeyCommand ??= new RelayCommand(ImportKeyExecute);

    private void ImportKeyExecute(object commandParameter)
    {
        string? content = Filesystem.GetContentFromFile(KEYS_DIR_NAME);
        if (content != null)
        {
            KeyTextBox.Text = content;
        }
    }

    private RelayCommand exportKeyCommand;
    public ICommand ExportKeyCommand => exportKeyCommand ??= new RelayCommand(ExportKeyExecute);

    private void ExportKeyExecute(object commandParameter)
    {
        if (KeyTextBox.Text.Length > 0)
        {
            Filesystem.PutContentToFile(KEYS_DIR_NAME, KeyTextBox.Text, KEY_FILE_PREFIX);
        }
    }

    #endregion File->Commands

    #region Encipher File commands

    private RelayCommand encodeFileCommand;
    public ICommand EncodeFileCommand => encodeFileCommand ??= new RelayCommand(EncodeFileExecute);

    private void EncodeFileExecute(object commandParameter)
    {
        // Check key
        if (! IsKeySet || Blowfish == null)
        {
            MessageBox.Show("Ключ не установлен. Необходимо сгенерировать/импортировать ключ.", "ВНИМАНИЕ!", MessageBoxButton.OK);
            return;
        }

        if (_key.KeyValue == null || _key.KeyValue.Length == 0)
        {
            MessageBoxResult dRes = MessageBox.Show("Сгенерировать ключ автоматически? Альтернативно можно воспользоваться иными способами задания ключа.",
                                        "Необходим ключ шифрования!",
                                        MessageBoxButton.OKCancel,
                                        MessageBoxImage.Question);

            if (dRes == MessageBoxResult.OK)
                AutoKeySet(true);
            else
                return;
        }
        else
        {
            var dRes = MessageBox.Show("Вы уверены, что хотите использовать введенный ключ?",
                            "Необходим ключ шифрования!",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

            if (dRes == MessageBoxResult.Yes)
                AutoKeySet();
            else
                return;
        }

        // Encrypt file
        string? content = Filesystem.GetContentFromFile(PLAIN_DIR_NAME);
        if (content != null)
            content = Encrypt(content);

        if (content?.Length > 0)
            Filesystem.PutContentToFile(ENCRYPTED_DIR_NAME, content, ENCRYPTED_FILE_PREFIX);
    }

    private RelayCommand decodeFileCommand;
    public ICommand DecodeFileCommand => decodeFileCommand ??= new RelayCommand(DecodeFileExecute);

    private void DecodeFileExecute(object commandParameter)
    {
        // Check key
        if (!IsKeySet || Blowfish == null)
        {
            MessageBox.Show("Ключ не установлен. Необходимо импортировать ключ.", "ВНИМАНИЕ!", MessageBoxButton.OK);
            return;
        }

        if (_key.KeyValue?.Length != 0)
        {
            MessageBoxResult dRes = MessageBox.Show("Желаете использовать введенный ключ (Да) или импортировать (Нет)?",
                            "Уточняем ключа шифрования!",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

            if (dRes == MessageBoxResult.No)
            {
                ImportKeyExecute(0);
                AutoKeySet();
            }
        }

        // Decrypt file
        string? content = Filesystem.GetContentFromFile(ENCRYPTED_DIR_NAME);
        if (content != null)
            content = Decrypt(content);

        if (content?.Length > 0)
            Filesystem.PutContentToFile(PLAIN_DIR_NAME, content, PLAIN_FILE_PREFIX);
    }

    #endregion Encipher File commands

    #region General commands

    private RelayCommand resetCommand;
    public ICommand ResetCommand => resetCommand ??= new RelayCommand(ResetExecute);

    private void ResetExecute(object commandParameter)
    {
        KeyTextBox.Text = "";
        PlainTextBox.Text = "";
        EncryptedTextBox.Text = "";
        Blowfish = null;
        IsKeySet = false;
    }

    private RelayCommand exitAppCommand;
    public ICommand ExitAppCommand => exitAppCommand ??= new RelayCommand(ExitAppExecute);

    private void ExitAppExecute(object commandParameter)
    {
        App.Current.Shutdown();
    }

    private RelayCommand helpAppCommand;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand HelpAppCommand => helpAppCommand ??= new RelayCommand(HelpAppExecute);

    private void HelpAppExecute(object commandParameter)
    {
        var directory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        var file = Path.Combine(directory, "Help.txt");

        if (File.Exists(file))
        {
            Process.Start(new ProcessStartInfo { FileName = file, UseShellExecute = true });
        }
        else
        {
            MessageBox.Show("Удачи!", "Help");
        }
    }

    #endregion General commands

    #region Button Click handlers

    private void ButtonGenerateKey_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new KeyGeneratorWindow();

        if (dialog.ShowDialog() != true) return;

        _key = dialog._key;
        if (_key.KeyValue.Length > 0)
        {
            KeyTextBox.Text = _key.KeyValue;
            AutoKeySet();
        }
    }

    private void ButtonAutoKey_Click(object sender, RoutedEventArgs e)
    {
        AutoKeySet(true);
    }

    private void ButtonSetKey_Click(object sender, RoutedEventArgs e)
    {
        AutoKeySet();
    }

    private void AutoKeySet(bool isNewKey = false)
    {
        if (isNewKey)
        {
            int length = _Key.MAX_CHAR_KEY_LENGTH;
            if (myUpDownControl.Value != null)
            {
                length = _Key.FilterKeyLength((int)myUpDownControl.Value);
            }

            string keyGenerated = GenerateRandomString.Generate(length);
            _key.KeyValue = keyGenerated;
            KeyTextBox.Text = _key.KeyValue;
        }
        string str = KeyTextBox.Text;
        byte[] strBytes = Encoding.Unicode.GetBytes(str);
        if (strBytes.Length < 4)
        {
            MessageBox.Show("Длина ключа должна быть не менее 32 бит",
                        "ВНИМАНИЕ! Ключ менее допустимой длины",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
            return;
        }
        else if (strBytes.Length < 24 && strBytes.Length >= 4)
        {
            MessageBoxResult mRes = MessageBox.Show("Применить этот ключ все равно? Ключ с низкой криптостойкостью обладает меньшей защитой.",
                        "ВНИМАНИЕ! Низкая криптостойкость ключа",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
            if (mRes == MessageBoxResult.No)
                return;
        }
        Blowfish = new BlowfishContext(strBytes);
        IsKeySet = true;
    }

    private void ButtonEncrypt_Click(object sender, RoutedEventArgs e)
    {
        PlaintText = PlainTextBox.Text;
        EncryptedText = Encrypt(PlaintText);
        EncryptedTextBox.Text = EncryptedText;
    }

    private string Encrypt(string plainText)
    {
        char blank = ' ';
        string blankStr = "";
        int plainTextTailLength = plainText.Length % 8;
        if (plainTextTailLength > 0)
        {
            for (int i = 0; i < 8 - plainTextTailLength; i++)
            {
                blankStr += blank;
            }
        }

        return Blowfish.Encrypt(plainText + blankStr);
    }

    private void ButtonDecrypt_Click(object sender, RoutedEventArgs e)
    {
        EncryptedText = EncryptedTextBox.Text;
        PlaintText = Decrypt(EncryptedText);
        PlainTextBox.Text = PlaintText;
    }

    private string Decrypt(string encryptedText)
    {
        return Blowfish.Decrypt(encryptedText);
    }

    #endregion Button Click handlers

    #region Event handlers

    private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Marshal.ReleaseComObject(Blowfish);
    }

    private void KeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        KeyTextSizeTextBlock.Text = (System.Text.ASCIIEncoding.Unicode.GetByteCount(KeyTextBox.Text) * 8).ToString();
        isKeySet = false;
    }

    private void PlainTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        PlainTextSizeTextBlock.Text = PlainTextBox.Text.Length.ToString();
    }

    private void EncryptedTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        EncryptedTextSizeTextBlock.Text = EncryptedTextBox.Text.Length.ToString();
    }

    #endregion Event handlers
}
