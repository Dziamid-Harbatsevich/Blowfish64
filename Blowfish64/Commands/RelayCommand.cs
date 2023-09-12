using System;
using System.Windows.Input;

namespace Blowfish64.Commands;

public class RelayCommand : ICommand
{
    /// <summary>
    /// Признак CanExecute
    /// </summary>
    private Func<object, bool> canExecute;
    /// <summary>
    /// делегат метода Execute
    /// </summary>
    private Action<object> executeMethod;

    public static bool IsExecuted { get; set; } = false;

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="executeMethod">делегат метода Execute</param>
    /// <param name="canExecute">делегат признака CanExecute</param>
    public RelayCommand(Action<object> executeMethod, Func<object, bool> canExecute)
    {
        this.executeMethod = executeMethod;
        this.canExecute = canExecute;
    }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="executeMethod">делегат метода Execute</param>
    public RelayCommand(Action<object> executeMethod)
    {
        this.executeMethod = executeMethod;
        IsExecuted = true;
    }

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object? parameter)
    {
        return canExecute?.Invoke(parameter) ?? true;
    }
    public void Execute(object? parameter)
    {
        executeMethod(parameter);
    }
}
