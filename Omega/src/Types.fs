module Omega.Types

open System

type Priority =
    | Low
    | Medium
    | High

module Priority =
    let toLabel =
        function
        | Low -> "Alacsony"
        | Medium -> "Közepes"
        | High -> "Magas"

    let toCssClass =
        function
        | Low -> "prio-low"
        | Medium -> "prio-medium"
        | High -> "prio-high"

    let toOrder =
        function
        | High -> 0
        | Medium -> 1
        | Low -> 2

    let all = [ Low; Medium; High ]

type Status =
    | Todo
    | InProgress
    | Done

module Status =
    let toLabel =
        function
        | Todo -> "Teendő"
        | InProgress -> "Folyamatban"
        | Done -> "Kész"

    let toCssClass =
        function
        | Todo -> "status-todo"
        | InProgress -> "status-inprogress"
        | Done -> "status-done"

    let next =
        function
        | Todo -> InProgress
        | InProgress -> Done
        | Done -> Todo

    let all = [ Todo; InProgress; Done ]

type TaskId = TaskId of Guid

module TaskId =
    let create () = TaskId(Guid.NewGuid())
    let value (TaskId g) = g
    let toString (TaskId g) = g.ToString()

    let tryParse (s: string) =
        match Guid.TryParse s with
        | true, g -> Some(TaskId g)
        | _ -> None

type StudentTask =
    { Id: TaskId
      Title: string
      Description: string
      DueDate: DateTime option
      Priority: Priority
      Status: Status
      CreatedAt: DateTime
      UpdatedAt: DateTime }

type StatusFilter =
    | All
    | OnlyStatus of Status

type SortBy =
    | ByDueDate
    | ByPriority
    | ByCreated

module SortBy =
    let toLabel =
        function
        | ByDueDate -> "Határidő szerint"
        | ByPriority -> "Prioritás szerint"
        | ByCreated -> "Létrehozás szerint"

    let all = [ ByDueDate; ByPriority; ByCreated ]

type FormData =
    { Title: string
      Description: string
      DueDate: string
      Priority: Priority
      Status: Status }

module FormData =
    let empty =
        { Title = ""
          Description = ""
          DueDate = ""
          Priority = Medium
          Status = Todo }

    let ofTask (t: StudentTask) =
        { Title = t.Title
          Description = t.Description
          DueDate =
            t.DueDate
            |> Option.map (fun d -> d.ToString("yyyy-MM-dd"))
            |> Option.defaultValue ""
          Priority = t.Priority
          Status = t.Status }

type FormErrors =
    { Title: string option
      DueDate: string option }

module FormErrors =
    let empty = { Title = None; DueDate = None }
    let hasAny e = e.Title.IsSome || e.DueDate.IsSome

type Theme =
    | Light
    | Dark

module Theme =
    let toggle =
        function
        | Light -> Dark
        | Dark -> Light

    let toCssClass =
        function
        | Light -> "theme-light"
        | Dark -> "theme-dark"

type Page =
    | ListPage
    | NewPage
    | EditPage of TaskId
    | NotFoundPage

type Model =
    { Tasks: StudentTask list
      CurrentPage: Page
      Filter: StatusFilter
      Sort: SortBy
      SearchText: string
      Form: FormData
      FormErrors: FormErrors
      Theme: Theme
      Loaded: bool }

type Msg =
    | UrlChanged of string list
    | LoadedFromStorage of StudentTask list
    | SetTitle of string
    | SetDescription of string
    | SetDueDate of string
    | SetPriority of Priority
    | SetStatus of Status
    | SubmitForm
    | ResetForm
    | DeleteTask of TaskId
    | ToggleStatus of TaskId
    | CycleStatus of TaskId
    | SetFilter of StatusFilter
    | SetSort of SortBy
    | SetSearch of string
    | ToggleTheme
