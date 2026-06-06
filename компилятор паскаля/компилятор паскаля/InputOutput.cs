
class TextPosition
{
    public uint lineNumber { get; set; } // номер строки
    public byte charNumber { get; set; } // номер позиции в строке
    public TextPosition(uint ln = 0, byte c = 0)
    {
        lineNumber = ln;
        charNumber = c;
    }
}

class Err
{
    public TextPosition errorPosition { get; set; }
    public byte errorCode { get; set; }
    public Err(TextPosition errorPosition, byte errorCode)
    {
        this.errorPosition = errorPosition;
        this.errorCode = errorCode;
    }
}

class InputOutput
{
    const byte ERRMAX = 200;
    private static string _line;
    private static byte _lastInLine;
    private static uint _errCount;
    private static uint _errShowCount;
    private static List<Err> _err;
    public static TextPosition PositionNow { get; set; }
    public static StreamReader File { get; set; }
    public static char Ch { get; set; }

    private static Dictionary<byte, string> _ErrorTable;



    public static void Init(string filePath)
    {
        File = new System.IO.StreamReader(filePath);
        _errCount = 0;
        _errShowCount = 0;
        _lastInLine = 0;
        PositionNow = new TextPosition(1, 0);
        _ErrorTable = new Dictionary<byte, string>
        {
            { 1,   "Неизвестный символ (лексическая ошибка)" },
            { 200, "Синтаксическая ошибка" },
            { 201, "Повторное объявление переменной" },
            { 202, "Повторное объявление процедуры" },
            { 203, "Слишком большое целое число (предел для Integer — 32767)" },
            { 204, "Закрывающий комментарий '*)' не имеет открывающего '(*'" },
            { 205, "Комментарий не закрыт" },
            { 206, "Строковая константа не закрыта" },
            { 207, "Необъявленная переменная" },
            { 208, "Необъявленная процедура" },
            { 209, "Ожидался оператор (присваивание, вызов процедуры или begin)" },
            { 210, "Лишние лексемы после точки" }
        };
        ReadNextLine();
        Ch = _line[0];
    }
    private static void ListThisLine()
    {
        Console.WriteLine(_line);
    }

    private static void ReadNextLine()
    {
        if (!File.EndOfStream)
        {
            _errShowCount = 0;
           
            _line = File.ReadLine() + "\n";
            _lastInLine = (byte)(_line.Length - 1);

            Console.WriteLine("      " + _line.TrimEnd('\n', '\r'));

            _err = new List<Err>();
        }
        else
        {
            if (_line != "\0") 
            {
                End();
                File.Close();
            }
            _line = "\0";
            _lastInLine = 0;
            _err = new List<Err>();
        }
    }

    public static void NextCh()
    {
        if (Ch == '\0')
        {
            return;
        }

        if (PositionNow.charNumber == _lastInLine)
        {
            if (_err.Count > 0)
            {
                ListErrors();
            }

            if (_line == "\0")
            {
                End();
                File.Close();
                Ch = '\0';
                return;
            }

            ReadNextLine();
            PositionNow.lineNumber = PositionNow.lineNumber + 1;
            PositionNow.charNumber = 0;
            Ch = _line[0];
        }
        else
        {
            PositionNow.charNumber = (byte)(PositionNow.charNumber + 1);
            Ch = _line[PositionNow.charNumber];
        }
    }

    private static void End()
    {
        Console.WriteLine($"Компиляция завершена: ошибок — {_errCount}!");
    }

    private static void ListErrors()
    {
        int ii = _err.Count-1;
        int spacesNeeded;
        string message;
        foreach (var e in _err)
        {
            Console.Write($"**{_errCount-ii-_errShowCount:D2}**");

            spacesNeeded = e.errorPosition.charNumber + 5 - 6;

            for (int i = 0; i < spacesNeeded+1; i++)
            {
                Console.Write(" ");
            }
            message = _ErrorTable[e.errorCode];

            Console.WriteLine($"^ ошибка код {e.errorCode}: {message}");
            ii--;
        }
    }
    public static void Error(byte errorCode, TextPosition position)
    {
        _errCount++;

        if (_err.Count < ERRMAX)
        {
            TextPosition savedPosition = new TextPosition(position.lineNumber, position.charNumber);
            Err e = new Err(savedPosition, errorCode);
            _err.Add(e);
        }
        else _errShowCount++;

        if (Ch == '\0')
        {
            ListErrors();
            _err.Clear();
        }
    }
}

