import { useDispatch } from "react-redux";
import { useMemo } from "react";
import { MockupActions } from "./actions";
import { bindActionCreators } from "redux";

export function useMockupActions() {
    var dispatch = useDispatch();
    return useMemo(() => {
        return bindActionCreators(MockupActions, dispatch);
    }, [dispatch]);
}
