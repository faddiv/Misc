import { FunctionComponent, useCallback, useEffect, useReducer } from "react";
import Alert from "react-bootstrap/Alert";
import { useService } from "../services";
import { Actions, AlertState } from "./actionsAndState";
import { reducer } from "./reducer";



interface AlertBoxProps {

}

const initialState: AlertState = { show: false, variant: "", message: "" };

export const AlertBox: FunctionComponent<AlertBoxProps> = () => {
  const alertService = useService("alert");
  const [state, dispatch] = useReducer(reducer, initialState);
  const { show, message, variant } = state;
  useEffect(() => {
    alertService.listen(dispatch);
    return () => alertService.listen(null);
  }, [alertService, dispatch]);
  const closeHandler = useCallback(() => {
    dispatch({ type: Actions.Hide });
  }, [dispatch]);
  return (
    <Alert show={show} variant={variant} onClose={closeHandler} dismissible>
      {message}
    </Alert>
  );
};
