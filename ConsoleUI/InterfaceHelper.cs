using System.Text;

namespace ConsoleUI;

public class InterfaceHelpers
{
    private const char verticalBorder = '│';
    private const char topLeft = '┌';
    private const char bottomLeft = '└';
    private const char topRight = '┐';
    private const char bottomRight = '┘';
    private const char topDown = '┬';
    private const char bottomUp = '┷';
    private const char leftT = '┝';
    private const char rightT = '┥';
    private const char horizontalBorder = '─';
    private const char horizontalSeparator = '━';
    private const char wavySeparator = '╍';
    private const char RightArrow = '⮞';
    private const char LeftArrow = '⮜';
    public const string MainBufferString = "\x1b[?1049l";
    public const string AltBufferString = "\x1b[?1049h";

    private readonly int _senderWidth;

    public InterfaceHelpers(int senderWidth)
    {
        _senderWidth = senderWidth;
    }

    public static string EncapsulatePadNoSender(string input, int consoleWidth)
    {
        return new StringBuilder().Append(verticalBorder).Append(input.PadRight(consoleWidth)).Append(verticalBorder).ToString();
    }

    public string Encapsulate(string input, string sender)
    {
        StringBuilder builder = new();
        builder.Append(verticalBorder);

        if (_senderWidth > 1)
        {
            builder.Append(Truncate(sender, _senderWidth - 1).PadRight(_senderWidth - 1));
            builder.Append(verticalBorder);
        }

        return builder.Append(input).Append(verticalBorder).ToString();
    }

    public string EncapsulateAndPadRight(string input, string sender, int usableConsoleWidth)
    {
        return Encapsulate(input.PadRight(usableConsoleWidth), sender);
    }

    public string ClearLine(int consoleWidth)
    {
        StringBuilder output = new StringBuilder().Append(verticalBorder).Append(new string(' ', consoleWidth)).Append(verticalBorder);

        if (_senderWidth > 1)
        {
            output[_senderWidth] = verticalBorder;
        }

        return output.ToString();
    }

    public string SeparateLine(int consoleWidth, bool scrolledRight, bool scrolledLeft, bool scrolledUp)
    {
        StringBuilder output = new StringBuilder().Append(scrolledRight ? LeftArrow : leftT).Append(
            new string(scrolledUp ? wavySeparator : horizontalSeparator, consoleWidth)).Append(scrolledLeft ? RightArrow : rightT);

        if (_senderWidth > 1)
        {
            output[_senderWidth] = bottomUp;
        }

        return output.ToString();
    }

    public string TopLine(int consoleWidth)
    {
        StringBuilder output = new StringBuilder().Append(topLeft).Append(new string(horizontalBorder, consoleWidth)).Append(topRight);

        if (_senderWidth > 1)
        {
            output[_senderWidth] = topDown;
        }

        return output.ToString();
    }

    public static string BottomLine(int consoleWidth)
    {
        return new StringBuilder().Append(bottomLeft).Append(new string(horizontalBorder, consoleWidth)).Append(bottomRight).ToString();
    }

    public static string Truncate(string value, int maxLength, string truncationSuffix = "…")
    {
        if (maxLength == 1 && value.Length > 0)
        {
            return value[0].ToString();
        }
        return value.Length > maxLength
            ? string.Concat(value.AsSpan(0, maxLength - 1), truncationSuffix)
            : value;
    }
}
