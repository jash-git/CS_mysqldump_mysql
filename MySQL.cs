using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Collections;

namespace SYWEB_V8_Workstation
{
    //----
    //增加資料庫欄位名稱替換類別
    public class ReplaceField
    {
        public string StrOldField;
        public string StrNewField;
        public ReplaceField()
        {
            StrOldField = "";
            StrNewField = "";
        }
    }
    //----

    public class External_MySQL
    {
        public string dbHost;// = "127.0.0.1";//資料庫位址
        public string dbport;// = "3306";
        public string dbUser;// = "root";//資料庫使用者帳號
        public string dbPass;// = "usbw";//資料庫使用者密碼
        public string dbName;// = "v8_workstation";//資料庫名稱
        public string connStr;// = "server=" + dbHost + ";Port=" + dbport + ";uid=" + dbUser + ";pwd=" + dbPass + ";database=" + dbName + ";charset=utf8";
        public MySqlConnection m_conn = null;
        public String m_StrLastSQL;// = "";
        public Process m_pro;
        public ArrayList m_ALReplaceField;//動態欄位替換陣列

        public External_MySQL()
        {
            dbHost="";
            dbport="";
            dbUser="";
            dbPass="";
            dbName="";

            m_ALReplaceField = new ArrayList();//動態欄位替換陣列
            m_ALReplaceField.Clear();
        }

        public bool CheckMySQL(String StrdbHost, String Strdbport, String StrdbUser, String StrdbPass)
        {
            bool blnAns = true;
            Thread.Sleep(1000);
            dbHost = StrdbHost;
            dbport = Strdbport;
            dbUser = StrdbUser;
            dbPass = StrdbPass;
            connStr = "server=" + dbHost + ";Port=" + dbport + ";uid=" + dbUser + ";pwd=" + dbPass + ";";
            m_conn = new MySqlConnection(connStr);
            try
            {
                m_conn.Open();
                if (m_conn.State == ConnectionState.Open)
                {
                    blnAns = true;
                    m_conn.Close();
                }
                else
                {
                    blnAns = false;
                }
            }
            catch
            {
                blnAns = false;
            }
            m_conn = null;
            return blnAns;
        }

        public bool DownloadDBTable(String StrTableName, String StrDBName = "SYRISCloudSystem")
        {
            bool blnAns = true;
            String StrCmd = "";
            dbName = StrDBName;

            string path = System.Windows.Forms.Application.StartupPath;

            //-h192.168.1.196 -P3306 -uhino -phinohino -e -q --default-character-set=utf8 SYRISCloudSystem  system_config > system_config.sql
            StrCmd = String.Format("\"" + path + "\\mysql\\bin\\mysqldump.exe\" -h{0} -P{1} -u{2} -p{3} -t -e -q --complete-insert --extended-insert=FALSE --default-character-set=utf8 {4} {5} > {5}.sql", dbHost, dbport, dbUser, dbPass, dbName, StrTableName);
            
            path += "\\mysql\\bin\\mysqldump.bat";
            StreamWriter sw = new StreamWriter(path);
            sw.WriteLine(StrCmd);// 寫入文字
            //sw.WriteLine("pause");// 寫入文字
            sw.Close();// 關閉串流
            if (m_pro == null)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(path);
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                try
                {
                    m_pro = Process.Start(startInfo);
                }
                catch
                {
                    return false;//找不到執行檔的防呆 at 2017/06/16
                }
                Thread.Sleep(100);
                if (m_pro != null)
                {

                    m_pro.WaitForExit();//下載SERVER資料
                    m_pro = null;

                    StreamReader sr = new StreamReader(StrTableName + ".sql", Encoding.ASCII);                    
                    StreamWriter sw1 = new StreamWriter("INSERTDB.sql",false,Encoding.ASCII);
                    sw1.WriteLine("USE `v8_workstation`;");// 寫入文字
                    while (!sr.EndOfStream)// 每次讀取一行，直到檔尾
                    {
                        string line = sr.ReadLine();// 讀取文字到 line 變數
                        if (line.Contains("INSERT INTO"))
                        {
                            sw1.WriteLine(line);// 寫入文字
                        }
                    }
                    sw1.Close();// 關閉串流
                    sr.Close();// 關閉串流
                    System.IO.File.Delete(StrTableName + ".sql");
                    System.IO.File.Delete(path);

                    MySQL.ClearTable(StrTableName);
                    path = System.Windows.Forms.Application.StartupPath;
                    StrCmd = "\"" + path + "\\mysql\\bin\\mysql.exe\" -uroot -pusbw -P 3307 < INSERTDB.sql";

                    path += "\\mysql\\bin\\mySQL_Import.bat";
                    StreamWriter sw2 = new StreamWriter(path);
                    sw2.WriteLine(StrCmd);// 寫入文字
                    //sw2.WriteLine("pause");// 寫入文字
                    sw2.Close();// 關閉串流

                    ProcessStartInfo startInfo1 = new ProcessStartInfo(path);
                    startInfo1.WindowStyle = ProcessWindowStyle.Hidden;
                    m_pro = Process.Start(startInfo1);
                    try
                    {
                        m_pro = Process.Start(startInfo1);
                    }
                    catch
                    {
                        return false;//找不到執行檔的防呆 at 2017/06/16
                    }
                    m_pro.WaitForExit();//下載SERVER資料
                    System.IO.File.Delete(path);

                    blnAns = true;
                }
                else
                {
                    blnAns = false;
                }
            }
            m_pro = null;
            return blnAns;
        }

