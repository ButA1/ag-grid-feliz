module datascience.Client

open AgGrid
open Elmish
open Elmish.React
open Fable.Core
open Feliz
open Feliz.Bulma
open System
open Thoth.Fetch
open Thoth.Json

open Fable.Core.JsInterop

importAll "bulma/css/bulma.css"

type Swimmer =
    { Id : Guid
      Athlete : string
      Age : int option
      Country : string
      Year : int
      Date : System.DateTime }
    static member Decoder =
        Decode.object (fun get ->
            { Id = Guid.NewGuid()
              Athlete = get.Required.Field "athlete" Decode.string
              Age = get.Optional.Field "age" Decode.int
              Country = get.Required.Field "country" Decode.string
              Year = get.Required.Field "year" Decode.int
              Date =
                let dateParts = (get.Required.Field "date" Decode.string).Split '/' |> Array.map int
                System.DateTime(dateParts.[2], dateParts.[1], dateParts.[0]) })

type Msg =
    | Loaded of Swimmer array
    | SelectionChanged of Swimmer array
    | UpdateSwimmer of Swimmer

type Model =
    { Data : Swimmer array
      Selected : Guid array }

module Api =
    let loadSwimmers() : JS.Promise<Swimmer array> =
        Fetch.get("https://raw.githubusercontent.com/ag-grid/ag-grid/master/grid-packages/ag-grid-docs/src/olympicWinnersSmall.json", decoder = Decode.array Swimmer.Decoder)

let init () =
    { Data = Array.empty
      Selected = Array.empty }, Cmd.OfPromise.perform Api.loadSwimmers () Loaded

let update msg model =
    match msg with
    | Loaded swimmers ->
        { model with Data = swimmers }, Cmd.none
    | SelectionChanged swimmers ->
        { model with Selected = swimmers |> Array.map(fun r -> r.Id) }, Cmd.none
    | UpdateSwimmer updated ->
        { model with
            Data = model.Data |> Array.map (fun swimmer -> if swimmer.Id = updated.Id then updated else swimmer) }, Cmd.none

module FormHelpers =
    let makeTextInput valueProp (label:string) placeholder (onchange:string -> unit) = [
        Bulma.field.div [
            Bulma.label label
        ]
        Bulma.control.div [
            Bulma.input.text [
                prop.onChange onchange
                prop.placeholder placeholder
                valueProp
            ]
        ]
    ]
    let makeTextStringInput (text:string) = makeTextInput (prop.value text)
    let makeTextNumericInput (text:int) = makeTextInput (prop.value text)

let view model dispatch =
    Bulma.section [
        Bulma.columns [
            Bulma.column [
                prop.children [
                    Html.div [
                        AgGrid.Themes.Alpine
                        prop.style [ style.height (Feliz.length.px 750) ]
                        prop.children [
                            AgGrid.grid [
                                AgGrid.columnDefs [
                                    { columnDef "Athlete" with  editable = true; filter = Text.FilterText; sortable = true; minWidth = 150; checkboxSelection = true; headerCheckboxSelection = true }
                                    { columnDef "Age" with      editable = true; filter = Number.FilterText; sortable = true; maxWidth = 90 }
                                    { columnDef "Country" with  editable = true; filter = Text.FilterText; sortable = true; minWidth = 150 }
                                    { columnDef "Year" with                      filter = Number.FilterText; sortable = true; maxWidth = 90 }
                                    { columnDef "Date" with                      filter = Date.FilterText; sortable = true }
                                ]
                                AgGrid.rowData model.Data

                                AgGrid.allowImmutableData
                                AgGrid.getRowNodeId (fun x -> x.Id)
                                AgGrid.allowRowDeselection
                                AgGrid.rowSelection AgGrid.Single
                                AgGrid.allowPagination
                                AgGrid.allowPaginationAutoPageSize
                                //AgGrid.paginationPageSize 10

                                AgGrid.onSelectionChanged (SelectionChanged >> dispatch)
                                AgGrid.onCellValueChanged (UpdateSwimmer >> dispatch)
                            ]
                        ]
                    ]
                ]
            ]
            Bulma.column [
                for swimmerId in model.Selected do
                    let swimmer = model.Data |> Array.find (fun r -> r.Id = swimmerId)
                    yield! FormHelpers.makeTextStringInput swimmer.Athlete "Name" "Enter Athlete Name" (fun t -> dispatch(UpdateSwimmer { swimmer with Athlete = t }))
                    match swimmer.Age with
                    | Some age -> yield! FormHelpers.makeTextNumericInput age "Age" "Enter Age"  ignore
                    | None -> ()
                    yield! FormHelpers.makeTextStringInput swimmer.Country "Country" "Enter Country" ignore
                    yield! FormHelpers.makeTextNumericInput swimmer.Year "Year" "Enter Year" ignore
            ]
        ]
    ]

open Elmish.Debug
open Elmish.HMR

Program.mkProgram init update view
|> Program.withConsoleTrace
|> Program.withReactSynchronous "elmish-app"
|> Program.withDebugger
|> Program.run
