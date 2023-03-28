import { randomUUID } from "crypto";
const cache = new Map<string, string>();

interface ITokenStorage {
  saveToken(token: string): Promise<string>;
  getToken(id: string): Promise<string | undefined>;
  deleteToken(token: string): Promise<void>;
}

export const tokenStorage: ITokenStorage = {
  saveToken(token: string) {
    const id = randomUUID();
    cache.set(id, token);
    return Promise.resolve(id);
  },
  getToken(id: string) {
    return Promise.resolve(cache.get(id));
  },
  deleteToken(id: string) {
    cache.delete(id);
    return Promise.resolve();
  },
};
