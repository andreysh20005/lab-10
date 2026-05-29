using System;

class LexicalAnalyzer
{
    public const byte
        star = 21, // *
        slash = 60, // /
        equal = 16, // =
        comma = 20, // ,
        semicolon = 14, // ;
        colon = 5, // :
        point = 61,	// .
        arrow = 62,	// ^
        leftpar = 9,	// (
        rightpar = 4,	// )
        lbracket = 11,	// [
        rbracket = 12,	// ]
        flpar = 63,	// {
        frpar = 64,	// }
        later = 65,	// <
        greater = 66,	// >
        laterequal = 67,	//  <=
        greaterequal = 68,	//  >=
        latergreater = 69,	//  <>
        plus = 70,	// +
        minus = 71,	// –
        lcomment = 72,	//  (*
        rcomment = 73,	//  *)
        assign = 51,	//  :=
        twopoints = 74,	//  ..
        ident = 2,	// идентификатор
        floatc = 82,	// вещественная константа
        intc = 15,	// целая константа
        stringc = 83, // строка
        casesy = 31, elsesy = 32, filesy = 57, gotosy = 33, thensy = 52,
        typesy = 34, untilsy = 53, dosy = 54, withsy = 37, ifsy = 56,
        insy = 100, ofsy = 101, orsy = 102, tosy = 103, endsy = 104,
        varsy = 105, divsy = 106, andsy = 107, notsy = 108, forsy = 109,
        modsy = 110, nilsy = 111, setsy = 112, beginsy = 113, whilesy = 114,
        arraysy = 115, constsy = 116, labelsy = 117, downtosy = 118,
        packedsy = 119, recordsy = 120, repeatsy = 121, programsy = 122,
        functionsy = 123, procedurensy = 124;

    private Keywords keywords;
    private byte symbol;
    private TextPosition token;
    private string addrName;
    private int nmb_int;
    private float nmb_float;
    private char one_symbol;
    private byte pendingToken;

    public LexicalAnalyzer()
    {
        keywords = new Keywords();
        token = new TextPosition(0, 0);
        pendingToken = 0;
        symbol = 0;
        nmb_int = 0;
        nmb_float = 0;
        addrName = "";
    }

    public byte NextSym()
    {
        if (pendingToken != 0)
        {
            byte savedSymbol = pendingToken;
            pendingToken = 0;
            return savedSymbol;
        }

        while (InputOutput.Ch == ' ' || InputOutput.Ch == '\n' || InputOutput.Ch == '\r' || InputOutput.Ch == '\t')
            InputOutput.NextCh();

        token.lineNumber = InputOutput.PositionNow.lineNumber;
        token.charNumber = InputOutput.PositionNow.charNumber;

        if (InputOutput.Ch == '\0') return 0;

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

            if (keywords.Kw.ContainsKey(len) && keywords.Kw[len].ContainsKey(name))
            {
                symbol = keywords.Kw[len][name];
            }
            else
            {
                symbol = ident;
                addrName = name;
            }
            return symbol;
        }

        if (InputOutput.Ch >= '0' && InputOutput.Ch <= '9')
        {
            byte digit;
            Int16 maxint = Int16.MaxValue;
            nmb_int = 0;
            bool isOverflow = false;

            while (InputOutput.Ch >= '0' && InputOutput.Ch <= '9')
            {
                digit = (byte)(InputOutput.Ch - '0');
                if (!isOverflow)
                {
                    if (nmb_int < maxint / 10 || (nmb_int == maxint / 10 && digit <= maxint % 10))
                        nmb_int = 10 * nmb_int + digit;
                    else
                    {
                        InputOutput.Error(203, token);
                        isOverflow = true;
                        nmb_int = 0;
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
                    pendingToken = twopoints;
                    InputOutput.NextCh();
                    symbol = intc;
                    return symbol;
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
                    nmb_float = (float)nmb_int + fractionalPart;
                    symbol = floatc;
                    return symbol;
                }
                else
                {
                    pendingToken = point;
                    symbol = intc;
                    return symbol;
                }
            }
            symbol = intc;
            return symbol;
        }

        switch (InputOutput.Ch)
        {
            case '<':
                InputOutput.NextCh();
                if (InputOutput.Ch == '=')
                {
                    symbol = laterequal; InputOutput.NextCh();
                }
                else
                    if (InputOutput.Ch == '>')
                    {
                        symbol = latergreater; InputOutput.NextCh();
                    }
                    else
                        symbol = later;
                break;
            case '>':
                InputOutput.NextCh();
                if (InputOutput.Ch == '=')
                {
                    symbol = greaterequal; InputOutput.NextCh();
                }
                else
                    symbol = greater;
                break;
            case ':':
                InputOutput.NextCh();
                if (InputOutput.Ch == '=')
                {
                    symbol = assign; InputOutput.NextCh();
                }
                else
                    symbol = colon;
                break;
            case ';':
                symbol = semicolon;
                InputOutput.NextCh();
                break;
            case '.':
                InputOutput.NextCh();
                if (InputOutput.Ch == '.')
                {
                    symbol = twopoints; InputOutput.NextCh();
                }
                else symbol = point;
                break;
            case ',':
                symbol = comma;
                InputOutput.NextCh();
                break;
            case '*':
                symbol = star;
                InputOutput.NextCh();
                break;
            case '/':
                symbol = slash;
                InputOutput.NextCh();
                break;
            case '=':
                symbol = equal;
                InputOutput.NextCh();
                break;
            case '^':
                symbol = arrow;
                InputOutput.NextCh();
                break;
            case '(':
                InputOutput.NextCh();
                if (InputOutput.Ch == '*')
                {
                    InputOutput.NextCh();

                    while (true)
                    {
                        if (InputOutput.Ch == '\0')
                        {
                            InputOutput.Error(1, token);
                            return 0;
                        }

                        if (InputOutput.Ch == '*')
                        {
                            InputOutput.NextCh();
                            if (InputOutput.Ch == ')')
                            {
                                InputOutput.NextCh(); 
                                break; 
                            }
                            continue; 
                        }
                        InputOutput.NextCh();
                    }

                    return NextSym();
                }
                else
                {
                    symbol = leftpar;
                }
                break;
            case ')':
                symbol = rightpar;
                InputOutput.NextCh();
                break;
            case '[':
                symbol = lbracket;
                InputOutput.NextCh();
                break;
            case ']':
                symbol = rbracket;
                InputOutput.NextCh();
                break;
            case '{':
                symbol = flpar;
                InputOutput.NextCh();
                break;
            case '}':
                symbol = frpar;
                InputOutput.NextCh();
                break;
            case '\'': 
            case '"':  
                char quoteType = InputOutput.Ch; 
                InputOutput.NextCh();           

                string strLiteral = "";

                
                while (InputOutput.Ch != quoteType && InputOutput.Ch != '\0')
                {
                    
                    if (InputOutput.Ch == '\n' || InputOutput.Ch == '\r')
                    {
                        break;
                    }

                    strLiteral += InputOutput.Ch; 
                    InputOutput.NextCh();
                }

                if (InputOutput.Ch == quoteType)
                {
                    InputOutput.NextCh();       
                    symbol = stringc;           
                    addrName = strLiteral;      
                }
                else
                {
                    
                    InputOutput.Error(1, token);
                    symbol = 0;
                }
                return symbol;
            default:
                InputOutput.Error(1, token);
                InputOutput.NextCh();
                symbol = 0;
                return 1;
        }

        return symbol;
    }
}