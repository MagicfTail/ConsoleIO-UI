using System;

namespace Tokenizer;

public class Tokenizer
{
    private string input = "";
    private string sanitizedInput = "";
    private int progressCounter = 0;
    private int depth = 0;
    private string ErrorMessage(char c) => $"Character '{c}' at position {progressCounter} in \"{sanitizedInput}\"";

    public string Input
    {
        get
        {
            return input;
        }
        set
        {
            input = value;
            sanitizedInput = value.Replace(" ", "");
        }
    }


    public Tokenizer(string input)
    {
        Input = input;
    }

    public Tokenizer()
    { }

    public Tree Tokenize()
    {
        Reset();
        return new Tree(TokenizeRec(TokenizeValue()));
    }

    private IToken TokenizeRec(IToken? left)
    {
        foreach (char c in sanitizedInput[progressCounter..])
        {
            if ((char.IsNumber(c) || c == '(') && left == null)
            {
                return TokenizeRec(TokenizeValue());
            }
            else if (char.IsNumber(c) || c == '(' && left != null)
            {
                throw new InvalidDataException("Input wasn't complete");
            }
            else if (left == null)
            {
                throw new InvalidDataException("Input wasn't complete");
            }

            IToken right;
            switch (c)
            {
                case '+':
                    ConsumeChar();
                    return new Addition(left, TokenizeRec(null));
                case '-':
                    ConsumeChar();
                    return new Subtraction(left, TokenizeRec(null));
                case '*':
                    ConsumeChar();
                    right = TokenizeValue();
                    return TokenizeRec(new Multiplication(left, right));
                case '/':
                    ConsumeChar();
                    right = TokenizeValue();
                    // We can check for division by 0 here, but we let the person using the tokenizer handle that when evaluating
                    // int rightVal = right.Eval();

                    // if (rightVal == 0)
                    // {
                    //     throw new DivideByZeroException();
                    // }
                    return TokenizeRec(new Division(left, right));
                case '(':
                    return TokenizeRec(TokenizeParent());
                case ')':
                    ConsumeChar();
                    depth--;

                    if (depth < 0)
                    {
                        throw new InvalidDataException(ErrorMessage(c));
                    }

                    return left;
                default:
                    throw new InvalidDataException(ErrorMessage(c));
            }
        }

        if (left == null || depth != 0)
        {
            throw new InvalidDataException("Input wasn't complete");
        }

        return left;
    }

    private IToken TokenizeValue()
    {
        if (progressCounter >= sanitizedInput.Length)
        {
            throw new InvalidDataException();
        }

        char c = sanitizedInput[progressCounter];

        if (char.IsNumber(c))
        {
            return TokenizeNumber();
        }
        else if (c == '(')
        {
            return TokenizeParent();
        }
        else
        {
            ConsumeChar();
            throw new InvalidDataException(ErrorMessage(c));
        }
    }

    private IToken TokenizeNumber()
    {
        string buffer = "";
        while (progressCounter < sanitizedInput.Length && char.IsNumber(sanitizedInput[progressCounter]))
        {
            buffer += sanitizedInput[progressCounter];
            ConsumeChar();
        }

        return new Number(int.Parse(buffer));
    }

    private IToken TokenizeParent()
    {
        ConsumeChar();
        depth++;
        return new ParentToken(TokenizeRec(TokenizeValue()));
    }

    private void ConsumeChar()
    {
        progressCounter++;
    }

    private void Reset()
    {
        progressCounter = 0;
        depth = 0;
    }
}