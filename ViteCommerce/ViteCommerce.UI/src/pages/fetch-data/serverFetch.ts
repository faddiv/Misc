let fetchExt: typeof fetch = import.meta.env.SSR
  ? (input, init) => {
      const { env } = process;
      let url: URL | null = null;
      if (typeof input === "string") {
        url = new URL(input, env.API_URL);
      } else if (input instanceof URL) {
        url = new URL(input.pathname, env.API_URL);
      } else {
        url = new URL(input.url, env.API_URL);
        init = Object.assign({}, input, { url: undefined });
      }
      //console.log("Utl to call:", url.toString());
      return fetch(url, init);
    }
  : fetch;

export { fetchExt as fetch };
