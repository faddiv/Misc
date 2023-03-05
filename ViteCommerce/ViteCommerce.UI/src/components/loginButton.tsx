import { useSession } from "next-auth/react";
import { FunctionComponent } from "react";

export const LoginButton: FunctionComponent = () => {

  const {status, data} = useSession();

  console.log("status:",status, "data:",data);
  if (status === "loading") {
    return <p>loading...</p>
  }

  if (!data) {
    return <>
      <h1>Who are you?</h1>
      <a href="/api/auth/signin"><button>sign in</button></a>
    </>
  }

  return (
    <>
      <h1>{`Hola ${data.user?.name}`}</h1>
      <a href="/api/auth/signout"><button>sign out</button></a>
    </>
  );
};
