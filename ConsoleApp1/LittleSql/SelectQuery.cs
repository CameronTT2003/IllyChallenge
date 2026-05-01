using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp1.LittleSql;

public sealed record SelectQuery(
    TableRef? FromTable,
    IReadOnlyList<Field> Fields,
    IReadOnlyList<Join> Joins,
    SqlBool? WhereClause)
{
    public static SelectQuery From(ISqlTable table) =>
        new(
            table.Ref,
            Array.Empty<Field>(),
            Array.Empty<Join>(),
            null);

    public SelectQuery Select(params Field[] fields) =>
        this with { Fields = fields };

    public SelectQuery Select(IEnumerable<Field> fields) =>
        this with { Fields = fields.ToArray() };

    public SelectQuery InnerJoin(ISqlTable table, SqlBool on) =>
        AddJoin(JoinKind.Inner, table, on);

    public SelectQuery LeftJoin(ISqlTable table, SqlBool on) =>
        AddJoin(JoinKind.LeftOuter, table, on);

    public SelectQuery RightJoin(ISqlTable table, SqlBool on) =>
        AddJoin(JoinKind.RightOuter, table, on);

    public SelectQuery FullJoin(ISqlTable table, SqlBool on) =>
        AddJoin(JoinKind.FullOuter, table, on);

    public SelectQuery Where(SqlBool condition) =>
        this with { WhereClause = condition };

    public string ToSql() => SqlPrinter.Print(this);

    private SelectQuery AddJoin(JoinKind kind, ISqlTable table, SqlBool on) =>
        this with
        {
            // I am copying the list here instead of changing  it.
            // It keeps the builder style simple and avoids hiden changes to old queries.
            Joins = Joins.Append(new Join(kind, table.Ref, on)).ToArray()
        };
}