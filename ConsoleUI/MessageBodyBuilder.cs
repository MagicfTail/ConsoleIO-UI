using System.Text;

namespace ConsoleUI;

public class MessagesBodyBuilder
{
    private readonly StringBuilder builder = new();
    private readonly Stack<Message> buffer = new();

    private readonly InterfaceHelpers _helper;

    public int lines = 0;

    private readonly int _usableWidth;
    private readonly int _usableHeight;

    public MessagesBodyBuilder(int usableWidth, int usableHeight, InterfaceHelpers helper)
    {
        _usableHeight = usableHeight;
        _usableWidth = usableWidth;
        _helper = helper;
    }

    public void AppendMessage(Message message)
    {
        lines += LinesUsed(message.Body);
        buffer.Push(message);
    }

    public bool IsFull()
    {
        return lines >= _usableHeight;
    }

    public StringBuilder Build()
    {
        int excessLines = lines - _usableHeight;

        // The first message drawn might have to be cut off
        if (excessLines > 0)
        {
            Message message = buffer.Pop();

            // Draw the first n-1 lines of the cut off message
            for (int i = excessLines; i < LinesUsed(message.Body) - 1; i++)
            {
                builder.AppendLine(_helper.Encapsulate(
                    message.Body[(_usableWidth * i)..(_usableWidth * (i + 1))], ""));
            }

            // Draw the last part
            int overhangLength = message.Body.Length % _usableWidth == 0 ? _usableWidth : message.Body.Length % _usableWidth;

            builder.AppendLine(_helper.EncapsulateAndPadRight(
                message.Body[^overhangLength..], "", _usableWidth));
        }

        while (buffer.Count > 0)
        {
            Message message = buffer.Pop();

            // Draw the remaining messages normally, over multiple lines
            int loop = 0;
            while (true)
            {
                if ((loop + 1) * _usableWidth >= message.Body.Length)
                {
                    builder.AppendLine(_helper.EncapsulateAndPadRight(
                        message.Body[(loop * _usableWidth)..], loop == 0 ? message.Sender : "", _usableWidth));
                    break;
                }

                builder.AppendLine(_helper.Encapsulate(
                    message.Body[(loop * _usableWidth)..((loop + 1) * _usableWidth)], loop == 0 ? message.Sender : ""));
                loop++;
            }
        }

        return builder;
    }

    private int LinesUsed(string message)
    {
        return Math.Max(1, (int)Math.Ceiling((double)message.Length / _usableWidth));
    }
}