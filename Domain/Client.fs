[<RequireQualifiedAccess>]
module Client

    open System
    
    type AccountNumber8 = AccountNumber8 of string
    type SortCode = SortCode of string
    type AccountDetail = AccountNumber8 * SortCode
    type SourceAccountDetail = AccountNumber8 * SortCode
    type DestAccountDetail = AccountNumber8 * SortCode
    type PaymentDate = PaymentDate of DateTime
    type PaymentId = PaymentId of Guid

    [<Measure>] type GBP
    type PaymentAmount = float<GBP>

    type ScheduledPayment = 
        | Scheduled of PaymentId * PaymentDate
        | Cancelled of PaymentId
        | Processed of PaymentId

    type State = {
        ScheduledPayments : ScheduledPayment list;
    }
    with static member Zero = { ScheduledPayments = [] }

    type Command =
        | SchedulePayment of PaymentId * SourceAccountDetail * DestAccountDetail * PaymentAmount * PaymentDate

    type Event = 
        | PaymentScheduled of PaymentId * SourceAccountDetail * DestAccountDetail * PaymentAmount * PaymentDate * DateTime

    let apply item = 
        function
        | PaymentScheduled (pid,sad,dad,amt,whenFor,occured) -> { item with ScheduledPayments = Scheduled (pid, whenFor) :: item.ScheduledPayments } 

    open Validator
    module private Assert = 
        let validDate date = validator (fun d -> d > PaymentDate DateTime.Today) ["Must be in future"] date

    let exec state = 
        function 
        | SchedulePayment(pid, sad, dad, amt, whenFor)   -> Assert.validDate whenFor <?> PaymentScheduled(pid,sad,dad,amt,whenFor,DateTime.Now)
