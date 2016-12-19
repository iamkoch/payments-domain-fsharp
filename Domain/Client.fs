[<RequireQualifiedAccess>]
module Client

    open System
    
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

    let exec state = 
        function 
        | SchedulePayment(id, ssc, san, dsc, dan, amt, whenFor)  
            -> Assert.validDate whenFor 
            
            <?> PaymentScheduled(id, ssc, san, dsc, dan, amt, whenFor, DateTime.Now)
