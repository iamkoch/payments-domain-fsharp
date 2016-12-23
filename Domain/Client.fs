[<RequireQualifiedAccess>]
module Client

    open System

    type ClientServices = {
        accountService: AccountService 
    }
    and AccountService = {
        getValidAccount: GetValidAccount
    }
    and GetValidAccount =
        AccountNumber -> Choice<Account, string list>
    and Account = {
        IsActive: bool
    }
    and AccountNumber = AccountNumber of string
    
    [<Measure>] type GBP
    type PaymentAmount = float<GBP>

    type ScheduledPayment = 
        | Scheduled of Guid
        | Cancelled of Guid * DateTime
        | Processed of Guid

    type State = {
        ScheduledPayments : ScheduledPayment list;
    }
    with static member Zero = { ScheduledPayments = [] }

    type Command =
        | SchedulePayment of Guid * string * string * string * string * decimal * DateTime

    type Event = 
        | PaymentScheduled of Guid * string * string * string * string * decimal * DateTime * DateTime

    let apply item = 
        function
        | PaymentScheduled (id, ssc, san, dsc, dan, amt, whenFor, occuredAt) -> { item with ScheduledPayments = Scheduled (id) :: item.ScheduledPayments } 

    open Validator
    module private Assert = 
        let validDate date = validator (fun d -> d > DateTime.Today) ["Must be in future"] date
        let validLength what len s = validator (fun (x:string) -> x.Length = len) [sprintf "%s must be 8 digits" what] s
        let validAccount a = validLength "account" 8 a
        let validSortcode s = validLength "sort code" 6 s
    
    let getValidAccount (service:AccountService) ac =
        service.getValidAccount ac

    let exec (services:ClientServices) state = 
        function 
        | SchedulePayment(id, ssc, san, dsc, dan, amt, whenFor) 
            -> Assert.validDate whenFor 
            <* Assert.validLength "source account" 8 san
            <* Assert.validLength "source sort code" 6 ssc
            <* Assert.validLength "destination account" 8 dan
            <* Assert.validLength "destination sort code" 6 dsc
            <* getValidAccount services.accountService (AccountNumber san)
            <?> PaymentScheduled(id, ssc, san, dsc, dan, amt, whenFor, DateTime.Now)