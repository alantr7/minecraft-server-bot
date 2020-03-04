using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace AlanInterface {

    [ComVisible(true)]
    public partial class FProgram : Form {

        public static Aternos Aternos;
        private static FProgram form;

        private bool Initialized = false;

        public FProgram() {

            InitializeExplorer();
            InitializeComponent();

            form = this;
            Program.LoadData();

            webBrowser.Url = new Uri(Program.APP_URL + "user-interface.php");
            webBrowser.DocumentCompleted += (s, e) => {
                Console.WriteLine("Loading Done. Initialized: " + Initialized);
                if (!Initialized) {
                    Aternos = new Aternos();

                    webBrowser.ObjectForScripting = this;
                    ExecuteScript("SetCookie", new object[] {
                        Program.PHPSESSID
                    });
                    Controls.Add(webBrowser);

                    CreateThread();
                    Initialized = true;

                    UpdateApplication();
                }
                else ExecuteScript("SetCookie", new object[] {
                    Program.PHPSESSID
                });
            };

        }

        void InitializeExplorer() {
            var fileName = Process.GetCurrentProcess().ProcessName + ".exe";

            using (var Key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true))
                Key.SetValue(fileName, 11000, RegistryValueKind.DWord);
        }

        void CreateThread() =>
            new Thread(() => {
                while (true) {
                    Aternos.v2RefreshStatus();
                    Thread.Sleep(5000);
                }
            }).Start();

        #region FORM STUFF + EXECUTE
        public static void ExecuteScript(string name, object[] o) =>
            form.Invoke(new Action(() => {
                form.webBrowser.Document.InvokeScript(name, o);
            }));

        public static void ExecuteScript(string name) =>
            form.Invoke(new Action(() => {
                form.webBrowser.Document.InvokeScript(name);
            }));

        private void FProgram_FormClosing(object sender, FormClosingEventArgs e) {
            Environment.Exit(0);
        }
        #endregion

        #region JAVASCRIPT INTERFACE - MINECRAFT
        public void StartServer() => Aternos.StartServer();
        public void SendMinecraftMessage(string time, string name, string msg) => Aternos.SendMinecraftMessage(time, name, msg);
        public void RefreshMinecraftChat() => Aternos.RefreshMinecraftChat();
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
                            ExecuteScript("AddMessage", new object[] { "", $"Preuzimam update &lv{v}", DateTime.Now.Ticks });
                            wc.DownloadFile("https://raw.githubusercontent.com/alantr7/minecraft-server-bot/master/mcsbot.file", Program.DIRECTORY + "mcsbot.update");

                            ExecuteScript("AddMessage", new object[] { "", $"Verzija {v} skinuta. Aplikacija ce se restartovati za 3 sekunde" });

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
                        else ExecuteScript("AddMessage", new object[] { "", $"Verzija programa - &lv{v}", DateTime.Now.Ticks });
                    }
                }
                catch (Exception e) {
                    ExecuteScript("AddMessage", new object[] { "",
                        $"Doslo je do greske kod provjere verzije. Posalji Alanu:<br> &l{e.Message}"
                    });
                }
            }).Start();
        }
    }
}