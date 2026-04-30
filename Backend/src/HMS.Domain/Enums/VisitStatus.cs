namespace HMS.Domain.Enums;

public enum VisitStatus
{
    CheckedIn = 1,
    WaitingDoctor = 2,
    Prepared = 3,
    InOp = 4,
    OpCompleted = 5,
    PostOp = 6,
    Completed = 7,
    PendingCheckoutNurse = 8,
    PendingCheckoutReception = 9
}