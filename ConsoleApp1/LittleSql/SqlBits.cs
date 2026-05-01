using System;

namespace ConsoleApp1.LittleSql;

public enum JoinKind
{
    Inner,
    LeftOuter,
    RightOuter,
    FullOuter
}

public interface ISqlTable
{
    TableRef Ref { get; }
}

public sealed record TableDef(string Name, string? Schema = null);

public sealed record TableRef(TableDef Definition, string? Alias = null)
{
    public Column<T> Column<T>(string name) => new(this, name);
}

public abstract class Table<TSelf> : ISqlTable where TSelf : Table<TSelf>
{
    public TableRef Ref { get; }

    protected Table(string tableName, string? alias = null, string? schema = null)
    {
        Ref = new TableRef(new TableDef(tableName, schema), alias);
    }

    // Shorter than writing Ref.Column<T>(.) in every  class.
    protected Column<T> Col<T>(string name) => Ref.Column<T>(name);

    public abstract TSelf As(string alias);
}

public sealed record Column<T>(TableRef Table, string Name)
{
    private ColumnValue Value => new(Table, Name);

    public Field As(string alias) => new(Value, alias);

    public SqlBool Eq(T value) =>
        SqlBool.Compare(Value, "=", new LiteralValue(value));

    public SqlBool Eq(Column<T> other) =>
        SqlBool.Compare(Value, "=", new ColumnValue(other.Table, other.Name));

    public SqlBool NotEq(T value) =>
        SqlBool.Compare(Value, "<>", new LiteralValue(value));

    public SqlBool Gt(T value) =>
        SqlBool.Compare(Value, ">", new LiteralValue(value));

    public SqlBool Lt(T value) =>
        SqlBool.Compare(Value, "<", new LiteralValue(value));

    public SqlBool IsNull() =>
        SqlBool.Compare(Value, "IS", new LiteralValue(null));

    // lets Select(events.Id, events.Name) work without wrapping each one manually
    public static implicit operator Field(Column<T> column) =>
        new(new ColumnValue(column.Table, column.Name));
}

public sealed record Field(SqlValue Value, string? Alias = null)
{
    public static Field All => new(new StarValue());

    public static Field AllFrom(ISqlTable table) =>
        new(new StarValue(table.Ref));
}

// Small set of values the printer knows how to turn into SQL.

public abstract record SqlValue;

public sealed record ColumnValue(TableRef Table, string Name) : SqlValue;

public sealed record LiteralValue(object? Value) : SqlValue;

public sealed record StarValue(TableRef? Table = null) : SqlValue;

public abstract record SqlBool
{
    public static SqlBool Compare(SqlValue left, string op, SqlValue right) =>
        new CompareBit(left, op, right);

    public static SqlBool operator &(SqlBool left, SqlBool right) =>
        new LogicBit(LogicalOperator.And, left, right);

    public static SqlBool operator |(SqlBool left, SqlBool right) =>
        new LogicBit(LogicalOperator.Or, left, right);
}

public enum LogicalOperator
{
    And,
    Or
}

public sealed record CompareBit(
    SqlValue Left,
    string Operator,
    SqlValue Right) : SqlBool;

public sealed record LogicBit(
    LogicalOperator Operator,
    SqlBool Left,
    SqlBool Right) : SqlBool;

public sealed record Join(
    JoinKind Kind,
    TableRef Table,
    SqlBool On);