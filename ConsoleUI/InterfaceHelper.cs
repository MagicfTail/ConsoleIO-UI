using System.Text;

namespace ConsoleUI;

public static class InterfaceHelpers
{
    private const char rightBorder = '│';
    private const char leftBorder = '│';
    private const char topLeft = '┌';
    private const char bottomLeft = '└';
    private const char topRight = '┐';
    private const char bottomRight = '┘';
    private const char leftT = '┝';
    private const char rightT = '┥';
    private const char horizontalBorder = '─';
    private const char horizontalSeparator = '━';
    private const char wavySeparator = '╍';
    private const char RightArrow = '⮞';
    private const char LeftArrow = '⮜';
    public const string MainBufferString = "\x1b[?1049l";
    public const string AltBufferString = "\x1b[?1049h";

    public static string Encapsulate(string input)
    {
        return new StringBuilder().Append(leftBorder).Append(input).Append(rightBorder).ToString();
    }

    public static string EncapsulateAndPadRight(string input, int consoleWidth)
    {
        return Encapsulate(input.PadRight(consoleWidth));
    }

    public static string ClearLine(int consoleWidth)
    {
        return new StringBuilder().Append(leftBorder).Append(new string(' ', consoleWidth)).Append(rightBorder).ToString();
    }

    public static string SeparateLine(int consoleWidth, bool scrolledRight, bool scrolledLeft, bool scrolledUp)
    {
        return new StringBuilder().Append(scrolledRight ? LeftArrow : leftT).Append(
            new string(scrolledUp ? wavySeparator : horizontalSeparator, consoleWidth)).Append(scrolledLeft ? RightArrow : rightT).ToString();
    }

    public static string TopLine(int consoleWidth)
    {
        return new StringBuilder().Append(topLeft).Append(new string(horizontalBorder, consoleWidth)).Append(topRight).ToString();
    }

    public static string BottomLine(int consoleWidth)
    {
        return new StringBuilder().Append(bottomLeft).Append(new string(horizontalBorder, consoleWidth)).Append(bottomRight).ToString();
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