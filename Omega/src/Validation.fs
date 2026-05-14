module Omega.Validation

open System
open Omega.Types

let private titleMaxLen = 100
let private descriptionMaxLen = 500

let private trim (s: string) = if isNull s then "" else s.Trim()

let validateTitle (raw: string) : string option =
    let t = trim raw

    if String.IsNullOrWhiteSpace t then
        Some "A cím kötelező mező."
    elif t.Length > titleMaxLen then
        Some(sprintf "A cím legfeljebb %d karakter lehet." titleMaxLen)
    else
        None

let validateDescription (raw: string) : string option =
    let t = if isNull raw then "" else raw

    if t.Length > descriptionMaxLen then
        Some(sprintf "A leírás legfeljebb %d karakter lehet." descriptionMaxLen)
    else
        None

let tryParseDueDate (raw: string) : Result<DateTime option, string> =
    let t = trim raw

    if String.IsNullOrEmpty t then
        Ok None
    else
        match
            DateTime.TryParse(
                t,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeLocal
            )
        with
        | true, d -> Ok(Some(d.Date))
        | _ -> Error "Érvénytelen dátum formátum (várható: ÉÉÉÉ-HH-NN)."

let validateDueDate (raw: string) : string option =
    match tryParseDueDate raw with
    | Ok _ -> None
    | Error msg -> Some msg

let validate (form: FormData) : FormErrors =
    { Title = validateTitle form.Title
      DueDate = validateDueDate form.DueDate }

let isValid (form: FormData) : bool = validate form |> FormErrors.hasAny |> not

let toTask (now: DateTime) (id: TaskId) (form: FormData) : Result<StudentTask, FormErrors> =
    let errs = validate form

    if FormErrors.hasAny errs then
        Error errs
    else
        let due =
            match tryParseDueDate form.DueDate with
            | Ok v -> v
            | Error _ -> None

        Ok
            { Id = id
              Title = trim form.Title
              Description = trim form.Description
              DueDate = due
              Priority = form.Priority
              Status = form.Status
              CreatedAt = now
              UpdatedAt = now }

let updateExisting (now: DateTime) (existing: StudentTask) (form: FormData) : Result<StudentTask, FormErrors> =
    let errs = validate form

    if FormErrors.hasAny errs then
        Error errs
    else
        let due =
            match tryParseDueDate form.DueDate with
            | Ok v -> v
            | Error _ -> None

        Ok
            { existing with
                Title = trim form.Title
                Description = trim form.Description
                DueDate = due
                Priority = form.Priority
                Status = form.Status
                UpdatedAt = now }
