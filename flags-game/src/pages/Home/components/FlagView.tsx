import { FunctionComponent, memo } from "react";
import { FlagInfo } from "../../../flagsService/flagList";
import { useChangeTracker } from "../../../common/logChanges";

interface  FlagViewProps {
  flagInfo: FlagInfo;
}

export const FlagView: FunctionComponent< FlagViewProps> = memo(({ flagInfo }) => {
  return (
    <img src={`Flags/${flagInfo.Pic}`} className="country-flag border" alt="logo" />
  );
});
