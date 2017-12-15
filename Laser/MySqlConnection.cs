using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text.RegularExpressions;

namespace Laser
{
    class MysqlConnection
    {
        MySqlConnection mysqlConnection;
        DataSet dataSet;
        string IP = "192.168.0.1";
        string UserName = "root";
        string Password = "qw63778";
        string Database = "sys";
        /// <summary>
        /// 建立mysql连接
        /// </summary>
        public MysqlConnection()
        {
            try
            {
                mysqlConnection = new MySqlConnection("datasource=" + IP + ";username=" + UserName + ";password=" + Password + ";database=" + Database + ";charset=utf8");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public MysqlConnection(string IP, string UserName, string Password, string Database)
        {
            try
            {
                string connectionString = "datasource=" + IP + ";username=" + UserName + ";password=" + Password + ";database=" + Database + ";charset=gb2312";
                mysqlConnection = new MySqlConnection(connectionString);
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public string MysqlInfo()
        {
            string mysqlInfo = null;
            try
            {
                mysqlConnection.Open();
                mysqlInfo += "Connection Opened." + Environment.NewLine;
                mysqlInfo += "Connection String:" + mysqlConnection.ConnectionString.ToString() + Environment.NewLine;
                mysqlInfo += "Database:" + mysqlConnection.Database.ToString() + Environment.NewLine;
                mysqlInfo += "Connection ServerVersion:" + mysqlConnection.ServerVersion.ToString() + Environment.NewLine;
                mysqlInfo += "Connection State:" + mysqlConnection.State.ToString() + Environment.NewLine;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("MySqlException Error:" + ex.ToString());
            }
            finally
            {
                mysqlConnection.Close();
            }
            return mysqlInfo;
        }
        /// <summary>
        /// 执行sql语句无返回结果
        /// </summary>
        public int MysqlCommand(string MysqlCommand)
        {
            try
            {
                mysqlConnection.Open();
                Console.WriteLine("MysqlConnection Opened.");
                MySqlCommand mysqlCommand = new MySqlCommand(MysqlCommand, mysqlConnection);
                return mysqlCommand.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("MySqlException Error:" + ex.ToString());
                if (Regex.IsMatch(ex.ToString(), ""))
                {
                    Console.WriteLine(ex.Message);
                }
            }
            finally
            {
                mysqlConnection.Close();
            }
            return -1;
        }

        /// <summary>
        /// 执行select 语句返回执行结果
        /// </summary>
        public DataView MysqlDataAdapter(string table)
        {
            DataView dataView = new DataView();
            try
            {
                mysqlConnection.Open();
                MySqlDataAdapter mysqlDataAdapter = new MySqlDataAdapter("Select * from " + table, mysqlConnection);
                dataSet = new DataSet();
                mysqlDataAdapter.Fill(dataSet, table);
                dataView = dataSet.Tables[table].DefaultView;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                mysqlConnection.Close();
            }
            return dataView;
        }
        /// <summary>
        /// 统计记录个数 参数：select count(*) from isns_users
        /// </summary>
        public long MysqlCountRow(string sql)
        {
            DataView dataView = new DataView();
            try
            {
                mysqlConnection.Open();

                MySqlCommand mycm = new MySqlCommand(sql, mysqlConnection);
                // MySqlDataReader msdr = mycm.ExecuteReader();
                long recordCount = (long)mycm.ExecuteScalar();
                return recordCount;
            }
            catch (MySqlException)
            {
                return -1;
                // Console.WriteLine(ex.Message);
            }
            finally
            {
                mysqlConnection.Close();
            }
            // return 0;
        }
        /// <summary>
        /// Mysql数据库插入语句 ：如果插入异常则创建一个新表
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataView MySqlInsert(string table)
        {
            DataView dataView = new DataView();
            try
            {
                if (mysqlConnection.State != ConnectionState.Open)
                {
                    mysqlConnection.Open();
                }
                
                MySqlDataAdapter mysqlDataAdapter = new MySqlDataAdapter("Insert into " + table, mysqlConnection);
                dataSet = new DataSet();
                mysqlDataAdapter.Fill(dataSet, table);
                dataView = dataSet.Tables[table].DefaultView;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message + "数据插入失败");


            }
            finally
            {
                mysqlConnection.Close();
            }
            return dataView;
        }

    }//end class
}
