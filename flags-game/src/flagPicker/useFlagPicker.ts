import { useCallback, useEffect, useReducer } from "react";
import { Actions } from "./actionsAndState";
import { initGameState, reducer } from "./reducer";

export function useFlagPicker(flags: string[]) {
  const [gameState, dispatch] = useReducer(reducer, flags, initGameState);

  useEffect(() => {
    if (gameState.flags === flags) {
      return;
    }
    dispatch({ type: Actions.Reset, flags });
  }, [gameState, flags]);

  const incrementCorrect = useCallback((flag: string) => {
    dispatch({ type: Actions.IncrementCorrect, flag });
  }, [dispatch]);

  const incrementWrong = useCallback((flag: string) => {
    dispatch({ type: Actions.IncrementWrong, flag });
  }, [dispatch]);

  const pickFlag = useCallback(() => {
    dispatch({ type: Actions.PickFlag });
  }, [dispatch]);

  return { gameState, incrementCorrect, incrementWrong, pickFlag };
}
