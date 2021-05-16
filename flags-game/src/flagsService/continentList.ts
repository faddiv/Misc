export enum Continent {
  Africa = "africa",
  Asia = "asia",
  Europe = "europe",
  NorthAmerica = "north-america",
  Oceania = "oceania",
  SouthAmerica = "south-america",
}

export interface ContinentInfo {
  name: Continent;
  displayName: string;
}

export const continents: ContinentInfo[] = [
  {
    name: Continent.NorthAmerica,
    displayName: "É.Am.",
  },
  {
    name: Continent.SouthAmerica,
    displayName: "D.Am.",
  },
  {
    name: Continent.Africa,
    displayName: "Afri.",
  },
  {
    name: Continent.Europe,
    displayName: "Euro.",
  },
  {
    name: Continent.Asia,
    displayName: "Ázsia",
  },
  {
    name: Continent.Oceania,
    displayName: "Óceá.",
  }
];
