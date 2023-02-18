import css from "./index.module.scss";

export { Page }

function Page() {
  return (
    <>
      <h1>About</h1>
      <p>
        Example of using <code className={css.code}>vite-plugin-ssr</code>.
      </p>
    </>
  )
}
