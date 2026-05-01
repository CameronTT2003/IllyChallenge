using ConsoleApp1;
using ConsoleApp1.LittleSql;

var ev = Db.Events.As("ev");
var evAtt = Db.EventAttendee.As("ea");
var att = Db.Attendee.As("att");

// Main example from the brief.
// Events -> EventAttendee -> Attendee, then bob OR important.
var briefQuery = SelectQuery
    .From(ev)
    .Select(
        ev.Id.As("EventId"),
        ev.Name.As("EventName"),
        att.Name.As("AttendeeName"))
    .InnerJoin(evAtt, ev.Id.Eq(evAtt.EventId))
    .InnerJoin(att, evAtt.AttendeeId.Eq(att.Id))
    .Where(att.Name.Eq("bob") | ev.Important.Eq(true));

Console.WriteLine("Example 1 - query from the brief");
Console.WriteLine(briefQuery.ToSql());
Console.WriteLine();


//  dynamic field example.
// I used a normal list here because this is probably how fields would be added
// from some option chosen by the caller
var cols = new List<Field>
{
    ev.Id,
    ev.Name
};

var includeImportance = true;

if (includeImportance)
{
    cols.Add(ev.Important.As("IsImportant"));
}

var dynamicSelect = SelectQuery
    .From(ev)
    .Select(cols)
    .Where(ev.Name.NotEq("Archived"));

Console.WriteLine("Example 2 - dynamic select list");
Console.WriteLine(dynamicSelect.ToSql());
Console.WriteLine();


// eft joins mostly to prove it is not just hardcoded to INNER JOIN.
var attendeeLookup = SelectQuery
    .From(ev)
    .Select(
        ev.Id,
        ev.Name,
        att.Email.As("EmailIfKnown"))
    .LeftJoin(evAtt, ev.Id.Eq(evAtt.EventId))
    .LeftJoin(att, evAtt.AttendeeId.Eq(att.Id))
    .Where(
        (att.Name.Eq("alice") | att.Name.Eq("bob"))
        & ev.Important.Eq(true));

Console.WriteLine("Example 3 - left joins and grouped where");
Console.WriteLine(attendeeLookup.ToSql());
Console.WriteLine();


// A small extra example for SELECT table.*
var justImportantEvents = SelectQuery
    .From(ev)
    .Select(Field.AllFrom(ev))
    .Where(ev.Important.Eq(true));

Console.WriteLine("Example 4 - select all columns from one table");
Console.WriteLine(justImportantEvents.ToSql());
Console.WriteLine();