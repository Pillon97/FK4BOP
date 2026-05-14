module Omega.State

open System
open Elmish
open Feliz.Router
open Omega.Types
open Omega.Validation
open Omega.Storage

let parseUrl (segments: string list) : Page =
    match segments with
    | []
    | [ "" ] -> ListPage
    | [ "new" ] -> NewPage
    | [ "edit"; id ] ->
        match TaskId.tryParse id with
        | Some tid -> EditPage tid
        | None -> NotFoundPage
    | _ -> NotFoundPage

let pageUrl =
    function
    | ListPage -> Router.format ("")
    | NewPage -> Router.format ("new")
    | EditPage(TaskId id) -> Router.format ("edit", id.ToString())
    | NotFoundPage -> Router.format ("")

let private findTask (id: TaskId) (tasks: StudentTask list) =
    tasks |> List.tryFind (fun t -> t.Id = id)

let private formForPage (page: Page) (tasks: StudentTask list) : FormData =
    match page with
    | EditPage id ->
        match findTask id tasks with
        | Some t -> FormData.ofTask t
        | None -> FormData.empty
    | _ -> FormData.empty

let init () : Model * Cmd<Msg> =
    let initialPage = parseUrl (Router.currentUrl ())
    let theme = loadTheme ()

    let model =
        { Tasks = []
          CurrentPage = initialPage
          Filter = All
          Sort = ByDueDate
          SearchText = ""
          Form = FormData.empty
          FormErrors = FormErrors.empty
          Theme = theme
          Loaded = false }

    let loadCmd = Cmd.OfFunc.perform loadTasks () LoadedFromStorage
    model, loadCmd

let private updateForm (model: Model) (f: FormData -> FormData) : Model =
    { model with
        Form = f model.Form
        FormErrors = FormErrors.empty }

let private withSavedTasks (tasks: StudentTask list) : Cmd<Msg> =
    Cmd.OfFunc.attempt saveTasks tasks (fun _ -> ResetForm)

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | UrlChanged segments ->
        let page = parseUrl segments
        let form = formForPage page model.Tasks

        { model with
            CurrentPage = page
            Form = form
            FormErrors = FormErrors.empty },
        Cmd.none

    | LoadedFromStorage tasks ->
        let form = formForPage model.CurrentPage tasks

        { model with
            Tasks = tasks
            Form = form
            Loaded = true },
        Cmd.none

    | SetTitle v -> updateForm model (fun f -> { f with Title = v }), Cmd.none

    | SetDescription v -> updateForm model (fun f -> { f with Description = v }), Cmd.none

    | SetDueDate v -> updateForm model (fun f -> { f with DueDate = v }), Cmd.none

    | SetPriority p -> updateForm model (fun f -> { f with Priority = p }), Cmd.none

    | SetStatus s -> updateForm model (fun f -> { f with Status = s }), Cmd.none

    | SubmitForm ->
        let now = DateTime.Now

        match model.CurrentPage with
        | NewPage ->
            match toTask now (TaskId.create ()) model.Form with
            | Ok task ->
                let updated = task :: model.Tasks
                let saveCmd = Cmd.OfFunc.attempt saveTasks updated (fun _ -> ResetForm)
                let navCmd = Cmd.navigate ("")

                { model with
                    Tasks = updated
                    Form = FormData.empty
                    FormErrors = FormErrors.empty },
                Cmd.batch [ saveCmd; navCmd ]
            | Error errs -> { model with FormErrors = errs }, Cmd.none

        | EditPage id ->
            match findTask id model.Tasks with
            | Some existing ->
                match updateExisting now existing model.Form with
                | Ok updatedTask ->
                    let updatedTasks =
                        model.Tasks
                        |> List.map (fun t -> if t.Id = id then updatedTask else t)

                    let saveCmd = Cmd.OfFunc.attempt saveTasks updatedTasks (fun _ -> ResetForm)
                    let navCmd = Cmd.navigate ("")

                    { model with
                        Tasks = updatedTasks
                        FormErrors = FormErrors.empty },
                    Cmd.batch [ saveCmd; navCmd ]
                | Error errs -> { model with FormErrors = errs }, Cmd.none
            | None -> model, Cmd.navigate ("")

        | _ -> model, Cmd.none

    | ResetForm ->
        { model with
            Form = FormData.empty
            FormErrors = FormErrors.empty },
        Cmd.none

    | DeleteTask id ->
        let updated = model.Tasks |> List.filter (fun t -> t.Id <> id)
        { model with Tasks = updated }, withSavedTasks updated

    | ToggleStatus id ->
        let now = DateTime.Now

        let updated =
            model.Tasks
            |> List.map (fun t ->
                if t.Id = id then
                    let nextStatus =
                        match t.Status with
                        | Done -> Todo
                        | _ -> Done

                    { t with
                        Status = nextStatus
                        UpdatedAt = now }
                else
                    t)

        { model with Tasks = updated }, withSavedTasks updated

    | CycleStatus id ->
        let now = DateTime.Now

        let updated =
            model.Tasks
            |> List.map (fun t ->
                if t.Id = id then
                    { t with
                        Status = Status.next t.Status
                        UpdatedAt = now }
                else
                    t)

        { model with Tasks = updated }, withSavedTasks updated

    | SetFilter f -> { model with Filter = f }, Cmd.none

    | SetSort s -> { model with Sort = s }, Cmd.none

    | SetSearch s -> { model with SearchText = s }, Cmd.none

    | ToggleTheme ->
        let nextTheme = Theme.toggle model.Theme

        { model with Theme = nextTheme }, Cmd.OfFunc.attempt saveTheme nextTheme (fun _ -> ResetForm)

let private matchesSearch (search: string) (t: StudentTask) =
    if String.IsNullOrWhiteSpace search then
        true
    else
        let needle = search.Trim().ToLowerInvariant()

        t.Title.ToLowerInvariant().Contains(needle)
        || t.Description.ToLowerInvariant().Contains(needle)

let private matchesFilter (filter: StatusFilter) (t: StudentTask) =
    match filter with
    | All -> true
    | OnlyStatus s -> t.Status = s

let private sortKey (sort: SortBy) (t: StudentTask) : IComparable =
    match sort with
    | ByDueDate ->
        match t.DueDate with
        | Some d -> d :> IComparable
        | None -> DateTime.MaxValue :> IComparable
    | ByPriority -> Priority.toOrder t.Priority :> IComparable
    | ByCreated -> -(t.CreatedAt.Ticks) :> IComparable

let visibleTasks (model: Model) : StudentTask list =
    model.Tasks
    |> List.filter (matchesFilter model.Filter)
    |> List.filter (matchesSearch model.SearchText)
    |> List.sortBy (sortKey model.Sort)

let counts (tasks: StudentTask list) =
    let total = List.length tasks
    let todo = tasks |> List.filter (fun t -> t.Status = Todo) |> List.length
    let inProgress = tasks |> List.filter (fun t -> t.Status = InProgress) |> List.length
    let doneCnt = tasks |> List.filter (fun t -> t.Status = Done) |> List.length

    {| Total = total
       Todo = todo
       InProgress = inProgress
       Done = doneCnt |}
