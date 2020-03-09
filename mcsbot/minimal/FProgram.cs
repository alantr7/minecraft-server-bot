using AlanInterface.Forms;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace AlanInterface {

    [ComVisible(true)]
    public partial class FProgram : Form {

        public static Aternos Aternos;
        public static FProgram form;

        public FProgram() {
            InitializeComponent();
            form = this;
            Program.LoadData();
            Aternos = new Aternos();
            CreateThread();
            UpdateApplication();
        }

        void CreateThread() =>
            new Thread(() => {
                using (WebClient wc = new WebClient()) {
                    wc.Headers.Add(HttpRequestHeader.Cookie, "PHPSESSID=" + Program.PHPSESSID);
                    Program.MINECRAFTNAME = wc.DownloadString($"{Program.HOST}minecraft/set-cookie.php?SESSION-ID={Program.PHPSESSID}");
                    if (Program.MINECRAFTNAME.Length > 0) PlayerManager.CreatePlayerData(Program.MINECRAFTNAME);
                }
                while (true) {
                    Aternos.v2RefreshStatus();
                    Thread.Sleep(5000);
                }
            }).Start();

        private void FProgram_FormClosing(object sender, FormClosingEventArgs e) {
            Environment.Exit(0);
        }

        #region MINECRAFT FUNCTIONS
        public void Tp(string n, string t) => Aternos.Tp(n, t);
        public void Link(string n, string c) => Aternos.Link(n, c);
        #endregion

        void UpdateApplication() {
            new Thread(() => {
                try {
                    using (WebClient wc = new WebClient()) {
                        wc.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);
                        int v = Int32.Parse(wc.DownloadString("https://raw.githubusercontent.com/alantr7/minecraft-server-bot/master/latest-version.txt"));
                        if (v > Program.VERSION) {
                            //ExecuteScript("AddMessage", new object[] { "", $"Preuzimam update &lv{v}", DateTime.Now.Ticks });
                            wc.DownloadFile("https://raw.githubusercontent.com/alantr7/minecraft-server-bot/master/mcsbot.file", Program.DIRECTORY + "mcsbot.update");

                            //ExecuteScript("AddMessage", new object[] { "", $"Verzija {v} skinuta. Aplikacija ce se restartovati za 3 sekunde" });

                            Thread.Sleep(3000);
                            File.WriteAllLines(Program.DIRECTORY + "updater.bat", new string[] {
                                "timeout 1",
                                $"del \"{Program.DIRECTORY}mcsbot.exe\"",
                                $"rename \"{Program.DIRECTORY}mcsbot.update\" mcsbot.exe",
                                "timeout 1",
                                $"start \"\" \"{Program.DIRECTORY}mcsbot.exe\""
                            });

                            Process p = new Process();
                            p.StartInfo.FileName = Program.DIRECTORY + "updater.bat";
                            p.StartInfo.UseShellExecute = true;
                            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            p.Start();

                            Invoke(new Action(() => { Close(); }));

                        }
                        else { }//ExecuteScript("AddMessage", new object[] { "", $"Verzija programa - &lv{v}", DateTime.Now.Ticks });
                    }
                }
                catch (Exception e) {
                    /*ExecuteScript("AddMessage", new object[] { "",
                        $"Doslo je do greske kod provjere verzije. Posalji Alanu:<br> &l{e.Message}"
                    });*/
                }
            }).Start();
        }

        private void FProgram_Load(object sender, EventArgs e) {
            Resize += (e1, s) => {
                tableLayoutPanel1.Location = new Point(tableLayoutPanel1.Location.X,
                    Height / 2 - tableLayoutPanel1.Height / 2
                    );
            };
        }

        private void button1_Click(object sender, EventArgs e) => Aternos.StartServer();

        private void button2_Click(object sender, EventArgs e) {
            if (pictureBox1.Image != null) pictureBox1.Image.Dispose();
            tableLayoutPanel1.Visible = false;
        }

        private void button3_Click(object sender, EventArgs e) {
            if (button3.Text == "Povezi") {
                // GENERATE CODE
                string Code = "";
                Random random = new Random();
                for (int i = 0; i < 5; i++)
                    Code += random.Next(0, 9);
                PlayerManager.LinkCode = Code;

                textBox1.Visible = true;
                Aternos.Link(label2.Text, PlayerManager.LinkCode);
            }
            else Aternos.Tp(Program.MINECRAFTNAME, label2.Text);
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e) {
            if (textBox1.TextLength > 0 && PlayerManager.LinkCode.Length > 0 && textBox1.Text == PlayerManager.LinkCode) {
                textBox1.Visible = false;
                Program.MINECRAFTNAME = label2.Text;
                button3.Text = "Teleport";
                using (WebClient wc = new WebClient()) {
                    wc.Headers.Add(HttpRequestHeader.Cookie, $"PHPSESSID={Program.PHPSESSID}");
                    wc.DownloadString($"{Program.HOST}minecraft/set-cookie.php?nick={label2.Text}");
                }
            }
        }

        private void button4_Click(object sender, EventArgs e) {

            Process p = new Process();
            p.StartInfo.FileName = Program.DIRECTORY + "mcsbot.exe";
            p.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            p.Start();

            Close();

        }
    }
}