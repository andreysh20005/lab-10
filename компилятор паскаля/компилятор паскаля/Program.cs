internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Запуск теста...");
        InputOutput.Init("test.txt");
        try
        { 
            while (true)
            {
                InputOutput.NextCh();
            }
        }
        catch
        {

        }

        
    }
}