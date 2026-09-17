using System;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Data;
using System.Security.Cryptography;
using System.Windows.Forms.VisualStyles;
using System.IO;
using System.ComponentModel;

namespace BKE_Air_Stack
{
    public partial class Form2 : Form
    {
        public static Form2 form2instance;
        public Label schedlabel;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string schedname { get; set; }


        SQLiteConnection con;
        SQLiteCommand cmd;
        SQLiteDataReader dr;
        SQLiteDataReader dr2;
        SQLiteDataAdapter DB;
        DataSet DS = new DataSet();
        DataTable DT = new DataTable();

        string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BKE AirStack/DataBase/BKEAirStack.db";
        string cs = @"URI=file:" + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BKE AirStack\\DataBase\\BKEAirStack.db";
        int buttonshow = 0;
        int sqlid;
        int rid1;
        int rid2;
        string logofilename;
        string obbfilename;
        string bedfilename;
        string bgfilename;
        string cbbfilename;



        public Form2()
        {
            InitializeComponent();
            form2instance = this;
            schedlabel = sched_label;


        }

        private void data_show()
        {
            var con = new SQLiteConnection(cs);
            con.Open();

            string stm = "SELECT * FROM " + schedname;
            DB = new SQLiteDataAdapter(stm, con);
            DS.Reset();
            DB.Fill(DS);
            DT = DS.Tables[0];
            dataGV.DataSource = DT;
            con.Close();
        }


        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form2_Load(object sender, EventArgs e)

        {

            data_show();

        }

        private void InsertBtn_Click(object sender, EventArgs e)
        {
            var con = new SQLiteConnection(cs);
            con.Open();

            string command = "INSERT INTO " + schedname + " (Title,Time,Episode,PLFileName,PLPath) values (@Title,@Time,@Episode,@PLFileName,@PLPath)";
            SQLiteCommand cmd = new SQLiteCommand(command, con);
            cmd.Parameters.AddWithValue("@Title", title_txt.Text);
            cmd.Parameters.AddWithValue("@Time", time_txt.Text);
            cmd.Parameters.AddWithValue("@Episode", episode_txt.Text);
            cmd.Parameters.AddWithValue("@PLFileName", plfilename_txt.Text);
            cmd.Parameters.AddWithValue("@PLPath", plpath_txt.Text);
            cmd.ExecuteNonQuery();
            data_show();
            con.Close();

        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            var con = new SQLiteConnection(cs);
            con.Open();
            string command = "DELETE FROM " + schedname + " WHERE id = @id";
            SQLiteCommand cmd = new SQLiteCommand(command, con);
            cmd.Parameters.AddWithValue("@id", sqlid);
            cmd.ExecuteNonQuery();
            data_show();
            con.Close();

        }

        private void dataGV_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGV_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex != -1)
                {
                    DataGridViewRow row = dataGV.Rows[e.RowIndex];
                    sqlid = Convert.ToInt32(row.Cells[0].Value.ToString());
                    title_txt.Text = row.Cells[1].Value.ToString();
                    time_txt.Text = row.Cells[2].Value.ToString();
                    episode_txt.Text = row.Cells[3].Value.ToString();
                    plfilename_txt.Text = row.Cells[4].Value.ToString();
                    plpath_txt.Text = row.Cells[5].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            var con = new SQLiteConnection(cs);
            con.Open();
            string command = "UPDATE " + schedname + " SET Title = @Title, Time = @Time, Episode = @Episode, PLFileName = @PLFileName, PLPath = @PLPath WHERE id = @id";
            SQLiteCommand cmd = new SQLiteCommand(command, con);
            cmd.Parameters.AddWithValue("@Title", title_txt.Text);
            cmd.Parameters.AddWithValue("@Time", time_txt.Text);
            cmd.Parameters.AddWithValue("@Episode", episode_txt.Text);
            cmd.Parameters.AddWithValue("@PLFileName", plfilename_txt.Text);
            cmd.Parameters.AddWithValue("@PLPath", plpath_txt.Text);
            cmd.Parameters.AddWithValue("@id", sqlid);
            cmd.ExecuteNonQuery();
            data_show();
            con.Close();
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            title_txt.Clear();
            time_txt.Clear();
            episode_txt.Clear();
            plfilename_txt.Clear();
            plpath_txt.Clear();
        }

        private void PlaylistBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "Select Playlist file";
            fdlg.InitialDirectory = @"c:\";
            fdlg.Filter = "XPL File (*.xpl)|*.xpl|All files (*.*)|*.*";
            fdlg.FilterIndex = 1;
            fdlg.RestoreDirectory = true;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                plfilename_txt.Text = fdlg.SafeFileName;
                plpath_txt.Text = fdlg.FileName;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var con = new SQLiteConnection(cs);
            con.Open();
            string command = "SELECT max(id) FROM " + schedname;
            SQLiteCommand cmd = new SQLiteCommand(command, con);
            SQLiteDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                rid1 = dr.GetInt32(0);
            }
            dr.Close();
            con.Close();
            MessageBox.Show(rid1.ToString());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var con = new SQLiteConnection(cs);
            con.Open();
            string command = "SELECT min(id) FROM " + schedname;
            SQLiteCommand cmd = new SQLiteCommand(command, con);
            SQLiteDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                rid2 = dr.GetInt32(0);
            }
            dr.Close();
            con.Close();
            MessageBox.Show(rid2.ToString());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show(schedname);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
    }
}
