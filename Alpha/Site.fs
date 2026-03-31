namespace Alpha

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Server
open WebSharper.UI.Html
open WebSharper.UI.Client

module Site =

    // ── Végpontok ────────────────────────────────────────────────────────────────
    
    type EndPoint =
        | [<EndPoint "GET /">]       Home
        | [<EndPoint "GET /about">]  About
    
    // ── Sudoku logika ─────────────────────────────────────────────────────────────
    
    module Sudoku =
    
        let isValid (board: int[,]) row col num =
            let rowOk = seq { 0..8 } |> Seq.forall (fun c -> board.[row, c] <> num)
            let colOk = seq { 0..8 } |> Seq.forall (fun r -> board.[r, col] <> num)
            let br, bc = (row / 3) * 3, (col / 3) * 3
            let boxOk =
                seq { for r in br .. br+2 do for c in bc .. bc+2 -> board.[r, c] }
                |> Seq.forall (fun v -> v <> num)
            rowOk && colOk && boxOk
    
        let rec private solve (board: int[,]) =
            let mutable found  = false
            let mutable result = false
            let mutable ri, ci = 0, 0
            let mutable stop   = false
            for r in 0..8 do
                for c in 0..8 do
                    if not stop && board.[r, c] = 0 then
                        ri <- r; ci <- c; found <- true; stop <- true
            if not found then true
            else
                let mutable num = 1
                while num <= 9 && not result do
                    if isValid board ri ci num then
                        board.[ri, ci] <- num
                        if solve board then result <- true
                        else board.[ri, ci] <- 0
                    num <- num + 1
                result
    
        let generateFull () =
            let rng   = System.Random()
            let board = Array2D.zeroCreate<int> 9 9
            let nums  = [| 1..9 |] |> Array.sortBy (fun _ -> rng.Next())
            for c in 0..8 do board.[0, c] <- nums.[c]
            solve board |> ignore
            board
    
        let removeClues (board: int[,]) =
            let rng    = System.Random()
            let puzzle = Array2D.copy board
            let remove = 45 
            let positions = [| for r in 0..8 do for c in 0..8 -> (r, c) |]
            let shuffled  = positions |> Array.sortBy (fun _ -> rng.Next())
            for i in 0 .. remove - 1 do
                let (r, c) = shuffled.[i]
                puzzle.[r, c] <- 0
            puzzle
    
        let generate () =
            removeClues (generateFull ())
    
    // ── CSS ──────────────────────────────────────────────────────────────────────
    
    [<JavaScript>]
    module Styles =
        let page      = "font-family:'Segoe UI',sans-serif;background:#f0f4f8;min-height:100vh;margin:0;padding:0;"
        let header    = "background:#2d3a8c;color:white;padding:18px 32px;display:flex;align-items:center;justify-content:space-between;"
        let titleS    = "margin:0;font-size:1.8rem;letter-spacing:2px;"
        let nav       = "display:flex;gap:16px;"
        let navLink   = "color:#aec6ff;text-decoration:none;font-size:1rem;"
        let mainS     = "max-width:700px;margin:40px auto;padding:0 16px;text-align:center;"
        let card      = "background:white;border-radius:12px;box-shadow:0 2px 12px rgba(0,0,0,0.10);padding:32px;margin-bottom:24px;display:inline-block;"
        let h2s       = "color:#2d3a8c;margin-top:0;"
        let footer    = "text-align:center;color:#90a4ae;padding:24px;font-size:0.9rem;"
        
        let sudokuTable = "border-collapse:collapse;margin:24px auto;border:3px solid #2d3a8c;background:#fff;"
        let sudokuCell  = "width:45px;height:45px;text-align:center;font-size:1.4rem;border:1px solid #ccd1d9;padding:0;"
        let sudokuCellRightThick = sudokuCell + "border-right:3px solid #2d3a8c;"
        let sudokuCellBottomThick = sudokuCell + "border-bottom:3px solid #2d3a8c;"
        let sudokuCellBothThick = sudokuCell + "border-right:3px solid #2d3a8c;border-bottom:3px solid #2d3a8c;"
        let sudokuInput = "width:100%;height:100%;border:none;text-align:center;font-size:1.4rem;color:#2d3a8c;background:#e8eaf6;outline:none;font-weight:bold;padding:0;box-sizing:border-box;"
        let sudokuText  = "display:flex;align-items:center;justify-content:center;width:100%;height:100%;font-weight:bold;color:#333;"

        let globalCss = "
            .sudoku-err { background-color: #ffcccc !important; }
            .sudoku-err input { background-color: #ffcccc !important; color: #d32f2f !important; }
            .sudoku-err span { color: #d32f2f !important; }
        "

    // ── Kliens oldali logika (Interaktív játéktér) ───────────────────────────────

    [<JavaScript>]
    module Client =
        // Egydimenziós (lapos) tömböt használunk az indexeléshez, így nem fagy ki a JavaScript.
        let hasConflict (idx: int) (boardState: string[]) =
            let v = boardState.[idx]
            if System.String.IsNullOrWhiteSpace(v) then false
            else
                let r = idx / 9
                let c = idx % 9
                let mutable conflict = false
                
                // Sor ellenőrzés
                for cc in 0..8 do
                    let i = r * 9 + cc
                    if i <> idx && boardState.[i] = v then conflict <- true
                // Oszlop ellenőrzés
                for rr in 0..8 do
                    let i = rr * 9 + c
                    if i <> idx && boardState.[i] = v then conflict <- true
                // 3x3-as doboz ellenőrzés
                let br = (r / 3) * 3
                let bc = (c / 3) * 3
                for rr in br .. br+2 do
                    for cc in bc .. bc+2 do
                        let i = rr * 9 + cc
                        if i <> idx && boardState.[i] = v then conflict <- true
                
                conflict

        let renderInteractiveBoard (flatBoard: int[]) =
            // Létrehozunk 81 reaktív változót egy sima egydimenziós tömbben
            let vars = 
                flatBoard |> Array.map (fun v -> 
                    Var.Create (if v = 0 then "" else string v)
                )

            // Összefűzzük őket egyetlen 'View'-ba, ami azonnal jelez, ha BÁRMI változik
            let allVarsView = 
                vars 
                |> Array.map (fun v -> v.View)
                |> View.Sequence
                |> View.Map Seq.toArray

            table [attr.style Styles.sudokuTable] [
                for r in 0 .. 8 do
                    tr [] [
                        for c in 0 .. 8 do
                            let idx = r * 9 + c
                            let rightThick = (c = 2 || c = 5)
                            let bottomThick = (r = 2 || r = 5)
                            
                            let cellStyle =
                                match rightThick, bottomThick with
                                | true, true  -> Styles.sudokuCellBothThick
                                | true, false -> Styles.sudokuCellRightThick
                                | false, true -> Styles.sudokuCellBottomThick
                                | false, false -> Styles.sudokuCell
                            
                            let isInitial = flatBoard.[idx] <> 0
                            
                            // Reakció: ha ütközés van a "lapos" tömbben, jelezzük
                            let dynClass =
                                allVarsView |> View.Map (fun currentBoard ->
                                    if hasConflict idx currentBoard then "sudoku-err" else ""
                                )
                            
                            let cellContent =
                                if isInitial then
                                    span [attr.style Styles.sudokuText] [text (string flatBoard.[idx])]
                                else
                                    Doc.InputType.Text [
                                        attr.maxlength "1"
                                        attr.style Styles.sudokuInput
                                    ] vars.[idx]

                            td [attr.style cellStyle; attr.classDyn dynClass] [cellContent]
                    ]
            ]

    // ── View (Szerver oldali felület) ────────────────────────────────────────────
    
    module View =
    
        let private layout (ctx: Context<EndPoint>) (pageTitle: string) (content: Doc) =
            Content.Page(
                Title = "Sudoku – " + pageTitle,
                Body = [
                    Doc.Verbatim ("<style>\n" + Styles.globalCss + "\n</style>")

                    div [attr.style Styles.page] [
                        header [attr.style Styles.header] [
                            h1 [attr.style Styles.titleS] [text "🔢 Sudoku"]
                            nav [attr.style Styles.nav] [
                                a [attr.href (ctx.Link Home);  attr.style Styles.navLink] [text "Játék"]
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
    
        let homePage ctx =
            let board = Sudoku.generate ()
            
            // TITKOS FEGYVER: A 9x9-es táblát 1 dimenzióssá lapítjuk (81 elem hosszú lesz).
            // Az int[] formátumot a JavaScript sosem értelmezi félre!
            let flatBoard = [| for r in 0..8 do for c in 0..8 -> board.[r, c] |]

            layout ctx "Játék" (
                div [attr.style Styles.card] [
                    h2 [attr.style Styles.h2s] [text "Jó játékot!"]
                    
                    ClientServer.client (Client.renderInteractiveBoard flatBoard)
                ]
            )
    
        let aboutPage ctx =
            layout ctx "Rólunk" (
                div [attr.style Styles.card] [
                    h2 [attr.style Styles.h2s] [text "Rólunk"]
                    p [] [text "Ez egy automatikusan generálódó, WebSharper UI-alapú reaktív Sudoku játék."]
                ]
            )
            
    // ── Entry ────────────────────────────────────────────────────────────────────

    [<Website>]
    let Main =
        Application.MultiPage (fun (ctx: Context<EndPoint>) endpoint ->
            match endpoint with
            | Home  -> View.homePage ctx
            | About -> View.aboutPage ctx
        )