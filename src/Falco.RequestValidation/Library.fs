namespace Falco.RequestValidation

open Falco

[<RequireQualifiedAccess>]
module Request =
    let mapValidateJson
        (validator: 'record -> Result<'success, 'validationErrors>)
        (onSuccess: 'success -> HttpHandler)
        (onValidationErrors: 'validationErrors -> HttpHandler)
        =
        let handleOk (record: 'record) : HttpHandler =
            match validator record with
            | Ok result -> onSuccess result
            | Error validationErrors -> onValidationErrors validationErrors

        Request.mapJson handleOk
