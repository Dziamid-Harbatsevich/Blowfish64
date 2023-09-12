using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Blowfish64.Entities;
using System.Windows.Controls.Primitives;
using System.Text;

namespace Blowfish64.Windows;

public partial class KeyGeneratorWindow : Window
{

    public int iter;
    bool isGenEnabled;
    Point p;
    Random rand;
    internal _Key _key;
    private Popup _popup;

    public KeyGeneratorWindow()
    {
        InitializeComponent();
        isGenEnabled = false;
        _key = new _Key()
        {
            KeyLength = 56,
            KeyValue = ""
        };
        grid.DataContext = _key;
        MainInfoLabel.Content = "Задайте длину секретного ключа в байтах (4-56)";
        KeyLengthTextBox.Focus();
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        if (KeyLengthTextBox.Text == "" || KeyLengthTextBox.Text == "0")
        {
            _key.KeyValue = "";
            MessageBox.Show("Длина ключа не определена.", "ВНИМАНИЕ!", MessageBoxButton.OK);
            KeyLengthTextBox.Text = "56";
            return;
        }
        MainInfoLabel.Content = "Перемещайте курсор для генерации ключа.";
        _key.KeyLength = int.Parse(KeyLengthTextBox.Text);
        NewKeyTextBox.Text = "";
        ResultMsgLabel.Content = "Генерируется:";
        iter = 0;
        KeyLiveLengthLabel.Content = iter.ToString();
        _key.KeyValue = "";
        rand = new Random();
        isGenEnabled = true;
        Generate.IsEnabled = false;
        SubmitButton.IsEnabled = false;
        KeyLengthSlider.IsEnabled = false;
        KeyLengthTextBox.IsReadOnly = true;

        InfoPanelBorder_MouseMove(sender, e);
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Escape)
            this.Close();
        if (e.Key == System.Windows.Input.Key.Enter)
            SubmitButton_Click(sender, e);
    }

    private void InfoPanelBorder_MouseMove(object sender, RoutedEventArgs e)
    {
        if (isGenEnabled == true)
        {
            OnLoaded(sender, e);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        _key.KeyValue = "";
        this.Close();
    }

    private void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        int keyByteLength = Encoding.Unicode.GetBytes(_key.KeyValue).Length;
        if (KeyLengthTextBox.Text == "" || int.Parse(KeyLengthTextBox.Text) != keyByteLength || KeyLengthTextBox.Text == "0")
        {
            _key.KeyValue = "";
            MessageBoxResult result = MessageBox.Show("Ключ не сгенерирован.", "ВНИМАНИЕ!", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.Cancel)
                return;
            else
                this.DialogResult = true;
        }
        else
            this.DialogResult = true;
    }

    private void KeyLengthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _key.KeyValue = "";
        NewKeyTextBox.Text = _key.KeyValue;
    }

    private void KeyLengthTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = ValidateIsNum(e.Text);
    }
    
    private void KeyLengthTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space || ((KeyLengthTextBox.Text == "" || KeyLengthTextBox.Text == "0") && (e.Key == Key.D0 || e.Key == Key.NumPad0)))
            e.Handled = true;
    }

    private void KeyLengthTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (ValidateIsNum(KeyLengthTextBox.Text))
        {
            MessageBox.Show("Недопустимый формат. Отменено.", "ВНИМАНИЕ!", MessageBoxButton.OK);
            KeyLengthTextBox.Text = "56";
        }

        if (KeyLengthTextBox.Text == "")
        {
            InfoMsgLabel.Foreground = Brushes.Red;
            InfoMsgLabel.Content = "Криптостойкость не задана.";
        }
        else
        {
            if (int.Parse(KeyLengthTextBox.Text) == 56)
            {
                InfoMsgLabel.Foreground = Brushes.DarkGreen;
                InfoMsgLabel.Content = "Криптостойкость максимальная.";
            }
            if (int.Parse(KeyLengthTextBox.Text) < 56 && int.Parse(KeyLengthTextBox.Text) >= 32)
            {
                InfoMsgLabel.Foreground = Brushes.Green;
                InfoMsgLabel.Content = "Криптостойкость высокая.";
            }
            if (int.Parse(KeyLengthTextBox.Text) < 32 && int.Parse(KeyLengthTextBox.Text) >= 24)
            {
                InfoMsgLabel.Foreground = Brushes.Brown;
                InfoMsgLabel.Content = "Криптостойкость средняя.";
            }
            if (int.Parse(KeyLengthTextBox.Text) < 24 && int.Parse(KeyLengthTextBox.Text) >= 8)
            {
                InfoMsgLabel.Foreground = Brushes.Orange;
                InfoMsgLabel.Content = "Криптостойкость ниже среднего.";
            }
            if (int.Parse(KeyLengthTextBox.Text) <= 8 && int.Parse(KeyLengthTextBox.Text) > 3)
            {
                InfoMsgLabel.Foreground = Brushes.Red;
                InfoMsgLabel.Content = "Криптостойкость низкая.";
            }
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _popup = new Popup
        {
            Child = new TextBlock {
                Text = "Водите мышью в области окон программы...",
                Background = Brushes.Yellow
            },
            Placement = PlacementMode.AbsolutePoint,
            StaysOpen = true,
            IsOpen = true
        };
        MouseMove += MouseMoveMethod;
        bool captured = CaptureMouse();
        if (! captured)
        {
            MessageBox.Show("Выход за границы графического ввода данных.", "ВНИМАНИЕ!", MessageBoxButton.OK);
        }
    }

    private void MouseMoveMethod(object sender, MouseEventArgs e)
    {
        var relativePosition = Mouse.GetPosition(this);
        var point = PointToScreen(relativePosition);
        _popup.HorizontalOffset = point.X;
        _popup.VerticalOffset = point.Y;
        int keyByteLength = Encoding.Unicode.GetBytes(_key.KeyValue).Length;
        if (keyByteLength < _key.KeyLength)
        {
            iter++;
            byte L = (byte)((point.X + rand.Next(0, 100) * point.Y + rand.Next(0, 100)) % 94); // {32; 125} ASCII
            L += 32;
            _key.KeyValue += (char)L;
            NewKeyTextBox.Text = _key.KeyValue;
            KeyLiveLengthLabel.Content = iter.ToString();
            Thread.Sleep(50);
        }
        else
        {
            _popup.IsOpen = false;
            UIElement el = (UIElement)sender;
            el.ReleaseMouseCapture();
            MouseMove -= MouseMoveMethod;
            ResultMsgLabel.Content = "Ключ сгенерирован.";
            MainInfoLabel.Content = "Примите ключ, перегенерируйте или откажитесь.";
            KeyLiveLengthLabel.Content = "";
            isGenEnabled = false;
            Generate.IsEnabled = true;
            SubmitButton.IsEnabled = true;
            KeyLengthSlider.IsEnabled = true;
            KeyLengthTextBox.IsReadOnly = false;
        }
    }

    private static bool ValidateIsNum(string value)
    {
        Regex regex = new("[^0-9]+");
        return regex.IsMatch(value);
    }
}
