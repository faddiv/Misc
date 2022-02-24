import { ForwardedRef, forwardRef } from "react";
import { GroupBase, Props, SelectInstance } from "react-select";
import { useSelectFormIntegration } from "./useSelectFormIntegration";
import OriginalSelect from "react-select/creatable";
import { ChangeHandler } from "react-hook-form";

export type WrapperProps<Option = unknown, IsMulti extends boolean = false, Group extends GroupBase<Option> = GroupBase<Option>> = Omit<
  Props<Option, IsMulti, Group>,
  "onChange"
> & {
  onChange?: ChangeHandler;
};

function Select<Option = unknown, IsMulti extends boolean = false, Group extends GroupBase<Option> = GroupBase<Option>>(
  { onChange, name, ...props }: WrapperProps<Option, IsMulti, Group>,
  ref: ForwardedRef<SelectInstance<Option, IsMulti, Group>>
) {
  const { selectRef, changeHandler } = useSelectFormIntegration<Option, IsMulti, Group>(ref, name, onChange);
  return <OriginalSelect ref={selectRef} name={name} onChange={changeHandler} {...props} />;
}

export default forwardRef(Select);
