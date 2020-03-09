using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using AlanLibrary;
using System.Threading;
using AlanInterface.Forms;

namespace AlanInterface {

    public class Aternos {

        #region VARIABLES
        private List<string> NextCommands = new List<string>();
        public static string[] Players = new string[0];
        #endregion

        public void v2RefreshStatus() {
            try {

                Console.WriteLine(GC.GetTotalMemory(false));

                WebClient wc;
                RunOnAternos(wc = new WebClient(), () => {
                    string r = wc.DownloadString("https://aternos.org/server");

                    string lastStatus = r
                        .Split(new string[] { "var lastStatus = {" }, StringSplitOptions.None)[1]
                        .Split(new string[] { "};" }, StringSplitOptions.None)[0];

                    string ip = AlanV.QuickFindValue(lastStatus, "ip")[0].Split('.')[0];
                    string software = AlanV.QuickFindValue(lastStatus, "software")[0];
                    string version = AlanV.QuickFindValue(lastStatus, "version")[0];
                    string status = AlanV.QuickFindValue(lastStatus, "class")[1];

                    switch (status) {
                        case "offline":
                            ServerManager.SetStatus("offline", ip, software, version, "", 0);
                            break;
                        case "online":
                            string ps = lastStatus
                                .Split(new string[] {
                                "\"playerlist\":["
                                }, StringSplitOptions.None)[1]
                                .Split(']')[0];

                            string[] players;
                            if (ps.Contains(","))
                                players = ps.Split(',');
                            else players = new string[] { ps };

                            for (int i = 0; i < players.Length; i++) {
                                players[i] = players[i].Replace("\"", "");
                                if (!Players.Contains(players[i]) && players[i].Length > 3)
                                    PlayerManager.AddPlayer(players[i]);
                            }

                            for (int i = 0; i < Players.Length; i++)
                                if (!players.Contains(Players[i]))
                                    PlayerManager.RemovePlayer(Players[i]);

                            Players = players;
                            ServerManager.SetStatus("online", ip, software, version, "", 0);                 break;
                        case "queueing":

                            int per = (int)Double.Parse(lastStatus
                                .Split(new string[] {
                                "\"percentage\":"
                                }, StringSplitOptions.None)[1]
                                .Split(',')[0]);

                            try {
                                string icon = AlanV.QuickFindValue(lastStatus, "icon")[0];
                                if (icon == "fa-exclamation-triangle")
                                    Confirm();
                            }
                            catch { }
                            ServerManager.SetStatus("queue", ip, software, version, AlanV.QuickFindValue(lastStatus, "time")[0], per);
                            break;
                        case "loading":
                        case "loading starting": ServerManager.SetStatus("queue", ip, software, version, "Ucitavanje resursa", 0);
                            break;
                    }
                    GC.Collect(2);
                    Console.WriteLine(GC.GetTotalMemory(false));
                }, false);
            }
            catch { }
        }
        public void SendMinecraftMessage(string time, string name, string msg) {
            NextCommands.Add($"tellraw @a {{\"time\":\"{time}\",\"text\":\"<{name}> " +
                $"{msg.Replace('&', '§').Replace("\\", "\\\\").Replace("\"", "\\\"")}\"}}");
            ExecuteCommands();
        }
        public void Confirm() {
            WebClient wc;
            RunOnAternos(wc = new WebClient(), () => {
                wc.DownloadString("https://aternos.org/panel/ajax/confirm.php?ASEC=fi0cgnx3yii00000%3Aki2uw8csgt000000");
            });
        }
        public void StartServer() {
            Console.WriteLine("[Aternos] Function called");
            ServerManager.SetStatus("queue", null, null, null, "Pokretanje", 0);
            WebClient wc;
            RunOnAternos(wc = new WebClient(), () => {
                Console.WriteLine("[Aternos] Starting server...");
                wc.DownloadString("https://aternos.org/panel/ajax/start.php?headstart=0&ASEC=fi0cgnx3yii00000%3Aki2uw8csgt000000");
            });
        }
        public void Link(string n, string code) {
            NextCommands.Add($"tellraw {n} {{\"text\":\"§cKod §8§l>> §7{code} \"}}");
            ExecuteCommands();
        }
        public string GenerateLinkCode() {
            string code = "";
            Random r = new Random();
            for (int i = 0; i < 5; i++)
                code += r.Next(0, 9);

            return code;
        }
        public void Tp(string n, string t) {
            NextCommands.Add("tp " + n + " " + t);
            NextCommands.Add("tellraw " + n + " {\"text\":\"§7Teleportovan si do §l" + t + "\"}");
            NextCommands.Add("tellraw " + t + " {\"text\":\"§7§l" + n + " §7se tp do tebe\"}");

            ExecuteCommands();
        }
        public void ExecuteCommands() {
            foreach (string s in NextCommands) {
                WebClient wc;
                RunOnAternos(wc = new WebClient(), () => {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    wc.UploadStringAsync(new Uri("https://aternos.org/panel/ajax/command.php?ASEC=fi0cgnx3yii00000%3Aki2uw8csgt000000"), "cmd=" + s.Replace(' ', '+'));
                }, false);
            }
            NextCommands.Clear();
        }

        public void RunOnAternos(WebClient wc, Action a, bool NewThread = true) {

            wc.UseDefaultCredentials = true;
            wc.Encoding = Encoding.UTF8;
            wc.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.116 Safari/537.36");
            wc.Headers.Add(HttpRequestHeader.Cookie, "ATERNOS_SESSION=" + Program.ATERNOS_SESSION + ";ATERNOS_SEC_fi0cgnx3yii00000=ki2uw8csgt000000;__cfduid=d733afe73174bb68643350c30f38a82c11581746695");

            if (NewThread) {
                new Thread(() => {
                    try {
                        a.Invoke();
                    }
                    catch { }
                    wc.Dispose();
                }).Start();
            }
            else {
                try {
                    a.Invoke();
                }
                catch { }
                wc.Dispose();
            }
        }

    }
}
