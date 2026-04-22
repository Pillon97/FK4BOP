namespace Alpha

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Server
open WebSharper.UI.Html

module Site =


    
    type Difficulty = 
        | [<EndPoint "easy">] Easy
        | [<EndPoint "medium">] Medium
        | [<EndPoint "hard">] Hard

    type EndPoint =
        | [<EndPoint "GET /">] Home
        | [<EndPoint "GET /game">] Game of difficulty: option<Difficulty>
        | [<EndPoint "GET /about">] About


    
    module Sudoku =

        let isValid (board: int[,]) row col num =
            let rowOk =
                seq { 0..8 }
                |> Seq.forall (fun c -> board.[row, c] <> num)

            let colOk =
                seq { 0..8 }
                |> Seq.forall (fun r -> board.[r, col] <> num)

            let br, bc = (row / 3) * 3, (col / 3) * 3

            let boxOk =
                seq {
                    for r in br .. br + 2 do
                        for c in bc .. bc + 2 -> board.[r, c]
                }
                |> Seq.forall (fun v -> v <> num)

            rowOk && colOk && boxOk

        let rec private solve (board: int[,]) =
            let mutable found = false
            let mutable result = false
            let mutable ri, ci = 0, 0
            let mutable stop = false

            for r in 0..8 do
                for c in 0..8 do
                    if not stop && board.[r, c] = 0 then
                        ri <- r
                        ci <- c
                        found <- true
                        stop <- true

            if not found then true
            else
                let mutable num = 1
                while num <= 9 && not result do
                    if isValid board ri ci num then
                        board.[ri, ci] <- num
                        if solve board then
                            result <- true
                        else
                            board.[ri, ci] <- 0
                    num <- num + 1
                result

        let generateFull () =
            let rng = System.Random()
            let board = Array2D.zeroCreate<int> 9 9
            let nums = [| 1..9 |] |> Array.sortBy (fun _ -> rng.Next())

            for c in 0..8 do
                board.[0, c] <- nums.[c]

            solve board |> ignore
            board

        let removeClues (board: int[,]) remove =
            let rng = System.Random()
            let puzzle = Array2D.copy board

            let positions =
                [| for r in 0..8 do
                       for c in 0..8 -> (r, c) |]

            let shuffled = positions |> Array.sortBy (fun _ -> rng.Next())

            for i in 0 .. remove - 1 do
                let (r, c) = shuffled.[i]
                puzzle.[r, c] <- 0

            puzzle

        let generate diff =
            let cluesToRemove =
                match diff with
                | Some Easy -> 30
                | Some Medium -> 45
                | Some Hard -> 55
                | None -> 45
            removeClues (generateFull ()) cluesToRemove

        let boardToString (board: int[,]) =
            System.String [|
                for r in 0..8 do
                    for c in 0..8 ->
                        char (int '0' + board.[r, c])
            |]

        let boardFromString (s: string) =
            let board = Array2D.zeroCreate<int> 9 9

            for i in 0 .. 80 do
                board.[i / 9, i % 9] <- int s.[i] - int '0'

            board


    
    module Styles =
        let page =
            "font-family: 'Inter', system-ui, -apple-system, sans-serif; background: linear-gradient(135deg, #f3f4f6 0%, #e5e7eb 100%); min-height: 100vh; margin: 0; padding: 0; color: #1f2937;"

        let header =
            "background: rgba(255, 255, 255, 0.8); backdrop-filter: blur(10px); color: #111827; padding: 20px 40px; display: flex; align-items: center; justify-content: space-between; border-bottom: 1px solid rgba(0,0,0,0.05); box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05);"

        let titleS = "margin: 0; font-size: 1.75rem; font-weight: 800; background: linear-gradient(to right, #4f46e5, #7c3aed); -webkit-background-clip: text; -webkit-text-fill-color: transparent;"
        let nav = "display: flex; gap: 24px;"
        let navLink = "color: #4b5563; text-decoration: none; font-size: 1rem; font-weight: 500; transition: color 0.2s;"
        let mainS = "max-width: 800px; margin: 60px auto; padding: 0 20px; text-align: center; display: flex; flex-direction: column; align-items: center;"
        
        let card = "background: white; border-radius: 20px; box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04); padding: 48px; margin-bottom: 32px; display: inline-block; width: 100%; max-width: 600px; border: 1px solid rgba(255,255,255,0.5); box-sizing: border-box;"
        
        let h2s = "color: #111827; margin-top: 0; font-size: 2rem; font-weight: 700; margin-bottom: 16px;"
        let footer = "text-align: center; color: #6b7280; padding: 32px; font-size: 0.95rem;"
        
        let button = "display: inline-block; background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%); color: white; padding: 14px 32px; border-radius: 9999px; text-decoration: none; font-size: 1.1rem; font-weight: 600; margin-top: 24px; box-shadow: 0 10px 15px -3px rgba(124, 58, 237, 0.3); transition: transform 0.2s, box-shadow 0.2s;"


        let sudokuTable =
            "border-collapse: collapse; margin: 32px auto; border: 4px solid #374151; background: #fff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);"

        let sudokuCell =
            "width: 50px; height: 50px; text-align: center; font-size: 1.5rem; border: 1px solid #d1d5db; padding: 0; transition: background 0.2s;"

        let sudokuCellRightThick =
            sudokuCell + " border-right: 3px solid #374151;"

        let sudokuCellBottomThick =
            sudokuCell + " border-bottom: 3px solid #374151;"

        let sudokuCellBothThick =
            sudokuCell + " border-right: 3px solid #374151; border-bottom: 3px solid #374151;"

        let sudokuInput =
            "width: 100%; height: 100%; border: none; text-align: center; font-size: 1.5rem; color: #4f46e5; background: #f5f3ff; outline: none; font-weight: 700; padding: 0; box-sizing: border-box; transition: background 0.2s;"

        let sudokuText =
            "display: flex; align-items: center; justify-content: center; width: 100%; height: 100%; font-weight: 700; color: #111827;"


    
    module View =

        let private layout (ctx: Context<EndPoint>) (pageTitle: string) (content: Doc) =
            Content.Page(
                Title = "Sudoku – " + pageTitle,
                Body = [
                    div [attr.style Styles.page] [
                        header [attr.style Styles.header] [
                            h1 [attr.style Styles.titleS] [text "🔢 Sudoku"]

                            nav [attr.style Styles.nav] [
                                a [attr.href (ctx.Link Home); attr.style Styles.navLink] [text "Home"]
                                a [attr.href (ctx.Link (Game None)); attr.style Styles.navLink] [text "Game"]
                                a [attr.href (ctx.Link About); attr.style Styles.navLink] [text "About Us"]
                            ]
                        ]

                        div [attr.style Styles.mainS] [content]

                        footer [attr.style Styles.footer] [
                            text "© 2025 Sudoku App – WebSharper F# project"
                        ]
                    ]
                ]
            )

        let renderBoard (board: int[,]) =
            table [attr.style Styles.sudokuTable] [
                for r in 0 .. 8 do
                    tr [] [
                        for c in 0 .. 8 do

                            let rightThick = (c = 2 || c = 5)
                            let bottomThick = (r = 2 || r = 5)

                            let cellStyle =
                                match rightThick, bottomThick with
                                | true, true -> Styles.sudokuCellBothThick
                                | true, false -> Styles.sudokuCellRightThick
                                | false, true -> Styles.sudokuCellBottomThick
                                | false, false -> Styles.sudokuCell

                            let cellContent =
                                if board.[r, c] = 0 then
                                    input [
                                        attr.name "type"
                                        attr.value ""
                                        attr.maxlength "1"
                                        attr.style Styles.sudokuInput
                                    ] []
                                else
                                    span [attr.style Styles.sudokuText] [
                                        text (string board.[r, c])
                                    ]

                            td [attr.style cellStyle] [cellContent]
                    ]
            ]

        let homePage ctx =
            layout ctx "Home" (
                div [attr.style Styles.card] [
                    h2 [attr.style Styles.h2s] [text "Welcome to Sudoku!"]
                    p [] [text "Ready to test your logic skills?"]
                    a [attr.href (ctx.Link (Game None)); attr.style Styles.button] [text "Start Game"]
                ]
            )

        let gamePage ctx diff =
            let board = Sudoku.generate diff

            let diffText =
                match diff with
                | Some Easy -> "Easy"
                | Some Medium -> "Medium"
                | Some Hard -> "Hard"
                | None -> "Medium"

            layout ctx "Game" (
                div [attr.style Styles.card] [
                    h2 [attr.style Styles.h2s] [text (sprintf "Enjoy the game! (%s)" diffText)]
                    
                    div [attr.style "margin-bottom: 20px; display: flex; gap: 10px; justify-content: center;"] [
                        a [attr.href (ctx.Link (Game (Some Easy))); attr.style Styles.button] [text "Easy"]
                        a [attr.href (ctx.Link (Game (Some Medium))); attr.style Styles.button] [text "Medium"]
                        a [attr.href (ctx.Link (Game (Some Hard))); attr.style Styles.button] [text "Hard"]
                    ]

                    renderBoard board
                ]
            )

        let aboutPage ctx =
            layout ctx "About Us" (
                div [attr.style Styles.card] [
                    h2 [attr.style Styles.h2s] [text "About Us"]
                    p [] [text "This is an automatically generated Sudoku game."]
                ]
            )



    [<Website>]
    let Main =
        Application.MultiPage (fun (ctx: Context<EndPoint>) endpoint ->
            match endpoint with
            | Home -> View.homePage ctx
            | Game diff -> View.gamePage ctx diff
            | About -> View.aboutPage ctx
        )