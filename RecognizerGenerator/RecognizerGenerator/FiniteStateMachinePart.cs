using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RecognizerGenerator
{
  public abstract class FiniteStateMachinePart : INotifyPropertyChanged
  {
    private string _name = "";

    public static event PropertyChangedEventHandler? PropertyChangedCommon;
    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name
    {
      get => _name;
      set => SetField(ref _name, value);
    }

    public FiniteStateMachinePart() { }
    public FiniteStateMachinePart(string parName)
    {
      Name = parName;
    }

    /// <summary>
    /// Устанавливает значение поля и уведомляет об изменении свойства
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="refField"></param>
    /// <param name="parValue"></param>
    /// <param name="parPropertyName"></param>
    protected void SetField<T>(ref T refField, T parValue, [CallerMemberName] string parPropertyName = "")
    {
      refField = parValue;
      NotifyAboutPropertyChanged(parPropertyName);
    }

    /// <summary>
    /// Функция для вызова события, возникающего при изменении некоторого свойства
    /// </summary>
    /// <param name="parPropertyName">Имя изменившегося свойства (при реализации внутри самих свойств параметр можно опустить)</param>
    protected void NotifyAboutPropertyChanged(string parPropertyName)
    {
      PropertyChangedEventArgs e = new(parPropertyName);
      PropertyChanged?.Invoke(this, e);
      PropertyChangedCommon?.Invoke(this, e);
    }
  }
}
