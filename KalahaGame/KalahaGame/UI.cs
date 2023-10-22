using KalahaGame.GameModel;

namespace KalahaGame;

public static class UI
{
    private const ConsoleColor gray = ConsoleColor.DarkGray;
    private static readonly int[] positions = new[] { 1, 2, 3, 4, 5, 6 };
    public static void PrintState(GameState state)
    {
        Console.ForegroundColor = gray;
        PrintSide(positions, reverse: true);
        Console.ForegroundColor = ConsoleColor.White;
        PrintSide(state.Player2Cups, reverse: true);
        PrintMancalas(state.Player2Mancala, state.Player1Mancala);
        PrintSide(state.Player1Cups);
        Console.ForegroundColor = gray;
        PrintSide(positions);
        Console.ForegroundColor = ConsoleColor.White;
    }

    public static void PrintSteps(Step[] steps)
    {
        Console.ForegroundColor = gray;
        Console.WriteLine(string.Join("; ", steps.AsEnumerable()));
        Console.ForegroundColor = ConsoleColor.White;
    }

    public static void PrintStep(Step? step)
    {
        ArgumentNullException.ThrowIfNull(step, nameof(step));

        Console.ForegroundColor = gray;
        Console.WriteLine("Taken step: {0}", step);
        Console.ForegroundColor = ConsoleColor.White;
    }

    public static Step ChooseStep(Step[] steps)
    {
        Step? step = null;
        do
        {
            Console.Write("Select Step:");
            var selection = Console.ReadLine();
            if (!int.TryParse(selection, out var position))
                continue;

            step = steps.FirstOrDefault(e => e.Position == position);
        } while (step == null);
        return step;
    }

    public static void PrintEmptyLines()
    {
        Console.WriteLine();
        Console.WriteLine();
    }

    public static void PrintPoints(GameState state)
    {
        Console.WriteLine("Player1: {0,2}", state.Player1Points);
        Console.WriteLine("Player2: {0,2}", state.Player2Points);
    }

    private static void PrintSide(Span<int> cups, bool reverse = false)
    {
        Console.Write("    ");
        for (int i = 0; i < cups.Length; i++)
        {
            Console.Write("{0,4}", cups[reverse ? cups.Length - (i + 1) : i]);
        }
        Console.WriteLine("    ");
    }

    private static void PrintMancalas(int player2Mancala, int player1Mancala)
    {
        Console.WriteLine("{0,4}{1,24}{2,4}", player2Mancala, "", player1Mancala);
    }

    internal static void ShowComputerScore(double evaluation)
    {
        if (evaluation > 0)
        {
            Console.WriteLine("This step favors the computer: {0}", evaluation);
        }
        else if (evaluation < 0)
        {
            Console.WriteLine("This step favors the player: {0}", evaluation);
        }
        else
        {
            Console.WriteLine("This step doesn't favor anyone.");
        }
    }

    internal static void Pause()
    {
        Console.WriteLine("Press any key to continue.");
        Console.ReadKey(true);
    }
}
