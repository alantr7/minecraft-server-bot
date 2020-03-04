using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using AlanLibrary;
using System.Threading;

namespace AlanInterface {

    public class Aternos {

        #region VARIABLES
        private List<string> NextCommands = new List<string>();
        public static string[] Players = new string[0];
        #endregion

        public void v2RefreshStatus() {
            try {
                WebClient wc;
                RunOnAternos(wc = new WebClient(), () => {
                    string r = wc.DownloadString("https://aternos.org/server");

                    string lastStatus = r
                        .Split(new string[] {
                        "var lastStatus = {"
                        }, StringSplitOptions.None)[1]
                        .Split(new string[] {
                        "};"
                        }, StringSplitOptions.None)[0];

                    AlanV rv = new AlanV(lastStatus);
                    string ip = rv.Get("ip")[0].Split('.')[0];
                    string software = rv.Get("software")[0];
                    string version = rv.Get("version")[0];
                    string status = rv.Get("class")[1];

                    switch (status) {
                        case "offline":
                            FProgram.ExecuteScript("SetStatus", new object[] {
                                "offline", ip, software, version, "", ""
                            });
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
                                    FProgram.ExecuteScript("AddPlayer", new object[] {
                                        players[i]
                                    });
                            }

                            for (int i = 0; i < Players.Length; i++)
                                if (!players.Contains(Players[i]))
                                    FProgram.ExecuteScript("RemovePlayer", new object[] { Players[i] });

                            Players = players;
                            FProgram.ExecuteScript("SetStatus", new object[] { "online", ip, software, version, "", "" });
                            break;
                        case "queueing":

                            int per = (int)Double.Parse(lastStatus
                                .Split(new string[] {
                                "\"percentage\":"
                                }, StringSplitOptions.None)[1]
                                .Split(',')[0]);

                            try {
                                string icon = rv.Get("icon")[0];
                                if (icon == "fa-exclamation-triangle")
                                    Confirm();
                            }
                            catch { }
                            FProgram.ExecuteScript("SetStatus", new object[] {
                                "queue", ip, software, version, rv.Get("time")[0], per
                            });
                            break;
                        case "loading":
                        case "loading starting":
                            FProgram.ExecuteScript("SetStatus", new object[] {
                                "queue", ip, software, version, "Ucitavanje resursa", 0
                            });
                            break;
                    }
                });
            }
            catch { }
        }
        public void SendMinecraftMessage(string time, string name, string msg) {
            NextCommands.Add($"tellraw @a {{\"time\":\"{time}\",\"text\":\"<{name}> " +
                $"{msg.Replace('&', '§').Replace("\"", "\\\"").Replace("\\", "\\\\")}\"}}");
            ExecuteCommands();
        }
        int MinecraftChatLineCount = 0;
        public void RefreshMinecraftChat() {
            WebClient wc;
            RunOnAternos(wc = new WebClient(), () => {
                try {
                    string[] lines = new string[0];
                    if (MinecraftChatLineCount >= 0)
                        lines = wc.DownloadString("https://aternos.org/panel/ajax/console.php?ASEC=fi0cgnx3yii00000%3Aki2uw8csgt000000").Split(new string[] { "\",\"" }, StringSplitOptions.None);


                    if (lines.Length < MinecraftChatLineCount) MinecraftChatLineCount = 0;
                    for (int i = MinecraftChatLineCount; i < lines.Length; i++) {
                        try {
                            string l = lines[i].Replace("\\u001b[m\\r", "").Replace("\\u001b[m\"]", "").Replace("\\\"}\\r", "").Replace("\\\"}\"]", "");
                            if (!l.Contains("Chat Thread") && !l.Contains("tellraw") && !l.Contains("time"))
                                continue;
                            if (!l.Contains("tellraw")) {
                                string time = l.Split('[')[2].Split(']')[0];
                                string message = l.Split(new string[] { "]: " }, StringSplitOptions.None)[1];
                                string sender = message.Substring(1).Split('>')[0];
                                string content = message.Substring(3 + sender.Length);

                                FProgram.ExecuteScript("AddMessage", new object[] {
                                    sender, content, time
                                });
                            }
                            else {
                                string m = l.Substring(24);
                                string t = m.Split('\\')[0];
                                m = m.Split(',')[1].Substring(11);

                                string sender = m.Substring(1).Split('>')[0];
                                string content = m.Substring(3 + sender.Length);

                                FProgram.ExecuteScript("AddMinecraftMessage", new object[] {
                                    t, sender, content
                                });
                            }
                        }
                        catch { }
                    }
                    MinecraftChatLineCount = lines.Length - 5;
                    lines = null;
                }
                catch { }
            });
        }
        public void Confirm() {
            WebClient wc;
            RunOnAternos(wc = new WebClient(), () => {
                wc.DownloadString("https://aternos.org/panel/ajax/confirm.php?ASEC=fi0cgnx3yii00000%3Aki2uw8csgt000000");
            });
        }
        public void StartServer() {
            Console.WriteLine("[Aternos] Function called");
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
                    wc.UploadString("https://aternos.org/panel/ajax/command.php?ASEC=fi0cgnx3yii00000%3Aki2uw8csgt000000", "cmd=" + s.Replace(' ', '+'));
                });
            }
            NextCommands.Clear();
        }
        public void RunOnAternos(WebClient wc, Action a) {
            new Thread(() => {
                wc.UseDefaultCredentials = true;
                wc.Encoding = Encoding.UTF8;
                wc.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.116 Safari/537.36");
                wc.Headers.Add(HttpRequestHeader.Cookie, "ATERNOS_SESSION=" + Program.ATERNOS_SESSION + ";ATERNOS_SEC_fi0cgnx3yii00000=ki2uw8csgt000000;__cfduid=d733afe73174bb68643350c30f38a82c11581746695");

                a.Invoke();
                wc.Dispose();
            }).Start();
        }

    }
}
