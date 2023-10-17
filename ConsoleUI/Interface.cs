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
        }
        else if (keyInfo.Key == ConsoleKey.Enter)
        {
            UserInputHandler(inputBuffer);
            ResetInput();
        }
        else if (keyInfo.Key == ConsoleKey.LeftArrow)
        {
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
        }
        else if (keyInfo.Key == ConsoleKey.RightArrow)
        {
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
        }
        else if (keyInfo.Key == ConsoleKey.Backspace)
        {
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
        }
        else if (keyInfo.Key == ConsoleKey.Delete)
        {
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
        }
        else if (keyInfo.Key == ConsoleKey.Escape)
        {
            GotoMainBuffer();
            exit = true;

            return;
        }

        DrawInterface();
    }

    private void DrawInterface()
    {
        lock (drawLock)
        {
            Console.SetCursorPosition(0, 0);

            StringBuilder interfaceBuffer = new();

            interfaceBuffer.Append(InterfaceHelpers.TopLine(ConsoleWidth));

            // #TODO: Fix messages that don't fit on 1 line
            lock (messageLock)
            {
                string overflow = "";
                int overflowOffset = 0;

                for (var i = ConsoleHeight - 2; i > 0; i--)
                {
                    if (messages.Count - i + overflowOffset < 0)
                    {
                        interfaceBuffer.AppendLine(InterfaceHelpers.ClearLine(ConsoleWidth));
                        continue;
                    }

                    string message = messages[^(i + overflowOffset)];

                    if (overflow == "")
                    {
                        overflow = message;
                    }

                    if (overflow.Length <= ConsoleWidth)
                    {
                        interfaceBuffer.Append(InterfaceHelpers.EncapsulateAndPadRight(overflow, ConsoleWidth));
                        overflow = "";
                    }
                    else
                    {
                        int overhangLength = overflow.Length % ConsoleWidth == 0 ? ConsoleWidth : overflow.Length % ConsoleWidth;
                        interfaceBuffer.Append(InterfaceHelpers.EncapsulateAndPadRight(overflow[^overhangLength..], ConsoleWidth));
                        overflow = overflow[..^overhangLength];
                        overflowOffset++;
                    }
                }
            }

            interfaceBuffer.AppendLine(InterfaceHelpers.SeparateLine(ConsoleWidth, ScrolledRight, ScrolledLeft));

            if (inputBuffer.Length <= ConsoleWidth)
            {
                interfaceBuffer.Append(InterfaceHelpers.EncapsulateAndPadRight(inputBuffer, ConsoleWidth));
            }
            else
            {
                interfaceBuffer.Append(InterfaceHelpers.Encapsulate(inputBuffer.Substring(windowOffset, ConsoleWidth)));
            }

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
}
