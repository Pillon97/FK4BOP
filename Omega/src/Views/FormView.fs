module Omega.Views.FormView

open Feliz
open Feliz.Router
open Omega.Types
open Omega.Views.Components

let private isEditMode =
    function
    | EditPage _ -> true
    | _ -> false

let private headerText page =
    if isEditMode page then "Feladat szerkesztése" else "Új feladat felvétele"

let private submitLabel page =
    if isEditMode page then "Módosítások mentése" else "Feladat hozzáadása"

let private subtitle page =
    if isEditMode page then
        "Frissítsd a feladat részleteit. A módosítás azonnal mentődik."
    else
        "Add meg a feladat alapadatait. A cím megadása kötelező — minden más opcionális."

let render (model: Model) (dispatch: Msg -> unit) =
    let form = model.Form
    let errs = model.FormErrors

    Html.section
        [ prop.className "form-view"
          prop.children
              [ Html.div
                    [ prop.className "page-header simple"
                      prop.children
                          [ Html.div
                                [ prop.className "page-titles"
                                  prop.children
                                      [ Html.h1 [ prop.text (headerText model.CurrentPage) ]
                                        Html.p
                                            [ prop.className "page-subtitle"
                                              prop.text (subtitle model.CurrentPage) ] ] ] ] ]
                Html.form
                    [ prop.className "task-form"
                      prop.onSubmit (fun e ->
                          e.preventDefault ()
                          dispatch SubmitForm)
                      prop.children
                          [ textField "Cím *" "task-title" form.Title errs.Title (fun v -> dispatch (SetTitle v))
                            textArea
                                "Leírás"
                                "task-description"
                                form.Description
                                None
                                (fun v -> dispatch (SetDescription v))
                            dateField
                                "Határidő"
                                "task-due-date"
                                form.DueDate
                                errs.DueDate
                                (fun v -> dispatch (SetDueDate v))
                            prioritySelector form.Priority (fun p -> dispatch (SetPriority p))
                            statusSelector form.Status (fun s -> dispatch (SetStatus s))
                            Html.div
                                [ prop.className "form-actions"
                                  prop.children
                                      [ Html.button
                                            [ prop.className "btn btn-primary"
                                              prop.type'.submit
                                              prop.text (submitLabel model.CurrentPage) ]
                                        Html.button
                                            [ prop.className "btn btn-secondary"
                                              prop.type'.button
                                              prop.text "Mégse"
                                              prop.onClick (fun e ->
                                                  e.preventDefault ()
                                                  dispatch ResetForm
                                                  Router.navigate ("")) ] ] ] ] ] ] ]
