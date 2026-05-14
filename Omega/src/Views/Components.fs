module Omega.Views.Components

open System
open Feliz
open Omega.Types

let priorityBadge (p: Priority) =
    Html.span
        [ prop.className [ "badge"; "priority"; Priority.toCssClass p ]
          prop.text (Priority.toLabel p) ]

let statusPill (s: Status) =
    Html.span
        [ prop.className [ "badge"; "status"; Status.toCssClass s ]
          prop.text (Status.toLabel s) ]

let primaryButton (label: string) (onClick: Browser.Types.MouseEvent -> unit) =
    Html.button
        [ prop.className "btn btn-primary"
          prop.onClick onClick
          prop.text label
          prop.type'.button ]

let secondaryButton (label: string) (onClick: Browser.Types.MouseEvent -> unit) =
    Html.button
        [ prop.className "btn btn-secondary"
          prop.onClick onClick
          prop.text label
          prop.type'.button ]

let dangerButton (label: string) (onClick: Browser.Types.MouseEvent -> unit) =
    Html.button
        [ prop.className "btn btn-danger"
          prop.onClick onClick
          prop.text label
          prop.type'.button ]

let iconButton (icon: string) (title: string) (onClick: Browser.Types.MouseEvent -> unit) =
    Html.button
        [ prop.className "icon-btn"
          prop.title title
          prop.ariaLabel title
          prop.onClick onClick
          prop.type'.button
          prop.text icon ]

let textField (label: string) (id: string) (value: string) (error: string option) (onChange: string -> unit) =
    Html.div
        [ prop.className "field"
          prop.children
              [ Html.label [ prop.htmlFor id; prop.text label ]
                Html.input
                    [ prop.id id
                      prop.type'.text
                      prop.value value
                      prop.onChange (onChange: string -> unit)
                      prop.className (if error.IsSome then "input has-error" else "input") ]
                match error with
                | Some err -> Html.div [ prop.className "field-error"; prop.text err ]
                | None -> Html.none ] ]

let textArea (label: string) (id: string) (value: string) (error: string option) (onChange: string -> unit) =
    Html.div
        [ prop.className "field"
          prop.children
              [ Html.label [ prop.htmlFor id; prop.text label ]
                Html.textarea
                    [ prop.id id
                      prop.rows 4
                      prop.value value
                      prop.onChange (onChange: string -> unit)
                      prop.className (if error.IsSome then "input has-error" else "input") ]
                match error with
                | Some err -> Html.div [ prop.className "field-error"; prop.text err ]
                | None -> Html.none ] ]

let dateField (label: string) (id: string) (value: string) (error: string option) (onChange: string -> unit) =
    Html.div
        [ prop.className "field"
          prop.children
              [ Html.label [ prop.htmlFor id; prop.text label ]
                Html.input
                    [ prop.id id
                      prop.type'.date
                      prop.value value
                      prop.onChange (onChange: string -> unit)
                      prop.className (if error.IsSome then "input has-error" else "input") ]
                match error with
                | Some err -> Html.div [ prop.className "field-error"; prop.text err ]
                | None -> Html.none ] ]

let prioritySelector (current: Priority) (onChange: Priority -> unit) =
    Html.div
        [ prop.className "field"
          prop.children
              [ Html.label [ prop.text "Prioritás" ]
                Html.div
                    [ prop.className "radio-group"
                      prop.children
                          [ for p in Priority.all ->
                                Html.label
                                    [ prop.className
                                          [ "radio"
                                            Priority.toCssClass p
                                            if p = current then "active" ]
                                      prop.children
                                          [ Html.input
                                                [ prop.type'.radio
                                                  prop.name "priority"
                                                  prop.value (Priority.toLabel p)
                                                  prop.onChange (fun (_: bool) -> onChange p) ]
                                            Html.span [ prop.text (Priority.toLabel p) ] ] ] ] ] ] ]

let statusSelector (current: Status) (onChange: Status -> unit) =
    Html.div
        [ prop.className "field"
          prop.children
              [ Html.label [ prop.htmlFor "status-select"; prop.text "Státusz" ]
                Html.select
                    [ prop.id "status-select"
                      prop.className "input"
                      prop.value (Status.toLabel current)
                      prop.onChange (fun (v: string) ->
                          Status.all
                          |> List.tryFind (fun s -> Status.toLabel s = v)
                          |> Option.iter onChange)
                      prop.children
                          [ for s in Status.all ->
                                Html.option [ prop.value (Status.toLabel s); prop.text (Status.toLabel s) ] ] ] ] ]

let relativeDueDate (now: DateTime) (d: DateTime option) =
    match d with
    | None -> Html.span [ prop.className "due-empty"; prop.text "Nincs határidő" ]
    | Some date ->
        let diff = (date.Date - now.Date).Days
        let absDays = abs diff

        let label =
            if diff = 0 then "Ma esedékes"
            elif diff = 1 then "Holnap"
            elif diff = -1 then "1 napja lejárt"
            elif diff > 0 then sprintf "%d nap múlva" diff
            else sprintf "%d napja lejárt" absDays

        let cssClass =
            if diff < 0 then "due-overdue"
            elif diff <= 2 then "due-soon"
            else "due-normal"

        Html.span
            [ prop.className [ "due"; cssClass ]
              prop.title (date.ToString("yyyy. MM. dd."))
              prop.text label ]

let emptyState (message: string) (actionLabel: string) (onAction: Browser.Types.MouseEvent -> unit) =
    Html.div
        [ prop.className "empty-state"
          prop.children
              [ Html.div [ prop.className "empty-icon"; prop.text "📚" ]
                Html.p [ prop.text message ]
                primaryButton actionLabel onAction ] ]
