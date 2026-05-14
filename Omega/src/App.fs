module Omega.App

open Elmish
open Elmish.React
open Omega.State
open Omega.View

Program.mkProgram init update render
|> Program.withReactSynchronous "app"
|> Program.run
