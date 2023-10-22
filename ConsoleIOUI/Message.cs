namespace ConsoleIOUI;

public class Message
{
    public string Body { get; set; }
    public string Sender { get; set; }
    public Message(string body, string sender)
    {
        Body = body;
        Sender = sender;
    }
}
