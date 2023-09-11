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
    public string KeyTmpValue;
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
        label1.Content = "Задайте длину секретного ключа в байтах (4-56)";
        textBox1.Focus();
    }

    private void Generate_Click(object sender, RoutedEventArgs e)
    {
        if (textBox1.Text == "" || textBox1.Text == "0")
        {
            _key.KeyValue = "";
            MessageBox.Show("Длина ключа не определена.", "ВНИМАНИЕ!", MessageBoxButton.OK);
            textBox1.Text = "56";
            return;
        }
        label1.Content = "Перемещайте курсор для генерации ключа.";
        _key.KeyLength = int.Parse(textBox1.Text);
        textBox2.Text = "";
        label3.Content = "Генерируется:";
        iter = 0;
        label4.Content = iter.ToString();
        _key.KeyValue = "";
        rand = new Random();
        isGenEnabled = true;
        Generate.IsEnabled = false;
        button3.IsEnabled = false;
        slider1.IsEnabled = false;
        textBox1.IsReadOnly = true;

        border1_MouseMove(sender, e);
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Escape)
            this.Close();
        if (e.Key == System.Windows.Input.Key.Enter)
            button3_Click(sender, e);
    }

    private void border1_MouseMove(object sender, RoutedEventArgs e)
    {
        if (isGenEnabled == true)
        {
            OnLoaded(sender, e);
        }
    }

    private void button2_Click(object sender, RoutedEventArgs e)
    {
        _key.KeyValue = "";
        this.Close();
    }

    private void button3_Click(object sender, RoutedEventArgs e)
    {
        int keyByteLength = Encoding.Unicode.GetBytes(_key.KeyValue).Length;
        if (textBox1.Text == "" || int.Parse(textBox1.Text) != keyByteLength || textBox1.Text == "0")
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

    private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _key.KeyValue = "";
        textBox2.Text = _key.KeyValue;
    }

    private void textBox1_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = ValidateIsNum(e.Text);
    }
    
    private void textBox1_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space || ((textBox1.Text == "" || textBox1.Text == "0") && (e.Key == Key.D0 || e.Key == Key.NumPad0)))
            e.Handled = true;
    }

    private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (ValidateIsNum(textBox1.Text))
        {
            MessageBox.Show("Недопустимый формат. Отменено.", "ВНИМАНИЕ!", MessageBoxButton.OK);
            textBox1.Text = "56";
        }

        if (textBox1.Text == "")
        {
            label5.Foreground = Brushes.Red;
            label5.Content = "Криптостойкость не задана.";
        }
        else
        {
            if (int.Parse(textBox1.Text) == 56)
            {
                label5.Foreground = Brushes.DarkGreen;
                label5.Content = "Криптостойкость максимальная.";
            }
            if (int.Parse(textBox1.Text) < 56 && int.Parse(textBox1.Text) >= 32)
            {
                label5.Foreground = Brushes.Green;
                label5.Content = "Криптостойкость высокая.";
            }
            if (int.Parse(textBox1.Text) < 32 && int.Parse(textBox1.Text) >= 24)
            {
                label5.Foreground = Brushes.Brown;
                label5.Content = "Криптостойкость средняя.";
            }
            if (int.Parse(textBox1.Text) < 24 && int.Parse(textBox1.Text) >= 8)
            {
                label5.Foreground = Brushes.Orange;
                label5.Content = "Криптостойкость ниже среднего.";
            }
            if (int.Parse(textBox1.Text) <= 8 && int.Parse(textBox1.Text) > 3)
            {
                label5.Foreground = Brushes.Red;
                label5.Content = "Криптостойкость низкая.";
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
            byte L = (byte)((point.X + rand.Next(0, 100) * point.Y + rand.Next(0, 100)) % 94);
            L += 32;
            _key.KeyValue += (char)L;
            textBox2.Text = _key.KeyValue;
            label4.Content = iter.ToString();
            Thread.Sleep(5);
        }
        else
        {
            _popup.IsOpen = false;
            UIElement el = (UIElement)sender;
            el.ReleaseMouseCapture();
            MouseMove -= MouseMoveMethod;
            label3.Content = "Ключ сгенерирован.";
            label1.Content = "Примите ключ, перегенерируйте или откажитесь.";
            label4.Content = "";
            isGenEnabled = false;
            Generate.IsEnabled = true;
            button3.IsEnabled = true;
            slider1.IsEnabled = true;
            textBox1.IsReadOnly = false;
        }
    }

    private static bool ValidateIsNum(string value)
    {
        Regex regex = new("[^0-9]+");
        return regex.IsMatch(value);
    }
}
