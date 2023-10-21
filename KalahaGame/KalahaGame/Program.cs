using KalahaGame;

Console.WriteLine("Hello");
var state = GameState.Initial();
while (!state.IsEndState())
{
    UI.PrintState(state);
    var steps = state.GetAvailableSteps();
    UI.PrintSteps(steps);
    var choosenStep = UI.ChooseStep(steps);
    state = state.Apply(choosenStep);
    UI.PrintEmptyLines();
}
UI.PrintState(state);
UI.PrintPoints(state);
