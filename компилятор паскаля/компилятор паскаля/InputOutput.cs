
struct TextPosition
{
    public uint lineNumber; // номер строки
    public byte charNumber; // номер позиции в строке

    public TextPosition(uint ln = 0, byte c = 0)
    {
        lineNumber = ln;
        charNumber = c;
    }
}

struct Err
{
    public TextPosition errorPosition;
    public byte errorCode;

    public Err(TextPosition errorPosition, byte errorCode)
    {
        this.errorPosition = errorPosition;
        this.errorCode = errorCode;
    }
}
class InputOutput
{
    const byte ERRMAX = 9;
    public static char Ch { get; set; }
    public static TextPosition positionNow = new TextPosition();
    static string line;
    static byte lastInLine = 0;
    public static List<Err> err;
    public static StreamReader File { get; set; }
    static uint errCount = 0;

    public static readonly Dictionary<byte, string> ErrorTable = new Dictionary<byte, string>
    {
        { 1, "Недопустимый символ" },
        { 2, "Не закрытая скобка!" },
        { 3, "Нет точки с запятой!" },
        { 4, "Неправильный конец строки!" }
    };



    public static void Init(string filePath)
    {
        File = new System.IO.StreamReader(filePath);
        positionNow = new TextPosition(1, 0);
        errCount = 0;

        ReadNextLine();

        Ch = line[0];
    }

    public static void NextCh()
    {
        if (Ch == '@' || Ch == '$')
        {
            Error(1, positionNow);
        }

        if (line[0] == '{' && line[line.Length - 2] != '}')
        {
            if (positionNow.charNumber == 0) Error(2, positionNow);
        }

        if (positionNow.charNumber > 0 && Ch == ';' && line[positionNow.charNumber - 1] == ' ')
        {
            Error(3, positionNow);
        }

        if (positionNow.charNumber == lastInLine - 1 && Ch != ';' && Ch != '}' && line.Trim() != "")
        {
            Error(4, positionNow);
        }

        if (positionNow.charNumber == lastInLine)
        {
            if (err.Count > 0)
            {
                ListErrors();
            }

            ReadNextLine();

            positionNow.lineNumber = positionNow.lineNumber + 1;
            positionNow.charNumber = 0;
            Ch = line[0];
        }
        else
        {
            positionNow.charNumber = (byte)(positionNow.charNumber + 1);
            Ch = line[positionNow.charNumber];
        }
    }

    private static void ListThisLine()
    {
        Console.WriteLine(line);
    }

    private static void ReadNextLine()
    {
        if (!File.EndOfStream)
        {
            line = File.ReadLine() + " ";
            lastInLine = (byte)(line.Length - 1);

            Console.WriteLine("      " + line.TrimEnd());

            err = new List<Err>();
        }
        else
        {
            End();
            File.Close();
            throw new System.IO.EndOfStreamException();
        }
    }

    static void End()
    {
        Console.WriteLine($"Компиляция завершена: : ошибок — {errCount}!");
    }

    private static void ListErrors()
    {
        foreach (var e in err)
        {

            Console.Write($"**{err.Count:D2}**");

            int spacesNeeded = e.errorPosition.charNumber + 5 - 6;

            for (int i = 0; i < spacesNeeded+1; i++)
            {
                Console.Write(" ");
            }

            string message = ErrorTable[e.errorCode];

            Console.WriteLine($"^ ошибка код {e.errorCode}: {message}");
        }
    }

    static public void Error(byte errorCode, TextPosition position)
    {
        errCount++;

        Err e;
        if (err.Count <= ERRMAX)
        {
            e = new Err(position, errorCode);
            err.Add(e);
        }
    }
}

