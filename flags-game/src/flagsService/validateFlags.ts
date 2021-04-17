import { flags } from "./flagList";

export function validateFlags(selectedFlags:string[]) {
  const invalidFlags: string[] = [];
  for (const flag of selectedFlags) {
    if(flags.findIndex(val => val.Pic === flag) === -1) {
      invalidFlags.push(flag);
    }
  }
  return invalidFlags.length > 0 ? invalidFlags : undefined;
}
