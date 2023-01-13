open System
open Falco
open Falco.RequestValidation
open FsToolkit.ErrorHandling

type BudgetName =
    private
    | BudgetName of string

    static member TryCreate(value: string) =
        if String.IsNullOrWhiteSpace value then
            Error "Budget name shouldn't be empty."
        elif value.Length > 150 then
            Error "Budget name length shouldn't be greater than 150"
        else
            Ok(BudgetName value)

type PositiveDecimal =
    private
    | PositiveDecimal of decimal

    static member TryCreate(value: decimal) =
        if value < 0m then
            Error "Value shouldn't be negative"
        else
            Ok(PositiveDecimal value)

module CreateBudget =
    type CreateBudgetRequest =
        { Name: string; MonthlyIncome: decimal }

    type ValidatedCreateBudgetRequest =
        { Name: BudgetName
          MonthlyIncome: PositiveDecimal }

    let validateRequest (createBudgetRequest: CreateBudgetRequest) =
        validation {
            let! budgetName = Result.tryCreate "Budget name" createBudgetRequest.Name
            and! monthlyIncome = Result.tryCreate "Monthly income" createBudgetRequest.MonthlyIncome

            return
                { Name = budgetName
                  MonthlyIncome = monthlyIncome }
        }

    let handler: HttpHandler =
        let createBudget (request: ValidatedCreateBudgetRequest) : HttpHandler =
            fun ctx ->
                task {
                    // ...
                    return ()
                }

        let handleValidationErrors errors =
            Response.ofJson {| errors = Map.ofList errors |}

        Request.mapValidateJson validateRequest createBudget handleValidationErrors
