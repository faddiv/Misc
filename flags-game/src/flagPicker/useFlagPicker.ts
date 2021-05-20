import { useCallback, useEffect, useReducer } from "react";
import { PlayList } from "../flagsService/playList";
import { Actions } from "./actionsAndState";
import { initGameState, reducer } from "./reducer";

export function useFlagPicker(playList: PlayList | null) {

  const [gameState, dispatch] = useReducer(reducer, playList, initGameState);
  const currentId = gameState.id;

  useEffect(() => {
    if (playList === null || currentId === playList.id) {
      return;
    }
    dispatch({ type: Actions.Reset, playList });
  }, [currentId, playList]);

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
