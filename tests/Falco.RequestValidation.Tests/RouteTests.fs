module RouteTests

open System
open Expecto
open Falco
open Common
open Microsoft.AspNetCore.Routing

type ValidatedName = ValidatedName of string
type RequestBody = { Name: string }
type ValidatedRequestBody = { Name: ValidatedName }

let reader (reader: RouteCollectionReader) : RequestBody =
    let name = reader.GetString "name"
    { Name = name }

let validator (requestBody: RequestBody) : Result<ValidatedRequestBody, string> =
    if requestBody.Name.Length < 5 then
        Error "Name cannot be less than 5 characters long"
    else
        Ok { Name = ValidatedName requestBody.Name }

[<Tests>]
let requestTests =
    testList
        "Route tests"
        [ testCaseTask "Successful validation test"
          <| task {
              let httpContext = getHttpContextWriteable ()

              let name = "Christian"
              httpContext.Request.RouteValues <- RouteValueDictionary({| name = name |})

              let onSuccess (success: ValidatedRequestBody) : HttpHandler =
                  fun _ ->
                      task {
                          let (ValidatedName value) = success.Name
                          Expect.equal value name "RequestBody.Name should equal ValidatedRequestBody.Name"
                      }

              let onFailure (error: string) : HttpHandler =
                  failtest $"Request validation failed where it shouldn't have. Error: {error}"

              do! Request.mapValidateRoute reader validator onSuccess onFailure httpContext
          }

          testCaseTask "Validation failure test"
          <| task {
              let httpContext = getHttpContextWriteable ()
              let name = "test"
              httpContext.Request.RouteValues <- RouteValueDictionary({| name = name |})

              let onSuccess (validatedRequestBody: ValidatedRequestBody) : HttpHandler =
                  failtest
                      $"Request validation succeeded where it shouldn't have. Validated request body: %A{validatedRequestBody}"

              let onFailure (error: string) : HttpHandler =
                  fun _ -> task { Expect.equal error "Name cannot be less than 5 characters long" "Errors should be equal." }

              do! Request.mapValidateRoute reader validator onSuccess onFailure httpContext
          } ]
