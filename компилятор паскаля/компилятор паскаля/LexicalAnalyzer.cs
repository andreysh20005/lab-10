class LexicalAnalyzer
{
    public const byte
        Star = 21, // *
        Slash = 60, // /
        Equal = 16, // =
        Comma = 20, // ,
        Semicolon = 14, // ;
        Colon = 5, // :
        Point = 61,    // .
        Arrow = 62,    // ^
        LeftPar = 9,    // (
        RightPar = 4,    // )
        LBracket = 11,    // [
        RBracket = 12,    // ]
        FlPar = 63,    // {
        FrPar = 64,    // }
        Later = 65,    // <
        Greater = 66,    // >
        LaterEqual = 67,    //  <=
        GreaterEqual = 68,    //  >=
        LaterGreater = 69,    //  <>
        Plus = 70,    // +
        Minus = 71,    // –
        LComment = 72,    //  (*
        RComment = 73,    //  *)
        Assign = 51,    //  :=
        TwoPoints = 74,    //  ..
        Ident = 2,    // идентификатор
        FloatC = 82,    // вещественная константа
        IntC = 15,    // целая константа
        StringC = 83, // строка
        CaseSy = 31, ElseSy = 32, FileSy = 57, GotoSy = 33, ThenSy = 52,
        TypeSy = 34, UntilSy = 53, DoSy = 54, WithSy = 37, IfSy = 56,
        InSy = 100, OfSy = 101, OrSy = 102, ToSy = 103, EndSy = 104,
        VarSy = 105, DivSy = 106, AndSy = 107, NotSy = 108, ForSy = 109,
        ModSy = 110, NilSy = 111, SetSy = 112, BeginSy = 113, WhileSy = 114,
        ArraySy = 115, ConstSy = 116, LabelSy = 117, DowntoSy = 118,
        PackedSy = 119, RecordSy = 120, RepeatSy = 121, ProgramSy = 122,
        FunctionSy = 123, ProcedurenSy = 124;

    private Keywords _keywords;
    private byte _symbol;
    private TextPosition _token;
    private string _addrName;
    private int _nmbInt;
    private float _nmbFloat;
    private char _oneSymbol;
    private byte _pendingToken;
    private string _stringConst;
    public LexicalAnalyzer()
    {
        _keywords = new Keywords();
        _token = new TextPosition(0, 0);
        _pendingToken = 0;
        _symbol = 0;
        _nmbInt = 0;
        _nmbFloat = 0;
        _addrName = "";
        _stringConst = "";
    }


    public byte CurrentSymbol => _symbol;
    public string IdentName => _addrName;
    public int IntValue => _nmbInt;
    public float FloatValue => _nmbFloat;
    public TextPosition TokenPosition => _token;
    public string StringConst => _stringConst;

    public byte NextSym()
    {
        if (_pendingToken != 0)
        {
            byte savedSymbol = _pendingToken;
            _pendingToken = 0;
            return savedSymbol;
        }

        while (InputOutput.Ch == ' ' || InputOutput.Ch == '\n' || InputOutput.Ch == '\r' || InputOutput.Ch == '\t')
        {
            InputOutput.NextCh();
        }

        _token.lineNumber = InputOutput.PositionNow.lineNumber;
        _token.charNumber = InputOutput.PositionNow.charNumber;

        if (InputOutput.Ch == '\0')
        {
            return 0;
        }

        if ((InputOutput.Ch >= 'a' && InputOutput.Ch <= 'z') || (InputOutput.Ch >= 'A' && InputOutput.Ch <= 'Z') || InputOutput.Ch == '_')
        {
            string name = "";
            while ((InputOutput.Ch >= 'a' && InputOutput.Ch <= 'z') ||
            (InputOutput.Ch >= 'A' && InputOutput.Ch <= 'Z') ||
            (InputOutput.Ch >= '0' && InputOutput.Ch <= '9') ||
            InputOutput.Ch == '_')
            {
                name += InputOutput.Ch;
                InputOutput.NextCh();
            }
            name = name.ToLower();
            byte len = (byte)name.Length;

            if (_keywords.Kw.ContainsKey(len) && _keywords.Kw[len].ContainsKey(name))
            {
                _symbol = _keywords.Kw[len][name];
            }
            else
            {
                _symbol = Ident;
                _addrName = name;
            }
            return _symbol;
        }

        if (InputOutput.Ch >= '0' && InputOutput.Ch <= '9')
        {
            byte digit;
            short maxInt = short.MaxValue;
            _nmbInt = 0;
            bool isOverflow = false;

            while (InputOutput.Ch >= '0' && InputOutput.Ch <= '9')
            {
                digit = (byte)(InputOutput.Ch - '0');
                if (!isOverflow)
                {
                    if (_nmbInt < maxInt / 10 || (_nmbInt == maxInt / 10 && digit <= maxInt % 10))
                    {
                        _nmbInt = 10 * _nmbInt + digit;
                    }
                    else
                    {
                        InputOutput.Error(203, _token);
                        isOverflow = true;
                        _nmbInt = 0;
                        while (InputOutput.Ch >= '0' && InputOutput.Ch <= '9') InputOutput.NextCh();
                        break;
                    }
                }
                InputOutput.NextCh();
            }

            if (InputOutput.Ch == '.')
            {
                InputOutput.NextCh();
                if (InputOutput.Ch == '.')
                {
                    _pendingToken = TwoPoints;
                    InputOutput.NextCh();
                    _symbol = IntC;
                    return _symbol;
                }
                else if (InputOutput.Ch >= '0' && InputOutput.Ch <= '9')
                {
                    float fractionalPart = 0;
                    float divisor = 10;
                    while (InputOutput.Ch >= '0' && InputOutput.Ch <= '9')
                    {
                        fractionalPart += (float)(InputOutput.Ch - '0') / divisor;
                        divisor *= 10;
                        InputOutput.NextCh();
                    }
                    _nmbFloat = (float)_nmbInt + fractionalPart;
                    _symbol = FloatC;
                    return _symbol;
                }
                else
                {
                    _pendingToken = Point;
                    _symbol = IntC;
                    return _symbol;
                }
            }
            _symbol = IntC;
            return _symbol;
        }

        switch (InputOutput.Ch)
        {
            case '<':
                InputOutput.NextCh();
                if (InputOutput.Ch == '=')
                {
                    _symbol = LaterEqual; InputOutput.NextCh();
                }
                else
                    if (InputOutput.Ch == '>')
                    {
                        _symbol = LaterGreater; InputOutput.NextCh();
                    }
                    else
                        _symbol = Later;
                break;
            case '>':
                InputOutput.NextCh();
                if (InputOutput.Ch == '=')
                {
                    _symbol = GreaterEqual; InputOutput.NextCh();
                }
                else
                    _symbol = Greater;
                break;
            case ':':
                InputOutput.NextCh();
                if (InputOutput.Ch == '=')
                {
                    _symbol = Assign; InputOutput.NextCh();
                }
                else
                    _symbol = Colon;
                break;
            case ';':
                _symbol = Semicolon;
                InputOutput.NextCh();
                break;
            case '.':
                InputOutput.NextCh();
                if (InputOutput.Ch == '.')
                {
                    _symbol = TwoPoints; InputOutput.NextCh();
                }
                else _symbol = Point;
                break;
            case ',':
                _symbol = Comma;
                InputOutput.NextCh();
                break;
            case '*':
                TextPosition starPosition = new TextPosition(InputOutput.PositionNow.lineNumber, InputOutput.PositionNow.charNumber);
                InputOutput.NextCh();

                if (InputOutput.Ch == ')')
                {

                    InputOutput.NextCh();
                    InputOutput.Error(204, starPosition);
                    return 1;
                }
                else
                {
                    _symbol = Star;
                    return _symbol;
                }
            case '/':
                _symbol = Slash;
                InputOutput.NextCh();
                break;
            case '=':
                _symbol = Equal;
                InputOutput.NextCh();
                break;
            case '^':
                _symbol = Arrow;
                InputOutput.NextCh();
                break;
            case '(':
                TextPosition commentStart = new TextPosition(InputOutput.PositionNow.lineNumber, InputOutput.PositionNow.charNumber);
                InputOutput.NextCh();

                if (InputOutput.Ch == '*')
                {
                    InputOutput.NextCh();

                    while (true)
                    {
                        if (InputOutput.Ch == '\0')
                        {
                            InputOutput.Error(205, commentStart);
                            return 1;
                        }

                        if (InputOutput.Ch == '*')
                        {
                            InputOutput.NextCh();
                            if (InputOutput.Ch == ')')
                            {
                                InputOutput.NextCh();
                                break;
                            }
                        }
                        else
                        {
                            InputOutput.NextCh();
                        }
                    }
                    return NextSym();
                }
                else
                {
                    _symbol = LeftPar;
                    return _symbol;
                }
            case ')':
                _symbol = RightPar;
                InputOutput.NextCh();
                break;
            case '[':
                _symbol = LBracket;
                InputOutput.NextCh();
                break;
            case ']':
                _symbol = RBracket;
                InputOutput.NextCh();
                break;
            case '{':
                _symbol = FlPar;
                InputOutput.NextCh();
                break;
            case '}':
                _symbol = FrPar;
                InputOutput.NextCh();
                break;
            case '\'':
            case '"':
                char quoteType = InputOutput.Ch;
                InputOutput.NextCh();
                int counter = 0;
                while (InputOutput.Ch != '\n' && InputOutput.Ch != quoteType)
                {
                    InputOutput.NextCh();
                    counter++;
                }

                if (InputOutput.Ch == quoteType)
                {
                    InputOutput.NextCh();
                    _symbol = StringC;
                    return _symbol;
                }
                else
                {
                    InputOutput.Error(206, _token);
                    _symbol = StringC;

                    return 1;
                }
            default:
                InputOutput.Error(1, _token);
                InputOutput.NextCh();
                return 1;
        }

        return _symbol;
    }
}