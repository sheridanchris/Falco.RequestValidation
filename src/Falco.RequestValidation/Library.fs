namespace Falco

open Falco

[<RequireQualifiedAccess>]
module Request =
    let private validate
        (validator: 'value -> Result<'success, 'validationErrors>)
        (onSuccess: 'success -> HttpHandler)
        (onValidationErrors: 'validationErrors -> HttpHandler)
        (value: 'value)
        =
        match validator value with
        | Ok result -> onSuccess result
        | Error validationErrors -> onValidationErrors validationErrors

    let mapValidateJson
        (validator: 'record -> Result<'success, 'validationErrors>)
        (onSuccess: 'success -> HttpHandler)
        (onValidationErrors: 'validationErrors -> HttpHandler)
        : HttpHandler =
        Request.mapJson (validate validator onSuccess onValidationErrors)

    let mapValidateRoute
        (reader: RouteCollectionReader -> 'value)
        (validator: 'value -> Result<'success, 'validationErrors>)
        (onSuccess: 'success -> HttpHandler)
        (onValidationErrors: 'validationErrors -> HttpHandler)
        : HttpHandler =
        Request.mapRoute reader (validate validator onSuccess onValidationErrors)

    let mapValidateQuery
        (reader: QueryCollectionReader -> 'value)
        (validator: 'value -> Result<'success, 'validationErrors>)
        (onSuccess: 'success -> HttpHandler)
        (onValidationErrors: 'validationErrors -> HttpHandler)
        : HttpHandler =
        Request.mapQuery reader (validate validator onSuccess onValidationErrors)
