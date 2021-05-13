import { FormEvent, useCallback, useState } from "react";
import { useFlagPicker } from "../flagPicker";
import { useService } from "../services";
import { FlagInfo, flags } from "./flagList";
import { PlayList } from "./playList";

const empty: FlagInfo = { Pic: "", Eng: "", Hun: "" };

export function useGame(selected: PlayList | null) {
  const alertService = useService("alert");
  const { gameState, incrementCorrect, incrementWrong, pickFlag } = useFlagPicker(selected?.flags ?? []);
  const [guessStep, setGuessStep] = useState(0);
  const [input, setInput] = useState("");
  const flagInfo = selected !== null ? flags.find(e => gameState.lastFlag === e.Pic) ?? empty : empty;
  const pickNew = useCallback(() => {
    alertService.hide();
    setGuessStep(0);
    setInput("");
    pickFlag();
  }, [alertService, pickFlag]);
  const guess = useCallback((evt: FormEvent) => {
    evt.preventDefault();
    if (flagInfo === undefined)
      return;
    if (selected === null)
      return;
    if (guessStep === 1) {
      pickNew();
      return;
    }
    if (input === "") {
      // info
      incrementWrong(flagInfo.Pic);
      alertService.show("info", `The answer is ${flagInfo.Hun}.`);
    } else if (input === flagInfo.Hun) {
      // correct
      incrementCorrect(flagInfo.Pic);
      alertService.show("success", "Correct!");
    } else if (flags.find(e => e.Hun === input) === undefined) {
      return;
    } else {
      // fail
      incrementWrong(flagInfo.Pic);
      alertService.show("danger", `Failed! The right answer is ${flagInfo.Hun}.`);
    }
    setGuessStep(1);
  }, [flagInfo, selected, guessStep, pickNew, input, alertService, incrementCorrect, incrementWrong]);

  return { pickNew, guess, flagInfo, input, setInput, gameState, guessStep };
}
