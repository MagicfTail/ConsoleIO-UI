using System.Text;

namespace ConsoleUI;

public abstract class ConsoleUI
{
    private readonly List<string> messages = new();
    private readonly List<string?> senders = new();

    private int fullWidth = Console.BufferWidth;
    private int fullHeight = Console.BufferHeight;
    private int ConsoleWidth => fullWidth - 2;
    private int ConsoleHeight => fullHeight - 2;

    private bool ScrolledRight => windowOffset > 0;
    private bool ScrolledLeft => windowOffset + ConsoleWidth < inputBuffer.Length;

    private string inputBuffer = "";
    private int cursorPosition = 0;
    private int windowOffset = 0;
    private int messageOffset = 0;

    readonly object drawLock = new();
    readonly object messageLock = new();

    public bool exit = false;

    public ConsoleUI() { }

    public void Start()
    {
        Thread readerThread = new(ReadWorker);

        GotoAlternateBuffer();
        DrawInterface();

        readerThread.Start();

        readerThread.Join();
    }

    public abstract void UserInputHandler(string input);

    public abstract void ExitHandler();

    public void Stop()
    {
        GotoMainBuffer();
        exit = true;
        ExitHandler();
    }

    private void ReadWorker()
    {
        while (!exit)
        {
            try
            {
                KeyHandler(Console.ReadKey(true));
            }
            finally
            { }
        }
    }

    private void KeyHandler(ConsoleKeyInfo keyInfo)
    {
        RefreshConsoleSize();

        if (keyInfo.KeyChar >= 32 && keyInfo.KeyChar <= 126) // Maybe add unicode support (|| keyInfo.KeyChar > 127)
        {
            inputBuffer = inputBuffer.Insert(cursorPosition + windowOffset, keyInfo.KeyChar.ToString());
            if (cursorPosition < ConsoleWidth)
            {
                // Move thr cursor along if when typing normally
                cursorPosition++;
            }
            else
            {
                // Move the window along if typing at the end of the input
                windowOffset++;
            }

            DrawInterface();
            return;
        }

        switch (keyInfo.Key)
        {
            case ConsoleKey.Enter:
                UserInputHandler(inputBuffer);
                ResetInput();
                break;
            case ConsoleKey.LeftArrow:
                if (cursorPosition + windowOffset == 0)
                {
                    Console.Beep();
                }
                else if (cursorPosition == 0 && ScrolledRight)
                {
                    windowOffset--;
                }
                else
                {
                    cursorPosition--;
                }
                break;
            case ConsoleKey.RightArrow:
                if (cursorPosition + windowOffset == inputBuffer.Length)
                {
                    Console.Beep();
                }
                else if (cursorPosition == ConsoleWidth)
                {
                    windowOffset++;
                }
                else
                {
                    cursorPosition++;
                }
                break;
            case ConsoleKey.UpArrow:
                if (messageOffset == messages.Count - 1)
                {
                    Console.Beep();
                }
                else
                {
                    messageOffset++;
                }
                break;
            case ConsoleKey.DownArrow:
                if (messageOffset == 0)
                {
                    Console.Beep();
                }
                else
                {
                    messageOffset--;
                }
                break;
            case ConsoleKey.PageUp:
                if (messageOffset == messages.Count - 1)
                {
                    Console.Beep();
                }
                else
                {
                    messageOffset = Math.Min(messages.Count - 1, messageOffset + 10);
                }
                break;
            case ConsoleKey.PageDown:
                if (messageOffset == 0)
                {
                    Console.Beep();
                }
                else
                {
                    messageOffset = Math.Max(0, messageOffset - 10);
                }
                break;
            case ConsoleKey.Home:
                messageOffset = 0;
                break;
            case ConsoleKey.Backspace:
                if (cursorPosition + windowOffset == 0)
                {
                    Console.Beep();
                }
                else if (ScrolledRight && windowOffset + ConsoleWidth >= inputBuffer.Length)
                {
                    inputBuffer = inputBuffer.Remove(cursorPosition + windowOffset - 1, 1);
                    windowOffset--;
                }
                else
                {
                    inputBuffer = inputBuffer.Remove(cursorPosition + windowOffset - 1, 1);
                    cursorPosition--;
                }
                break;
            case ConsoleKey.Delete:
                if (cursorPosition + windowOffset == inputBuffer.Length)
                {
                    Console.Beep();
                }
                else if (ScrolledRight && windowOffset + ConsoleWidth >= inputBuffer.Length)
                {
                    inputBuffer = inputBuffer.Remove(cursorPosition + windowOffset, 1);
                    windowOffset--;
                }
                else
                {
                    inputBuffer = inputBuffer.Remove(cursorPosition + windowOffset, 1);
                }
                break;
            case ConsoleKey.Escape:
                Stop();
                return;
        }

        DrawInterface();
    }

