module Omega.Views.ListView

open System
open Feliz
open Feliz.Router
open Omega.Types
open Omega.State
open Omega.Views.Components

let private filterTab (label: string) (count: int) (active: bool) (onClick: unit -> unit) =
    Html.button
        [ prop.className [ "filter-tab"; if active then "active" ]
          prop.onClick (fun _ -> onClick ())
          prop.type'.button
          prop.children
              [ Html.span [ prop.text label ]
                Html.span [ prop.className "tab-count"; prop.text (string count) ] ] ]

let private filterBar
    (model: Model)
    (counts:
        {| Total: int
           Todo: int
           InProgress: int
           Done: int |})
    (dispatch: Msg -> unit)
    =
    Html.div
        [ prop.className "filter-bar"
          prop.children
              [ Html.div
                    [ prop.className "filter-tabs"
                      prop.children
                          [ filterTab "Mind" counts.Total (model.Filter = All) (fun () -> dispatch (SetFilter All))
                            filterTab
                                "Teendő"
                                counts.Todo
                                (model.Filter = OnlyStatus Todo)
                                (fun () -> dispatch (SetFilter(OnlyStatus Todo)))
                            filterTab
                                "Folyamatban"
                                counts.InProgress
                                (model.Filter = OnlyStatus InProgress)
                                (fun () -> dispatch (SetFilter(OnlyStatus InProgress)))
                            filterTab
                                "Kész"
                                counts.Done
                                (model.Filter = OnlyStatus Done)
                                (fun () -> dispatch (SetFilter(OnlyStatus Done))) ] ]
                Html.div
                    [ prop.className "filter-tools"
                      prop.children
                          [ Html.input
                                [ prop.type'.search
                                  prop.className "search-input"
                                  prop.placeholder "Keresés cím vagy leírás alapján…"
                                  prop.value model.SearchText
                                  prop.onChange (fun (v: string) -> dispatch (SetSearch v)) ]
                            Html.select
                                [ prop.className "sort-select"
                                  prop.value (SortBy.toLabel model.Sort)
                                  prop.onChange (fun (v: string) ->
                                      SortBy.all
                                      |> List.tryFind (fun s -> SortBy.toLabel s = v)
                                      |> Option.iter (fun s -> dispatch (SetSort s)))
                                  prop.children
                                      [ for s in SortBy.all ->
                                            Html.option
                                                [ prop.value (SortBy.toLabel s); prop.text (SortBy.toLabel s) ] ] ] ] ] ] ]

let private taskCard (now: DateTime) (task: StudentTask) (dispatch: Msg -> unit) =
    let descriptionBlock =
        if String.IsNullOrWhiteSpace task.Description then
            Html.none
        else
            Html.p [ prop.className "task-description"; prop.text task.Description ]

    Html.article
        [ prop.className [ "task-card"; Status.toCssClass task.Status ]
          prop.key (TaskId.toString task.Id)
          prop.children
              [ Html.div
                    [ prop.className "task-header"
                      prop.children
                          [ Html.h3 [ prop.className "task-title"; prop.text task.Title ]
                            Html.div
                                [ prop.className "task-badges"
                                  prop.children [ priorityBadge task.Priority; statusPill task.Status ] ] ] ]
                descriptionBlock
                Html.div
                    [ prop.className "task-meta"
                      prop.children [ relativeDueDate now task.DueDate ] ]
                Html.div
                    [ prop.className "task-actions"
                      prop.children
                          [ secondaryButton
                                "Státusz váltás"
                                (fun _ -> dispatch (CycleStatus task.Id))
                            secondaryButton
                                (if task.Status = Done then "Visszanyit" else "Kész")
                                (fun _ -> dispatch (ToggleStatus task.Id))
                            Html.button
                                [ prop.className "btn btn-secondary"
                                  prop.type'.button
                                  prop.text "Szerkesztés"
                                  prop.onClick (fun e ->
                                      e.preventDefault ()
                                      Router.navigate ("edit", TaskId.toString task.Id)) ]
                            dangerButton "Törlés" (fun _ ->
                                dispatch (DeleteTask task.Id)) ] ] ] ]

let render (model: Model) (dispatch: Msg -> unit) =
    let now = DateTime.Now
    let allCounts = counts model.Tasks
    let visible = visibleTasks model

    Html.section
        [ prop.className "list-view"
          prop.children
              [ Html.div
                    [ prop.className "page-header"
                      prop.children
                          [ Html.div
                                [ prop.className "page-titles"
                                  prop.children
                                      [ Html.h1 [ prop.text "Feladataim" ]
                                        Html.p
                                            [ prop.className "page-subtitle"
                                              prop.text
                                                  (sprintf
                                                      "%d feladat összesen — %d hátravan, %d kész"
                                                      allCounts.Total
                                                      (allCounts.Todo + allCounts.InProgress)
                                                      allCounts.Done) ] ] ]
                            Html.button
                                [ prop.className "btn btn-primary"
                                  prop.type'.button
                                  prop.text "+ Új feladat"
                                  prop.onClick (fun e ->
                                      e.preventDefault ()
                                      Router.navigate ("new")) ] ] ]
                filterBar model allCounts dispatch
                if List.isEmpty model.Tasks && model.Loaded then
                    emptyState
                        "Még nincs egyetlen feladatod sem. Vegyél fel egyet és kezdj el szervezetten tanulni!"
                        "+ Új feladat hozzáadása"
                        (fun e ->
                            e.preventDefault ()
                            Router.navigate ("new"))
                elif List.isEmpty visible then
                    Html.div
                        [ prop.className "empty-state subtle"
                          prop.children
                              [ Html.p
                                    [ prop.text
                                          "Nincs a szűrőnek megfelelő feladat. Próbálj egy másik szűrőt vagy keresési kifejezést." ] ] ]
                else
                    Html.div
                        [ prop.className "task-grid"
                          prop.children [ for t in visible -> taskCard now t dispatch ] ] ] ]
