using System;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Data;
using System.Security.Cryptography;
using System.Windows.Forms.VisualStyles;
using System.IO;

namespace BKE_Air_Stack
{
    public partial class Form2 : Form
    {
        public static Form2 form2instance;
        public Label schedlabel;
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
            var cmd = new SQLiteCommand(con);
            try
            {
                cmd.CommandText = "INSERT INTO " + schedname + "(PROGRAM_NAME,ANCHOR1_NAME,ANCHOR1_POSITION,ANCHOR2_NAME,ANCHOR2_POSITION,ANCHOR3_NAME,ANCHOR3_POSITION,LOGO,OBB,BEDDING,BACKGROUND,CBB,LOGO_FN,OBB_FN,BEDDING_FN,BACKGROUND_FN,CBB_FN) VALUES(@PROGRAM_NAME,@ANCHOR1_NAME,@ANCHOR1_POSITION,@ANCHOR2_NAME,@ANCHOR2_POSITION,@ANCHOR3_NAME,@ANCHOR3_POSITION,@LOGO,@OBB,@BEDDING,@BACKGROUND,@CBB,@LOGO_FN,@OBB_FN,@BEDDING_FN,@BACKGROUND_FN,@CBB_FN)";

                string PN = tBProgram.Text;
                string A1 = tBAnchor1.Text;
                string[] A1split = A1.Split(',');
                string A1N = A1split[0];
                string A1P = A1split[1];
                string A2 = tBAnchor2.Text;
                string[] A2split = A2.Split(',');
                string A2N = A2split[0];
                string A2P = A2split[1];
                string A3 = tBAnchor3.Text;
                string[] A3split = A3.Split(',');
                string A3N = A3split[0];
                string A3P = A3split[1];
                string L = tBLogo.Text;
                string O = tBOBB.Text;
                string B = tBBedding.Text;
                string Bg = tBBG.Text;
                string C = tBCBB.Text;
                // string LFN = lblLogo_Name.Text;
                //string OFN = lblObb_Name.Text;
                //string BFN = lblBedding_Name.Text;

                cmd.Parameters.AddWithValue("@PROGRAM_NAME", PN);
                cmd.Parameters.AddWithValue("@ANCHOR1_NAME", A1N);
                cmd.Parameters.AddWithValue("@ANCHOR1_POSITION", A1P);
                cmd.Parameters.AddWithValue("@ANCHOR2_NAME", A2N);
                cmd.Parameters.AddWithValue("@ANCHOR2_POSITION", A2P);
                cmd.Parameters.AddWithValue("@ANCHOR3_NAME", A3N);
                cmd.Parameters.AddWithValue("@ANCHOR3_POSITION", A3P);
                cmd.Parameters.AddWithValue("@LOGO", L);
                cmd.Parameters.AddWithValue("@OBB", O);
                cmd.Parameters.AddWithValue("@BEDDING", B);
                cmd.Parameters.AddWithValue("@BACKGROUND", Bg);
                cmd.Parameters.AddWithValue("@CBB", C);
                cmd.Parameters.AddWithValue("@LOGO_FN", logofilename);
                cmd.Parameters.AddWithValue("@OBB_FN", obbfilename);
                cmd.Parameters.AddWithValue("@BEDDING_FN", bedfilename);
                cmd.Parameters.AddWithValue("@BACKGROUND_FN", bgfilename);
                cmd.Parameters.AddWithValue("@CBB_FN", cbbfilename);
                cmd.ExecuteNonQuery();
                data_show();
                XML_Data();
                dataGV.CurrentCell.Selected = false;
                MessageBox.Show("Data inserted", "System", MessageBoxButtons.OK, MessageBoxIcon.Information);



            }
            catch (Exception)
            {
                MessageBox.Show("Double check data, Please retry!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            var con = new SQLiteConnection(cs);
            con.Open();
            var cmd = new SQLiteCommand(con);
            try
            {
                cmd.CommandText = "UPDATE " + schedname + " SET PROGRAM_NAME=@PROGRAM_NAME, ANCHOR1_NAME=@ANCHOR1_NAME, ANCHOR1_POSITION=@ANCHOR1_POSITION, ANCHOR2_NAME=@ANCHOR2_NAME, ANCHOR2_POSITION=@ANCHOR2_POSITION, ANCHOR3_NAME=@ANCHOR3_NAME, ANCHOR3_POSITION=@ANCHOR3_POSITION, LOGO=@LOGO, OBB=@OBB, BEDDING=@BEDDING, BACKGROUND=@BACKGROUND, CBB=@CBB, LOGO_FN=@LOGO_FN, OBB_FN=@OBB_FN, BEDDING_FN=@BEDDING_FN, BACKGROUND_FN=@BACKGROUND_FN, CBB_FN=@CBB_FN where id = '" + sqlid + "'";
                cmd.Prepare();
                string PN = tBProgram.Text;
                string A1 = tBAnchor1.Text;
                string[] A1split = A1.Split(',');
                string A1N = A1split[0];
                string A1P = A1split[1];
                string A2 = tBAnchor2.Text;
                string[] A2split = A2.Split(',');
                string A2N = A2split[0];
                string A2P = A2split[1];
                string A3 = tBAnchor3.Text;
                string[] A3split = A3.Split(',');
                string A3N = A3split[0];
                string A3P = A3split[1];
                string L = tBLogo.Text;
                string O = tBOBB.Text;
                string B = tBBedding.Text;
                string Bg = tBBG.Text;
                string C = tBCBB.Text;
                //string LFN = lblLogo_Name.Text;
                //string OFN = lblObb_Name.Text;
                //string BFN = lblBedding_Name.Text;

                cmd.Parameters.AddWithValue("@PROGRAM_NAME", PN);
                cmd.Parameters.AddWithValue("@ANCHOR1_NAME", A1N);
                cmd.Parameters.AddWithValue("@ANCHOR1_POSITION", A1P);
                cmd.Parameters.AddWithValue("@ANCHOR2_NAME", A2N);
                cmd.Parameters.AddWithValue("@ANCHOR2_POSITION", A2P);
                cmd.Parameters.AddWithValue("@ANCHOR3_NAME", A3N);
                cmd.Parameters.AddWithValue("@ANCHOR3_POSITION", A3P);
                cmd.Parameters.AddWithValue("@LOGO", L);
                cmd.Parameters.AddWithValue("@OBB", O);
                cmd.Parameters.AddWithValue("@BEDDING", B);
                cmd.Parameters.AddWithValue("@BACKGROUND", Bg);
                cmd.Parameters.AddWithValue("@CBB", C);
                cmd.Parameters.AddWithValue("@LOGO_FN", logofilename);
                cmd.Parameters.AddWithValue("@OBB_FN", obbfilename);
                cmd.Parameters.AddWithValue("@BEDDING_FN", bedfilename);
                cmd.Parameters.AddWithValue("@BACKGROUND_FN", bgfilename);
                cmd.Parameters.AddWithValue("@CBB_FN", cbbfilename);
                cmd.ExecuteNonQuery();
                data_show();
                XML_Data();
                dataGV.CurrentCell.Selected = false;
                MessageBox.Show("Data updated", "System", MessageBoxButtons.OK, MessageBoxIcon.Information);


            }
            catch (Exception)
            {
                MessageBox.Show("Cannot update data!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            var con = new SQLiteConnection(cs);
            con.Open();
            var cmd = new SQLiteCommand(con);
            try
            {
                if (rid1 == rid2 && dataGV.CurrentCell != null)
                {
                    string PN = tBProgram.Text;
                    cmd.CommandText = "DELETE FROM " + schedname + " where PROGRAM_NAME=@PROGRAM_NAME";
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@PROGRAM_NAME", PN);
                    cmd.ExecuteNonQuery();
                    data_show();
                    XML_Data();
                    if (dataGV.CurrentCell != null)
                    {
                        dataGV.CurrentCell.Selected = false;
                    }

                    MessageBox.Show("Succesfully Deleted", "System", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                else
                {
                    if (dataGV.CurrentCell == null)
                    {
                        MessageBox.Show("No data!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                    else
                    {
                        MessageBox.Show("Cannot delete data!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        MessageBox.Show("Please move the data to the bottom list", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }


                }

            }
            catch (Exception)
            {
                MessageBox.Show("Cannot delete data!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void dataGV_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            try
            {
                if (dataGV.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
                {

                    string PN = tBProgram.Text;
                    string A1 = tBAnchor1.Text;
                    string A2 = tBAnchor2.Text;
                    string A3 = tBAnchor3.Text;
                    string L = tBLogo.Text;
                    string O = tBOBB.Text;
                    string B = tBBedding.Text;
                    string Bg = tBBG.Text;
                    string C = tBCBB.Text;
                    dataGV.CurrentRow.Selected = true;
                    tBProgram.Text = dataGV.Rows[e.RowIndex].Cells["PROGRAM_NAME"].FormattedValue.ToString();
                    tBAnchor1.Text = dataGV.Rows[e.RowIndex].Cells["ANCHOR1_NAME"].FormattedValue.ToString() + "," + dataGV.Rows[e.RowIndex].Cells["ANCHOR1_POSITION"].FormattedValue.ToString();
                    tBAnchor2.Text = dataGV.Rows[e.RowIndex].Cells["ANCHOR2_NAME"].FormattedValue.ToString() + "," + dataGV.Rows[e.RowIndex].Cells["ANCHOR2_POSITION"].FormattedValue.ToString();
                    tBAnchor3.Text = dataGV.Rows[e.RowIndex].Cells["ANCHOR3_NAME"].FormattedValue.ToString() + "," + dataGV.Rows[e.RowIndex].Cells["ANCHOR3_POSITION"].FormattedValue.ToString();
                    tBLogo.Text = dataGV.Rows[e.RowIndex].Cells["LOGO"].FormattedValue.ToString();
                    tBOBB.Text = dataGV.Rows[e.RowIndex].Cells["OBB"].FormattedValue.ToString();
                    tBBedding.Text = dataGV.Rows[e.RowIndex].Cells["BEDDING"].FormattedValue.ToString();
                    tBBG.Text = dataGV.Rows[e.RowIndex].Cells["BACKGROUND"].FormattedValue.ToString();
                    tBCBB.Text = dataGV.Rows[e.RowIndex].Cells["CBB"].FormattedValue.ToString();
                    string sqlrowid = dataGV.Rows[e.RowIndex].Cells["id"].FormattedValue.ToString();
                    logofilename = dataGV.Rows[e.RowIndex].Cells["LOGO_FN"].FormattedValue.ToString();
                    obbfilename = dataGV.Rows[e.RowIndex].Cells["OBB_FN"].FormattedValue.ToString();
                    bedfilename = dataGV.Rows[e.RowIndex].Cells["BEDDING_FN"].FormattedValue.ToString();
                    bgfilename = dataGV.Rows[e.RowIndex].Cells["BACKGROUND_FN"].FormattedValue.ToString();
                    cbbfilename = dataGV.Rows[e.RowIndex].Cells["CBB_FN"].FormattedValue.ToString();
                    sqlid = int.Parse(sqlrowid);
                    buttonshow = dataGV.CurrentRow.Index;
                    rid1 = dataGV.RowCount;
                    rid2 = dataGV.CurrentRow.Index + 2;


                    if (buttonshow >= 1 && buttonshow + 1 != rid1)
                    {
                        sqlupBtn.Visible = true;
                    }
                    else
                    {
                        sqlupBtn.Visible = false;
                    }
                    if (buttonshow + 2 == rid1)
                    {
                        sqldownBtn.Visible = false;
                    }
                    else

                    {
                        sqldownBtn.Visible = true;
                    }
                }


            }
            catch (Exception)
            {
                return;
            }
        }

        private void tBLogo_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void tBLogo_DragDrop(object sender, DragEventArgs e)
        {
            string[] dragdropfiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            tBLogo.Text = dragdropfiles[0];
            //lblLogo_Name.Text = Path.GetFileName(dragdropfiles[0]);
            logofilename = Path.GetFileName(dragdropfiles[0]);


        }

        private void tBOBB_DragDrop(object sender, DragEventArgs e)
        {
            string[] dragdropfiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            tBOBB.Text = dragdropfiles[0];
            obbfilename = Path.GetFileName(dragdropfiles[0]);
        }

        private void tBBedding_DragDrop(object sender, DragEventArgs e)
        {
            string[] dragdropfiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            tBBedding.Text = dragdropfiles[0];
            bedfilename = Path.GetFileName(dragdropfiles[0]);
        }

        private void tBBG_DragDrop(object sender, DragEventArgs e)
        {
            string[] dragdropfiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            tBBG.Text = dragdropfiles[0];
            bgfilename = Path.GetFileName(dragdropfiles[0]);
        }

        private void tBCBB_DragDrop(object sender, DragEventArgs e)
        {
            string[] dragdropfiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            tBCBB.Text = dragdropfiles[0];
            cbbfilename = Path.GetFileName(dragdropfiles[0]);
        }




        private void sqlupBtn_Click(object sender, EventArgs e)
        {
            string PNold = "";
            string A1Nold = "";
            string A1Pold = "";
            string A2Nold = "";
            string A2Pold = "";
            string A3Nold = "";
            string A3Pold = "";
            string Lold = "";
            string Oold = "";
            string Bold = "";
            string Cold = "";
            string Bgold = "";
            string oldlogo = "";
            string oldobb = "";
            string oldbed = "";
            string oldbg = "";
            string oldcbb = "";


            string PNnew = "";
            string A1Nnew = "";
            string A1Pnew = "";
            string A2Nnew = "";
            string A2Pnew = "";
            string A3Nnew = "";
            string A3Pnew = "";
            string Lnew = "";
            string Onew = "";
            string Bnew = "";
            string CBnew = "";
            string Bgnew = "";
            string newlogo = "";
            string newobb = "";
            string newbed = "";
            string newbg = "";
            string newcbb = "";
            int rid = dataGV.CurrentRow.Index + 1;
            if (buttonshow >= 1 && buttonshow + 1 != rid1)
            {
                var con = new SQLiteConnection(cs);
                con.Open();
                var cmd = new SQLiteCommand(con);
                string stm = "SELECT * FROM " + schedname + " Where id = '" + rid.ToString() + "'";
                string stm2 = "SELECT * FROM " + schedname + " Where id = '" + (rid - 1).ToString() + "'";
                var cmd2 = new SQLiteCommand(stm, con);
                var cmd3 = new SQLiteCommand(stm2, con);
                dr = cmd2.ExecuteReader();
                dr2 = cmd3.ExecuteReader();
                while (dr.Read())
                {
                    PNold = dr.GetValue(0).ToString();
                    A1Nold = dr.GetValue(1).ToString();
                    A1Pold = dr.GetValue(2).ToString();
                    A2Nold = dr.GetValue(3).ToString();
                    A2Pold = dr.GetValue(4).ToString();
                    A3Nold = dr.GetValue(5).ToString();
                    A3Pold = dr.GetValue(6).ToString();
                    Lold = dr.GetValue(7).ToString();
                    Oold = dr.GetValue(8).ToString();
                    Bold = dr.GetValue(9).ToString();
                    Bgold = dr.GetValue(10).ToString();
                    Cold = dr.GetValue(11).ToString();
                    oldlogo = dr.GetValue(13).ToString();
                    oldobb = dr.GetValue(14).ToString();
                    oldbed = dr.GetValue(15).ToString();
                    oldbg = dr.GetValue(16).ToString();
                    oldcbb = dr.GetValue(17).ToString();
                }
                while (dr2.Read())
                {
                    PNnew = dr2.GetValue(0).ToString();
                    A1Nnew = dr2.GetValue(1).ToString();
                    A1Pnew = dr2.GetValue(2).ToString();
                    A2Nnew = dr2.GetValue(3).ToString();
                    A2Pnew = dr2.GetValue(4).ToString();
                    A3Nnew = dr2.GetValue(5).ToString();
                    A3Pnew = dr2.GetValue(6).ToString();
                    Lnew = dr2.GetValue(7).ToString();
                    Onew = dr2.GetValue(8).ToString();
                    Bnew = dr2.GetValue(9).ToString();
                    Bgnew = dr2.GetValue(10).ToString();
                    CBnew = dr2.GetValue(11).ToString();
                    newlogo = dr2.GetValue(13).ToString();
                    newobb = dr2.GetValue(14).ToString();
                    newbed = dr2.GetValue(15).ToString();
                    newbg = dr2.GetValue(16).ToString();
                    newcbb = dr2.GetValue(17).ToString();

                }
                try
                {
                    cmd.CommandText = "UPDATE " + schedname + " SET PROGRAM_NAME=@PROGRAM_NAME, ANCHOR1_NAME=@ANCHOR1_NAME, ANCHOR1_POSITION=@ANCHOR1_POSITION, ANCHOR2_NAME=@ANCHOR2_NAME, ANCHOR2_POSITION=@ANCHOR2_POSITION, ANCHOR3_NAME=@ANCHOR3_NAME, ANCHOR3_POSITION=@ANCHOR3_POSITION, LOGO=@LOGO, OBB=@OBB, BEDDING=@BEDDING, BACKGROUND=@BACKGROUND, CBB=@CBB, LOGO_FN=@LOGO_FN, OBB_FN=@OBB_FN, BEDDING_FN=@BEDDING_FN, BACKGROUND_FN=@BACKGROUND_FN, CBB_FN=@CBB_FN where id = '" + (rid - 1).ToString() + "'";
                    cmd.Prepare();

                    cmd.Parameters.AddWithValue("@PROGRAM_NAME", PNold);
                    cmd.Parameters.AddWithValue("@ANCHOR1_NAME", A1Nold);
                    cmd.Parameters.AddWithValue("@ANCHOR1_POSITION", A1Pold);
                    cmd.Parameters.AddWithValue("@ANCHOR2_NAME", A2Nold);
                    cmd.Parameters.AddWithValue("@ANCHOR2_POSITION", A2Pold);
                    cmd.Parameters.AddWithValue("@ANCHOR3_NAME", A3Nold);
                    cmd.Parameters.AddWithValue("@ANCHOR3_POSITION", A3Pold);
                    cmd.Parameters.AddWithValue("@LOGO", Lold);
                    cmd.Parameters.AddWithValue("@OBB", Oold);
                    cmd.Parameters.AddWithValue("@BEDDING", Bold);
                    cmd.Parameters.AddWithValue("@BACKGROUND", Bgold);
                    cmd.Parameters.AddWithValue("@CBB", Cold);
                    cmd.Parameters.AddWithValue("@LOGO_FN", oldlogo);
                    cmd.Parameters.AddWithValue("@OBB_FN", oldobb);
                    cmd.Parameters.AddWithValue("@BEDDING_FN", oldbed);
                    cmd.Parameters.AddWithValue("@BACKGROUND_FN", oldbg);
                    cmd.Parameters.AddWithValue("@CBB_FN", oldcbb);
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "UPDATE " + schedname + " SET PROGRAM_NAME=@PROGRAM_NAME, ANCHOR1_NAME=@ANCHOR1_NAME, ANCHOR1_POSITION=@ANCHOR1_POSITION, ANCHOR2_NAME=@ANCHOR2_NAME, ANCHOR2_POSITION=@ANCHOR2_POSITION, ANCHOR3_NAME=@ANCHOR3_NAME, ANCHOR3_POSITION=@ANCHOR3_POSITION, LOGO=@LOGO, OBB=@OBB, BEDDING=@BEDDING, BACKGROUND=@BACKGROUND, CBB=@CBB, LOGO_FN=@LOGO_FN, OBB_FN=@OBB_FN, BEDDING_FN=@BEDDING_FN, BACKGROUND_FN=@BACKGROUND_FN, CBB_FN=@CBB_FN where id = '" + rid.ToString() + "'";
                    cmd.Prepare();

                    cmd.Parameters.AddWithValue("@PROGRAM_NAME", PNnew);
                    cmd.Parameters.AddWithValue("@ANCHOR1_NAME", A1Nnew);
                    cmd.Parameters.AddWithValue("@ANCHOR1_POSITION", A1Pnew);
                    cmd.Parameters.AddWithValue("@ANCHOR2_NAME", A2Nnew);
                    cmd.Parameters.AddWithValue("@ANCHOR2_POSITION", A2Pnew);
                    cmd.Parameters.AddWithValue("@ANCHOR3_NAME", A3Nnew);
                    cmd.Parameters.AddWithValue("@ANCHOR3_POSITION", A3Pnew);
                    cmd.Parameters.AddWithValue("@LOGO", Lnew);
                    cmd.Parameters.AddWithValue("@OBB", Onew);
                    cmd.Parameters.AddWithValue("@BEDDING", Bnew);
                    cmd.Parameters.AddWithValue("@BACKGROUND", Bgnew);
                    cmd.Parameters.AddWithValue("@CBB", CBnew);
                    cmd.Parameters.AddWithValue("@LOGO_FN", newlogo);
                    cmd.Parameters.AddWithValue("@OBB_FN", newobb);
                    cmd.Parameters.AddWithValue("@BEDDING_FN", newbed);
                    cmd.Parameters.AddWithValue("@BACKGROUND_FN", newbg);
                    cmd.Parameters.AddWithValue("@CBB_FN", newcbb);
                    cmd.ExecuteNonQuery();
                    data_show();
                    XML_Data();
                    dataGV.CurrentCell.Selected = false;
                    sqlupBtn.Visible = false;
                    sqldownBtn.Visible = false;
                    string PN = tBProgram.Text;
                    string A1 = tBAnchor1.Text;
                    string A2 = tBAnchor2.Text;
                    string A3 = tBAnchor3.Text;
                    string L = tBLogo.Text;
                    string O = tBOBB.Text;
                    string B = tBBedding.Text;
                    string Bg = tBBG.Text;

                    tBProgram.Text = dataGV.Rows[rid - 2].Cells["PROGRAM_NAME"].FormattedValue.ToString();
                    tBAnchor1.Text = dataGV.Rows[rid - 2].Cells["ANCHOR1_NAME"].FormattedValue.ToString() + "," + dataGV.Rows[rid - 2].Cells["ANCHOR1_POSITION"].FormattedValue.ToString();
                    tBAnchor2.Text = dataGV.Rows[rid - 2].Cells["ANCHOR2_NAME"].FormattedValue.ToString() + "," + dataGV.Rows[rid - 2].Cells["ANCHOR2_POSITION"].FormattedValue.ToString();
                    tBAnchor3.Text = dataGV.Rows[rid - 2].Cells["ANCHOR3_NAME"].FormattedValue.ToString() + "," + dataGV.Rows[rid - 2].Cells["ANCHOR3_POSITION"].FormattedValue.ToString();
                    tBLogo.Text = dataGV.Rows[rid - 2].Cells["LOGO"].FormattedValue.ToString();
                    tBOBB.Text = dataGV.Rows[rid - 2].Cells["OBB"].FormattedValue.ToString();
                    tBBedding.Text = dataGV.Rows[rid - 2].Cells["BEDDING"].FormattedValue.ToString();
                    tBBG.Text = dataGV.Rows[rid - 2].Cells["BACKGROUND"].FormattedValue.ToString();
                    tBCBB.Text = dataGV.Rows[rid - 2].Cells["CBB"].FormattedValue.ToString();
                    string sqlrowid = dataGV.Rows[rid - 2].Cells["id"].FormattedValue.ToString();
                    logofilename = dataGV.Rows[rid - 2].Cells["LOGO_FN"].FormattedValue.ToString();
                    obbfilename = dataGV.Rows[rid - 2].Cells["OBB_FN"].FormattedValue.ToString();
                    bedfilename = dataGV.Rows[rid - 2].Cells["BEDDING_FN"].FormattedValue.ToString();
                    bgfilename = dataGV.Rows[rid - 2].Cells["BACKGROUND_FN"].FormattedValue.ToString();
                    sqlid = int.Parse(sqlrowid);
                    buttonshow = dataGV.CurrentRow.Index;
                    rid1 = dataGV.RowCount;
                    rid2 = dataGV.CurrentRow.Index + 2;
                }
                catch (Exception)
                {
                    return;
                }

            }
        }
        private void highlight_text()
        {

        }
        private void dataGV_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {
            dataGV.Columns[e.Column.Index].SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        private void sqldownBtn_Click(object sender, EventArgs e)
        {
            string PNold = "";
            string A1Nold = "";
            string A1Pold = "";
            string A2Nold = "";
            string A2Pold = "";
            string A3Nold = "";
            string A3Pold = "";
            string Lold = "";
            string Oold = "";
            string Bold = "";
            string Cold = "";
            string Bgold = "";
            string oldlogo = "";
            string oldobb = "";
            string oldbed = "";
            string oldbg = "";
            string oldcbb = "";


            string PNnew = "";
            string A1Nnew = "";
            string A1Pnew = "";
            string A2Nnew = "";
            string A2Pnew = "";
            string A3Nnew = "";
            string A3Pnew = "";
            string Lnew = "";
            string Onew = "";
            string Bnew = "";
            string CBnew = "";
            string Bgnew = "";
            string newlogo = "";
            string newobb = "";
            string newbed = "";
            string newbg = "";
            string newcbb = "";
            int rid = dataGV.CurrentRow.Index + 1;
            try
            {
                var con = new SQLiteConnection(cs);
                con.Open();
                var cmd = new SQLiteCommand(con);
                string stm = "SELECT * FROM " + schedname + " Where id = '" + rid.ToString() + "'";
                string stm2 = "SELECT * FROM " + schedname + " Where id = '" + (rid + 1).ToString() + "'";
                var cmd2 = new SQLiteCommand(stm, con);
                var cmd3 = new SQLiteCommand(stm2, con);
                dr = cmd2.ExecuteReader();
                dr2 = cmd3.ExecuteReader();
                while (dr.Read())
                {
                    PNold = dr.GetValue(0).ToString();
                    A1Nold = dr.GetValue(1).ToString();
                    A1Pold = dr.GetValue(2).ToString();
                    A2Nold = dr.GetValue(3).ToString();
                    A2Pold = dr.GetValue(4).ToString();
                    A3Nold = dr.GetValue(5).ToString();
                    A3Pold = dr.GetValue(6).ToString();
                    Lold = dr.GetValue(7).ToString();
                    Oold = dr.GetValue(8).ToString();
                    Bold = dr.GetValue(9).ToString();
                    Bgold = dr.GetValue(10).ToString();
                    Cold = dr.GetValue(11).ToString();
                    oldlogo = dr.GetValue(13).ToString();
                    oldobb = dr.GetValue(14).ToString();
                    oldbed = dr.GetValue(15).ToString();
                    oldbg = dr.GetValue(16).ToString();
                    oldcbb = dr.GetValue(17).ToString();
                }
                while (dr2.Read())
                {
                    PNnew = dr2.GetValue(0).ToString();
                    A1Nnew = dr2.GetValue(1).ToString();
                    A1Pnew = dr2.GetValue(2).ToString();
                    A2Nnew = dr2.GetValue(3).ToString();
                    A2Pnew = dr2.GetValue(4).ToString();
                    A3Nnew = dr2.GetValue(5).ToString();
                    A3Pnew = dr2.GetValue(6).ToString();
                    Lnew = dr2.GetValue(7).ToString();
                    Onew = dr2.GetValue(8).ToString();
                    Bnew = dr2.GetValue(9).ToString();
                    Bgnew = dr2.GetValue(10).ToString();
                    CBnew = dr2.GetValue(11).ToString();
                    newlogo = dr2.GetValue(13).ToString();
                    newobb = dr2.GetValue(14).ToString();
                    newbed = dr2.GetValue(15).ToString();
                    newbg = dr2.GetValue(16).ToString();
                    newcbb = dr2.GetValue(17).ToString();
                }
                try
                {
                    cmd.CommandText = "UPDATE " + schedname + " SET PROGRAM_NAME=@PROGRAM_NAME, ANCHOR1_NAME=@ANCHOR1_NAME, ANCHOR1_POSITION=@ANCHOR1_POSITION, ANCHOR2_NAME=@ANCHOR2_NAME, ANCHOR2_POSITION=@ANCHOR2_POSITION, ANCHOR3_NAME=@ANCHOR3_NAME, ANCHOR3_POSITION=@ANCHOR3_POSITION, LOGO=@LOGO, OBB=@OBB, BEDDING=@BEDDING, BACKGROUND=@BACKGROUND, CBB=@CBB, LOGO_FN=@LOGO_FN, OBB_FN=@OBB_FN, BEDDING_FN=@BEDDING_FN, BACKGROUND_FN=@BACKGROUND_FN, CBB_FN=@CBB_FN where id = '" + (rid + 1).ToString() + "'";
                    cmd.Prepare();

                    cmd.Parameters.AddWithValue("@PROGRAM_NAME", PNold);
                    cmd.Parameters.AddWithValue("@ANCHOR1_NAME", A1Nold);
                    cmd.Parameters.AddWithValue("@ANCHOR1_POSITION", A1Pold);
                    cmd.Parameters.AddWithValue("@ANCHOR2_NAME", A2Nold);
                    cmd.Parameters.AddWithValue("@ANCHOR2_POSITION", A2Pold);
                    cmd.Parameters.AddWithValue("@ANCHOR3_NAME", A3Nold);
                    cmd.Parameters.AddWithValue("@ANCHOR3_POSITION", A3Pold);
                    cmd.Parameters.AddWithValue("@LOGO", Lold);
                    cmd.Parameters.AddWithValue("@OBB", Oold);
                    cmd.Parameters.AddWithValue("@BEDDING", Bold);
                    cmd.Parameters.AddWithValue("@BACKGROUND", Bgold);
                    cmd.Parameters.AddWithValue("@CBB", Cold);
                    cmd.Parameters.AddWithValue("@LOGO_FN", oldlogo);
                    cmd.Parameters.AddWithValue("@OBB_FN", oldobb);
                    cmd.Parameters.AddWithValue("@BEDDING_FN", oldbed);
                    cmd.Parameters.AddWithValue("@BACKGROUND_FN", oldbg);
                    cmd.Parameters.AddWithValue("@CBB_FN", oldcbb);
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "UPDATE " + schedname + " SET PROGRAM_NAME=@PROGRAM_NAME, ANCHOR1_NAME=@ANCHOR1_NAME, ANCHOR1_POSITION=@ANCHOR1_POSITION, ANCHOR2_NAME=@ANCHOR2_NAME, ANCHOR2_POSITION=@ANCHOR2_POSITION, ANCHOR3_NAME=@ANCHOR3_NAME, ANCHOR3_POSITION=@ANCHOR3_POSITION, LOGO=@LOGO, OBB=@OBB, BEDDING=@BEDDING, BACKGROUND=@BACKGROUND, CBB=@CBB, LOGO_FN=@LOGO_FN, OBB_FN=@OBB_FN, BEDDING_FN=@BEDDING_FN, BACKGROUND_FN=@BACKGROUND_FN, CBB_FN=@CBB_FN where id = '" + rid.ToString() + "'";
                    cmd.Prepare();

                    cmd.Parameters.AddWithValue("@PROGRAM_NAME", PNnew);
                    cmd.Parameters.AddWithValue("@ANCHOR1_NAME", A1Nnew);
                    cmd.Parameters.AddWithValue("@ANCHOR1_POSITION", A1Pnew);
                    cmd.Parameters.AddWithValue("@ANCHOR2_NAME", A2Nnew);
                    cmd.Parameters.AddWithValue("@ANCHOR2_POSITION", A2Pnew);
                    cmd.Parameters.AddWithValue("@ANCHOR3_NAME", A3Nnew);
                    cmd.Parameters.AddWithValue("@ANCHOR3_POSITION", A3Pnew);
                    cmd.Parameters.AddWithValue("@LOGO", Lnew);
                    cmd.Parameters.AddWithValue("@OBB", Onew);
                    cmd.Parameters.AddWithValue("@BEDDING", Bnew);
                    cmd.Parameters.AddWithValue("@BACKGROUND", Bgnew);
                    cmd.Parameters.AddWithValue("@CBB", CBnew);
                    cmd.Parameters.AddWithValue("@LOGO_FN", newlogo);
                    cmd.Parameters.AddWithValue("@OBB_FN", newobb);
                    cmd.Parameters.AddWithValue("@BEDDING_FN", newbed);
                    cmd.Parameters.AddWithValue("@BACKGROUND_FN", newbg);
                    cmd.Parameters.AddWithValue("@CBB_FN", newcbb);
                    cmd.ExecuteNonQuery();
                    data_show();
                    XML_Data();
                    dataGV.CurrentCell.Selected = false;
                    sqlupBtn.Visible = false;
                    sqldownBtn.Visible = false;
                    string PN = tBProgram.Text;
                    string A1 = tBAnchor1.Text;
                    string A2 = tBAnchor2.Text;
                    string A3 = tBAnchor3.Text;
                    string L = tBLogo.Text;
                    string O = tBOBB.Text;
                    string B = tBBedding.Text;

                    tBProgram.Text = dataGV.Rows[rid - 2].Cells["PROGRAM_NAME"].FormattedValue.ToString();
                    tBAnchor1.Text = dataGV.Rows[rid - 2].Cells["ANCHOR1_NAME"].FormattedValue.ToString() + "," + dataGV.Rows[rid - 2].Cells["ANCHOR1_POSITION"].FormattedValue.ToString();
                    tBAnchor2.Text = dataGV.Rows[rid - 2].Cells["ANCHOR2_NAME"].FormattedValue.ToString() + "," + dataGV.Rows[rid - 2].Cells["ANCHOR2_POSITION"].FormattedValue.ToString();
                    tBAnchor3.Text = dataGV.Rows[rid - 2].Cells["ANCHOR3_NAME"].FormattedValue.ToString() + "," + dataGV.Rows[rid - 2].Cells["ANCHOR3_POSITION"].FormattedValue.ToString();
                    tBLogo.Text = dataGV.Rows[rid - 2].Cells["LOGO"].FormattedValue.ToString();
                    tBOBB.Text = dataGV.Rows[rid - 2].Cells["OBB"].FormattedValue.ToString();
                    tBBedding.Text = dataGV.Rows[rid - 2].Cells["BEDDING"].FormattedValue.ToString();
                    tBBG.Text = dataGV.Rows[rid - 2].Cells["BACKGROUND"].FormattedValue.ToString();
                    string sqlrowid = dataGV.Rows[rid - 2].Cells["id"].FormattedValue.ToString();
                    logofilename = dataGV.Rows[rid - 2].Cells["LOGO_FN"].FormattedValue.ToString();
                    obbfilename = dataGV.Rows[rid - 2].Cells["OBB_FN"].FormattedValue.ToString();
                    bedfilename = dataGV.Rows[rid - 2].Cells["BEDDING_FN"].FormattedValue.ToString();
                    bgfilename = dataGV.Rows[rid - 2].Cells["BACKGROUND_FN"].FormattedValue.ToString();
                    sqlid = int.Parse(sqlrowid);
                    buttonshow = dataGV.CurrentRow.Index;
                    rid1 = dataGV.RowCount;
                    rid2 = dataGV.CurrentRow.Index + 2;


                }
                catch (Exception)
                {
                    return;
                }

            }
            catch (Exception)
            {
                return;
            }
        }

        private void XML_Data()
        {

            DS.DataSetName = schedname.ToString();
            DT.WriteXml(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BKE AirStack/Data_Source/" + schedname.ToString() + ".xml");
        }

        private void dataGV_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
