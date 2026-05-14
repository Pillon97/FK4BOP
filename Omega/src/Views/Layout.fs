module Omega.Views.Layout

open Feliz
open Feliz.Router
open Omega.Types

let private navLink (href: string) (label: string) (isActive: bool) =
    Html.a
        [ prop.className [ "nav-link"; if isActive then "active" ]
          prop.href ("#/" + href)
          prop.onClick (fun e ->
              e.preventDefault ()
              Router.navigate (href))
          prop.text label ]

let navbar (page: Page) (theme: Theme) (onToggleTheme: unit -> unit) =
    let themeIcon =
        match theme with
        | Light -> "🌙"
        | Dark -> "☀️"

    let themeTitle =
        match theme with
        | Light -> "Sötét téma"
        | Dark -> "Világos téma"

    Html.header
        [ prop.className "navbar"
          prop.children
              [ Html.div
                    [ prop.className "navbar-brand"
                      prop.children
                          [ Html.span [ prop.className "brand-icon"; prop.text "Ω" ]
                            Html.span [ prop.className "brand-text"; prop.text "Omega" ]
                            Html.span [ prop.className "brand-tag"; prop.text "Feladatkezelő" ] ] ]
                Html.nav
                    [ prop.className "navbar-links"
                      prop.children
                          [ navLink "" "Feladatok" (page = ListPage)
                            navLink "new" "Új feladat" (page = NewPage) ] ]
                Html.button
                    [ prop.className "theme-toggle"
                      prop.title themeTitle
                      prop.ariaLabel themeTitle
                      prop.onClick (fun _ -> onToggleTheme ())
                      prop.type'.button
                      prop.text themeIcon ] ] ]

let footer () =
    Html.footer
        [ prop.className "footer"
          prop.children
              [ Html.span [ prop.text "Omega · Hallgatói feladatkezelő · F# + Fable + Elmish" ] ] ]

let pageWrapper (theme: Theme) (children: ReactElement list) =
    Html.div
        [ prop.className [ "app"; Theme.toCssClass theme ]
          prop.children
              [ Html.main [ prop.className "container"; prop.children children ] ] ]
