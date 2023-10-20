using System.Text;

namespace ConsoleUI;

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