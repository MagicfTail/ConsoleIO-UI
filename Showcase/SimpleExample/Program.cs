class Example
{
    static void Main()
    {
        Program program = new();

        program.Start();
    }

    private class Program : ConsoleUI.ConsoleUI
    {
        public Program() : base() { }

        public override void UserInputHandler(string input)
        {
            AddMessage(input, null);
        }
    }
}

