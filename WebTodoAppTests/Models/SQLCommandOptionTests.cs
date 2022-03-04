namespace WebTodoApp.Models.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using WebTodoApp.Models;

    [TestClass]
    public class SQLCommandOptionTests
    {
        [TestMethod]
        public void BuildSQLTest()
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
                commandOption.BuildSQL(),
                "select * from testTableName where 1=1 order by firstColumn DESC ,secondColumn DESC LIMIT 20;");
        }
    }
}