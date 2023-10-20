using System.Text;

namespace ConsoleUI;

public static class InterfaceHelpers
{
    public const char rightBorder = '│';
    public const char leftBorder = '│';
    public const char topLeft = '┌';
    public const char bottomLeft = '└';
    public const char topRight = '┐';
    public const char bottomRight = '┘';
    public const char leftT = '├';
    public const char rightT = '┤';
    public const char horizontalSeparator = '─';
    public const char RightArrow = '⮞';
    public const char LeftArrow = '⮜';
    public const string MainBufferString = "\x1b[?1049l";
    public const string AltBufferString = "\x1b[?1049h";

    public static string Encapsulate(string input)
    {
        return new StringBuilder().Append(leftBorder).Append(input).Append(rightBorder).ToString();
    }

    public static string EncapsulateAndPadRight(string input, int ConsoleWidth)
    {
        return Encapsulate(input.PadRight(ConsoleWidth));
    }

    public static string ClearLine(int ConsoleWidth)
    {
        return new StringBuilder().Append(leftBorder).Append(new string(' ', ConsoleWidth)).Append(rightBorder).ToString();
    }

    public static string SeparateLine(int ConsoleWidth, bool ScrolledRight, bool ScrolledLeft)
    {
        return new StringBuilder().Append(ScrolledRight ? LeftArrow : leftT).Append(new string(horizontalSeparator, ConsoleWidth)).Append(ScrolledLeft ? RightArrow : rightT).ToString();
    }

    public static string TopLine(int ConsoleWidth)
    {
        return new StringBuilder().Append(topLeft).Append(new string(horizontalSeparator, ConsoleWidth)).Append(topRight).ToString();
    }

    public static string BottomLine(int ConsoleWidth)
    {
        return new StringBuilder().Append(bottomLeft).Append(new string(horizontalSeparator, ConsoleWidth)).Append(bottomRight).ToString();
    }
}

public class MessagesBodyBuilder
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