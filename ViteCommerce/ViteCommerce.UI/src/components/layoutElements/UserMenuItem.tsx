import { FunctionComponent, useCallback } from "react";
import { Button, Nav } from "react-bootstrap";
import { signIn, signOut, useSession } from "next-auth/react";

export const UserMenuItem: FunctionComponent = () => {
  console.log("UserMenuItem rendered");
  var session = useSession();
  const googleSignin = useCallback(() => {
    console.log("signin clicked");
    signIn("google");
  }, []);
  const googleSignout = useCallback(() => {
    console.log("signout clicked");
    signOut();
  }, []);
  switch (session.status) {
    case "authenticated":
      return (
        <>
          <Nav.Link href="/">{session.data.user?.name}</Nav.Link>
          <Nav.Link as={Button} variant="link" onClick={googleSignout}>
            Logout
          </Nav.Link>
        </>
      );
    case "loading":
      return (
        <>
          <Nav.Link href="/">Loading</Nav.Link>
        </>
      );
    default:
      return (
        <>
          <Nav.Link as={Button} variant="link" onClick={googleSignin}>
            Login
          </Nav.Link>
        </>
      );
  }
};
