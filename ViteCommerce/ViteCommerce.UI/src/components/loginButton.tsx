import { useSession } from "next-auth/react";
import { FunctionComponent } from "react";
import { Button } from "react-bootstrap";

export const LoginButton: FunctionComponent = () => {
  const { status, data } = useSession();

  if (status === "loading") {
    return <p>loading...</p>;
  }

  if (!data) {
    return (
      <>
        <h1>Who are you?</h1>
        <Button as="a" variant="primary" href="/api/auth/signin">
          sign in
        </Button>
      </>
    );
  }

  return (
    <>
      <h1>{`Hola ${data.user?.name}`}</h1>
      <Button as="a" variant="primary" href="/api/auth/signout">
        sign out
      </Button>
    </>
  );
};
