class Example
{
    static void Main()
    {
        Program program = new();

        program.Start();
    }

    private class Program : ConsoleUI.ConsoleUI
    {
        bool exitSignal = false;

        public Program() : base()
        {
            Thread thread = new(() =>
            {
                int i = 1;
                while (!exitSignal)
                {
                    AddMessage($"Message {i}", null);
                    i++;
                    Thread.Sleep(1000);
                }
            });

            thread.Start();
        }

        public override void UserInputHandler(string input)
        {
            AddMessage(input, null);
        }

        public override void ExitHandler()
        {
            exitSignal = true;
        }
    }
}
