using ConsoleUI;

class Program
{
    static void Main()
    {
        Calculator calculator = new();

        calculator.Start();
    }

    private class Calculator : ConsoleInterface
    {
        private readonly Tokenizer.Tokenizer tokenizer;
        public Calculator() : base()
        {
            tokenizer = new();
        }

        public override void UserInputHandler(string input)
        {
            try
            {
                tokenizer.Input = input;
                int value = tokenizer.Tokenize().Eval();
                AddMessage($"{input} = {value}", null);
            }
            catch (InvalidDataException)
            {
                AddMessage($"{input} = Invalid Input", null);
            }
            catch (DivideByZeroException)
            {
                AddMessage($"{input} = Division By Zero", null);
            }
        }

        public override void ExitHandler() { }
    }
}

