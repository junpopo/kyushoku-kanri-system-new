using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 給食管理システム.Dto
{
    class LocalDto
    {
        public static int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public static string IniLocalSet =
            "'id' INTEGER NOT NULL," +
            "'name' TEXT NOT NULL";

        public static string InsertLocalSql =
         $"INSERT INTO {SystemData.localTable} VALUES" +
         "(1,'職員室')";

    }
}
