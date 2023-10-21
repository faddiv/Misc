namespace KalahaGame;

public record class Step(
    Player Player,
    int Position)
{
    public int CupsIndex =>
        Player == Player.Player1
        ? Position - 1
        : Position + 7 - 1;

    public override string ToString()
    {
        return $"({Player}->{Position})";
    }
}
