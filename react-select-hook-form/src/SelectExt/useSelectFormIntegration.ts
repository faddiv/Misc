import { ForwardedRef, useCallback, useImperativeHandle, useLayoutEffect, useRef } from "react";
import { ChangeHandler } from "react-hook-form";
import { GroupBase, OnChangeValue, SelectInstance } from "react-select";

export function useSelectFormIntegration<Option, IsMulti extends boolean, Group extends GroupBase<Option> = GroupBase<Option>>(
  wrapperComponentRef: ForwardedRef<SelectInstance<Option, IsMulti, Group>>,
  name: string | undefined,
  onChange: ChangeHandler | undefined
) {
  const selectRef = useRef<SelectInstance<Option, IsMulti, Group>>(null);

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

  useImperativeHandle(
    wrapperComponentRef,
    () => {
      if (!Object.prototype.hasOwnProperty.call(selectRef.current, "value")) {
        Object.defineProperty(selectRef.current, "value", {
          get() {
            const selectInstance = this as SelectInstance<Option, IsMulti, Group>;
            if (!selectInstance) {
              return returnEmpty(false);
            }
            return getSelectValue(selectInstance, selectInstance.props.isMulti);
          },
          set(newValue: OnChangeValue<Option, IsMulti>) {
            const selectInstance = this as SelectInstance<Option, IsMulti, Group>;
            if (selectInstance) {
              selectInstance.setValue(newValue, "select-option");
            }
          },
        });
      }
      return selectRef.current || ({} as any);
    },
    []
  );

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

function getSelectValue<Option, IsMulti extends boolean>(
  select: SelectInstance<Option, IsMulti, any>,
  isMulti: IsMulti
): OnChangeValue<Option, IsMulti> {
  const selectedOptions = select.getValue();
  if (isMulti) {
    return selectedOptions as OnChangeValue<Option, IsMulti>;
  } else {
    return (selectedOptions.length ? selectedOptions[0] : null) as OnChangeValue<Option, IsMulti>;
  }
}
