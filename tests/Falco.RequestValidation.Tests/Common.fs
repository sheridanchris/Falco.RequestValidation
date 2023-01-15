[<AutoOpen>]
module Common

open System.IO
open System.IO.Pipelines
open Microsoft.AspNetCore.Http
open NSubstitute
open Expecto

// Copied from Falco -- https://github.com/pimbrouwers/Falco/blob/master/test/Falco.Tests/Common.fs
let getHttpContextWriteable () =
    let ctx = Substitute.For<HttpContext>()

    let req = Substitute.For<HttpRequest>()
    req.Headers.Returns(Substitute.For<HeaderDictionary>()) |> ignore

    let resp = Substitute.For<HttpResponse>()
    let respBody = new MemoryStream()

    resp.Headers.Returns(Substitute.For<HeaderDictionary>()) |> ignore
    resp.BodyWriter.Returns(PipeWriter.Create(respBody)) |> ignore
    resp.Body <- respBody
    resp.StatusCode <- 200

    ctx.Request.Returns(req) |> ignore
    ctx.Response.Returns(resp) |> ignore

    ctx

let testCaseTask name task =
    testCaseAsync name (task |> Async.AwaitTask)
