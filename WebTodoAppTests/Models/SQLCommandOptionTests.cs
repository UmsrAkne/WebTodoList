using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebTodoApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTodoApp.Models.Tests
{
    [TestClass()]
    public class SQLCommandOptionTests
    {
        [TestMethod()]
        public void buildSQLTest()
        {

            var commandOption = new SQLCommandOption();
            commandOption.Limit = 20;
            commandOption.TableName = "testTableName";

            commandOption.OrderByColumns.Add(
                new SQLCommandOption.SQLCommandColumnOption()
                {
                    Name = "firstColumn",
                    DESC = true
                });

            commandOption.OrderByColumns.Add(
                new SQLCommandOption.SQLCommandColumnOption()
                {
                    Name = "secondColumn",
                    DESC = true
                });

            Assert.AreEqual(
                commandOption.buildSQL(),
                "select * from testTableName where 1=1 order by firstColumn DESC ,secondColumn DESC LIMIT 20;");

        }
    }
}