using ConsoleUI;

class Example
{
    static void Main()
    {
        Program program = new(15);
        program.thread.Start();

        program.Start();

        program.thread.Join();
    }

    private class Program : ConsoleInterface
    {
        bool exitSignal = false;

        public Thread thread;

        public Program(int senderWidth = 0) : base(senderWidth)
        {
            thread = new(() =>
            {
                int i = 1;
                while (!exitSignal)
                {
                    AddMessage($"Message {i}", "Important Process");
                    i++;
                    Thread.Sleep(1000);
                }
            });
        }

        public override void UserInputHandler(string input)
        {
            AddMessage(input, "Me :)");
        }

        public override void ExitHandler()
        {
            exitSignal = true;
        }
    }
}