        //----
        //在MySQL.cs中的External_MySQL類別中增加 DownloadDBTable()/UploadDBTable()函數
        public bool DownloadDBTable(String StrTableName, String StrSQLcmd,bool blnRunSQL=true, String StrDBName = "SYRISCloudSystem")
        {
            bool blnAns = true;
            String StrCmd = "";
            dbName = StrDBName;

            string path = System.Windows.Forms.Application.StartupPath;

            //-h192.168.1.196 -P3306 -uhino -phinohino -e -q --default-character-set=utf8 SYRISCloudSystem  system_config > system_config.sql
            StrCmd = String.Format("\"" + path + "\\mysql\\bin\\mysqldump.exe\" -h{0} -P{1} -u{2} -p{3} -t -e -q --complete-insert --extended-insert=FALSE --default-character-set=utf8 --hex-blob {4} {5} > {5}.sql", dbHost, dbport, dbUser, dbPass, dbName, StrTableName);

            path += "\\mysql\\bin\\mysqldump.bat";
            StreamWriter sw = new StreamWriter(path);
            sw.WriteLine(StrCmd);// 寫入文字
            //sw.WriteLine("pause");// 寫入文字
            sw.Close();// 關閉串流
            if (m_pro == null)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(path);
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                try
                {
                    m_pro = Process.Start(startInfo);
                }
                catch
                {
                    return false;//找不到執行檔的防呆 at 2017/06/16
                }
                Thread.Sleep(100);
                if (m_pro != null)
                {

                    m_pro.WaitForExit();//下載SERVER資料
                    m_pro = null;

                    StreamReader sr = new StreamReader(StrTableName + ".sql");
                    StreamWriter sw1 = new StreamWriter("INSERTDB.sql", false);
                    sw1.WriteLine("USE `v8_workstation`;");// 寫入文字
                    while (!sr.EndOfStream)// 每次讀取一行，直到檔尾
                    {
                        string line = sr.ReadLine();// 讀取文字到 line 變數
                        if (line.Contains("INSERT INTO"))
                        {
                            for (int i = 0; i < m_ALReplaceField.Count; i++)
                            {
                                ReplaceField RF_Data = ((ReplaceField)m_ALReplaceField[i]);
                                line = line.Replace(RF_Data.StrOldField, RF_Data.StrNewField);
                            }
                            line = line.Replace("INSERT INTO", StrSQLcmd);
                            sw1.WriteLine(line);// 寫入文字
                        }
                    }
                    sw1.Close();// 關閉串流
                    sr.Close();// 關閉串流
                    System.IO.File.Delete(StrTableName + ".sql");
                    System.IO.File.Delete(path);

                    //MySQL.ClearTable(StrTableName);
                    path = System.Windows.Forms.Application.StartupPath;
                    StrCmd = "\"" + path + "\\mysql\\bin\\mysql.exe\" -uroot -pusbw -P 3307 < INSERTDB.sql";

                    path += "\\mysql\\bin\\mySQL_Import.bat";
                    StreamWriter sw2 = new StreamWriter(path);
                    sw2.WriteLine(StrCmd);// 寫入文字
                    //sw2.WriteLine("pause");// 寫入文字
                    sw2.Close();// 關閉串流

                    if (!blnRunSQL)
                    {
                        return (!blnRunSQL);
                    }

                    ProcessStartInfo startInfo1 = new ProcessStartInfo(path);
                    startInfo1.WindowStyle = ProcessWindowStyle.Hidden;
                    m_pro = Process.Start(startInfo1);
                    try
                    {
                        m_pro = Process.Start(startInfo1);
                    }
                    catch
                    {
                        return false;//找不到執行檔的防呆 at 2017/06/16
                    }
                    m_pro.WaitForExit();//下載SERVER資料
                    System.IO.File.Delete(path);

                    blnAns = true;
                }
                else
                {
                    blnAns = false;
                }
            }
            m_pro = null;
            return blnAns;
        }

        public bool UploadDBTable(String StrTableName, String StrSQLcmd, bool blnRunSQL = true, String StrDBName = "SYRISCloudSystem")
        {
            bool blnAns = true;
            String StrCmd = "";
            dbName = StrDBName;

            string path = System.Windows.Forms.Application.StartupPath;

            //-h192.168.1.196 -P3306 -uhino -phinohino -e -q --default-character-set=utf8 SYRISCloudSystem  system_config > system_config.sql
            StrCmd = String.Format("\"" + path + "\\mysql\\bin\\mysqldump.exe\" -h{0} -P{1} -u{2} -p{3} -t -e -q --complete-insert --extended-insert=FALSE --default-character-set=utf8 --hex-blob {4} {5} > {5}.sql", "127.0.0.1", "3307", "root", "usbw", "v8_workstation", StrTableName);

            path += "\\mysql\\bin\\mysqldump.bat";
            StreamWriter sw = new StreamWriter(path);
            sw.WriteLine(StrCmd);// 寫入文字
            //sw.WriteLine("pause");// 寫入文字
            sw.Close();// 關閉串流
            if (m_pro == null)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(path);
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                try
                {
                    m_pro = Process.Start(startInfo);
                }
                catch
                {
                    return false;//找不到執行檔的防呆 at 2017/06/16
                }
                Thread.Sleep(100);
                if (m_pro != null)
                {

                    m_pro.WaitForExit();//下載SERVER資料
                    m_pro = null;

                    StreamReader sr = new StreamReader(StrTableName + ".sql");
                    StreamWriter sw1 = new StreamWriter("INSERTDB.sql", false);
                    sw1.WriteLine("USE `" + StrDBName + "`;");// 寫入文字
                    while (!sr.EndOfStream)// 每次讀取一行，直到檔尾
                    {
                        string line = sr.ReadLine();// 讀取文字到 line 變數
                        if (line.Contains("INSERT INTO"))
                        {
                            for (int i = 0; i < m_ALReplaceField.Count; i++)
                            {
                                ReplaceField RF_Data = ((ReplaceField)m_ALReplaceField[i]);
                                line = line.Replace(RF_Data.StrOldField, RF_Data.StrNewField);
                            }
                            line = line.Replace("INSERT INTO", StrSQLcmd);
                            sw1.WriteLine(line);// 寫入文字
                        }
                    }
                    sw1.Close();// 關閉串流
                    sr.Close();// 關閉串流
                    System.IO.File.Delete(StrTableName + ".sql");
                    System.IO.File.Delete(path);

                    //MySQL.ClearTable(StrTableName);
                    path = System.Windows.Forms.Application.StartupPath;
                    StrCmd = String.Format("\"" + path + "\\mysql\\bin\\mysql.exe\" -h{0} -P{1} -u{2} -p{3} < INSERTDB.sql", dbHost, dbport, dbUser, dbPass, dbName, StrTableName);

                    path += "\\mysql\\bin\\mySQL_Import.bat";
                    StreamWriter sw2 = new StreamWriter(path);
                    sw2.WriteLine(StrCmd);// 寫入文字
                    //sw2.WriteLine("pause");// 寫入文字
                    sw2.Close();// 關閉串流

                    if (!blnRunSQL)
                    {
                        return (!blnRunSQL);
                    }

                    ProcessStartInfo startInfo1 = new ProcessStartInfo(path);
                    startInfo1.WindowStyle = ProcessWindowStyle.Hidden;
                    m_pro = Process.Start(startInfo1);
                    try
                    {
                        m_pro = Process.Start(startInfo1);
                    }
                    catch
                    {
                        return false;//找不到執行檔的防呆 at 2017/06/16
                    }
                    m_pro.WaitForExit();//下載SERVER資料
                    System.IO.File.Delete(path);

                    blnAns = true;
                }
                else
                {
                    blnAns = false;
                }
            }
            m_pro = null;
            return blnAns;
        }
        //----
    }
    public class MySQL
    {
        public static string dbHost = "127.0.0.1";//資料庫位址
        public static string dbport = "3307";
        public static string dbUser = "root";//資料庫使用者帳號
        public static string dbPass = "usbw";//資料庫使用者密碼
        public static string dbName = "v8_workstation";//資料庫名稱
        public static string connStr = "server=" + dbHost + ";Port=" + dbport + ";uid=" + dbUser + ";pwd=" + dbPass + ";database=" + dbName + ";charset=utf8";
        public static MySqlConnection m_conn = null;
        public static String m_StrLastSQL = "";
        public static Process m_pro;
        public static bool startMySQL()
        {
            bool blnAns = true;
            if (m_pro == null)
            {
                string path = System.Windows.Forms.Application.StartupPath;
                path += "\\mysql\\bin\\mysqld_v8.exe";
                ProcessStartInfo startInfo = new ProcessStartInfo(path);
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                try
                {
                    m_pro = Process.Start(startInfo);
                }
                catch
                {
                    return false;//找不到執行檔的防呆 at 2017/06/16
                }
                Thread.Sleep(100);
                if (m_pro != null)
                {
                    if (!m_pro.HasExited)// 取得程式是否已執行完成
                    {
                        blnAns = true;
                    }
                    else
                    {
                        blnAns = false;
                    }
                }
            }
            return blnAns;
        }
        public static void stopMySQL()
        {
            if (m_pro != null)
            {
                if (!m_pro.HasExited)// 取得程式是否已執行完成
                {
                    m_pro.Kill();//關閉程式
                    m_pro.Close();//把m_pro清空被執行程式的資源,但m_pro實體存在
                    m_pro = null;// 清空m_pro實體
                }
            }
        }

        public static bool CheckMySQL()
        {
            bool blnAns = true;
            Thread.Sleep(1000);
            connStr = "server=" + dbHost + ";Port=" + dbport + ";uid=" + dbUser + ";pwd=" + dbPass + ";";
            m_conn = new MySqlConnection(connStr);
            try
            {
                m_conn.Open();
                if (m_conn.State == ConnectionState.Open)
                {
                    blnAns = true;
                    m_conn.Close();
                }
                else
                {
                    blnAns = false;
                }
            }
            catch
            {
                blnAns = false;
            }
            return blnAns;
        }
        public static bool initMySQLDB()
        {
            bool blnAns = true;
            startMySQL();//啟動MYSQL，啟動失敗 有可能已在執行，所以這裡不判斷
            if (CheckMySQL())//登錄MYSQL測試
            {
                if (!Open())//開啟資料庫
                {
                    blnAns = false;
                }
            }
            else
            {
                blnAns = false;
            }
            return blnAns;
        }
        public static void ClearTable(String name)
        {
            InsertUpdateDelete("DELETE FROM " + name + ";");
        }
        public static void CleanDB()//清空資料表
        {
            String StrSQL;
        }
        public static bool CreateDB(String DBName)
        {
            connStr = "server=" + dbHost + ";Port=" + dbport + ";uid=" + dbUser + ";pwd=" + dbPass + ";";
            m_conn = new MySqlConnection(connStr);
            try
            {
                m_conn.Open();
                if (m_conn.State == ConnectionState.Open)
                {
                    String s0 = String.Format("CREATE DATABASE IF NOT EXISTS `{0}`;", DBName);
                    MySqlCommand cmd = new MySqlCommand(s0, m_conn);
                    cmd.ExecuteNonQuery();
                    m_conn.Close();
                    dbName = DBName;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
        public static bool Open(String StrHost, String port, String User, String PassWord, String DataBase)
        {
            dbHost = StrHost;//資料庫位址
            dbport = port;
            dbUser = User;//資料庫使用者帳號
            dbPass = PassWord;//資料庫使用者密碼
            dbName = DataBase;//資料庫名稱
            connStr = "server=" + dbHost + ";Port=" + dbport + ";uid=" + dbUser + ";pwd=" + dbPass + ";database=" + dbName + ";charset=utf8";
            return Open();
        }
        public static bool Open()
        {
            connStr = "server=" + dbHost + ";Port=" + dbport + ";uid=" + dbUser + ";pwd=" + dbPass + ";database=" + dbName + ";charset=utf8";
            m_conn = new MySqlConnection(connStr);
            try
            {
                m_conn.Open();
                if (m_conn.State == ConnectionState.Open)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
        public static bool InsertUpdateDelete(String SqlString)//新增資料程式
        {
            bool blnAns = false;
            MySqlTransaction transaction = m_conn.BeginTransaction();
            try
            {
                MySqlCommand cmd = new MySqlCommand(SqlString, m_conn, transaction);
                cmd.ExecuteNonQuery();
                transaction.Commit();//交易完成
                m_StrLastSQL = SqlString;//紀錄最後一次執行成功的SQL，除錯用
                blnAns = true;
            }
            catch
            {
                transaction.Rollback();//取消交易，復原至交易前
                blnAns = false;
            }
            return blnAns;
        }
        public static MySqlDataReader GetDataReader(String SqlString)//讀取資料程式
        {
            MySqlDataReader reader = null;
            try
            {
                MySqlCommand cmd = new MySqlCommand(SqlString, m_conn);
                reader = cmd.ExecuteReader();
                /*
                while (reader.Read())
                {
                    String Str1 = reader["uid"].ToString();
                    String Str2 = reader["ID"].ToString();
                    String Str3 = reader["C_name"].ToString();
                }
                */
                m_StrLastSQL = SqlString;//紀錄最後一次執行成功的SQL，除錯用
            }
            catch
            {
                reader.Close();
            }
            return reader;//--使用完釋放資源 reader.Close();
        }
        public static DataTable GetDataTable(String SqlString)
        {
            DataTable myDataTable = new DataTable();

            MySqlCommand cmd = new MySqlCommand();
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Connection = m_conn;
            cmd.CommandText = SqlString;
            cmd.CommandTimeout = 600;
            DataSet ds = new DataSet();
            ds.Clear();
            da.Fill(ds);
            myDataTable = ds.Tables[0];

            m_StrLastSQL = SqlString;//紀錄最後一次執行成功的SQL，除錯用
            return myDataTable;
        }

        public static bool CheckDateTimeType(string txtDateStart)
        {
            DateTime sd;//供判斷暫存之用
            if (String.IsNullOrEmpty(txtDateStart) || !DateTime.TryParse(txtDateStart, out sd))
            {
                return false;
            }
            return true;
        }
    }
}
