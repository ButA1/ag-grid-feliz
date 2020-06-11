
module AgGrid

open Fable.Core
open Fable.Core.JsInterop
open Feliz

let agGrid : obj = import "AgGridReact" "ag-grid-react"

importAll "ag-grid-community/dist/styles/ag-grid.css"
importAll "ag-grid-community/dist/styles/ag-theme-alpine.css"
importAll "ag-grid-community/dist/styles/ag-theme-alpine-dark.css"
importAll "ag-grid-community/dist/styles/ag-theme-balham.css"
importAll "ag-grid-community/dist/styles/ag-theme-balham-dark.css"
importAll "ag-grid-community/dist/styles/ag-theme-material.css"

type RowSelection = Single | Multiple
type RowFilter = Number | Text | Date member this.FilterText = sprintf "ag%OColumnFilter" this

module Themes =
    let Alpine = prop.className "ag-theme-alpine"
    let AlpineDark = prop.className "ag-theme-alpine-dark"
    let Balham = prop.className "ag-theme-balham"
    let BalhamDark = prop.className "ag-theme-balham-dark"
    let Material = prop.className "ag-theme-material"

[<Erase>]
type AgGrid =
    static member inline onSelectionChanged callback = prop.custom("onRowSelected", fun x -> x?api?getSelectedRows() |> callback)
    static member inline onCellValueChanged callback = prop.custom("onCellValueChanged", fun x -> callback x?data)
    static member inline allowRowDeselection = prop.custom("rowDeselection", true)
    static member inline rowSelection (s:RowSelection) = prop.custom("rowSelection", s.ToString().ToLower())
    static member inline allowImmutableData = prop.custom("immutableData", true)
    static member inline rowData data = prop.custom("rowData", data)
    static member inline getRowNodeId callback = prop.custom("getRowNodeId", callback)
    static member inline columnDefs columns = prop.custom("columnDefs", Seq.toArray columns)
    static member inline paginationPageSize (pageSize:int) = prop.custom("paginationPageSize", pageSize)
    static member inline allowPaginationAutoPageSize = prop.custom("paginationAutoPageSize", true)
    static member inline allowPagination = prop.custom("pagination", true)

    static member inline grid props = Interop.reactApi.createElement (agGrid, createObj !!props)

type ColumnDef =
    { editable : bool
      filter : string
      sortable : bool
      field : string
      minWidth : int
      maxWidth : int
      headerCheckboxSelection : bool
      checkboxSelection : bool }

let columnDef field =
    { editable = false; filter = ""; sortable = false; field = field; minWidth = 0; maxWidth = 1000; headerCheckboxSelection = false; checkboxSelection = false }