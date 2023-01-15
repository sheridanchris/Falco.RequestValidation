module QueryTests

open System.Collections.Generic
open Expecto
open Common
open Falco
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives

type ValidatedName = ValidatedName of string
type RequestBody = { Name: string }
type ValidatedRequestBody = { Name: ValidatedName }

let reader (reader: QueryCollectionReader) : RequestBody =
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
        "Query tests"
        [ testCaseTask "Successful validation test"
          <| task {
              let httpContext = getHttpContextWriteable ()

              let name = "Christian"
              let query = Dictionary<string, StringValues>()
              query.Add("name", StringValues(name))
              httpContext.Request.Query <- QueryCollection(query)

              let onSuccess (success: ValidatedRequestBody) : HttpHandler =
                  fun _ ->
                      task {
                          let (ValidatedName value) = success.Name
                          Expect.equal value name "RequestBody.Name should equal ValidatedRequestBody.Name"
                      }

              let onFailure (error: string) : HttpHandler =
                  failtest $"Request validation failed where it shouldn't have. Error: {error}"

              do! Request.mapValidateQuery reader validator onSuccess onFailure httpContext
          }

          testCaseTask "Validation failure test"
          <| task {
              let httpContext = getHttpContextWriteable ()

              let name = "test"
              let query = Dictionary<string, StringValues>()
              query.Add("name", StringValues(name))
              httpContext.Request.Query <- QueryCollection(query)

              let onSuccess (validatedRequestBody: ValidatedRequestBody) : HttpHandler =
                  failtest
                      $"Request validation succeeded where it shouldn't have. Validated request body: %A{validatedRequestBody}"

              let onFailure (error: string) : HttpHandler =
                  fun _ ->
                      task { Expect.equal error "Name cannot be less than 5 characters long" "Errors should be equal." }

              do! Request.mapValidateQuery reader validator onSuccess onFailure httpContext
          } ]
