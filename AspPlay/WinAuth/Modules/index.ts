import "@fortawesome/fontawesome-free";
import "./main.scss";
import "jquery";
import { MvcGrid } from "./mvc-grid";
import "datatables.net";
import "datatables.net-bs4";

console.log("Main loaded.");

[].forEach.call(document.getElementsByClassName('mvc-grid'), function (element) {
    new MvcGrid(element);
});