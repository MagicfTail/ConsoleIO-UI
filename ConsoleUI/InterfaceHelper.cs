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
