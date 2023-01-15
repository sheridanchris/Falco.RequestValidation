module RequestTests

open System
open System.IO
open System.Text
open System.Text.Json
open Expecto
open Falco
open NSubstitute

type NonEmptyString = NonEmptyString of string
type RequestBody = { Name: string }
type ValidatedRequestBody = { Name: NonEmptyString }

let getMemoryStream (requestBody: RequestBody) =
    let jsonBody = JsonSerializer.Serialize(requestBody)
    new MemoryStream(Encoding.UTF8.GetBytes(jsonBody))

let validator (requestBody: RequestBody) : Result<ValidatedRequestBody, string> =
    if String.IsNullOrWhiteSpace requestBody.Name then
        Error "Name cannot be null or whitespace"
    else
        Ok { Name = NonEmptyString requestBody.Name }

[<Tests>]
let requestTests =
    testList
        "Json body tests"
        [ testCaseTask "Successful validation test"
          <| task {
              let httpContext = getHttpContextWriteable ()

              let body: RequestBody = { Name = "Christian" }
              use ms = getMemoryStream body
              httpContext.Request.Body.Returns(ms) |> ignore

              let onSuccess (success: ValidatedRequestBody) : HttpHandler =
                  fun _ ->
                      task {
                          let (NonEmptyString value) = success.Name
                          Expect.equal value body.Name "RequestBody.Name should equal ValidatedRequestBody.Name"
                      }

              let onFailure (error: string) : HttpHandler =
                  failtest $"Request validation failed where it shouldn't have. Error: {error}"

              do! Request.mapValidateJson validator onSuccess onFailure httpContext
          }

          testCaseTask "Validation failure test"
          <| task {
              let httpContext = getHttpContextWriteable ()

              let body: RequestBody = { Name = "" }
              use ms = getMemoryStream body
              httpContext.Request.Body.Returns(ms) |> ignore

              let onSuccess (validatedRequestBody: ValidatedRequestBody) : HttpHandler =
                  failtest
                      $"Request validation succeeded where it shouldn't have. Validated request body: %A{validatedRequestBody}"

              let onFailure (error: string) : HttpHandler =
                  fun _ -> task { Expect.equal error "Name cannot be null or whitespace" "Errors should be equal." }

              do! Request.mapValidateJson validator onSuccess onFailure httpContext
          } ]
