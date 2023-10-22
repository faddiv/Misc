using KalahaGame.GameModel;
using MinMaxSearch;

namespace KalahaGame.Computer
{
    public class MinMaxStateImpl : IDeterministicState, IEquatable<MinMaxStateImpl>
    {
        private readonly GameModel.Player _favoredPlayer;

        public MinMaxStateImpl(
            GameState gameState,
            GameModel.Player favoredPlayer,
            Step? previousStep = null)
        {
            GameState = gameState;
            PreviousStep = previousStep;
            _favoredPlayer = favoredPlayer;
        }

        public MinMaxSearch.Player Turn =>
            GameState.CurrentPlayer == _favoredPlayer
                ? MinMaxSearch.Player.Max
                : MinMaxSearch.Player.Min;

        public GameState GameState { get; }
        public Step? PreviousStep { get; }

        public double Evaluate(int depth, List<IState> passedThroughStates)
        {
            var weight = WeightedPoints(GameState.Player2Mancala, GameState.Player2Pieces)
                - WeightedPoints(GameState.Player1Mancala, GameState.Player1Pieces);
            return _favoredPlayer == GameModel.Player.Player2
                ? weight
                : -weight;
        }

        public IEnumerable<IState> GetNeighbors()
        {
            var steps = GameState.GetAvailableSteps();
            foreach (var step in steps)
            {
                yield return new MinMaxStateImpl(
                    GameState.Apply(step),
                    _favoredPlayer,
                    step);
            }
        }

        private static int WeightedPoints(int mancala, int pieces)
        {
            return (mancala * 2) + pieces;
        }

        public override bool Equals(object? obj)
        {
            var otherState = obj as MinMaxStateImpl;
            return Equals(otherState);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_favoredPlayer, GameState.GetHashCode());
        }

        public bool Equals(MinMaxStateImpl? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return _favoredPlayer == other._favoredPlayer
                && GameState.Equals(other.GameState);
        }
    }
}
