namespace WebApplicationBasic.Controllers

open System
open System.Collections.Generic
open System.Linq
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

[<Route("api/[controller]")>]
type PaymentController() =
    inherit Controller()

    let handleCommand' =
        Aggregate.makeHandler 
            { zero = Client.State.Zero; apply = Client.apply; exec = Client.exec }
            (EventStore.makeRepository Global.EventStore.Value "Client" Serialization.serializer)
