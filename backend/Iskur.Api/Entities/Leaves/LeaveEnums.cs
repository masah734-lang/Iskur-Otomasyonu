namespace Iskur.Api.Entities.Leaves;

public enum LeaveStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3
}

public enum LeaveMovementType
{
    Entitlement = 0, // Hak tanımlama
    Usage = 1,       // Kullanım
    Refund = 2,      // İade
    Adjustment = 3   // Düzeltme
}