using Microsoft.Win32;
using System;
using System.Collections.Specialized;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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
    public partial class Form1 : Form
    {
        internal static class ExpiryLite
        {
            // bas.dat at %LOCALAPPDATA%
            private static readonly string FilePath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "bas.dat");

            private const string VERSION = "B1"; // format version marker

            /// <summary>
            /// Call once at startup. If bas.dat doesn't exist, creates a trial for N days.
            /// If it exists, enforces expiry + crude anti-clock-back.
            /// </summary>
            public static void InitializeOrCreateTrial(int trialDays)
            {
                var now = DateTime.UtcNow;

                if (!File.Exists(FilePath))
                {
                    var expiry = trialDays > 0 ? now.AddDays(trialDays) : now;
                    Save(expiry, now);
                    return;
                }

                var (exp, last, bind) = Load();

                // crude anti-clock-back (5 min tolerance)
                if (now.AddMinutes(5) < last)
                    Fail("System clock manipulation detected.");

                // forward-only lastSeen
                if (now > last)
                    Save(exp, now, bind);

                if (now > exp)
                    Fail("This copy has expired.");
            }

            /// <summary>One-time setter (e.g., via hidden CLI): writes the new expiry.</summary>
            public static void SetExpiryUtc(DateTime expiryUtc)
            {
                var now = DateTime.UtcNow;
                Save(expiryUtc.ToUniversalTime(), now);
            }

            public static DateTime GetExpiryUtc()
            {
                var (exp, _, _) = Load();
                return exp;
            }

            // ---------------- internals ----------------

            private static (DateTime exp, DateTime last, string bind) Load()
            {
                try
                {
                    var raw = File.ReadAllBytes(FilePath);
                    var text = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(raw)));

                    // Format: VERSION|expTicks|lastTicks|bindHex
                    var parts = text.Split('|');
                    if (parts.Length != 4 || parts[0] != VERSION)
                        throw new InvalidDataException("Invalid bas.dat format.");

                    var exp = new DateTime(long.Parse(parts[1]), DateTimeKind.Utc);
                    var last = new DateTime(long.Parse(parts[2]), DateTimeKind.Utc);
                    var bind = parts[3];

                    // machine/user bind check
                    if (bind != GetBinding())
                        throw new InvalidDataException("bas.dat is not for this machine/user.");

                    return (exp, last, bind);
                }
                catch (Exception ex)
                {
                    Fail("brd.dat unreadable or invalid.\n" + ex.Message);
                    throw; // never reached (Fail exits)
                }
            }

            private static void Save(DateTime expiryUtc, DateTime lastSeenUtc, string? existingBind = null)
            {
                var bind = existingBind ?? GetBinding();
                var payload = string.Join("|", VERSION, expiryUtc.Ticks.ToString(), lastSeenUtc.Ticks.ToString(), bind);

                // Light obfuscation: UTF8 -> Base64 -> UTF8 (no DPAPI)
                var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
                var outBytes = Encoding.UTF8.GetBytes(b64);

                var dir = Path.GetDirectoryName(FilePath)!;

                // 0) Ensure parent dir exists and is writable
                Directory.CreateDirectory(dir);
                using (var fs = new FileStream(
                    Path.Combine(dir, ".$perm_test"),
                    FileMode.OpenOrCreate, FileAccess.Write, FileShare.None,
                    bufferSize: 1, FileOptions.DeleteOnClose)) { /* if this fails, you truly can't write here */ }

                // 1) If "bas.dat" is a directory, bail with a clear message
                if (Directory.Exists(FilePath))
                {
                    MessageBox.Show(
                        $"'{FilePath}' is a FOLDER, not a file. Delete or rename that folder, then relaunch.",
                        "BKE AirStack",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    Environment.Exit(1);
                }

                // 2) Clear read-only/system attributes so we can overwrite
                if (File.Exists(FilePath))
                {
                    try
                    {
                        var attr = File.GetAttributes(FilePath);
                        var cleared = attr & ~(FileAttributes.ReadOnly | FileAttributes.System);
                        if (cleared != attr) File.SetAttributes(FilePath, cleared);
                    }
                    catch { /* best effort */ }
                }

                // 3) Atomic write: write to temp, then replace/move
                var tmp = Path.Combine(dir, ".$bas.tmp");
                File.WriteAllBytes(tmp, outBytes);

                try
                {
                    if (File.Exists(FilePath))
                        File.Replace(tmp, FilePath, null);   // atomic replace if destination exists
                    else
                        File.Move(tmp, FilePath);            // first write
                }
                catch (IOException)
                {
                    // Fallback if Replace not supported (e.g., different volume)
                    if (File.Exists(FilePath)) File.Delete(FilePath);
                    File.Move(tmp, FilePath);
                }
                finally
                {
                    if (File.Exists(tmp)) { try { File.Delete(tmp); } catch { } }
                }

                // 4) Hide it (cosmetic)
                try { File.SetAttributes(FilePath, FileAttributes.Hidden); } catch { /* best effort */ }
            }


            private static void Fail(string reason)
            {
                MessageBox.Show($"{reason}\n\nContact support.", "BKE AirStack", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }

            // Bind to machine + user so a copied file won't work elsewhere
            private static string GetBinding()
            {
                try
                {
                    using var lm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    using var k = lm.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
                    var machineGuid = k?.GetValue("MachineGuid")?.ToString() ?? "noguid";
                    var sid = System.Security.Principal.WindowsIdentity.GetCurrent()?.User?.Value ?? "nosid";

                    using var sha = SHA256.Create();
                    var hash = sha.ComputeHash(Encoding.UTF8.GetBytes($"{machineGuid}|{sid}"));
                    return Convert.ToHexString(hash);
                }
                catch
                {
                    return "UNKNOWN_BIND";
                }
            }

            // Optional helper: parse a CLI switch to set expiry quickly
            public static bool TryHandleCli()
            {
                var args = Environment.GetCommandLineArgs();

                // --- show expiry ---
                if (args.Any(a => a.Equals("--show-expiry", StringComparison.OrdinalIgnoreCase)))
                {
                    if (!File.Exists(FilePath))
                    {
                        MessageBox.Show("bas.dat not found. No expiry is set yet.",
                            "BKE AirStack", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true; // handled
                    }

                    try
                    {
                        var exp = GetExpiryUtc();
                        var now = DateTime.UtcNow;
                        var remaining = exp - now;
                        var remainText = remaining.TotalSeconds <= 0
                            ? "Expired."
                            : $"{remaining.Days}d {remaining.Hours}h {remaining.Minutes}m remaining";

                        MessageBox.Show(
                            $"Expiry (UTC): {exp:u}\nNow   (UTC): {now:u}\n\nStatus: {remainText}",
                            "BKE AirStack",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Could not read bas.dat:\n" + ex.Message,
                            "BKE AirStack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    return true; // handled
                }

                // --- set expiry explicitly ---
                const string setKey = "--set-expiry=";
                var setArg = args.FirstOrDefault(a => a.StartsWith(setKey, StringComparison.OrdinalIgnoreCase));
                if (setArg != null)
                {
                    var s = setArg.Substring(setKey.Length);
                    if (!DateTime.TryParse(s, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var dt))
                    {
                        MessageBox.Show($"Invalid datetime: {s}",
                            "BKE AirStack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return true;
                    }
                    if (dt.Kind != DateTimeKind.Utc) dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);

                    SetExpiryUtc(dt);
                    MessageBox.Show($"Expiry set to {dt:u}",
                        "BKE AirStack", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }

                // --- optional: extend by N days from current expiry ---
                const string extKey = "--extend-days=";
                var extArg = args.FirstOrDefault(a => a.StartsWith(extKey, StringComparison.OrdinalIgnoreCase));
                if (extArg != null)
                {
                    if (!int.TryParse(extArg.Substring(extKey.Length), out var days) || days == 0)
                    {
                        MessageBox.Show("Use a non-zero integer for --extend-days=N",
                            "BKE AirStack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return true;
                    }

                    if (!File.Exists(FilePath))
                    {
                        MessageBox.Show("bas.dat not found. Set an expiry first with --set-expiry=...",
                            "BKE AirStack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return true;
                    }

                    var curr = GetExpiryUtc();
                    var next = curr.AddDays(days);
                    SetExpiryUtc(next);
                    MessageBox.Show($"Expiry changed:\nOld: {curr:u}\nNew: {next:u}",
                        "BKE AirStack", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }

                return false; // nothing handled
            }

        }

        SQLiteDataAdapter DB2;
        DataSet DS2 = new DataSet();
        DataTable DT2 = new DataTable();
        SQLiteDataReader dr;
        string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BKE AirStack/DataBase/BKEAirStack.db";
        string cs = @"URI=file:" + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BKE AirStack\\DataBase\\BKEAirStack.db";
        public static Form1 form1instance;
        string breakcode = "|0|0|True|False\r\n";
        string logopath = "";
        string obbpath = "";
        string bedpath = "";
        string adspath;
        string bgpath;
        string cbbpath;
        string xp;
        string d_url;
        private void Data_Folder()
        {

            // The folder for the roaming current user 
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BKE AirStack";

            // Combine the base folder with your specific folder....
            string ads = Path.Combine(folder, "Data_Source");
            string at = Path.Combine(folder, "Template");
            string adb = Path.Combine(folder, "DataBase");

            // CreateDirectory will check if every folder in path exists and, if not, create them.
            // If all folders exist then CreateDirectory will do nothing.
            Directory.CreateDirectory(ads);
            Directory.CreateDirectory(at);
            Directory.CreateDirectory(adb);

        }
        private void create_DB()
        {
            if (!System.IO.File.Exists(path))
            {
                SQLiteConnection.CreateFile(path);
                using (var sqlite = new SQLiteConnection(@"Data Source=" + path))
                {
                    sqlite.Open();
                    string sqlM = "create table Mdata(PROGRAM_NAME varchar(100),ANCHOR1_NAME varchar(100), ANCHOR1_POSITION varchar(100), ANCHOR2_NAME varchar(100), ANCHOR2_POSITION varchar(100), ANCHOR3_NAME varchar(100), ANCHOR3_POSITION varchar(100), LOGO varchar(100), OBB varchar(100), BEDDING varchar(100), BACKGROUND varchar(100), CBB varchar(100), id INTEGER PRIMARY KEY, LOGO_FN varchar(100), OBB_FN varchar(100), BEDDING_FN varchar(100), BACKGROUND_FN varchar(100), CBB_FN varchar(100))";
                    SQLiteCommand commandM = new SQLiteCommand(sqlM, sqlite);
                    commandM.ExecuteNonQuery();
                    string sqlT = "create table Tdata(PROGRAM_NAME varchar(100),ANCHOR1_NAME varchar(100), ANCHOR1_POSITION varchar(100), ANCHOR2_NAME varchar(100), ANCHOR2_POSITION varchar(100), ANCHOR3_NAME varchar(100), ANCHOR3_POSITION varchar(100), LOGO varchar(100), OBB varchar(100), BEDDING varchar(100), BACKGROUND varchar(100), CBB varchar(100), id INTEGER PRIMARY KEY, LOGO_FN varchar(100), OBB_FN varchar(100), BEDDING_FN varchar(100), BACKGROUND_FN varchar(100), CBB_FN varchar(100))";
                    SQLiteCommand commandT = new SQLiteCommand(sqlT, sqlite);
                    commandT.ExecuteNonQuery();
                    string sqlW = "create table Wdata(PROGRAM_NAME varchar(100),ANCHOR1_NAME varchar(100), ANCHOR1_POSITION varchar(100), ANCHOR2_NAME varchar(100), ANCHOR2_POSITION varchar(100), ANCHOR3_NAME varchar(100), ANCHOR3_POSITION varchar(100), LOGO varchar(100), OBB varchar(100), BEDDING varchar(100), BACKGROUND varchar(100), CBB varchar(100), id INTEGER PRIMARY KEY, LOGO_FN varchar(100), OBB_FN varchar(100), BEDDING_FN varchar(100), BACKGROUND_FN varchar(100), CBB_FN varchar(100))";
                    SQLiteCommand commandW = new SQLiteCommand(sqlW, sqlite);
                    commandW.ExecuteNonQuery();
                    string sqlTh = "create table Thdata(PROGRAM_NAME varchar(100),ANCHOR1_NAME varchar(100), ANCHOR1_POSITION varchar(100), ANCHOR2_NAME varchar(100), ANCHOR2_POSITION varchar(100), ANCHOR3_NAME varchar(100), ANCHOR3_POSITION varchar(100), LOGO varchar(100), OBB varchar(100), BEDDING varchar(100), BACKGROUND varchar(100), CBB varchar(100), id INTEGER PRIMARY KEY, LOGO_FN varchar(100), OBB_FN varchar(100), BEDDING_FN varchar(100), BACKGROUND_FN varchar(100), CBB_FN varchar(100))";
                    SQLiteCommand commandTh = new SQLiteCommand(sqlTh, sqlite);
                    commandTh.ExecuteNonQuery();
                    string sqlF = "create table Fdata(PROGRAM_NAME varchar(100),ANCHOR1_NAME varchar(100), ANCHOR1_POSITION varchar(100), ANCHOR2_NAME varchar(100), ANCHOR2_POSITION varchar(100), ANCHOR3_NAME varchar(100), ANCHOR3_POSITION varchar(100), LOGO varchar(100), OBB varchar(100), BEDDING varchar(100), BACKGROUND varchar(100), CBB varchar(100), id INTEGER PRIMARY KEY, LOGO_FN varchar(100), OBB_FN varchar(100), BEDDING_FN varchar(100), BACKGROUND_FN varchar(100), CBB_FN varchar(100))";
                    SQLiteCommand commandF = new SQLiteCommand(sqlF, sqlite);
                    commandF.ExecuteNonQuery();
                    string sqlSat = "create table Satdata(PROGRAM_NAME varchar(100),ANCHOR1_NAME varchar(100), ANCHOR1_POSITION varchar(100), ANCHOR2_NAME varchar(100),ANCHOR2_POSITION varchar(100), ANCHOR3_NAME varchar(100), ANCHOR3_POSITION varchar(100), LOGO varchar(100), OBB varchar(100), BEDDING varchar(100), BACKGROUND varchar(100), CBB varchar(100), id INTEGER PRIMARY KEY, LOGO_FN varchar(100), OBB_FN varchar(100), BEDDING_FN varchar(100), BACKGROUND_FN varchar(100), CBB_FN varchar(100))";
                    SQLiteCommand commandSat = new SQLiteCommand(sqlSat, sqlite);
                    commandSat.ExecuteNonQuery();
                    string sqlSun = "create table Sundata(PROGRAM_NAME varchar(100),ANCHOR1_NAME varchar(100), ANCHOR1_POSITION varchar(100), ANCHOR2_NAME varchar(100),ANCHOR2_POSITION varchar(100), ANCHOR3_NAME varchar(100), ANCHOR3_POSITION varchar(100), LOGO varchar(100), OBB varchar(100), BEDDING varchar(100), BACKGROUND varchar(100), CBB varchar(100), id INTEGER PRIMARY KEY, LOGO_FN varchar(100), OBB_FN varchar(100), BEDDING_FN varchar(100), BACKGROUND_FN varchar(100), CBB_FN varchar(100))";
                    SQLiteCommand commandSun = new SQLiteCommand(sqlSun, sqlite);
                    commandSun.ExecuteNonQuery();
                    string sqlAds = "create table Adsdata(ADVERTISEMENT varchar(100), ADVERTISEMENT_NAME varchar(100))";
                    SQLiteCommand commandAds = new SQLiteCommand(sqlAds, sqlite);
                    commandAds.ExecuteNonQuery();
                }
            }
            else
            {
                return;
            }
        }
        private void calll_datagridview()
        {
            DataGridView dgvm = new DataGridView();
            DataGridView dgvt = new DataGridView();
            DataGridView dgvw = new DataGridView();
            DataGridView dgvth = new DataGridView();
            DataGridView dgvf = new DataGridView();
            DataGridView dgvsat = new DataGridView();
            DataGridView dgvsun = new DataGridView();
            var con = new SQLiteConnection(cs);
            con.Open();
        }
        private void XML_Data()
        {
            DataGridView dgvm = new DataGridView();
            DataGridView dgvt = new DataGridView();
            DataGridView dgvw = new DataGridView();
            DataGridView dgvth = new DataGridView();
            DataGridView dgvf = new DataGridView();
            DataGridView dgvsat = new DataGridView();
            DataGridView dgvsun = new DataGridView();
            var con = new SQLiteConnection(cs);
            con.Open();

            string stm = "SELECT * FROM Mdata";
            DB2 = new SQLiteDataAdapter(stm, con);
            DS2.Reset();
            DB2.Fill(DS2);
            DT2 = DS2.Tables[0];
            dgvm.DataSource = DT2;
            DS2.DataSetName = "Mdata";
            DT2.WriteXml("datasource_Mdata.xml");
            con.Close();

            con.Open();

            string stt = "SELECT * FROM Tdata";
            DB2 = new SQLiteDataAdapter(stt, con);
            DS2.Reset();
            DB2.Fill(DS2);
            DT2 = DS2.Tables[0];
            dgvt.DataSource = DT2;
            DS2.DataSetName = "Tdata";
            DT2.WriteXml("datasource_Tdata.xml");
            con.Close();

            string stw = "SELECT * FROM Tdata";
            DB2 = new SQLiteDataAdapter(stw, con);
            DS2.Reset();
            DB2.Fill(DS2);
            DT2 = DS2.Tables[0];
            dgvw.DataSource = DT2;
            DS2.DataSetName = "Wdata";
            DT2.WriteXml("datasource_Wdata.xml");
            con.Close();

            string stth = "SELECT * FROM Thdata";
            DB2 = new SQLiteDataAdapter(stth, con);
            DS2.Reset();
            DB2.Fill(DS2);
            DT2 = DS2.Tables[0];
            dgvth.DataSource = DT2;
            DS2.DataSetName = "Thdata";
            DT2.WriteXml("datasource_Thdata.xml");
            con.Close();

            string stF = "SELECT * FROM Fdata";
            DB2 = new SQLiteDataAdapter(stF, con);
            DS2.Reset();
            DB2.Fill(DS2);
            DT2 = DS2.Tables[0];
            dgvf.DataSource = DT2;
            DS2.DataSetName = "Fdata";
            DT2.WriteXml("datasource_Fdata.xml");
            con.Close();

            string stsat = "SELECT * FROM Satdata";
            DB2 = new SQLiteDataAdapter(stsat, con);
            DS2.Reset();
            DB2.Fill(DS2);
            DT2 = DS2.Tables[0];
            dgvsat.DataSource = DT2;
            DS2.DataSetName = "Satdata";
            DT2.WriteXml("datasource_Satdata.xml");
            con.Close();

            string stsun = "SELECT * FROM Sundata";
            DB2 = new SQLiteDataAdapter(stsun, con);
            DS2.Reset();
            DB2.Fill(DS2);
            DT2 = DS2.Tables[0];
            dgvsun.DataSource = DT2;
            DS2.DataSetName = "Sundata";
            DT2.WriteXml("datasource_Sundata.xml");
            con.Close();


        }
        Image original = null;

        static Bitmap SetAlpha(Bitmap bmpIn, int alpha)
        {
            Bitmap bmpOut = new Bitmap(bmpIn.Width, bmpIn.Height);
            float a = alpha / 255f;
            Rectangle r = new Rectangle(0, 0, bmpIn.Width, bmpIn.Height);

            float[][] matrixItems = {
        new float[] {1, 0, 0, 0, 0},
        new float[] {0, 1, 0, 0, 0},
        new float[] {0, 0, 1, 0, 0},
        new float[] {0, 0, 0, a, 0},
        new float[] {0, 0, 0, 0, 1}};

            ColorMatrix colorMatrix = new ColorMatrix(matrixItems);

            ImageAttributes imageAtt = new ImageAttributes();
            imageAtt.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            using (Graphics g = Graphics.FromImage(bmpOut))
                g.DrawImage(bmpIn, r, r.X, r.Y, r.Width, r.Height, GraphicsUnit.Pixel, imageAtt);

            return bmpOut;
        }
        private void Logo_Opacity()
        {
            if (original == null) original = (Bitmap)LOGO_BKE.Image.Clone();
            LOGO_BKE.BackColor = Color.Transparent;
            LOGO_BKE.Image = SetAlpha((Bitmap)original, 20);

        }
        public Form1()
        {
            InitializeComponent();
            form1instance = this;
            hideSubMenu();


        }


        [DllImport("user32.dll", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);
        private void hideSubMenu()
        {
            panelMediaSubMenu.Visible = false;
            panelPlaylistSubMenu.Visible = false;
            panelToolsSubMenu.Visible = false;
        }

        private void showSubMenu(Panel subMenu)
        {
            if (subMenu.Visible == false)
            {
                hideSubMenu();
                subMenu.Visible = true;
            }
            else
                subMenu.Visible = false;
        }

        private void btnMedia_Click(object sender, EventArgs e)
        {
            showSubMenu(panelMediaSubMenu);
            closeChildForm();
        }

        #region MediaSubMenu
        private void Advertisement()
        {
            if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BKE AirStack/Data_Source/Adsdata.xml"))
            {
                var con = new SQLiteConnection(cs);
                con.Open();
                var cmd = new SQLiteCommand(con);
                string stm = "SELECT * FROM Adsdata";
                var cmd2 = new SQLiteCommand(stm, con);
                dr = cmd2.ExecuteReader();
                StringBuilder asb = new StringBuilder();
                for (int i = 0; i < dr.StepCount; i++)
                {
                    while (dr.Read())
                    {
                        string a = dr.GetValue(i + 0).ToString() + "|" + dr.GetValue(i + 1);

                        asb.Append(a + breakcode);

                    }


                    adspath = asb.ToString();
                }
            }

        }
        private void omBtn_Click(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BKE AirStack/Data_Source/Mdata.xml"))
            {
                var con = new SQLiteConnection(cs);
                con.Open();
                var cmd = new SQLiteCommand(con);
                string stm = "SELECT * FROM Mdata";
                var cmd2 = new SQLiteCommand(stm, con);
                dr = cmd2.ExecuteReader();
                string[] Ldata;
                StringBuilder lsb = new StringBuilder();
                StringBuilder osb = new StringBuilder();
                StringBuilder bsb = new StringBuilder();
                StringBuilder bgsb = new StringBuilder();
                StringBuilder cbbsb = new StringBuilder();
                for (int i = 0; i < dr.StepCount; i++)
                {
                    while (dr.Read())
                    {
                        string l = dr.GetValue(i + 7).ToString() + "|" + dr.GetValue(i + 13);
                        string o = dr.GetValue(i + 8).ToString() + "|" + dr.GetValue(i + 14);
                        string b = dr.GetValue(i + 9).ToString() + "|" + dr.GetValue(i + 15);
                        string bg = dr.GetValue(i + 10).ToString() + "|" + dr.GetValue(i + 16);
                        string c = dr.GetValue(i + 11).ToString() + "|" + dr.GetValue(i + 17);
                        lsb.Append(l + breakcode);
                        osb.Append(o + breakcode);
                        bsb.Append(b + breakcode);
                        bgsb.Append(bg + breakcode);
                        cbbsb.Append(c + breakcode);

                    }


                    logopath = lsb.ToString();
                    obbpath = osb.ToString();
                    bedpath = bsb.ToString();
                    bgpath = bgsb.ToString();
                    cbbpath = cbbsb.ToString();
                }
                xp = "Mdata/Table";
                d_url = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BKE AirStack\\Data_Source\\Mdata.xml"; ;

                to_Vmix();
                hideSubMenu();
            }
            else
            {
                MessageBox.Show("Please create MONDAY schedule first!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        private void otBtn_Click(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BKE AirStack/Data_Source/Tdata.xml"))
            {
                var con = new SQLiteConnection(cs);
                con.Open();
                var cmd = new SQLiteCommand(con);
                string stm = "SELECT * FROM Tdata";
                var cmd2 = new SQLiteCommand(stm, con);
                dr = cmd2.ExecuteReader();
                string[] Ldata;
                StringBuilder lsb = new StringBuilder();
                StringBuilder osb = new StringBuilder();
                StringBuilder bsb = new StringBuilder();
                StringBuilder bgsb = new StringBuilder();
                StringBuilder cbbsb = new StringBuilder();
                for (int i = 0; i < dr.StepCount; i++)
                {
                    while (dr.Read())
                    {
                        string l = dr.GetValue(i + 7).ToString() + "|" + dr.GetValue(i + 13);
                        string o = dr.GetValue(i + 8).ToString() + "|" + dr.GetValue(i + 14);
                        string b = dr.GetValue(i + 9).ToString() + "|" + dr.GetValue(i + 15);
                        string bg = dr.GetValue(i + 10).ToString() + "|" + dr.GetValue(i + 16);
                        string c = dr.GetValue(i + 11).ToString() + "|" + dr.GetValue(i + 17);
                        lsb.Append(l + breakcode);
                        osb.Append(o + breakcode);
                        bsb.Append(b + breakcode);
                        bgsb.Append(bg + breakcode);
                        cbbsb.Append(c + breakcode);

                    }


                    logopath = lsb.ToString();
                    obbpath = osb.ToString();
                    bedpath = bsb.ToString();
                    bgpath = bgsb.ToString();
                    cbbpath = cbbsb.ToString();
                }
                xp = "Tdata/Table";
                d_url = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BKE AirStack\\Data_Source\\Tdata.xml"; ;

                to_Vmix();
                hideSubMenu();
            }
            else
            {
                MessageBox.Show("Please create TUESDAY schedule first!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        private void owBtn_Click(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BKE AirStack/Data_Source/Wdata.xml"))
            {
                var con = new SQLiteConnection(cs);
                con.Open();
                var cmd = new SQLiteCommand(con);
                string stm = "SELECT * FROM Wdata";
                var cmd2 = new SQLiteCommand(stm, con);
                dr = cmd2.ExecuteReader();
                string[] Ldata;
                StringBuilder lsb = new StringBuilder();
                StringBuilder osb = new StringBuilder();
                StringBuilder bsb = new StringBuilder();
                StringBuilder bgsb = new StringBuilder();
                StringBuilder cbbsb = new StringBuilder();
                for (int i = 0; i < dr.StepCount; i++)
                {
                    while (dr.Read())
                    {
                        string l = dr.GetValue(i + 7).ToString() + "|" + dr.GetValue(i + 13);
                        string o = dr.GetValue(i + 8).ToString() + "|" + dr.GetValue(i + 14);
                        string b = dr.GetValue(i + 9).ToString() + "|" + dr.GetValue(i + 15);
                        string bg = dr.GetValue(i + 10).ToString() + "|" + dr.GetValue(i + 16);
                        string c = dr.GetValue(i + 11).ToString() + "|" + dr.GetValue(i + 17);
                        lsb.Append(l + breakcode);
                        osb.Append(o + breakcode);
                        bsb.Append(b + breakcode);
                        bgsb.Append(bg + breakcode);
                        cbbsb.Append(c + breakcode);

                    }


                    logopath = lsb.ToString();
                    obbpath = osb.ToString();
                    bedpath = bsb.ToString();
                    bgpath = bgsb.ToString();
                    cbbpath = cbbsb.ToString();
                }
                xp = "Wdata/Table";
                d_url = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BKE AirStack" +
                    "\\Data_Source\\Wdata.xml"; ;

                to_Vmix();
                hideSubMenu();
            }
            else
            {
                MessageBox.Show("Please create WEDNESDAY schedule first!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        private void othBtn_Click(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BKE AirStack/Data_Source/Thdata.xml"))
            {
                var con = new SQLiteConnection(cs);
                con.Open();
                var cmd = new SQLiteCommand(con);
                string stm = "SELECT * FROM Thdata";
                var cmd2 = new SQLiteCommand(stm, con);
                dr = cmd2.ExecuteReader();
                string[] Ldata;
                StringBuilder lsb = new StringBuilder();
                StringBuilder osb = new StringBuilder();
                StringBuilder bsb = new StringBuilder();
                StringBuilder bgsb = new StringBuilder();
                StringBuilder cbbsb = new StringBuilder();
                for (int i = 0; i < dr.StepCount; i++)
                {
                    while (dr.Read())
                    {
                        string l = dr.GetValue(i + 7).ToString() + "|" + dr.GetValue(i + 13);
                        string o = dr.GetValue(i + 8).ToString() + "|" + dr.GetValue(i + 14);
                        string b = dr.GetValue(i + 9).ToString() + "|" + dr.GetValue(i + 15);
                        string bg = dr.GetValue(i + 10).ToString() + "|" + dr.GetValue(i + 16);
                        string c = dr.GetValue(i + 11).ToString() + "|" + dr.GetValue(i + 17);
                        lsb.Append(l + breakcode);
                        osb.Append(o + breakcode);
                        bsb.Append(b + breakcode);
                        bgsb.Append(bg + breakcode);
                        cbbsb.Append(c + breakcode);

                    }


                    logopath = lsb.ToString();
                    obbpath = osb.ToString();
                    bedpath = bsb.ToString();
                    bgpath = bgsb.ToString();
                    cbbpath = cbbsb.ToString();
                }
                xp = "Thdata/Table";
                d_url = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BKE AirStack\\Data_Source\\Thdata.xml"; ;

                to_Vmix();
                hideSubMenu();
            }
            else
            {
                MessageBox.Show("Please create THURSDAY schedule first!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        private void ofBtn_Click(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BKE AirStack/Data_Source/Fdata.xml"))
            {
                var con = new SQLiteConnection(cs);
                con.Open();
                var cmd = new SQLiteCommand(con);
                string stm = "SELECT * FROM Fdata";
                var cmd2 = new SQLiteCommand(stm, con);
                dr = cmd2.ExecuteReader();
                string[] Ldata;
                StringBuilder lsb = new StringBuilder();
                StringBuilder osb = new StringBuilder();
                StringBuilder bsb = new StringBuilder();
                StringBuilder bgsb = new StringBuilder();
                StringBuilder cbbsb = new StringBuilder();
                for (int i = 0; i < dr.StepCount; i++)
                {
                    while (dr.Read())
                    {
                        string l = dr.GetValue(i + 7).ToString() + "|" + dr.GetValue(i + 13);
                        string o = dr.GetValue(i + 8).ToString() + "|" + dr.GetValue(i + 14);
                        string b = dr.GetValue(i + 9).ToString() + "|" + dr.GetValue(i + 15);
                        string bg = dr.GetValue(i + 10).ToString() + "|" + dr.GetValue(i + 16);
                        string c = dr.GetValue(i + 11).ToString() + "|" + dr.GetValue(i + 17);
                        lsb.Append(l + breakcode);
                        osb.Append(o + breakcode);
                        bsb.Append(b + breakcode);
                        bgsb.Append(bg + breakcode);
                        cbbsb.Append(c + breakcode);

                    }


                    logopath = lsb.ToString();
                    obbpath = osb.ToString();
                    bedpath = bsb.ToString();
                    bgpath = bgsb.ToString();
                    cbbpath = cbbsb.ToString();
                }
                xp = "Fdata/Table";
                d_url = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BKE AirStack\\Data_Source\\Fdata.xml"; ;

                to_Vmix();
                hideSubMenu();
            }
            else
            {
                MessageBox.Show("Please create FRIDAY schedule first!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        private void osatBtn_Click(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BKE AirStack/Data_Source/Satdata.xml"))
            {
                var con = new SQLiteConnection(cs);
                con.Open();
                var cmd = new SQLiteCommand(con);
                string stm = "SELECT * FROM Satdata";
                var cmd2 = new SQLiteCommand(stm, con);
                dr = cmd2.ExecuteReader();
                string[] Ldata;
                StringBuilder lsb = new StringBuilder();
                StringBuilder osb = new StringBuilder();
                StringBuilder bsb = new StringBuilder();
                StringBuilder bgsb = new StringBuilder();
                StringBuilder cbbsb = new StringBuilder();
                for (int i = 0; i < dr.StepCount; i++)
                {
                    while (dr.Read())
                    {
                        string l = dr.GetValue(i + 7).ToString() + "|" + dr.GetValue(i + 13);
                        string o = dr.GetValue(i + 8).ToString() + "|" + dr.GetValue(i + 14);
                        string b = dr.GetValue(i + 9).ToString() + "|" + dr.GetValue(i + 15);
                        string bg = dr.GetValue(i + 10).ToString() + "|" + dr.GetValue(i + 16);
                        string c = dr.GetValue(i + 11).ToString() + "|" + dr.GetValue(i + 17);
                        lsb.Append(l + breakcode);
                        osb.Append(o + breakcode);
                        bsb.Append(b + breakcode);
                        bgsb.Append(bg + breakcode);
                        cbbsb.Append(c + breakcode);

                    }


                    logopath = lsb.ToString();
                    obbpath = osb.ToString();
                    bedpath = bsb.ToString();
                    bgpath = bgsb.ToString();
                    cbbpath = cbbsb.ToString();
                }
                xp = "Satdata/Table";
                d_url = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BKE AirStack\\Data_Source\\Satdata.xml"; ;

                to_Vmix();
                hideSubMenu();
            }
            else
            {
                MessageBox.Show("Please create SATURDAY schedule first!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        private void osunBtn_Click(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BKE AirStack/Data_Source/Sundata.xml"))
            {
                var con = new SQLiteConnection(cs);
                con.Open();
                var cmd = new SQLiteCommand(con);
                string stm = "SELECT * FROM Sundata";
                var cmd2 = new SQLiteCommand(stm, con);
                dr = cmd2.ExecuteReader();
                string[] Ldata;
                StringBuilder lsb = new StringBuilder();
                StringBuilder osb = new StringBuilder();
                StringBuilder bsb = new StringBuilder();
                StringBuilder bgsb = new StringBuilder();
                StringBuilder cbbsb = new StringBuilder();
                for (int i = 0; i < dr.StepCount; i++)
                {
                    while (dr.Read())
                    {
                        string l = dr.GetValue(i + 7).ToString() + "|" + dr.GetValue(i + 13);
                        string o = dr.GetValue(i + 8).ToString() + "|" + dr.GetValue(i + 14);
                        string b = dr.GetValue(i + 9).ToString() + "|" + dr.GetValue(i + 15);
                        string bg = dr.GetValue(i + 10).ToString() + "|" + dr.GetValue(i + 16);
                        string c = dr.GetValue(i + 11).ToString() + "|" + dr.GetValue(i + 17);
                        lsb.Append(l + breakcode);
                        osb.Append(o + breakcode);
                        bsb.Append(b + breakcode);
                        bgsb.Append(bg + breakcode);
                        cbbsb.Append(c + breakcode);

                    }


                    logopath = lsb.ToString();
                    obbpath = osb.ToString();
                    bedpath = bsb.ToString();
                    bgpath = bgsb.ToString();
                    cbbpath = cbbsb.ToString();
                }
                xp = "Sundata/Table";
                d_url = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BKE AirStack\\Data_Source\\Sundata.xml"; ;

                to_Vmix();
                hideSubMenu();
            }
            else
            {
                MessageBox.Show("Please create SUNDAY schedule first!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        #endregion

        private void schedbTn_Click(object sender, EventArgs e)
        {
            showSubMenu(panelPlaylistSubMenu);

        }

        #region PlayListManagemetSubMenu
        private void mBtn_Click(object sender, EventArgs e)
        {

            Form2 form2 = new Form2();
            form2.schedname = "Mdata";
            openChildForm(form2);
            Form2.form2instance.schedlabel.Text = "MONDAY SCHEDULE";

            //..
            //your codes
            //..
            hideSubMenu();
        }
        private void tBtn_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.schedname = "Tdata";
            openChildForm(form2);
            Form2.form2instance.schedlabel.Text = "TUESDAY SCHEDULE";
            //..
            //your codes
            //..
            hideSubMenu();
        }
        private void wBtn_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.schedname = "Wdata";
            openChildForm(form2);
            Form2.form2instance.schedlabel.Text = "WEDNESDAY SCHEDULE";
            //..
            //your codes
            //..
            hideSubMenu();
        }
        private void thBtn_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.schedname = "Thdata";
            openChildForm(form2);
            Form2.form2instance.schedlabel.Text = "THURSDAY SCHEDULE";
            //..
            //your codes
            //..
            hideSubMenu();
        }
        private void fBtn_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.schedname = "Fdata";
            openChildForm(form2);
            Form2.form2instance.schedlabel.Text = "FRIDAY SCHEDULE";
            //..
            //your codes
            //..
            hideSubMenu();
        }
        private void satBtn_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.schedname = "Satdata";
            openChildForm(form2);
            Form2.form2instance.schedlabel.Text = "SATURDAY SCHEDULE";
            //..
            //your codes
            //..
            hideSubMenu();
        }
        private void sunBtn_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.schedname = "Sundata";
            openChildForm(form2);
            Form2.form2instance.schedlabel.Text = "SUNDAY SCHEDULE";
            //..
            //your codes
            //..
            hideSubMenu();
        }

        #endregion

        private void btnSettings_Click(object sender, EventArgs e)
        {
            showSubMenu(panelToolsSubMenu);
        }
        #region ToolsSubMenu
        private void templateBtn_Click(object sender, EventArgs e)
        {
            string sfn = "D:/Template.vmix";
            string dfn = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BKE AirStack/Template/Template.vmix";
            var processExists = Process.GetProcesses().Any(p => p.ProcessName.Contains("vMix64"));
            if (processExists == true)
            {
                MessageBox.Show("Please close vmix application first!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;

            }
            else
            {
                File.Move(dfn, sfn, true);
                Cursor.Current = Cursors.WaitCursor;
                Thread.Sleep(3000);
                Process.Start("C:\\Program Files (x86)\\vMix\\vMix64.exe", "D:/Template.vmix");
                //..
                //your codes
                //..
                Thread.Sleep(30000);
                var processExists2 = Process.GetProcesses().Any(p => p.ProcessName.Contains("vMix64"));
                if (processExists2 == true)
                {

                    Process.GetProcessesByName("vMix64").ElementAt(0).WaitForExit();

                }
                File.Move(sfn, dfn, true);
                Cursor.Current = Cursors.Default;
                hideSubMenu();
            }
        }

        private void dtemplateBtn_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Set template to default?", "Template", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BKE AirStack/Template/Template.vmix");
                File.Copy("default.vmix", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BKE AirStack/Template/Template.vmix");
                //..
            }
            else if (dialogResult == DialogResult.No)
            {
                return;
            }


            //your codes
            //..
            hideSubMenu();
        }

        private void adsBtn_Click(object sender, EventArgs e)
        {
            Form3 form3 = new Form3();
            openChildForm(form3);//..
            //your codes
            //..
            hideSubMenu();
        }

        private void a_Click(object sender, EventArgs e)
        {
            //..
            //your codes
            //..
            hideSubMenu();
        }
        #endregion

        private void btnTools_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = "BKE AirStack TOOLS.exe", UseShellExecute = true });
            hideSubMenu();
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            //..
            //your codes
            //..
            hideSubMenu();
        }
        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private Form activeForm = null;
        private void openChildForm(Form childForm)
        {
            if (activeForm != null) activeForm.Close();
            activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            panelChildForm.Controls.Add(childForm);
            panelChildForm.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }
        private void closeChildForm()
        {
            if (activeForm != null) activeForm.Close();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Logo_Opacity();
            Data_Folder();
            create_DB();
            important_file();
        }
        protected override void WndProc(ref Message m)
        {
            const int WM_NCCALSIZE = 0x0083;
            if (m.Msg == WM_NCCALSIZE && m.WParam.ToInt32() == 1)
            {
                return;

            }
            base.WndProc(ref m);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = @"https://www.facebook.com/jan2xo", UseShellExecute = true });
        }
        private void to_Vmix()
        {
            Advertisement();
            if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BKE AirStack/Template/Template.vmix"))
            {
                Cursor.Current = Cursors.WaitCursor;
                File.Delete("D:/BKE AirStack.vmix");
                string dl = "C:/Users/jan2x/Downloads/GOLD MEDALIST.mp4|GOLD MEDALIST.mp4|0|0|True|False&#xD;&#xA;C:/Users/jan2x/Downloads/PBBM NFA.mp4|PBBM NFA.mp4|0|0|True|False";
                XmlDocument xmlDoc = new XmlDocument();
                string obb = "OBB";
                xmlDoc.Load(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BKE AirStack/Template/Template.vmix");
                XmlNode node = xmlDoc.SelectSingleNode("/XML/Input[@Title='LOGO']");
                node.Attributes["Videos"].Value = logopath;
                XmlNode node1 = xmlDoc.SelectSingleNode("/XML/Input[@Title='OBB']");
                node1.Attributes["Videos"].Value = obbpath;
                XmlNode node2 = xmlDoc.SelectSingleNode("/XML/Input[@Title='BEDDING']");
                node2.Attributes["Videos"].Value = bedpath;
                XmlNode node3 = xmlDoc.SelectSingleNode("/XML/DataSources/datasources/datasource/instance/state/xml/XPath");
                node3.InnerText = xp;
                XmlNode node4 = xmlDoc.SelectSingleNode("/XML/DataSources/datasources//datasource/instance/state/xml/url");
                node4.InnerText = d_url;
                XmlNode node5 = xmlDoc.SelectSingleNode("/XML/Input[@Title='ADVERTISEMENT']");
                node5.Attributes["Videos"].Value = adspath;
                XmlNode node6 = xmlDoc.SelectSingleNode("/XML/Input[@Title='BACKGROUND']");
                node6.Attributes["Videos"].Value = bgpath;
                XmlNode node7 = xmlDoc.SelectSingleNode("/XML/Input[@Title='CBB']");
                node7.Attributes["Videos"].Value = cbbpath;
                xmlDoc.Save(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BKE AirStack/Template/BKE AirStack.vmix");
                //string text = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CPIO TELE-RADYO/Template/CPIO TELE-RADYO.vmix");
                //text = text.Replace("&amp;#xD;&amp;#xA;", "&#xD;&#xA;");
                //File.WriteAllTextAsync(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CPIO TELE-RADYO/Template/CPIO TELE-RADYO.vmix", text); // READALLTEXT AND WRITE ALLTEXT IS THE CULPRIT
                Thread.Sleep(1000);
                File.Copy(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BKE AirStack/Template/BKE AirStack.vmix", "D:/BKE AirStack.vmix"); //Path.GetTempPath()+
                //Process.Start("C:\\Program Files (x86)\\vMix\\vMix64.exe",Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CPIO TELE-RADYO/Template/CPIO TELE-RADYO.vmix");
                Thread.Sleep(1000);
                Process.Start("C:\\Program Files (x86)\\vMix\\vMix64.exe", @"""D:/BKE AirStack.vmix");
                //Process.Start(new ProcessStartInfo { FileName = @"C:\Users\jan2x\AppData\Roaming\CPIO TELE-RADYO\Template\CPIO TELE-RADYO.vmix", UseShellExecute = true }); ITO DAPAT!!!!!!!!
                Cursor.Current = Cursors.Default;
            }
            else
            {
                MessageBox.Show("Please copy Template.vmix file into Template Folder", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        private void important_file()
        {

            if (!System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BKE AirStack/Template/Template.vmix"))
            {
                File.Copy("default.vmix", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BKE AirStack/Template/Template.vmix");
                MessageBox.Show("File copied", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void LOGO_TELERADYO_Click(object sender, EventArgs e)
        {
            DateTime n = DateTime.Now;
            string b = DateTime.Now.DayOfWeek.ToString();
            var processExists = Process.GetProcesses().Any(p => p.ProcessName.Contains("vMix64"));
            if (processExists == true)
            {
                MessageBox.Show("Please close vmix application first!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;

            }
            else
            {

                if (n.DayOfWeek.ToString() == "Monday")
                {
                    omBtn_Click(sender, e);
                }
                if (n.DayOfWeek.ToString() == "Tuesday")
                {
                    otBtn_Click(sender, e);
                }
                if (n.DayOfWeek.ToString() == "Wednesday")
                {
                    owBtn_Click(sender, e);
                }
                if (n.DayOfWeek.ToString() == "Thursday")
                {
                    othBtn_Click(sender, e);
                }
                if (n.DayOfWeek.ToString() == "Friday")
                {
                    ofBtn_Click(sender, e);
                }
                if (n.DayOfWeek.ToString() == "Saturday")
                {
                    osatBtn_Click(sender, e);
                }
                if (n.DayOfWeek.ToString() == "Sunday")
                {
                    osunBtn_Click(sender, e);
                }
            }

        }
    }
}
