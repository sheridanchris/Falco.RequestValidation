open System
open Falco

type BudgetName = private BudgetName of string
type PositiveDecimal = private PositiveDecimal of decimal

[<RequireQualifiedAccess>]
module BudgetName =
    let value (BudgetName name) = name

    let create (name: string) : Result<BudgetName, string> =
        if String.IsNullOrWhiteSpace name then
            Error "Budget name can't be null or empty"
        elif name.Length > 150 then
            Error "Budget can't be greater than 150 characters long"
        else
            Ok(BudgetName name)

[<RequireQualifiedAccess>]
module PositiveDecimal =
    let value (PositiveDecimal decimal) = decimal

    let create (value: decimal) : Result<PositiveDecimal, string> =
        if value >= 0m then
            Ok(PositiveDecimal value)
        else
            Error "Value cannot be negative"

module CreateBudget =
    type CreateBudgetRequest =
        { Name: string; MonthlyIncome: decimal }

    type ValidatedCreateBudgetRequest =
        { Name: BudgetName
          MonthlyIncome: PositiveDecimal }

    let validateRequest (createBudgetRequest: CreateBudgetRequest) : Result<ValidatedCreateBudgetRequest, string list> =
        let budgetNameResult = BudgetName.create createBudgetRequest.Name
        let monthlyIncomeResult = PositiveDecimal.create createBudgetRequest.MonthlyIncome

        match budgetNameResult, monthlyIncomeResult with
        | Ok budgetName, Ok monthlyIncome ->
            Ok
                { Name = budgetName
                  MonthlyIncome = monthlyIncome }
        | Error budgetNameError, Error monthlyIncomeError -> Error [ budgetNameError; monthlyIncomeError ]
        | Error error, _
        | _, Error error -> Error [ error ]

    let handler: HttpHandler =
        let createBudget (request: ValidatedCreateBudgetRequest) : HttpHandler =
            fun ctx ->
                task {
                    // ...
                    return ()
                }

        let handleValidationErrors (errors: string list) = Response.ofJson {| errors = errors |}
        Request.mapValidateJson validateRequest createBudget handleValidationErrors
