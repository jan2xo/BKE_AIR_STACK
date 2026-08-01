using System;
using System.Collections.Specialized;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using System.Xml;


namespace BKE_Air_Stack
{
    public partial class Form3 : Form
    {
        FolderBrowserDialog openfolder = new FolderBrowserDialog();
        SQLiteDataAdapter DB;
        DataSet DS = new DataSet();
        DataTable DT = new DataTable();
        SQLiteDataReader dr;
        string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BKE AirStack/DataBase/BKEAirStack.db";
        string cs = @"URI=file:" + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BKE AirStack\\DataBase\\BKEAirStack.db";
        public Form3()
        {
            InitializeComponent();
        }
        private void data_show()
        {
            var con = new SQLiteConnection(cs);
            con.Open();

            string stm = "SELECT * FROM Adsdata";
            DB = new SQLiteDataAdapter(stm, con);
            DS.Reset();
            DB.Fill(DS);
            DT = DS.Tables[0];
            adsGV.DataSource = DT;
            con.Close();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var con = new SQLiteConnection(cs);
            con.Open();
            var cmd = new SQLiteCommand(con);
            cmd.CommandText = "DELETE FROM Adsdata";
            cmd.ExecuteNonQuery();

            if (openfolder.ShowDialog() == DialogResult.OK)
            {
                string folderpath = openfolder.SelectedPath;

                
                foreach (string adsfile in Directory.GetFiles(folderpath))
                {
                    string adsfilename = Path.GetFileName(adsfile);
                    try
                    {
                        
                        cmd.CommandText = "INSERT INTO Adsdata(ADVERTISEMENT,ADVERTISEMENT_NAME) Values (@ADVERTISEMENT,@ADVERTISEMENT_NAME)";
                        cmd.Parameters.AddWithValue("@ADVERTISEMENT", adsfile);
                        cmd.Parameters.AddWithValue("@ADVERTISEMENT_NAME", adsfilename);
                        cmd.ExecuteNonQuery();
                        
                    }
                    catch (Exception)
                    {
                        return;
                    }
                    continue;
                }
                data_show();
                XML_Data();

            }
        }
        private void XML_Data()
        {

            DS.DataSetName = "Adsdata";
            DT.WriteXml(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BKE AirStack/Data_Source/Adsdata.xml");
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            data_show();
        }
    }
}
