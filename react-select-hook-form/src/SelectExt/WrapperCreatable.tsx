import { ForwardedRef, forwardRef } from "react";
import { GroupBase, Props } from "react-select";
import { SimpleSelect, useSelectFormIntegration } from "./useSelectFormIntegration";
import OriginalSelect from "react-select/creatable";
import { ChangeHandler } from "react-hook-form";

export type WrapperProps<Option = unknown, IsMulti extends boolean = false, Group extends GroupBase<Option> = GroupBase<Option>> = Omit<
  Props<Option, IsMulti, Group>,
  "onChange"
> & {
  onChange?: ChangeHandler;
};

function Select<Option = unknown, IsMulti extends boolean = false, Group extends GroupBase<Option> = GroupBase<Option>>(
  { onChange, isMulti, name, ...props }: WrapperProps<Option, IsMulti, Group>,
  ref: ForwardedRef<SimpleSelect<Option, IsMulti>>
) {
  const { selectRef, changeHandler } = useSelectFormIntegration<Option, IsMulti, Group>(ref, (isMulti || false) as IsMulti, name, onChange);
  return <OriginalSelect ref={selectRef} name={name} isMulti={isMulti} onChange={changeHandler} {...props} />;
}

export default forwardRef(Select);
