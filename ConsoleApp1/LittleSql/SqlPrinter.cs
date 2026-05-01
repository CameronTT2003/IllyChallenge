using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ConsoleApp1.LittleSql;

public static class SqlPrinter
{
    public static string Print(SelectQuery query)
    {
        if (query.FromTable is null)
        {
            // This should not happen when using SelectQuery.From(.),
            // but the guard makes failures easier to understand.

            throw new InvalidOperationException("Cannot print SQL without a FROM table.");
        }

        var sql = new StringBuilder();

        sql.Append("SELECT ");
        sql.Append(query.Fields.Count == 0

            ? "*"
            : string.Join(", ", query.Fields.Select(PrintField)));

        sql.AppendLine();
        sql.Append("FROM ");
        sql.Append(PrintTable(query.FromTable));

        foreach (var join in query.Joins)
        {
            sql.AppendLine();
            sql.Append(PrintJoin(join.Kind));
            sql.Append(" JOIN ");
            sql.Append(PrintTable(join.Table));
            sql.Append(" ON ");
            sql.Append(PrintCondition(join.On));
        }

        if (query.WhereClause is not null)
        {

            sql.AppendLine();
            sql.Append("WHERE ");
            sql.Append(PrintCondition(query.WhereClause));
        }

        sql.Append(';');

        return sql.ToString();
    }

    private static string PrintField(Field field) =>
        field.Alias is null
            ? PrintValue(field.Value)
            : $"{PrintValue(field.Value)} AS {WrapName(field.Alias)}";

    private static string PrintTable(TableRef table)
    {
        var tableName = table.Definition.Schema is null
            ? WrapName(table.Definition.Name)
            : $"{WrapName(table.Definition.Schema)}.{WrapName(table.Definition.Name)}";

        return table.Alias is null
            ? tableName
            : $"{tableName} AS {WrapName(table.Alias)}";
    }

    private static string PrintJoin(JoinKind kind) =>
        kind switch
        {
            JoinKind.Inner => "INNER",
            JoinKind.LeftOuter => "LEFT OUTER",
            JoinKind.RightOuter => "RIGHT OUTER",
            JoinKind.FullOuter => "FULL OUTER",

            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

    private static string PrintCondition(SqlBool condition) =>
        condition switch
        {
            CompareBit c =>
                $"{PrintValue(c.Left)} {c.Operator} {PrintValue(c.Right)}",

            // Always adding brackets ,
            // avoids And / OR  bugs in generated SQL.
            LogicBit l =>
                $"({PrintCondition(l.Left)} {PrintLogic(l.Operator)} {PrintCondition(l.Right)})",

            _ => throw new ArgumentOutOfRangeException(nameof(condition))
        };

    private static string PrintLogic(LogicalOperator op) =>
        op switch
        {
            LogicalOperator.And => "AND",
            LogicalOperator.Or => "OR",
            _ => throw new ArgumentOutOfRangeException(nameof(op))
        };

    private static string PrintValue(SqlValue value) =>
        value switch
        {
            ColumnValue col =>
                $"{WrapName(col.Table.Alias ?? col.Table.Definition.Name)}.{WrapName(col.Name)}",

            LiteralValue lit =>
                PrintLiteral(lit.Value),

            StarValue { Table: null } =>
                "*",

            StarValue { Table: { } table } =>
                $"{WrapName(table.Alias ?? table.Definition.Name)}.*",

            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };

    private static string PrintLiteral(object? value) =>
    value switch
    {
        null => "NULL",

 
        string s => $"N'{Escape(s)}'",

        bool b => b ? "1" : "0",

        DateTime d => $"'{d.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)}'",

        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture) ?? "NULL",

        _ => $"N'{Escape(value.ToString() ?? string.Empty)}'"
    };

    // Square brackets are T-SQL friendly and stop reserved words causing trouble.
    private static string WrapName(string name) =>
        $"[{name.Replace("]", "]]")}]";

    private static string Escape(string value) =>

        value.Replace("'", "''");
}