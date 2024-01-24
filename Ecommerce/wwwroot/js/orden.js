
let datatable;

$(document).ready(function () {
    loadDatatable();
});

function loadDatatable() {
    datatable = $("#tablaData").DataTable({
        "ajax": {
            "url": "/Orden/ObtenerListaOrdenes"
        },
        "columns": [
            { "data": "id", "width": "10%" },
            { "data": "nombreCompleto", "width": "15%" },
            { "data": "telefono", "width": "15%" },
            { "data": "email", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class"text-center">
                        <a href="/Orden/Detalle/${data}" class="btn btn-success text-white" style="cursor:pointer;">
                            <i class="fa-solid fa-pen-to-square"></i>
                        </a>
                        </div>
                    `;
                }, "width": "5%"
            }
        ]
    })

}