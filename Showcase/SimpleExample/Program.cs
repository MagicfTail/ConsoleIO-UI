using ConsoleIOUI;

class Example
{
    static void Main()
    {
        Program program = new(15);

        program.Start();
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
                    if (i == 5)
                    {
                        Stop();
                        return;
                    }
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

        public override void EntranceHandler()
        {
            thread.Start();
        }

        public override void ExitHandler()
        {
            exitSignal = true;
            Console.WriteLine("Program exited");
        }
    }
}
