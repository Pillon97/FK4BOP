namespace Alpha

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Server
open WebSharper.UI.Html

module Site =

    // ── Végpontok ────────────────────────────────────────────────────────────────
    
    type EndPoint =
        | [<EndPoint "GET /">] Home
        | [<EndPoint "GET /about">] About

    // ── Sudoku logika ────────────────────────────────────────────────────────────
    
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

        let removeClues (board: int[,]) =
            let rng = System.Random()
            let puzzle = Array2D.copy board
            let remove = 45

            let positions =
                [| for r in 0..8 do
                       for c in 0..8 -> (r, c) |]

            let shuffled = positions |> Array.sortBy (fun _ -> rng.Next())

            for i in 0 .. remove - 1 do
                let (r, c) = shuffled.[i]
                puzzle.[r, c] <- 0

            puzzle

        let generate () =
            removeClues (generateFull ())

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

    // ── CSS ──────────────────────────────────────────────────────────────────────
    
    module Styles =
        let page =
            "font-family:'Segoe UI',sans-serif;background:#f0f4f8;min-height:100vh;margin:0;padding:0;"

        let header =
            "background:#2d3a8c;color:white;padding:18px 32px;display:flex;align-items:center;justify-content:space-between;"

        let titleS = "margin:0;font-size:1.8rem;letter-spacing:2px;"
        let nav = "display:flex;gap:16px;"
        let navLink = "color:#aec6ff;text-decoration:none;font-size:1rem;"
        let mainS = "max-width:700px;margin:40px auto;padding:0 16px;text-align:center;"
        let card = "background:white;border-radius:12px;box-shadow:0 2px 12px rgba(0,0,0,0.10);padding:32px;margin-bottom:24px;display:inline-block;"
        let h2s = "color:#2d3a8c;margin-top:0;"
        let footer = "text-align:center;color:#90a4ae;padding:24px;font-size:0.9rem;"

        // Sudoku CSS
        let sudokuTable =
            "border-collapse:collapse;margin:24px auto;border:3px solid #2d3a8c;background:#fff;"

        let sudokuCell =
            "width:45px;height:45px;text-align:center;font-size:1.4rem;border:1px solid #ccd1d9;padding:0;"

        let sudokuCellRightThick =
            sudokuCell + "border-right:3px solid #2d3a8c;"

        let sudokuCellBottomThick =
            sudokuCell + "border-bottom:3px solid #2d3a8c;"

        let sudokuCellBothThick =
            sudokuCell + "border-right:3px solid #2d3a8c;border-bottom:3px solid #2d3a8c;"

        let sudokuInput =
            "width:100%;height:100%;border:none;text-align:center;font-size:1.4rem;color:#2d3a8c;background:#e8eaf6;outline:none;font-weight:bold;padding:0;box-sizing:border-box;"

        let sudokuText =
            "display:flex;align-items:center;justify-content:center;width:100%;height:100%;font-weight:bold;color:#333;"

    // ── View ─────────────────────────────────────────────────────────────────────
    
    module View =

        let private layout (ctx: Context<EndPoint>) (pageTitle: string) (content: Doc) =
            Content.Page(
                Title = "Sudoku – " + pageTitle,
                Body = [
                    div [attr.style Styles.page] [
                        header [attr.style Styles.header] [
                            h1 [attr.style Styles.titleS] [text "🔢 Sudoku"]

                            nav [attr.style Styles.nav] [
                                a [attr.href (ctx.Link Home); attr.style Styles.navLink] [text "Játék"]
                                a [attr.href (ctx.Link About); attr.style Styles.navLink] [text "Rólunk"]
                            ]
                        ]

                        div [attr.style Styles.mainS] [content]

                        footer [attr.style Styles.footer] [
                            text "© 2025 Sudoku App – WebSharper F# projekt"
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
            let board = Sudoku.generate ()

            layout ctx "Játék" (
                div [attr.style Styles.card] [
                    h2 [attr.style Styles.h2s] [text "Jó játékot!"]
                    renderBoard board
                ]
            )

        let aboutPage ctx =
            layout ctx "Rólunk" (
                div [attr.style Styles.card] [
                    h2 [attr.style Styles.h2s] [text "Rólunk"]
                    p [] [text "Ez egy automatikusan generálódó Sudoku játék."]
                ]
            )

    // ── Entry ────────────────────────────────────────────────────────────────────

    [<Website>]
    let Main =
        Application.MultiPage (fun (ctx: Context<EndPoint>) endpoint ->
            match endpoint with
            | Home -> View.homePage ctx
            | About -> View.aboutPage ctx
        )