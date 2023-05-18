using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecognizerGenerator
{
  class CodeGeneratorToPascal
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
    // TODO подумать над генерацией множества значений для конечных состояний вместо задания отдельного типа
    public const string TYPE_NAME_FINAL_STATES = "TFinalState";
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
    public const string VAR_NAME_STATE_SUCCESS = "SuccessState";
    public const string VAR_NAME_SINGLE_CHAR = "SingleChar";
    public const string VAR_NAME_SINGLE_CHAR_KIND = "InputSymbolKind";
    #endregion
    #endregion

    private readonly FiniteStateMachine _recognizerStateMachine;

    // TODO сделать проверку имени
    public string RecognizerProgramName { get; set; } = "recognizer";

    public CodeGeneratorToPascal(FiniteStateMachine recognizerStateMachine)
    {
      _recognizerStateMachine = recognizerStateMachine;
    }

    /// <summary>
    /// Создаёт текст программы-распознавателя по заданному конечному автомату
    /// </summary>
    /// <returns>Текст программы-распознавателя</returns>
    public string[] GenerateRecognizerCode()
    {
      // TODO
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
      List<string> constantSection = new() { "const" };
      constantSection.AddRange(GetStatesConstants());
      constantSection.Add("");
      constantSection.AddRange(GetInputSymbolsConstants());
      constantSection.Add("");
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
        $"{TYPE_NAME_STATE} = {OUT_PREFIX_STATE}{states[0]}..{OUT_PREFIX_STATE}{states.Last()};",
        // TODO подумать, что делать с конечными состояниями
        $"{TYPE_NAME_FINAL_STATES} = {OUT_PREFIX_STATE}{states[0]}..{OUT_PREFIX_STATE}{states.Last()};",
        $"{TYPE_NAME_INPUT_SYMBOL} = {OUT_PREFIX_INPUT_SYMBOL}{inputSymbols[0]}..{OUT_PREFIX_INPUT_SYMBOL}{inputSymbols.Last()};"
      };
    }

    private List<string> GetVariableSection()
    {
      List<string> variableSection = new()
      {
        "var",
        $"{VAR_NAME_CURRENT_STATE} : {TYPE_NAME_STATE};",
        $"{VAR_NAME_INPUT_STRING_LENGTH} : integer;",
        $"{VAR_NAME_COUNTER_I} : integer;",
        $"{VAR_NAME_TRANSITION_TABLE} : array[0..{CONSTANT_NAME_STATES_COUNT} - 1, 0..{CONSTANT_NAME_INPUT_SYMBOLS_COUNT} - 1] of {TYPE_NAME_STATE};",
        $"{VAR_NAME_INPUT_STRING} : string;",
        $"{VAR_NAME_STATE_SUCCESS} : {TYPE_NAME_FINAL_STATES};",
        $"{VAR_NAME_SINGLE_CHAR} : char;",
        $"{VAR_NAME_SINGLE_CHAR_KIND} : {TYPE_NAME_INPUT_SYMBOL};"
      };
      variableSection.AddRange(_recognizerStateMachine.InputSymbols.Select(
        s => $"{REAL_INPUT_SYMBOLS_PREFIX}{s} : set of char;"));
      return variableSection;
    }

    private List<string> GetProgramStatements()
    {
      List<string> statements = new() { "begin" };
      statements.AddRange(GetDataInitializationBlock());
      statements.Add("end.");
      return statements;
    }

    private List<string> GetDataInitializationBlock()
    {
      List<string> dataInitialization = new();
      return dataInitialization;
    }
  }
}
