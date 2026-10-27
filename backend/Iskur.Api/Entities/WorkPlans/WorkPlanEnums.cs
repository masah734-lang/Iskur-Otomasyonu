namespace Iskur.Api.Entities.WorkPlans;

public enum PlanStatus
{
    Draft = 0,
    Submitted = 1
}

public enum WorkType
{
    FullDay = 0,
    Morning = 1,
    Afternoon = 2
}

public enum SessionType
{
    Morning = 0,
    Afternoon = 1
}

public enum DecisionStatus
{
    Draft = 0,
    Pending = 1,
    Approved = 2,
    Rejected = 3
}