namespace Falco.RequestValidation

open Falco

[<RequireQualifiedAccess>]
module Request =
    let mapValidateJson
        (validator: 'a -> Result<'b, Map<string, string list>>)
        (onSuccess: 'b -> HttpHandler)
        (onValidationErrors: Map<string, string list> -> HttpHandler)
        =
        let handleOk (record: 'a) : HttpHandler =
            match validator record with
            | Ok result -> onSuccess result
            | Error validationErrors -> onValidationErrors validationErrors

        Request.mapJson handleOk
