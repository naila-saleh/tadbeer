using System.Text.Json.Serialization;

namespace Tadbeer.DAL.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    SuperAdmin,
    Admin,
    Worker,
    User
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserStatus
{
    Existed,
    Deleted
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BookingStatus
{
    Pending,
    Accepted,
    Rejected,
    Completed,
    Cancelled
}

[JsonConverter(typeof(JsonStringEnumConverter))]
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