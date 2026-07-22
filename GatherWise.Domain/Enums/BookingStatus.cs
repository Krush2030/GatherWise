namespace GatherWise.Domain.Enums
{
    public enum BookingStatus
    {
        // 1. Initial State: Host requested the booking, waiting for Venue Owner to approve
        PendingApproval = 1,

        // 2. Transition State: Owner approved! Waiting for Host payment within 1 hour
        Approved = 2,

        // 3. Rejection State: Owner declined the request
        Rejected = 3,

        // 4. Cancelled manually by user or system
        Cancelled = 4,

        // 5. System Timeout State: Release the slot if Owner doesn't approve in 1hr OR Host doesn't pay in 1hr
        CancelledByTimeout = 5,

        // 6. Complete State: Booking fully paid and event finished
        Completed = 6
    }
}