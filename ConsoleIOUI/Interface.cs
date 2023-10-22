using System.Text;

namespace ConsoleIOUI;

public abstract class ConsoleInterface
{
    private readonly List<Message> messages = new();

    readonly object drawLock = new();
    readonly object messageLock = new();
    readonly object consoleSizeLock = new();

    private readonly InterfaceHelpers _helper;
    private readonly int _senderWidth;

    private int fullWidth = Console.BufferWidth;
    private int fullHeight = Console.BufferHeight;
    private int ConsoleWidth => fullWidth - 2;
    private int ConsoleHeight => fullHeight - 2;
    private int UsableWidth => ConsoleWidth - SenderAreaWidth;
    private int UsableHeight => ConsoleHeight - 2;
    private int SenderAreaWidth => _senderWidth + 1;

    private string inputBuffer = "";
    private int cursorPosition = 0;
    private int windowOffset = 0;
    private int messageOffset = 0;

    private bool exit = false;

    private bool ScrolledRight => windowOffset > 0;
    private bool ScrolledLeft => windowOffset + ConsoleWidth < inputBuffer.Length;
    private bool ScrolledUp => messageOffset > 0;

    public ConsoleInterface(int senderWidth = 0)
    {
        _senderWidth = senderWidth > 0 ? senderWidth : 0;
        _helper = new(SenderAreaWidth);
    }

    public abstract void UserInputHandler(string input);

    public abstract void ExitHandler();

    public void Start()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Thread readerThread = new(ReadWorker);

        GotoAlternateBuffer();
        DrawInterface();

        readerThread.Start();

        readerThread.Join();
    }

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
        if (keyInfo.KeyChar >= 32 && keyInfo.KeyChar <= 126) // Maybe add unicode support (|| keyInfo.KeyChar > 127)
        {
            inputBuffer = inputBuffer.Insert(cursorPosition + windowOffset, keyInfo.KeyChar.ToString());
            lock (consoleSizeLock)
            {
                RefreshConsoleSize();
                if (cursorPosition < ConsoleWidth)
                {
                    // Move thr cursor along when typing normally
                    cursorPosition++;
                }
                else
                {
                    // Move the window along if typing at the end of the input
                    windowOffset++;
                }
            }

            DrawInterface();
            return;
        }

        lock (consoleSizeLock)
        {
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
        }

        DrawInterface();
    }

    private void DrawInterface()
    {
        lock (consoleSizeLock)
        {
            RefreshConsoleSize();
            lock (drawLock)
            {
                MessagesBodyBuilder bodyBuilder = new(UsableWidth, UsableHeight, _helper);

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
                interfaceBuffer.Append(_helper.TopLine(ConsoleWidth));

                // If there aren't enough messages to fill the screen, add whitespace
                int missingLines = UsableHeight - bodyBuilder.lines;

                if (missingLines > 0)
                {
                    foreach (int i in Enumerable.Range(0, missingLines))
                    {
                        interfaceBuffer.AppendLine(_helper.ClearLine(ConsoleWidth));
                    }
                }

                // Add the messages
                interfaceBuffer.Append(bodyBuilder.Build());

                // Add separator
                interfaceBuffer.AppendLine(_helper.SeparateLine(ConsoleWidth, ScrolledRight, ScrolledLeft, ScrolledUp));

                // Add current user input
                if (inputBuffer.Length <= ConsoleWidth)
                {
                    interfaceBuffer.Append(InterfaceHelpers.EncapsulatePadNoSender(inputBuffer, ConsoleWidth));
                }
                else
                {
                    interfaceBuffer.Append(InterfaceHelpers.EncapsulatePadNoSender(inputBuffer.Substring(windowOffset, ConsoleWidth), ConsoleWidth));
                }

                // Add bottom line
                interfaceBuffer.Append(InterfaceHelpers.BottomLine(ConsoleWidth));

                Console.SetCursorPosition(0, 0);
                Console.Write(interfaceBuffer);
                Console.SetCursorPosition(cursorPosition + 1, ConsoleHeight);
            }
        }
    }

    public void AddMessage(string body, string? sender)
    {
        lock (messageLock)
        {
            messages.Add(new Message(body, String.IsNullOrEmpty(sender) ? "" : sender));

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
        else if (ConsoleWidth + windowOffset > inputBuffer.Length)
        {
            windowOffset += inputBuffer.Length - (ConsoleWidth + windowOffset);
            cursorPosition = ConsoleWidth;
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
        Console.Write(InterfaceHelpers.DefaultCursorString);
        Console.Write(InterfaceHelpers.MainBufferString);
    }

    private static void GotoAlternateBuffer()
    {
        Console.Write(InterfaceHelpers.AltBufferString);
        Console.Clear();
        Console.Write(InterfaceHelpers.SteadyBarString);
    }
}
