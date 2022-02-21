import { ForwardedRef, RefObject, useCallback, useImperativeHandle, useMemo, useRef } from "react";
import { ChangeHandler } from "react-hook-form";
import { GroupBase, OnChangeValue, SelectInstance } from "react-select";

export type SimpleSelect<Option = unknown, IsMulti extends boolean = false, Group extends GroupBase<Option> = GroupBase<Option>> = {
  focusInput: SelectInstance<Option, IsMulti, Group>["focusInput"];
  blurInput: SelectInstance<Option, IsMulti, Group>["blurInput"];
  focus: SelectInstance<Option, IsMulti, Group>["focus"];
  blur: SelectInstance<Option, IsMulti, Group>["blur"];
  openMenu: SelectInstance<Option, IsMulti, Group>["openMenu"];
  focusValue: SelectInstance<Option, IsMulti, Group>["focusValue"];
  focusOption: SelectInstance<Option, IsMulti, Group>["focusOption"];
  setValue: SelectInstance<Option, IsMulti, Group>["setValue"];
  selectOption: SelectInstance<Option, IsMulti, Group>["selectOption"];
  removeValue: SelectInstance<Option, IsMulti, Group>["removeValue"];
  clearValue: SelectInstance<Option, IsMulti, Group>["clearValue"];
  popValue: SelectInstance<Option, IsMulti, Group>["popValue"];
  getTheme: SelectInstance<Option, IsMulti, Group>["getTheme"];
  getValue: SelectInstance<Option, IsMulti, Group>["getValue"];
  getOptionLabel: SelectInstance<Option, IsMulti, Group>["getOptionLabel"];
  getOptionValue: SelectInstance<Option, IsMulti, Group>["getOptionValue"];
  getStyles: SelectInstance<Option, IsMulti, Group>["getStyles"];
  getElementId: SelectInstance<Option, IsMulti, Group>["getElementId"];
  hasValue: SelectInstance<Option, IsMulti, Group>["hasValue"];
  hasOptions: SelectInstance<Option, IsMulti, Group>["hasOptions"];
  isClearable: SelectInstance<Option, IsMulti, Group>["isClearable"];
  value: OnChangeValue<Option, IsMulti>;
  readonly name: string | undefined;
};

function createWrapper<Option, IsMulti extends boolean, Group extends GroupBase<Option>>(
  selectRef: RefObject<SelectInstance<Option, IsMulti, Group>>,
  functionName: keyof SelectInstance<Option, IsMulti, Group>
) {
  return (...args: any[]) => {
    if (!selectRef.current) {
      return;
    }
    return selectRef.current[functionName](...args);
  };
}

export function useSelectFormIntegration<Option, IsMulti extends boolean, Group extends GroupBase<Option> = GroupBase<Option>>(
  wrapperComponentRef: ForwardedRef<SimpleSelect<Option, IsMulti>>,
  isMulti: IsMulti,
  name: string | undefined,
  onChange: ChangeHandler | undefined
) {
  const selectRef = useRef<SelectInstance<Option, IsMulti, Group>>(null);

  const wrapperComponent = useMemo<SimpleSelect<Option, IsMulti>>(
    () => ({
      focus: createWrapper(selectRef, "focus"),
      blur: createWrapper(selectRef, "blur"),
      focusInput: createWrapper(selectRef, "focusInput"),
      blurInput: createWrapper(selectRef, "blurInput"),
      clearValue: createWrapper(selectRef, "clearValue"),
      focusOption: createWrapper(selectRef, "focusOption"),
      focusValue: createWrapper(selectRef, "focusValue"),
      getElementId: createWrapper(selectRef, "getElementId"),
      getOptionLabel: createWrapper(selectRef, "getOptionLabel"),
      getOptionValue: createWrapper(selectRef, "getOptionValue"),
      getStyles: createWrapper(selectRef, "getStyles"),
      getTheme: createWrapper(selectRef, "getTheme"),
      getValue: createWrapper(selectRef, "getValue"),
      hasOptions: createWrapper(selectRef, "hasOptions"),
      hasValue: createWrapper(selectRef, "hasValue"),
      isClearable: createWrapper(selectRef, "isClearable"),
      openMenu: createWrapper(selectRef, "openMenu"),
      popValue: createWrapper(selectRef, "popValue"),
      removeValue: createWrapper(selectRef, "removeValue"),
      selectOption: createWrapper(selectRef, "selectOption"),
      setValue: createWrapper(selectRef, "setValue"),
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
    }),
    [isMulti, name]
  );

  const changeHandler = useCallback(
    (newValue: OnChangeValue<Option, IsMulti>) => {
      if (!onChange) return;
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

  useImperativeHandle(wrapperComponentRef, () => wrapperComponent, [wrapperComponent]);

  return { changeHandler, selectRef };
}

const emptyArray: unknown = [];

function returnEmpty<Option, IsMulti extends boolean>(isMulti: IsMulti): OnChangeValue<Option, IsMulti> {
  if (isMulti) {
    return emptyArray as OnChangeValue<Option, IsMulti>;
  } else {
    return null as OnChangeValue<Option, IsMulti>;
  }
}

function getSelectValue<Option, IsMulti extends boolean>(select: SelectInstance<Option, IsMulti, any>, isMulti: IsMulti): OnChangeValue<Option, IsMulti> {
  const selectedOptions = select.getValue();
  if (isMulti) {
    return selectedOptions as OnChangeValue<Option, IsMulti>;
  } else {
    return (selectedOptions.length ? selectedOptions[0] : null) as OnChangeValue<Option, IsMulti>;
  }
}
