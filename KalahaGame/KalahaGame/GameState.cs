namespace KalahaGame;

public record class GameState(
GameState? PreviousState,
Player CurrentPlayer,
int[] Cups)
{
    public Span<int> Player1Cups => new(Cups, 0, 6);
    public int Player1Mancala => Cups[6];
    public Span<int> Player2Cups => new(Cups, 7, 6);
    public int Player2Mancala => Cups[13];

    public int Player1Points => Player1Mancala + SumPieces(Player1Cups);

    public int Player2Points => Player2Mancala + SumPieces(Player2Cups);

    public static GameState Initial()
    {
        return new GameState(
            null,
            Player.Player1,
            new[] { 4, 4, 4, 4, 4, 4, 4, 0, 4, 4, 4, 4, 4, 0 });
    }

    public Step[] GetAvailableSteps()
    {
        var currentPlayerCups = CurrentPlayer == Player.Player1
            ? Player1Cups
            : Player2Cups;
        var steps = new Step[CountNonEmptyCups(currentPlayerCups)];
        var j = 0;
        for (var i = 0; i < currentPlayerCups.Length; i++)
        {
            if (currentPlayerCups[i] == 0)
                continue;

            steps[j] = new Step(CurrentPlayer, i + 1);
            j++;
        }
        return steps;
    }

    internal bool IsEndState()
    {
        return
            CountNonEmptyCups(Player1Cups) == 0 ||
            CountNonEmptyCups(Player2Cups) == 0;
    }
    public GameState Apply(Step step)
    {
        int[] nextCups = CloneCups();
        var currentCupContent = nextCups[step.CupsIndex];
        nextCups[step.CupsIndex] = 0;
        var currentCupIndex = step.CupsIndex;
        for (int i = 0; i < currentCupContent; i++)
        {
            currentCupIndex = NextCupIndex(currentCupIndex);
            nextCups[currentCupIndex]++;
        }
        var nextPlayer = GetNextPlayer(currentCupIndex);
        if (CupCaptured(currentCupIndex, nextCups[currentCupIndex]))
        {
            var oppositeCupIndex = GetOppositeCupIndex(currentCupIndex);
            var oppositeCupContent = nextCups[oppositeCupIndex];
            nextCups[oppositeCupIndex] = 0;
            nextCups[currentCupIndex] += oppositeCupContent;
        }
        return new GameState(
            this,
            nextPlayer,
            nextCups);
    }

    private int[] CloneCups()
    {
        var nextCups = new int[Cups.Length];
        Cups.CopyTo(nextCups, 0);
        return nextCups;
    }

    private int NextCupIndex(int currentCupIndex)
    {
        var nextCupIndex = currentCupIndex;
        do
        {
            nextCupIndex = (nextCupIndex + 1) % 14;
        } while (nextCupIndex == EnemyCupIndex());
        return nextCupIndex;
    }

    private Player GetNextPlayer(int currentCupIndex)
    {
        if (currentCupIndex == OwnCupIndex())
            return CurrentPlayer;

        return CurrentPlayer == Player.Player1
            ? Player.Player2
            : Player.Player1;
    }

    private bool CupCaptured(int currentCupIndex, int currentCupContent)
    {
        if (currentCupIndex == OwnCupIndex())
            return false;
        if (currentCupIndex == EnemyCupIndex())
            return false;
        if (!IsOwnSide(currentCupIndex))
            return false;
        return currentCupContent == 1;
    }

    private bool IsOwnSide(int currentCupIndex)
    {
        return CurrentPlayer == Player.Player1
            ? currentCupIndex <= 6
            : currentCupIndex > 6;
    }

    private int GetOppositeCupIndex(int currentCupIndex)
    {
        return 12 - currentCupIndex;
    }

    private int OwnCupIndex()
    {
        return CurrentPlayer == Player.Player1
            ? 6
            : 13;
    }

    private int EnemyCupIndex()
    {
        return CurrentPlayer == Player.Player1
            ? 13
            : 6;
    }

    private static int CountNonEmptyCups(Span<int> currentPlayerCups)
    {
        var count = 0;
        for (int i = 0; i < currentPlayerCups.Length; i++)
        {
            if (currentPlayerCups[i] > 0)
                count++;
        }
        return count;
    }

    private static int SumPieces(Span<int> cups)
    {
        var sum = 0;
        for (int i = 0; i < cups.Length; i++)
        {
            sum += cups[i];
        }
        return sum;
    }
}
