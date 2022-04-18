using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Получение клиентом сообщения от сервера
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Tcpmodule_Receive(object sender, ReceiveEventArgs e)
        {
            if (sender is TcpClientData client)
            {
                // для загруженного набора данных получаем массив имён таблиц
                var dataSet = e.SendInfo.DataSet;
                var tableNames = dataSet.Tables.Cast<DataTable>().Select(item => item.TableName);
                // для всех таблиц из набора DataSet
                foreach (var tableName in tableNames)
                {
                    var table = dataSet.Tables[tableName];
                    // для текущей таблицы получаем массив имён столбцов
                    var columnNames = table.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
                    // выводим сцепленные через табуляцию значения
                    Console.WriteLine(string.Join("\t", columnNames));
                    // перебираем все строки текущей таблицы
                    foreach (var row in table.Rows.Cast<DataRow>())
                    {
                        // для текущей строки таблицы
                        var columnValues = new List<string>();
                        // добавляем текстовое представление значения в список columnValues
                        foreach (var columnName in columnNames)
                            columnValues.Add($"{row[columnName]}");
                        // выводим сцепленные через табуляцию значения
                        Console.WriteLine(string.Join("\t", columnValues));
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
