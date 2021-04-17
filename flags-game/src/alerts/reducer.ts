import { Actions, AlertActions, AlertState } from "./actionsAndState";

export function reducer(state: AlertState, action: AlertActions): AlertState {
  switch (action.type) {
    case Actions.Show:
      return {
        show: true,
        variant: action.variant,
        message: action.message
      };
    case Actions.Hide:
      if (!state.show)
        return state;
      return {
        show: false,
        variant: "",
        message: ""
      };
    default:
      return state;
  }
}
