namespace Tadbeer.DAL.Models;

public enum UserRole
{
    Admin,
    Worker,
    User
}

public enum UserStatus
{
    Existed,
    Deleted
}

public enum BookingStatus
{
    Pending,
    Accepted,
    Rejected,
    Completed,
    Cancelled
}

public enum WeekDay
{
    Saturday,
    Sunday,
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday
}