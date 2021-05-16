import { FormEvent, useCallback, useState } from "react";
import { useFlagPicker } from "../flagPicker";
import { useService } from "../services";
import { Continent } from "./continentList";
import { FlagInfo, flags } from "./flagList";
import { PlayList } from "./playList";

const empty: FlagInfo = { Pic: "", Eng: "", Hun: "", Loc: null };

export function useGame(selected: PlayList | null) {
  const alertService = useService("alert");
  const { gameState, incrementCorrect, incrementWrong, pickFlag } = useFlagPicker(selected?.flags ?? []);
  const [guessStep, setGuessStep] = useState(0);
  const [input, setInput] = useState("");
  const [continent, setContinent] = useState<Continent | null>(null);
  const flagInfo = selected !== null ? flags.find(e => gameState.lastFlag === e.Pic) ?? empty : empty;
  const pickNew = useCallback(() => {
    alertService.hide();
    setGuessStep(0);
    setInput("");
    setContinent(null);
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
      setContinent(flagInfo.Loc);
      incrementWrong(flagInfo.Pic);
      alertService.show("info", `The answer is ${flagInfo.Hun}.`);
    } else if (input === flagInfo.Hun) {
      // correct
      setContinent(flagInfo.Loc);
      incrementCorrect(flagInfo.Pic);
      alertService.show("success", "Correct!");
    } else if (flags.find(e => e.Hun === input) === undefined) {
      return;
    } else {
      // fail
      setContinent(flagInfo.Loc);
      incrementWrong(flagInfo.Pic);
      alertService.show("danger", `Failed! The right answer is ${flagInfo.Hun}.`);
    }
    setGuessStep(1);
  }, [flagInfo, selected, guessStep, pickNew, input, alertService, incrementCorrect, incrementWrong]);

  return { pickNew, guess, flagInfo, input, setInput, gameState, guessStep, continent, setContinent };
}
