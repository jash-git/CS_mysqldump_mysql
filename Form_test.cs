using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SYWEB_V8_Workstation
{
    public partial class Form_test : Form
    {
        public Form_test()
        {
            InitializeComponent();
        }

        private void Form_test_Load(object sender, EventArgs e)
        {
            External_MySQL m_ExMySQL = new External_MySQL();
            bool blnAns = m_ExMySQL.CheckMySQL("127.0.0.1", "3306", "root", "usbw");
            blnAns = m_ExMySQL.DownloadDBTable("card", "REPLACE INTO", true);
            blnAns = m_ExMySQL.UploadDBTable("card", "REPLACE INTO",true);
        }
    }
}
