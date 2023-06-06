using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RecognizerGenerator
{
  internal class CodeGeneratorToPython3
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

    #region Имена префиксов для переменных под множества символов из реального входного потока
    public const string REAL_INPUT_SYMBOLS_PREFIX = "input_char_set_";
    #endregion

    #region Название функции для получения диапазона символов
    private const string FUNCTION_CHAR_RANGE_NAME = "char_range";
    #endregion

    #region Имена переменных в секции переменных
    public const string VAR_NAME_CURRENT_STATE = "current_state";
    public const string VAR_NAME_TRANSITION_TABLE = "transition_table";
    public const string VAR_NAME_INPUT_STRING = "input_string";
    public const string VAR_NAME_SINGLE_CHAR = "single_char";
    public const string VAR_NAME_SINGLE_CHAR_KIND = "input_symbol_kind";
    public const string VAR_NAME_FINAL_STATES_SET = "final_states";
    #endregion

    #region Сообщения программы
    public const string MESSAGE_ACCEPTED = "Допускается";
    public const string MESSAGE_REJECTED = "Не допускается";
    #endregion
    #endregion

    private readonly FiniteStateMachine _recognizerStateMachine;
    private readonly bool _isLastCharacterUniversal;

    public CodeGeneratorToPython3(FiniteStateMachine recognizerStateMachine, bool isLastCharacterUniversal)
    {
      _recognizerStateMachine = recognizerStateMachine;
      _isLastCharacterUniversal = isLastCharacterUniversal;
    }

    private static string Tab(int n = 4)
    {
      return new(' ', n);
    }

    /// <summary>
    /// Создаёт текст программы-распознавателя по заданному конечному автомату
    /// </summary>
    /// <returns>Текст программы-распознавателя</returns>
    public string[] GenerateRecognizerCode()
    {
      List<string> code = new();
      code.AddRange(GetConstantSection());
      code.Add("");
      code.Add("");
      code.AddRange(GetCharRangeFunctionSection());
      code.Add("");
      code.Add("");
      code.AddRange(GetProgramStatements());
      return code.ToArray();
    }

    private List<string> GetConstantSection()
    {
      List<string> constantSection = new()
      {
        "# Состояния автомата"
      };
      constantSection.AddRange(GetStatesConstants());
      constantSection.Add("");
      constantSection.Add("# Входные символы автомата");
      constantSection.AddRange(GetInputSymbolsConstants());
      constantSection.Add("");
      constantSection.Add("# Количество строк (состояний) и столбцов (входных символов) таблицы переходов");
      constantSection.AddRange(GetQuantitativeConstants());
      return constantSection;
    }

    private List<string> GetStatesConstants()
    {
      int counter = 0;
      return _recognizerStateMachine.States.Select(
        s => $"{OUT_PREFIX_STATE}{s} = {counter++}")
        .ToList();
    }

    private List<string> GetInputSymbolsConstants()
    {
      int counter = 0;
      return _recognizerStateMachine.InputSymbols.Select(
        s => $"{OUT_PREFIX_INPUT_SYMBOL}{s} = {counter++}")
        .ToList();
    }

    private List<string> GetQuantitativeConstants()
    {
      return new List<string>()
      {
        $"{CONSTANT_NAME_STATES_COUNT} = {_recognizerStateMachine.States.Count}",
        $"{CONSTANT_NAME_INPUT_SYMBOLS_COUNT} = {_recognizerStateMachine.InputSymbols.Count}"
      };
    }

    private static List<string> GetCharRangeFunctionSection()
    {
      return new List<string>()
      {
        $"def {FUNCTION_CHAR_RANGE_NAME}(c1: str, c2: str):",
        $"{Tab()}return {{chr(c) for c in range(ord(c1), ord(c2) + 1)}}"
      };
    }

    private List<string> GetProgramStatements()
    {
      List<string> statements = new() { "def recognizer():" };
      statements.AddRange(GetDataInitializationBlock());
      statements.AddRange(GetLoopStatement());
      statements.AddRange(GetEndChecking());
      statements.Add("");
      statements.Add("");
      statements.Add("recognizer()");
      return statements;
    }

    private List<string> GetDataInitializationBlock()
    {
      List<string> dataInitialization = new() { $"{Tab()}# Определение соответствия реальных символов входным символам автомата" };
      dataInitialization.AddRange(GetInputSymbolSets());
      dataInitialization.Add("");
      dataInitialization.Add($"{Tab()}# Заполнение таблицы переходов автомата");
      dataInitialization.AddRange(GetTransitionTableInitialization());
      dataInitialization.Add($"{Tab()}# Считывание входной строки");
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
          $"{Tab()}{REAL_INPUT_SYMBOLS_PREFIX}{inputSymbol.Name} = {{");
        symbolsSetInitializationExpression.Append(GetRealCharactersSetExpression(inputSymbol.AcceptedCharactersExpression));
        symbolsSetInitializationExpression.Append('}');
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
          setInitializationExpression.Append($"*{FUNCTION_CHAR_RANGE_NAME}(\'{start}\', \'{end}\'), ");
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
      List<string> transitionTableInitialization = new()
      {
        $"{Tab()}{VAR_NAME_TRANSITION_TABLE} = tuple([0 for _ in range({CONSTANT_NAME_INPUT_SYMBOLS_COUNT})] for _ in range({CONSTANT_NAME_STATES_COUNT}))"
      };
      for (int i = 0; i < states.Count; i++)
      {
        for (int j = 0; j < inputSymbols.Count; j++)
        {
          transitionTableInitialization.Add(
            $"{Tab()}{VAR_NAME_TRANSITION_TABLE}[{OUT_PREFIX_STATE}{states[i].Name}][{OUT_PREFIX_INPUT_SYMBOL}{inputSymbols[j].Name}]" +
            $" = {OUT_PREFIX_STATE}{_recognizerStateMachine.TransitionTable[i][j].Name}");
        }
        transitionTableInitialization.Add("");
      }
      return transitionTableInitialization;
    }

    private static List<string> GetInputStringReading()
    {
      return new()
      {
        $"{Tab()}{VAR_NAME_INPUT_STRING} = input()"
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
        $"{Tab()}# Определение конечных состояний",
        $"{Tab()}{VAR_NAME_FINAL_STATES_SET} = {{{finalStates}}}",
        "",
        $"{Tab()}# Установка автомата в начальное состояние",
        $"{Tab()}{VAR_NAME_CURRENT_STATE} = {OUT_PREFIX_STATE}{_recognizerStateMachine.InitialState.Name}"
      };
    }

    private List<string> GetLoopStatement()
    {
      List<string> loopStatement = new()
      {
        "",
        $"{Tab()}for {VAR_NAME_SINGLE_CHAR} in {VAR_NAME_INPUT_STRING}:",
        $"{Tab()}{Tab()}# Взятие очередного символа и определение его типа"
      };
      loopStatement.AddRange(GetIfStatements());
      loopStatement.Add("");
      loopStatement.Add($"{Tab()}{Tab()}# Переход автомата в новое состояние и смещение на следующий символ");
      loopStatement.Add($"{Tab()}{Tab()}{VAR_NAME_CURRENT_STATE} = {VAR_NAME_TRANSITION_TABLE}[{VAR_NAME_CURRENT_STATE}][{VAR_NAME_SINGLE_CHAR_KIND}]");
      return loopStatement;
    }

    private List<string> GetIfStatements()
    {
      List<string> ifStatements = new();
      List<InputSymbol> symbolsInConditions = _isLastCharacterUniversal
        ? _recognizerStateMachine.InputSymbols.Take(_recognizerStateMachine.InputSymbols.Count - 1).ToList()
        : _recognizerStateMachine.InputSymbols;
      int counter = 0;
      foreach (InputSymbol inputSymbol in symbolsInConditions)
      {
        StringBuilder expr = counter++ > 0 ? new($"{Tab()}{Tab()}elif ") : new($"{Tab()}{Tab()}if ");
        if (inputSymbol.Excusion)
          expr.Append($"not {VAR_NAME_SINGLE_CHAR} in {REAL_INPUT_SYMBOLS_PREFIX}{inputSymbol.Name}:");
        else
          expr.Append($"{VAR_NAME_SINGLE_CHAR} in {REAL_INPUT_SYMBOLS_PREFIX}{inputSymbol.Name}:");
        ifStatements.Add(expr.ToString());
        ifStatements.Add($"{Tab()}{Tab()}{Tab()}{VAR_NAME_SINGLE_CHAR_KIND} = {OUT_PREFIX_INPUT_SYMBOL}{inputSymbol.Name}");
      }
      ifStatements.Add($"{Tab()}{Tab()}else:");

      if (_isLastCharacterUniversal)
        ifStatements.Add($"{Tab()}{Tab()}{Tab()}{VAR_NAME_SINGLE_CHAR_KIND} = {OUT_PREFIX_INPUT_SYMBOL}{_recognizerStateMachine.InputSymbols.Last().Name}");
      else
      {
        ifStatements.Add($"{Tab()}{Tab()}{Tab()}print(\'{MESSAGE_REJECTED}\')");
        ifStatements.Add($"{Tab()}{Tab()}{Tab()}return");
      }
      return ifStatements;
    }

    private static List<string> GetEndChecking()
    {
      return new()
      {
        "",
        $"{Tab()}# Вывод результата",
        $"{Tab()}if {VAR_NAME_CURRENT_STATE} in {VAR_NAME_FINAL_STATES_SET}:",
        $"{Tab()}{Tab()}print(\'{MESSAGE_ACCEPTED}\')",
        $"{Tab()}else:",
        $"{Tab()}{Tab()}print(\'{MESSAGE_REJECTED}\')"
      };
    }
  }
}
