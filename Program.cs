namespace Chess.Core;

static class Program
{

    static void Main()
    {
        EngineSettings settings = new EngineSettings(
            true,
            @"\Engine\Opening Book\Book.txt"
        );

        UCI engine = new UCI(settings);

        string command = "";
        while (command != "quit")
        {
            command = Console.ReadLine();
            engine.ReceiveCommand(command);
        }

    }
}