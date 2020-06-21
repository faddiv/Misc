import $ from "jquery";
import "datatables.net";
import "datatables.net-bs4";

$(function () {
    $("#datatable").DataTable({
        ajax: {
            url: "/DatatablesNet/Data",
            data: (data, _) => {
                return JSON.stringify(data);
            },
            method: "POST",
            contentType: "application/json"
        },
        processing: true,
        serverSide: true,
        columns: [
            { data: "id" },
            { data: "name" },
            { data: "email" },
        ]
    })
});
