using Microsoft.SqlServer.Management.Smo;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace MSSqlTableExporter {
    class Program {
        static void Main(string[] args) {
            if (args.Length != 4) {
                Console.WriteLine("參數必須是[1]資料庫主機位址 [2]資料庫名稱 [3]表格名稱清單(以\",\"區隔) [4]目的路徑");

                return;
            }

            var usunSql = new Server(args[0]);
            var efgpDB = usunSql.Databases[args[1]];
            var tableNames = args[2].Split(',').Select(tableName => tableName.Trim());
            var scriptCreater = new Scripter(usunSql);

            scriptCreater.Options.ScriptData = true;
            scriptCreater.Options.ScriptSchema = false;

            foreach (Table table in efgpDB.Tables) {
                if (table.IsSystemObject == true) {
                    continue;
                }

                if (!tableNames.Contains(table.Name)) {
                    continue;
                }

                var result = new StringBuilder(string.Empty);
                var script = scriptCreater.EnumScript(new SqlSmoObject[] { table });

                foreach (var line in script) {
                    result.AppendLine(line);
                }

                if (!Directory.Exists(args[3])) {
                    Directory.CreateDirectory(args[3]);
                }

                var fileName = args[3] + "\\" + $"{table.Schema}.{table.Name}.Table.sql";
                File.WriteAllText(fileName, result.ToString());
                Console.WriteLine($"表格[{table.Name}]匯出完成");
            }

            Console.WriteLine($"全部表格匯出完成");
        }
    }
}
