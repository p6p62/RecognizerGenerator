using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecognizerGenerator
{
  /// <summary>
  /// Интерфейс, реализуемый генераторами кода
  /// </summary>
  internal interface ICodeGenerator
  {
    /// <summary>
    /// Создаёт текст программы-распознавателя по заданному конечному автомату
    /// </summary>
    /// <returns>Текст программы-распознавателя</returns>
    public string[] GenerateRecognizerCode();
  }
}
