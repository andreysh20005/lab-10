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

        var lex = new LexicalAnalyzer();
        var parser = new SyntaxAnalyzer(lex);
        parser.Parse();

    }
}
