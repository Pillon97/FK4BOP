module Omega.View

open Feliz
open Feliz.Router
open Omega.Types
open Omega.Views

let private notFoundView () =
    Html.section
        [ prop.className "not-found"
          prop.children
              [ Html.h1 [ prop.text "404 — Oldal nem található" ]
                Html.p [ prop.text "A keresett oldal nem létezik." ]
                Html.a
                    [ prop.href "#/"
                      prop.text "Vissza a feladatlistához"
                      prop.onClick (fun e ->
                          e.preventDefault ()
                          Router.navigate ("")) ] ] ]

let private pageContent (model: Model) dispatch =
    match model.CurrentPage with
    | ListPage -> ListView.render model dispatch
    | NewPage -> FormView.render model dispatch
    | EditPage _ -> FormView.render model dispatch
    | NotFoundPage -> notFoundView ()

let render (model: Model) (dispatch: Msg -> unit) =
    React.router
        [ router.onUrlChanged (fun segs -> dispatch (UrlChanged segs))
          router.children
              [ Layout.pageWrapper
                    model.Theme
                    [ Layout.navbar model.CurrentPage model.Theme (fun () -> dispatch ToggleTheme)
                      pageContent model dispatch
                      Layout.footer () ] ] ]
