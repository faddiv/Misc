
export function lastItem<T>(arr: T[]) {
  return arr.length > 0
    ? arr[arr.length - 1]
    : undefined;
}

export function flat<T>(arr: T[][]) {
  return arr.reduce((prev, current) => prev.concat(current), []);
}

export function zip<T1, T2, T3>(arr1: T1[], arr2: T2[], resultSelector: (v1: T1, v2: T2) => T3) {
  var result: T3[] = [];
  var length = Math.min(arr1.length, arr2.length);
  for (let index = 0; index < length; index++) {
    const element1 = arr1[index];
    const element2 = arr2[index];
    result.push(resultSelector(element1, element2));
  }
  return result;
}


export function zipArray<T1, T2>(arr1: T1[], arr2: T2[]) {
  return zip(arr1, arr2, (v1, v2) => [v1, v2]);
}

export function distinct<T>(arr: T[], eqFunc: (left: T, right: T) => boolean) {
  var result: T[] = [];
  for (const item of arr) {
    if (result.some(h => eqFunc(h, item)))
      continue;
    result.push(item);
  }
  return result;
}

export function forEach2D<T>(arr: T[][], fun: (val: T) => void) {
  for (const row of arr) {
    for (const item of row) {
      fun(item);
    }
  }
}
