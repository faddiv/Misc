import { PageContext, PageProps } from "./types"

export function Page(props: PageContext) {
  console.log("Error page shown:", props)
  if(props.is404) {
    return (
      <div>
        <h1>Page 404</h1>
        <p>You are at the wrong place!</p>
      </div>
    );
  } else {
    return (
      <div>
        <h1>Page 500</h1>
        <p>Some internal error happened!</p>
      </div>
    );

  }
}
