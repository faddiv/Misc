import { findValIndex } from "./index";

describe("findValIndex", () => {
  let values = [0.5,1.0,1.5, 2];

  it("returns 0 in case 0.2", () => {
    const result = findValIndex(0.2, values, 0, values.length);

    expect(result).toBe(0);
  });
  it("returns 1 in case 0.7", () => {
    const result = findValIndex(0.7, values, 0, values.length);

    expect(result).toBe(1);

  });
  it("returns 2 in case 1.2", () => {
    const result = findValIndex(1.2, values, 0, values.length);

    expect(result).toBe(2);
  });
  it("returns 3 in case 1.7", () => {
    const result = findValIndex(1.7, values, 0, values.length);

    expect(result).toBe(3);
  });
  it("returns 3 in case 2.7", () => {
    const result = findValIndex(2.7, values, 0, values.length);

    expect(result).toBe(3);
  });
});
