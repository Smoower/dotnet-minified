using System.ComponentModel.DataAnnotations.Schema;

namespace Smoower.Minified.EFCore;

// [Col("LongColumnName")] pins the database column so the C# property can be a
// short handle without changing the schema. EF's attribute convention resolves
// derived ColumnAttribute instances, so this is recognised exactly like [Column].
// This is what lets `public int? RD` map to the "RecurrenceDays" column - the long
// name is paid once here, the short name everywhere it's used.
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class ColAttribute(string name) : ColumnAttribute(name);
