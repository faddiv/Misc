import { Dispatch } from "react";
import { Actions, AlertActions } from "./actionsAndState";

export class AlertProvider {

  private listener: Dispatch<AlertActions> | null = null;

  show(variant: string, message: string) {
    if (this.listener === null)
      return;
    this.listener({ type: Actions.Show, variant, message });
  }

  hide() {
    if (this.listener === null)
      return;
    this.listener({ type: Actions.Hide });
  }

  listen(handler: Dispatch<AlertActions> | null) {
    this.listener = handler;
  }
}
