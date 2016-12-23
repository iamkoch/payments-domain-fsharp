namespace WebApplicationBasic.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Net
open System.Net.Http
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Newtonsoft.Json
open Models
open Global

type HomeController () = 
    inherit Controller ()

    member this.Index () = this.View()

    member this.About () =
        this.ViewData.["Message"] <- "Your application description page."
        this.View()

    member this.Contact() =
        this.ViewData.["Message"] <- "Your contact page.";
        this.View();

    member this.Error() = this.View()

[<Route("api/[controller]")>]
type TestController() =
    inherit Controller()
    member this.Index() =
        let x = { Hello = "test" }
        this.Created("one/two/three", x)

[<CLIMutable>]
[<JsonObject(MemberSerialization=MemberSerialization.OptOut)>]
type CreatePaymentModel = {
    id : Guid;
    sourceAccount : string;
    sourceSortCode : string;
    destinationAccount : string;
    destinationSortCode : string;
    amount : decimal;
    date : DateTime;
}

type CreatePaymentResponse = 
    | Success of bool
    | Error of bool * string list

[<Route("api/[controller]")>]
type PaymentController() =
    inherit Controller()

    let gva (an:Client.AccountNumber) =
        let ac = { Client.IsActive = true; }
        Choice2Of2 ["oops something went wrong"] (* make Choice1Of2 ac to succeed*)

    let services = {
        Client.accountService = { getValidAccount = gva; }
    }

    let handleCommand' =
        Aggregate.makeHandler 
            { zero = Client.State.Zero; apply = Client.apply; exec = Client.exec services }
            (EventStore.makeRepository Global.EventStore.Value "Client" Serialization.serializer)

    let handleCommand (id,v) c = handleCommand' (id,v) c |> Async.RunSynchronously

    member x.Post ([<FromBody>]item:CreatePaymentModel) =
        let result = 
            Client.SchedulePayment (item.id, item.sourceSortCode, item.sourceAccount, item.destinationSortCode, item.destinationAccount, item.amount, item.date) 
            |> handleCommand (item.id, 0)
        match result with
            | Choice1Of2 x -> Success(true)
            | Choice2Of2 x -> Error (false, x)