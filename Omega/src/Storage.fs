module Omega.Storage

open System
open Browser.WebStorage
open Thoth.Json
open Omega.Types

let private tasksKey = "omega.tasks.v1"
let private themeKey = "omega.theme.v1"

let private priorityEncoder =
    function
    | Low -> Encode.string "Low"
    | Medium -> Encode.string "Medium"
    | High -> Encode.string "High"

let private priorityDecoder: Decoder<Priority> =
    Decode.string
    |> Decode.andThen (fun s ->
        match s with
        | "Low" -> Decode.succeed Low
        | "Medium" -> Decode.succeed Medium
        | "High" -> Decode.succeed High
        | other -> Decode.fail (sprintf "Ismeretlen prioritás: %s" other))

let private statusEncoder =
    function
    | Todo -> Encode.string "Todo"
    | InProgress -> Encode.string "InProgress"
    | Done -> Encode.string "Done"

let private statusDecoder: Decoder<Status> =
    Decode.string
    |> Decode.andThen (fun s ->
        match s with
        | "Todo" -> Decode.succeed Todo
        | "InProgress" -> Decode.succeed InProgress
        | "Done" -> Decode.succeed Done
        | other -> Decode.fail (sprintf "Ismeretlen státusz: %s" other))

let private dueDateEncoder (d: DateTime option) =
    match d with
    | Some v -> Encode.string (v.ToString("yyyy-MM-dd"))
    | None -> Encode.nil

let private dueDateDecoder: Decoder<DateTime option> =
    Decode.oneOf
        [ Decode.nil None
          Decode.string
          |> Decode.andThen (fun s ->
              match
                  DateTime.TryParse(
                      s,
                      System.Globalization.CultureInfo.InvariantCulture,
                      System.Globalization.DateTimeStyles.AssumeLocal
                  )
              with
              | true, d -> Decode.succeed (Some d.Date)
              | _ -> Decode.fail (sprintf "Érvénytelen dátum: %s" s)) ]

let private dateTimeEncoder (d: DateTime) = Encode.string (d.ToString("o"))

let private dateTimeDecoder: Decoder<DateTime> =
    Decode.string
    |> Decode.andThen (fun s ->
        match
            DateTime.TryParse(
                s,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind
            )
        with
        | true, d -> Decode.succeed d
        | _ -> Decode.fail (sprintf "Érvénytelen időbélyeg: %s" s))

let private taskEncoder (t: StudentTask) =
    Encode.object
        [ "id", Encode.string (TaskId.toString t.Id)
          "title", Encode.string t.Title
          "description", Encode.string t.Description
          "dueDate", dueDateEncoder t.DueDate
          "priority", priorityEncoder t.Priority
          "status", statusEncoder t.Status
          "createdAt", dateTimeEncoder t.CreatedAt
          "updatedAt", dateTimeEncoder t.UpdatedAt ]

let private taskDecoder: Decoder<StudentTask> =
    Decode.object (fun get ->
        let idStr = get.Required.Field "id" Decode.string

        let id =
            match TaskId.tryParse idStr with
            | Some v -> v
            | None -> TaskId.create ()

        { Id = id
          Title = get.Required.Field "title" Decode.string
          Description = get.Optional.Field "description" Decode.string |> Option.defaultValue ""
          DueDate = get.Optional.Field "dueDate" dueDateDecoder |> Option.defaultValue None
          Priority = get.Required.Field "priority" priorityDecoder
          Status = get.Required.Field "status" statusDecoder
          CreatedAt = get.Required.Field "createdAt" dateTimeDecoder
          UpdatedAt = get.Required.Field "updatedAt" dateTimeDecoder })

let private tasksEncoder (tasks: StudentTask list) =
    tasks |> List.map taskEncoder |> Encode.list

let private tasksDecoder: Decoder<StudentTask list> = Decode.list taskDecoder

let loadTasks () : StudentTask list =
    try
        match localStorage.getItem tasksKey with
        | null -> []
        | "" -> []
        | json ->
            match Decode.fromString tasksDecoder json with
            | Ok tasks -> tasks
            | Error _ -> []
    with _ ->
        []

let saveTasks (tasks: StudentTask list) : unit =
    try
        let json = tasksEncoder tasks |> Encode.toString 0
        localStorage.setItem (tasksKey, json)
    with _ ->
        ()

let loadTheme () : Theme =
    try
        match localStorage.getItem themeKey with
        | "dark" -> Dark
        | "light" -> Light
        | _ -> Light
    with _ ->
        Light

let saveTheme (theme: Theme) : unit =
    try
        let value =
            match theme with
            | Light -> "light"
            | Dark -> "dark"

        localStorage.setItem (themeKey, value)
    with _ ->
        ()
