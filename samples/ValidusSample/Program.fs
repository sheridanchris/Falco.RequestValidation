open Falco
open Validus
open Validus.Operators

type BudgetName = private BudgetName of string
type PositiveDecimal = private PositiveDecimal of decimal

[<RequireQualifiedAccess>]
module BudgetName =
    let value (BudgetName name) = name

    let create name =
        let validator = Check.String.notEmpty <+> Check.String.lessThanLen 150

        validate {
            let! name = validator "Budget name" name
            return BudgetName name
        }

[<RequireQualifiedAccess>]
module PositiveDecimal =
    let value (PositiveDecimal decimal) = decimal

    let create value =
        validate {
            let! name = Check.Decimal.greaterThanOrEqualTo 0m "amount" value
            return PositiveDecimal name
        }

module CreateBudget =
    type CreateBudgetRequest =
        { Name: string; MonthlyIncome: decimal }

    type ValidatedCreateBudgetRequest =
        { Name: BudgetName
          MonthlyIncome: PositiveDecimal }

    let validateRequest (createBudgetRequest: CreateBudgetRequest) =
        validate {
            let! budgetName = BudgetName.create createBudgetRequest.Name
            let! monthlyIncome = PositiveDecimal.create createBudgetRequest.MonthlyIncome

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
            Response.ofJson {| errors = ValidationErrors.toMap errors |}

        Request.mapValidateJson validateRequest createBudget handleValidationErrors
