using KalahaGame;
using KalahaGame.Computer;
using KalahaGame.GameModel;

Console.WriteLine("Hello");
var computerPlayer = Player.Player2;
var searchDepth = 5;
var state = GameState.Initial();
var searchEngine = new MinMaxSearch.SearchEngine();
while (!state.IsEndState())
{
    UI.PrintState(state);
    if (state.CurrentPlayer == computerPlayer)
    {
        var minMaxState = new MinMaxStateImpl(state, Player.Player2);
        var result = searchEngine.Search(minMaxState, searchDepth);
        UI.ShowComputerScore(result.Evaluation);
        var recommendation = (MinMaxStateImpl)result.NextMove;
        state = recommendation.GameState;
        UI.PrintStep(recommendation.PreviousStep);
    }
    else
    {
        var steps = state.GetAvailableSteps();
        UI.PrintSteps(steps);
        var choosenStep = UI.ChooseStep(steps);
        state = state.Apply(choosenStep);
    }
    UI.PrintEmptyLines();
}
UI.PrintState(state);
UI.PrintPoints(state);
