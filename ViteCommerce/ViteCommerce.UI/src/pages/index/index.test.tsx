//import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { Page } from "./index.page";

describe("Home", () => {
  it("Renders", () => {
    render(<Page data="Rendered content" />);

    expect(screen.getByText("Data: Rendered content")).toBeInTheDocument();
  });
})
