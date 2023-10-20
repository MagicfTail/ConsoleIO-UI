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