    private void DrawInterface()
    {
        lock (drawLock)
        {
            Console.SetCursorPosition(0, 0);

            MessagesBodyBuilder bodyBuilder = new(ConsoleHeight, ConsoleWidth);

            lock (messageLock)
            {
                // Keep adding messages until we run out or the screen is filled
                for (int i = messages.Count - messageOffset; i > 0; i--)
                {
                    bodyBuilder.AppendMessage(messages[i - 1]);

                    if (bodyBuilder.IsFull())
                    {
                        break;
                    }
                }
            }

            StringBuilder interfaceBuffer = new();

            // Add top
            interfaceBuffer.Append(InterfaceHelpers.TopLine(ConsoleWidth));

            // If there aren't enough messages to fill the screen, add whitespace
            int missingLines = ConsoleHeight - bodyBuilder.lines - 2;

            if (missingLines > 0)
            {
                foreach (int i in Enumerable.Range(0, missingLines))
                {
                    interfaceBuffer.AppendLine(InterfaceHelpers.ClearLine(ConsoleWidth));
                }
            }

            // Add the messages
            interfaceBuffer.Append(bodyBuilder.Build());

            // Add separator
            interfaceBuffer.AppendLine(InterfaceHelpers.SeparateLine(ConsoleWidth, ScrolledRight, ScrolledLeft));

            // Add current user input
            if (inputBuffer.Length <= ConsoleWidth)
            {
                interfaceBuffer.Append(InterfaceHelpers.EncapsulateAndPadRight(inputBuffer, ConsoleWidth));
            }
            else
            {
                interfaceBuffer.Append(InterfaceHelpers.Encapsulate(inputBuffer.Substring(windowOffset, ConsoleWidth)));
            }

            // Add bottom line
            interfaceBuffer.Append(InterfaceHelpers.BottomLine(ConsoleWidth));

            Console.Write(interfaceBuffer);
            Console.SetCursorPosition(cursorPosition + 1, ConsoleHeight);
        }
    }

    public void AddMessage(string message, string? sender)
    {
        lock (messageLock)
        {
            messages.Add(message);
            senders.Add(sender);

            if (messageOffset != 0)
            {
                messageOffset++;
            }
        }

        DrawInterface();
    }

    private void RefreshConsoleSize()
    {
        fullWidth = Console.WindowWidth;
        fullHeight = Console.WindowHeight;

        if (ConsoleWidth > inputBuffer.Length)
        {
            windowOffset = 0;
        }

        if (cursorPosition > ConsoleWidth)
        {
            windowOffset += cursorPosition - ConsoleWidth;
            cursorPosition = ConsoleWidth;
        }
    }

    private void ResetInput()
    {
        inputBuffer = "";
        cursorPosition = 0;
        windowOffset = 0;
    }

    private static void GotoMainBuffer()
    {
        Console.Clear();
        Console.Write(InterfaceHelpers.MainBufferString);
    }

    private static void GotoAlternateBuffer()
    {
        Console.Write(InterfaceHelpers.AltBufferString);
        Console.Clear();
    }

    private class MessagesBodyBuilder
    {
        private readonly StringBuilder builder = new();
        private readonly Stack<string> buffer = new();

        public int lines = 0;

        private readonly int usableHeight;
        private readonly int usableWidth;

        public MessagesBodyBuilder(int consoleHeight, int consoleWidth)
        {
            usableHeight = consoleHeight - 2;
            usableWidth = consoleWidth;
        }

        public void AppendMessage(string message)
        {
            lines += LinesUsed(message);
            buffer.Push(message);
        }

        public bool IsFull()
        {
            return lines >= usableHeight;
        }

        public StringBuilder Build()
        {
            int excessLines = lines - usableHeight;

            // The first message drawn might have to be cut off
            if (excessLines > 0)
            {
                string message = buffer.Pop();

                // Draw the first n-1 lines of the cut off message
                for (int i = excessLines; i < LinesUsed(message) - 1; i++)
                {
                    builder.AppendLine(InterfaceHelpers.Encapsulate(
                        message[(usableWidth * i)..(usableWidth * (i + 1))]));
                }

                // Draw the last part
                int overhangLength = message.Length % usableWidth == 0 ? usableWidth : message.Length % usableWidth;

                builder.AppendLine(InterfaceHelpers.EncapsulateAndPadRight(
                    message[^overhangLength..], usableWidth));
            }

            while (buffer.Count > 0)
            {
                string message = buffer.Pop();

                // Draw the remaining messages normally, over multiple lines
                int loop = 0;
                while (true)
                {
                    if ((loop + 1) * usableWidth >= message.Length)
                    {
                        builder.AppendLine(InterfaceHelpers.EncapsulateAndPadRight(
                            message[(loop * usableWidth)..], usableWidth));
                        break;
                    }

                    builder.AppendLine(InterfaceHelpers.Encapsulate(
                        message[(loop * usableWidth)..((loop + 1) * usableWidth)]));
                    loop++;
                }
            }

            return builder;
        }

        private int LinesUsed(string message)
        {
            return Math.Max(1, (int)Math.Ceiling((double)message.Length / usableWidth));
        }
    }
}
