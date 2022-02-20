import { ForwardedRef, useCallback, useImperativeHandle, useMemo, useRef } from "react";
import { ChangeHandler } from "react-hook-form";
import { GroupBase, OnChangeValue, SelectInstance } from "react-select";

export type SimpleSelect<Option, IsMulti extends boolean> = {
  focus: () => void;
  value: OnChangeValue<Option, IsMulti>;
  readonly name: string | undefined;
}

export function useSelectFormIntegration<Option, IsMulti extends boolean, Group extends GroupBase<Option> = GroupBase<Option>>(
  wrapperComponentRef: ForwardedRef<SimpleSelect<Option, IsMulti>>,
  isMulti: IsMulti,
  name: string | undefined,
  onChange: ChangeHandler | undefined
) {
  const selectRef = useRef<SelectInstance<Option, IsMulti, Group>>(null);

  const wrapperComponent = useMemo<SimpleSelect<Option, IsMulti>>(() => ({
    focus() {
      if (selectRef.current) {
        selectRef.current.focus();
      }
    },
    get name(): string | undefined {
      return name;
    },
    get value() {
      if (!selectRef.current) {
        return returnEmpty(isMulti);
      }
      return getSelectValue(selectRef.current, isMulti);
    },
    set value(newValue: OnChangeValue<Option, IsMulti>) {
      if (!selectRef.current) return;

      selectRef.current.setValue(newValue, "select-option");
    },
  }), [isMulti, name]);

  const changeHandler = useCallback(
    (newValue: OnChangeValue<Option, IsMulti>) => {
      if(!onChange) return;
      const event = {
        target: {
          name,
          value: newValue,
        },
        type: "change",
      };
      onChange(event);
    },
    [name, onChange]
  );

  useImperativeHandle(
    wrapperComponentRef,
    () => (wrapperComponent),
    [wrapperComponent]
  );

  return { changeHandler, selectRef };
}

const emptyArray: unknown = [];

function returnEmpty<Option, IsMulti extends boolean>(isMulti: IsMulti): OnChangeValue<Option,IsMulti> {
  if(isMulti) {
    return emptyArray as OnChangeValue<Option, IsMulti>;
  } else {
    return null as OnChangeValue<Option, IsMulti>;
  }
}

function getSelectValue<Option, IsMulti extends boolean>(select: SelectInstance<Option, IsMulti, any>, isMulti: IsMulti): OnChangeValue<Option,IsMulti> {
  const selectedOptions = select.getValue();
  if(isMulti) {
    return selectedOptions as OnChangeValue<Option, IsMulti>;
  } else {
    return (selectedOptions.length ? selectedOptions[0] : null) as OnChangeValue<Option, IsMulti>;
  }
}
