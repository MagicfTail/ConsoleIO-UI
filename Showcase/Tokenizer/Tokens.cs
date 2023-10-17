namespace Tokenizer;

public enum Tokens
{
    Number,
    Plus,
    Minus,
    Times,
    Divide
}

public interface IToken
{
    public int Eval();
    public string ToJSON();
}

public abstract class LeftRightToken : IToken
{
    protected IToken _left;
    protected IToken _right;
    protected char _symbol;

    protected LeftRightToken(IToken left, IToken right, char symbol)
    {
        _left = left;
        _right = right;
        _symbol = symbol;
    }

    public abstract int Eval();
    public string ToJSON()
    {
        return $"{{\"Operation\": \"{_symbol}\", \"Left\": {_left.ToJSON()}, \"Right\": {_right.ToJSON()}}}";
    }
}

public class ParentToken : IToken
{
    private readonly IToken _entry;

    public ParentToken(IToken entry)
    {
        _entry = entry;
    }

    public int Eval()
    {
        return _entry.Eval();
    }

    public string ToJSON()
    {
        return $"{{\"Parent\": {_entry.ToJSON()}}}";
    }
}

public class Tree : IToken
{
    private readonly IToken _entry;

    public Tree(IToken entry)
    {
        _entry = entry;
    }

    public int Eval()
    {
        return _entry.Eval();
    }

    public string ToJSON()
    {
        return $"{_entry.ToJSON()}";
    }
}

public class Number : IToken
{
    private readonly int _value;

    public Number(int value)
    {
        _value = value;
    }

    public int Eval()
    {
        return _value;
    }

    public string ToJSON()
    {
        return $"{_value}";
    }
}

public class Addition : LeftRightToken
{
    public Addition(IToken left, IToken right) : base(left, right, '+') { }

    public override int Eval()
    {
        return _left.Eval() + _right.Eval();
    }
}

public class Subtraction : LeftRightToken
{
    public Subtraction(IToken left, IToken right) : base(left, right, '-') { }

    public override int Eval()
    {
        return _left.Eval() - _right.Eval();
    }
}

public class Multiplication : LeftRightToken
{
    public Multiplication(IToken left, IToken right) : base(left, right, '*') { }

    public override int Eval()
    {
        return _left.Eval() * _right.Eval();
    }
}

public class Division : LeftRightToken
{
    public Division(IToken left, IToken right) : base(left, right, '/') { }

    public override int Eval()
    {
        int right = _right.Eval();
        if (right != 0)
        {
            return _left.Eval() / _right.Eval();
        }
        else
        {
            throw new DivideByZeroException();
        }
    }
}
