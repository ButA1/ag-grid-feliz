module AgChart

open Fable.Core
open Fable.Core.JsInterop
open Feliz
open System

let agChart : obj = import "AgChartsReact " "ag-charts-react"
type ChartPosition = Bottom | Left | Right | Top

[<RequireQualifiedAccess>]
type Sizing =
    | Auto
    | Manual of width:int * height:int
type MarkerShape =
    | Circle
    | Cross
    | Diamond
    | Plus
    | Square
    | Triangle
type Legend =
    { Spacing : int; Position : ChartPosition }
type SeriesKind = Line | Column | Area
[<RequireQualifiedAccess>]
type AxisKind = Category | Number | Time
type Axis =
    { Kind : AxisKind
      Position : ChartPosition
      Title : string option }
    static member Default =
        seq {
          { Kind = AxisKind.Category; Position = Bottom; Title = None }
          { Kind = AxisKind.Number; Position = Left; Title = None }
        }
type Series =
    { Data : seq<obj>
      Kind : SeriesKind
      XKey : string
      XName : string
      YKeys : string seq
      YNames : string seq
      Visible : bool
      ShowInLegend : bool
      TooltipEnabled : bool
      TooltipRenderer : (obj -> string) option
      Label : bool
      Marker : MarkerShape option
      Fills : string seq
      Strokes : string seq
      Opacity : float option
      NormalisedTo : int option }
let inline makeSeries<'T> kind =
    { Data = Seq.empty
      Kind = kind
      XKey = ""
      XName = ""
      YKeys = Seq.empty
      YNames = Seq.empty
      Visible = true
      ShowInLegend = true
      TooltipEnabled = true
      TooltipRenderer = None
      Label = true
      Marker = Some Square
      Fills = Seq.empty
      Strokes = Seq.empty
      Opacity = None
      NormalisedTo = None }
type ChartType<'T> =
    { Data : seq<'T>
      Sizing : Sizing
      Title : string
      Subtitle : string
      Series : Series seq
      Legend : Legend
      ShowNavigator : bool
      Axes : Axis seq }
let makeChart =
    { Data = Seq.empty
      Sizing = Sizing.Auto
      ShowNavigator = false
      Title = ""
      Subtitle = ""
      Series = Seq.empty
      Legend = { Spacing = 40; Position = ChartPosition.Right }
      Axes = Seq.empty }
let toSeries series =
    let (|Stackable|Single|) = function Area | Column -> Stackable | _ -> Single
    let yKeys, yNames = Seq.toArray series.YKeys, Seq.toArray series.YNames
    createObj [
        if Seq.isEmpty series.Data then () else "data" ==> Seq.toArray series.Data
        match series.NormalisedTo with None -> () | Some v -> "normalizedTo" ==> v
        "type" ==> series.Kind.ToString().ToLower()
        "xKey" ==> series.XKey
        "xName" ==> series.XName
        match series.Kind with Stackable -> () | _ -> "yKey" ==> yKeys.[0]
        match series.Kind with Stackable -> () | _ -> "yName" ==> yNames.[0]
        match series.Kind with Stackable -> "yKeys" ==> yKeys | _ -> ()
        match series.Kind with Stackable -> "yNames" ==> yNames | _ -> ()
        "visible" ==> series.Visible
        "showInLegend" ==> series.ShowInLegend
        "tooltipEnabled" ==> series.TooltipEnabled
        "label" ==> {| enabled = series.Label |}
        match series.TooltipRenderer with Some renderer -> "tooltipRenderer" ==> renderer | None -> ()
        match series.Marker with Some marker -> "marker" ==> {| enabled = true; shape = marker.ToString().ToLower() |} | None -> ()
        if Seq.isEmpty series.Fills then () else "fills" ==> series.Fills
        if Seq.isEmpty series.Strokes then () else "strokes" ==> series.Strokes
        match series.Opacity with Some opacity -> "fillOpacity" ==> opacity | None -> ()
    ]
let toOptions chart =
    createObj [
        if Seq.isEmpty chart.Data then () else "data" ==> Seq.toArray chart.Data
        "title" ==> {| text = chart.Title; enabled = not (String.IsNullOrEmpty chart.Title) |}
        "subtitle" ==> {| text = chart.Subtitle; enabled = not (String.IsNullOrEmpty chart.Subtitle) |}
        "navigator" ==> {| enabled = chart.ShowNavigator |}
        "width" ==> match chart.Sizing with Sizing.Manual(width,_) -> width | Sizing.Auto -> -1
        "height" ==> match chart.Sizing with Sizing.Manual(_,height) -> height | Sizing.Auto -> -1
        "autoSize" ==> match chart.Sizing with Sizing.Auto -> true | Sizing.Manual _ -> false
        "series" ==> (chart.Series |> Seq.map toSeries |> Seq.toArray)
        "legend" ==> {| spacing = chart.Legend.Spacing; position = chart.Legend.Position.ToString().ToLower() |}
        if Seq.isEmpty chart.Axes then ()
        else
            "axes" ==>
                [| for axis in chart.Axes do
                   {| ``type`` = axis.Kind.ToString().ToLower()
                      position = axis.Position.ToString().ToLower()
                      title = match axis.Title with Some title -> box {| enabled = true; text = title |} | None -> box {| enabled = false |} |}
                |]
    ]

[<Erase>]
type AgChart =
    static member inline options value = prop.custom("options", value)
    static member inline chart props = Interop.reactApi.createElement (agChart, createObj !!props)
