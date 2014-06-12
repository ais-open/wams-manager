namespace Ais.Internal.Dcm.Business
{
    public class JobNotificationSubscription
    {
        public TargetJobState TargetJobState { get; private set; }
        public string NotificationEndPointId { get; private set; }
    }

    public enum TargetJobState
    {
        FinalStatesOnly = 1,
        AllStates = 2,
        None = 0
    }

    public enum JobState 
    {
        Queued = 0,

        Scheduled = 1,

        Processing = 2,

        Finished = 3,

        Error = 4,

        Canceled = 5,

        Canceling = 6
    }
}
