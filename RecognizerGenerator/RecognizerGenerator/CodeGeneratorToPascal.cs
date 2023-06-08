using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RecognizerGenerator
{
  internal class CodeGeneratorToPascal : ICodeGenerator
  {
    #region Константы для настройки имён в генерации выходной программы
    #region Префиксы для констант состояний и входных символов
    public const string OUT_PREFIX_STATE = "state_";
    public const string OUT_PREFIX_INPUT_SYMBOL = "input_symbol_";
    #endregion

    #region Имена для "размерных" (количественных) констант
    public const string CONSTANT_NAME_STATES_COUNT = "states_count";
    public const string CONSTANT_NAME_INPUT_SYMBOLS_COUNT = "input_symbols_count";
    #endregion

    #region Имена типов в секции типов (type)
    public const string TYPE_NAME_STATE = "TState";
    public const string TYPE_NAME_INPUT_SYMBOL = "TInputSymbol";
    #endregion

    #region Имена префиксов для переменных под множества символов из реального входного потока
    public const string REAL_INPUT_SYMBOLS_PREFIX = "InputCharSet_";
    #endregion

    #region Имена переменных в секции переменных (var)
    public const string VAR_NAME_CURRENT_STATE = "CurrentState";
    public const string VAR_NAME_INPUT_STRING_LENGTH = "InputStringLength";
    public const string VAR_NAME_COUNTER_I = "i";
    public const string VAR_NAME_TRANSITION_TABLE = "TransitionTable";
    public const string VAR_NAME_INPUT_STRING = "InputString";
    public const string VAR_NAME_SINGLE_CHAR = "SingleChar";
    public const string VAR_NAME_SINGLE_CHAR_KIND = "InputSymbolKind";
    public const string VAR_NAME_FINAL_STATES_SET = "FinalStates";
    #endregion

    #region Сообщения программы
    public const string MESSAGE_ACCEPTED = "Допускается";
    public const string MESSAGE_REJECTED = "Не допускается";
    #endregion
    #endregion

    private readonly FiniteStateMachine _recognizerStateMachine;
    private readonly bool _isLastCharacterUniversal;

    public string RecognizerProgramName { get; set; } = "recognizer";

    public CodeGeneratorToPascal(FiniteStateMachine recognizerStateMachine, bool isLastCharacterUniversal)
    {
      _recognizerStateMachine = recognizerStateMachine;
      _isLastCharacterUniversal = isLastCharacterUniversal;
    }

    /// <summary>
    /// Создаёт текст программы-распознавателя по заданному конечному автомату
    /// </summary>
    /// <returns>Текст программы-распознавателя</returns>
    public string[] GenerateRecognizerCode()
    {
      List<string> code = new() { GetRecognizerProgramName() };
      code.AddRange(GetConstantSection());
      code.Add("");
      code.AddRange(GetTypeSection());
      code.Add("");
      code.AddRange(GetVariableSection());
      code.Add("");
      code.AddRange(GetProgramStatements());
      return code.ToArray();
    }

    private string GetRecognizerProgramName()
    {
      return $"program {RecognizerProgramName};";
    }

    private List<string> GetConstantSection()
    {
      List<string> constantSection = new()
      {
        "const",
        "{Состояния автомата}"
      };
      constantSection.AddRange(GetStatesConstants());
      constantSection.Add("");
      constantSection.Add("{Входные символы автомата}");
      constantSection.AddRange(GetInputSymbolsConstants());
      constantSection.Add("");
      constantSection.Add("{Количество строк (состояний) и столбцов (входных символов) таблицы переходов}");
      constantSection.AddRange(GetQuantitativeConstants());
      return constantSection;
    }

    private List<string> GetStatesConstants()
    {
      int counter = 0;
      return _recognizerStateMachine.States.Select(
        s => $"{OUT_PREFIX_STATE}{s} = {counter++};")
        .ToList();
    }

    private List<string> GetInputSymbolsConstants()
    {
      int counter = 0;
      return _recognizerStateMachine.InputSymbols.Select(
        s => $"{OUT_PREFIX_INPUT_SYMBOL}{s} = {counter++};")
        .ToList();
    }

    private List<string> GetQuantitativeConstants()
    {
      return new List<string>()
      {
        $"{CONSTANT_NAME_STATES_COUNT} = {_recognizerStateMachine.States.Count};",
        $"{CONSTANT_NAME_INPUT_SYMBOLS_COUNT} = {_recognizerStateMachine.InputSymbols.Count};"
      };
    }

    private List<string> GetTypeSection()
    {
      List<MachineState> states = _recognizerStateMachine.States;
      List<InputSymbol> inputSymbols = _recognizerStateMachine.InputSymbols;
      return new List<string>()
      {
        "type",
        "{Тип состояния автомата}",
        $"{TYPE_NAME_STATE} = {OUT_PREFIX_STATE}{states[0]}..{OUT_PREFIX_STATE}{states.Last()};",
        "",
        "{Тип входного символа автомата}",
        $"{TYPE_NAME_INPUT_SYMBOL} = {OUT_PREFIX_INPUT_SYMBOL}{inputSymbols[0]}..{OUT_PREFIX_INPUT_SYMBOL}{inputSymbols.Last()};"
      };
    }

    private List<string> GetVariableSection()
    {
      List<string> variableSection = new()
      {
        "var",
        "{Текущее состояние автомата в момент работы}",
        $"{VAR_NAME_CURRENT_STATE} : {TYPE_NAME_STATE};",
        $"{VAR_NAME_INPUT_STRING_LENGTH} : integer;",
        $"{VAR_NAME_COUNTER_I} : integer;",
        $"{VAR_NAME_TRANSITION_TABLE} : array[0..{CONSTANT_NAME_STATES_COUNT} - 1, 0..{CONSTANT_NAME_INPUT_SYMBOLS_COUNT} - 1] of {TYPE_NAME_STATE};",
        $"{VAR_NAME_INPUT_STRING} : string;",
        "",
        "{Обрабатываемый символ из входного потока}",
        $"{VAR_NAME_SINGLE_CHAR} : char;",
        "",
        "{Тип обрабатываемого символа}",
        $"{VAR_NAME_SINGLE_CHAR_KIND} : {TYPE_NAME_INPUT_SYMBOL};",
        "",
        "{Конечные состояния автомата}",
        $"{VAR_NAME_FINAL_STATES_SET} : set of {TYPE_NAME_STATE};",
        "",
        "{Множества для сопоставления реальных поступающих символов их типам}"
      };
      variableSection.AddRange(_recognizerStateMachine.InputSymbols.Select(
        s => $"{REAL_INPUT_SYMBOLS_PREFIX}{s} : set of char;"));
      return variableSection;
    }

    private List<string> GetProgramStatements()
    {
      List<string> statements = new() { "begin" };
      statements.AddRange(GetDataInitializationBlock());
      statements.AddRange(GetWhileLoopStatement());
      statements.AddRange(GetEndChecking());
      statements.Add("end.");
      return statements;
    }

    private List<string> GetDataInitializationBlock()
    {
      List<string> dataInitialization = new() { "{Определение соответствия реальных символов входным символам автомата}" };
      dataInitialization.AddRange(GetInputSymbolSets());
      dataInitialization.Add("");
      dataInitialization.Add("{Заполнение таблицы переходов автомата}");
      dataInitialization.AddRange(GetTransitionTableInitialization());
      dataInitialization.Add("{Считывание входной строки}");
      dataInitialization.AddRange(GetInputStringReading());
      dataInitialization.Add("");
      dataInitialization.AddRange(GetVariableInitialization());
      return dataInitialization;
    }

    private List<string> GetInputSymbolSets()
    {
      List<string> symbolSets = new();

      // для каждого входного символа формируется соответствующее множество со значениями из пользовательского ввода
      foreach (InputSymbol inputSymbol in _recognizerStateMachine.InputSymbols)
      {
        StringBuilder symbolsSetInitializationExpression = new(
          $"{REAL_INPUT_SYMBOLS_PREFIX}{inputSymbol.Name} := [");
        symbolsSetInitializationExpression.Append(GetRealCharactersSetExpression(inputSymbol.AcceptedCharactersExpression));
        symbolsSetInitializationExpression.Append("];");
        symbolSets.Add(symbolsSetInitializationExpression.ToString());
      }
      return symbolSets;
    }

    private static StringBuilder GetRealCharactersSetExpression(string acceptedCharactersExpression)
    {
      // распознаёт и захватывает диапазоны непробельных символов вида <начальный символ>-<конечный символ>
      // примеры: A-Z, f-q, !-5,
      // а также одиночные символы: s, t, _, *, +
      Regex charRangeRegex = new(@"(?<rangeStart>\S)\-(?<rangeEnd>\S)|(?<single>\S)", RegexOptions.ExplicitCapture);

      StringBuilder setInitializationExpression = new();
      MatchCollection matchedCharRanges = charRangeRegex.Matches(acceptedCharactersExpression);
      foreach (Match match in matchedCharRanges.Cast<Match>())
      {
        if (match.Groups["single"].Success)
        {
          string single = match.Groups["single"].Value;
          setInitializationExpression.Append($"\'{single}\', ");
        }
        else
        {
          string start = match.Groups["rangeStart"].Value;
          string end = match.Groups["rangeEnd"].Value;
          setInitializationExpression.Append($"\'{start}\'..\'{end}\', ");
        }
      }

      // удаление последней запятой с пробелом
      if (setInitializationExpression.Length >= 2)
        setInitializationExpression.Remove(setInitializationExpression.Length - 2, 2);
      return setInitializationExpression;
    }

    private List<string> GetTransitionTableInitialization()
    {
      List<MachineState> states = _recognizerStateMachine.States;
      List<InputSymbol> inputSymbols = _recognizerStateMachine.InputSymbols;
      List<string> transitionTableInitialization = new();
      for (int i = 0; i < states.Count; i++)
      {
        for (int j = 0; j < inputSymbols.Count; j++)
        {
          transitionTableInitialization.Add(
            $"{VAR_NAME_TRANSITION_TABLE}[{OUT_PREFIX_STATE}{states[i].Name}, {OUT_PREFIX_INPUT_SYMBOL}{inputSymbols[j].Name}]" +
            $" := {OUT_PREFIX_STATE}{_recognizerStateMachine.TransitionTable[i][j].Name};");
        }
        transitionTableInitialization.Add("");
      }
      return transitionTableInitialization;
    }

    private static List<string> GetInputStringReading()
    {
      return new()
      {
        $"readln({VAR_NAME_INPUT_STRING});",
        $"{VAR_NAME_INPUT_STRING_LENGTH} := Length({VAR_NAME_INPUT_STRING});"
      };
    }

    private List<string> GetVariableInitialization()
    {
      StringBuilder finalStates = new();
      foreach (MachineState state in _recognizerStateMachine.States)
      {
        if (state.IsFinalState)
          finalStates.Append($"{OUT_PREFIX_STATE}{state}, ");
      }

      // удаление последней запятой с пробелом
      if (finalStates.Length > 2)
        finalStates.Remove(finalStates.Length - 2, 2);
      return new()
      {
        "{Определение конечных состояний}",
        $"{VAR_NAME_FINAL_STATES_SET} := [{finalStates}];",
        "",
        "{Установка автомата в начальное состояние}",
        $"{VAR_NAME_CURRENT_STATE} := {OUT_PREFIX_STATE}{_recognizerStateMachine.InitialState.Name};",
        $"{VAR_NAME_COUNTER_I} := 1;"
      };
    }

    private List<string> GetWhileLoopStatement()
    {
      List<string> loopStatement = new()
      {
        "",
        "{Пока в строке не кончились символы}",
        $"while {VAR_NAME_COUNTER_I} <= {VAR_NAME_INPUT_STRING_LENGTH} do",
        "begin",
        "{Взятие очередного символа и определение его типа}",
        $"{VAR_NAME_SINGLE_CHAR} := {VAR_NAME_INPUT_STRING}[{VAR_NAME_COUNTER_I}];"
      };
      loopStatement.AddRange(GetIfStatements());
      loopStatement.Add("");
      loopStatement.Add("{Переход автомата в новое состояние и смещение на следующий символ}");
      loopStatement.Add($"{VAR_NAME_CURRENT_STATE} := {VAR_NAME_TRANSITION_TABLE}[{VAR_NAME_CURRENT_STATE}, {VAR_NAME_SINGLE_CHAR_KIND}];");
      loopStatement.Add($"{VAR_NAME_COUNTER_I} := {VAR_NAME_COUNTER_I} + 1;");
      loopStatement.Add("end;");
      return loopStatement;
    }

    private List<string> GetIfStatements()
    {
      List<string> ifStatements = new();
      List<InputSymbol> symbolsInConditions = _isLastCharacterUniversal
        ? _recognizerStateMachine.InputSymbols.Take(_recognizerStateMachine.InputSymbols.Count - 1).ToList()
        : _recognizerStateMachine.InputSymbols;
      foreach (InputSymbol inputSymbol in symbolsInConditions)
      {
        if (inputSymbol.Excusion)
          ifStatements.Add($"if not ({VAR_NAME_SINGLE_CHAR} in {REAL_INPUT_SYMBOLS_PREFIX}{inputSymbol.Name}) then");
        else
          ifStatements.Add($"if {VAR_NAME_SINGLE_CHAR} in {REAL_INPUT_SYMBOLS_PREFIX}{inputSymbol.Name} then");
        ifStatements.Add($"{VAR_NAME_SINGLE_CHAR_KIND} := {OUT_PREFIX_INPUT_SYMBOL}{inputSymbol.Name}");
        ifStatements.Add("else");
      }

      if (_isLastCharacterUniversal)
        ifStatements.Add($"{VAR_NAME_SINGLE_CHAR_KIND} := {OUT_PREFIX_INPUT_SYMBOL}{_recognizerStateMachine.InputSymbols.Last().Name};");
      else
      {
        ifStatements.Add("begin");
        ifStatements.Add($"writeln(\'{MESSAGE_REJECTED}\');");
        ifStatements.Add("exit;");
        ifStatements.Add("end;");
      }
      return ifStatements;
    }

    private static List<string> GetEndChecking()
    {
      return new()
      {
        "",
        "{Вывод результата}",
        $"if {VAR_NAME_CURRENT_STATE} in {VAR_NAME_FINAL_STATES_SET} then",
        $"writeln(\'{MESSAGE_ACCEPTED}\')",
        $"else",
        $"writeln(\'{MESSAGE_REJECTED}\')"
      };
    }
  }
}
