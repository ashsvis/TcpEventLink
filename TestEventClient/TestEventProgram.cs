using System;
using System.Data;
using System.Linq;
using TcpEventCommon;

namespace TestEventClient
{
    class TestEventProgram
    {
        static Guid id = Guid.Empty;

        static void Main(string[] args)
        {
            var tcpmodule = new TcpModule();
            tcpmodule.Receive += Tcpmodule_Receive;
            tcpmodule.Disconnected += Tcpmodule_Disconnected;
            tcpmodule.Connected += Tcpmodule_Connected;
            tcpmodule.ConnectClient("127.0.0.1");
            string line;
            do
            {
                line = Console.ReadLine();
                //tcpmodule.SendData("Клиент по месту:", line);
                var ds = new DataSet("ClientDataSet");
                var table = MakeTable("Name", "Value");
                table.TableName = "TableOne";
                ds.Tables.Add(table);
                var row = table.NewRow();
                row["Name"] = 1;
                row["Value"] = "111";
                table.Rows.Add(row);

                table = MakeTable("Name", "Value");
                table.TableName = "TableTwo";
                ds.Tables.Add(table);
                row = table.NewRow();
                row["Name"] = 2;
                row["Value"] = "222";
                table.Rows.Add(row);

                tcpmodule.SendData(ds);
            } while (line != "exit");
            tcpmodule.DisconnectClient();
        }

        private static DataTable MakeTable(string c1Name, string c2Name)
        {
            var table = new DataTable();
            var column = new DataColumn(c1Name, typeof(int));
            table.Columns.Add(column);
            column = new DataColumn(c2Name, typeof(string));
            table.Columns.Add(column);
            return table;
        }

        private static void Tcpmodule_Receive(object sender, ReceiveEventArgs e)
        {
            if (sender is TcpClientData client)
            {
                foreach (var table in e.SendInfo.DataSet.Tables.Cast<DataTable>())
                {
                    Console.WriteLine(string.Join("\t", table.Columns.Cast<DataColumn>().Select(column => column.ColumnName)));
                    foreach (var row in table.Rows.Cast<DataRow>())
                    {
                        Console.WriteLine(string.Join("\t", row.ItemArray.Select(item => $"{item}")));
                    }

                    Console.WriteLine();
                }
            }
        }

        private static void Tcpmodule_Disconnected(object sender, string result)
        {
            Console.WriteLine("Отключился от сервера"); ;
        }

        private static void Tcpmodule_Connected(object sender, string result)
        {
            if (sender is TcpModule module)
            {
                id = module.GetLastClientId();
                Console.WriteLine($"Клиент {id} подключился к серверу");
            }
        }
    }
}
