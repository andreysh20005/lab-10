internal class Program
{
    private static void Main(string[] args)
    {
        
        string file1 = "test2.txt";
        string file2 = "codes_output.txt";

        if (!File.Exists(file1))
        {
            Console.WriteLine("файл не найден!");
            return;
        }

        InputOutput.Init(file1);

        LexicalAnalyzer lexer = new LexicalAnalyzer();
        List<string> tokenCodes = new List<string>();

        try
        {
            while (true)
            {
                byte code = lexer.NextSym();

                if (code == 0)
                    break;

                tokenCodes.Add(code.ToString());
            }
        }
        catch (EndOfStreamException)
        {
            Console.WriteLine("анализ завершён");
        }

        string resultStr = string.Join(" ", tokenCodes);
        File.WriteAllText(file2, resultStr);


        Console.WriteLine($"Сгенерированные коды: {resultStr}");

    }
}
