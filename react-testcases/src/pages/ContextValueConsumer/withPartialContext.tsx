import { Context, ComponentType, PropsWithChildren, useContext, memo } from "react";


export function withPartialContext<TContext,
  TContextValues extends Extract<keyof TContext, string>
>(context: Context<TContext>, useMemo: boolean, ...injectable: TContextValues[]) {

  return function valueInjector<P extends {
    [key in TContextValues]: TContext[key];
  }>(Component: ComponentType<P>): ComponentType<Omit<P, TContextValues>> {

    if (useMemo) {
      Component = memo(Component);
    }
    function ComponentWithContextValue(props: PropsWithChildren<Omit<P, TContextValues>>) {
      const value = useContext(context);
      const enhancedProps: any = { ...props };
      for (const key of injectable) {
        if (Object.prototype.hasOwnProperty.call(value, key)) {
          const element = value[key];
          enhancedProps[key] = element;
        }
      }

      return <Component {...enhancedProps} />;
    }

    return ComponentWithContextValue;
  };
}
