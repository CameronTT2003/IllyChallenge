using ConsoleApp1.LittleSql;

namespace ConsoleApp1;

// This is the small fake schema used by the console examples.

public static class Db
{
    public static Events Events => new();

    public static EventAttendee EventAttendee => new();

    public static Attendee Attendee => new();
}

public sealed class Events : Table<Events>
{
    public Events(string? alias = null) : base("Events", alias)
    {
    }

    public override Events As(string alias) => new(alias);

    public Column<int> Id => Col<int>("Id");

    public Column<string> Name => Col<string>("Name");

    public Column<bool> Important => Col<bool>("Important");
}

public sealed class EventAttendee : Table<EventAttendee>
{
    public EventAttendee(string? alias = null) : base("EventAttendee", alias)
    {
    }

    public override EventAttendee As(string alias) => new(alias);

    public Column<int> EventId => Col<int>("EventId");

    public Column<int> AttendeeId => Col<int>("AttendeeId");
}

public sealed class Attendee : Table<Attendee>
{
    public Attendee(string? alias = null) : base("Attendee", alias)
    {
    }

    public override Attendee As(string alias) => new(alias);

    public Column<int> Id => Col<int>("Id");

    public Column<string> Name => Col<string>("Name");

    public Column<string> Email => Col<string>("Email");
}