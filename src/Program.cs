

internal static class Program
{
    private static readonly ICalculatorEngine Calculator; private const string CmdExit = "EXIT";
    static Program() {
        var parser = new UserInputParser();
        Calculator = new CalculatorEngine(parser);
    }
    private static void Main(string[] args) {
        string cmd;
        do {
            cmd = InteractionHelper.AskInput();
            if (!IsExit(cmd)) {
                var result = Calculator.Eval(cmd);
                InteractionHelper.DisplayResult(result);
            }
        }
        while (!IsExit(cmd));
    }
    private static bool IsExit(string userInput) {
        return string.Equals(userInput, CmdExit, StringComparison.OrdinalIgnoreCase);
    }

}

internal static class InteractionHelper
{
    private static readonly Version AppVersion;
    static InteractionHelper() {
        AppVersion = Assembly.GetExecutingAssembly().GetName().Version;
    }
    public static string AskInput() {
        Console.WriteLine("=======================");
        Console.WriteLine("USELESS CALC v.{0}", AppVersion);
        Console.WriteLine("-----------------------");
        Console.WriteLine("Cosa posso fare per te?");
        return Console.ReadLine();
    }
    public static void DisplayError(string userInput) {
        var help = "USO di UselessCalc:\n\n   exit    Esce dall'applicazione\n";
        Console.WriteLine("ERRORE: Il comando '{0}' non è valido o non è supportato", userInput);
        Console.WriteLine(Environment.NewLine + help);
    }
    public static void DisplayResult(object result) {
        Console.WriteLine("RESPONSE: {0}", result);
    }
}

public class SumParserTest
{
    private readonly UserInputParser _sut;
    public SumParserTest() {
        _sut = new UserInputParser();
    }
    [Theory(DisplayName = "Deve gestire le addizioni")]
    [InlineData("8+34", true, 42)]
    [InlineData("10-34", false, 0)]
    [InlineData("10+34+4", false, 0)]
    [InlineData("10 34+4", false, 0)]
    [InlineData("a+1", false, 0)]
    [InlineData(null, false, 0)]
    public void Parse_should_support_adds(string input, bool canParse, decimal expected)
    {
        IOperation operation;
        var res = _sut.TryParse(input, out operation);
        res.Should().Be(canParse);
        if (canParse) {
            operation.Should().Be.OfType<SumOperation>();
            operation.Eval().Should().Be.EqualTo(expected); }
        else
        {
            operation.Should().Be.Null();
        }
    }
}

public class SumOperation : IOperation { public decimal Eval() { throw new System.NotImplementedException(); } }


public class DiffTest
{
    private readonly UserInputParser _sut;
    public DiffTest() { _sut = new UserInputParser(); }
    [Theory(DisplayName = "Deve gestire le sottrazioni")]
    [InlineData("34-8", true, 26)]
    [InlineData("a-1", false, 0)]
    [InlineData(null, false, 0)]
    public void Parse_should_support_diffs(string input, bool canParse, decimal expected)
    {
        IOperation operation;
        var res = _sut.TryParse(input, out operation);
        res.Should().Be(canParse); if (canParse)
        {
            operation.Should().Be.OfType<DiffOperation>();
            operation.Eval().Should().Be.EqualTo(expected);
        }
        else
        {
            operation.Should().Be.Null();
        }
    }
}

public class UserInputParser : ICalculatorParser
{
    public bool TryParse(string userInput, out IOperation request)
    {
        if (IsDiff(userInput)) { request = ParseDiff(userInput); return true; }
        request = null; return false;
    }
    private static IOperation ParseDiff(string userInput) {
        var parts = userInput.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
        var a = int.Parse(parts.ElementAt(0));
        var b = int.Parse(parts.ElementAt(1));
        return new DiffOperation(a, b);
    }
    private static bool IsDiff(string userInput) {
        return userInput != null && Regex.IsMatch(userInput, @"^[0-9]+\-[0-9]+$");
    }
}

public class DiffOperation : IOperation
{
    private readonly int _a; private readonly int _b;
    public DiffOperation(int a, int b) { _a = a; _b = b; }
    public decimal Eval() { return _a - _b; }
}
