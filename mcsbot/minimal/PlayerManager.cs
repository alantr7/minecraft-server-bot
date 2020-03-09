using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlanInterface.Forms {
    public class PlayerManager {

        public static string LinkCode = "";

        public static Dictionary<string, FlowLayoutPanel> Players = new Dictionary<string, FlowLayoutPanel>(); 

        public static void AddPlayer(string Name) {

            FlowLayoutPanel Panel = new FlowLayoutPanel();
            Panel.Width = FProgram.form.flowLayoutPanel1.Width - 6;
            Panel.Height = 44;
            Panel.FlowDirection = FlowDirection.LeftToRight;
            Panel.MouseEnter += (e, s) => {
                Panel.BackColor = System.Drawing.Color.FromArgb(255, 27, 37, 47);
            };
            Panel.MouseLeave += (e, s) => {
                Panel.BackColor = System.Drawing.Color.FromArgb(255, 37, 47, 57);
            };

            PictureBox AvatarImg = new PictureBox();
            AvatarImg.Width = 36;
            AvatarImg.Height = 36;
            AvatarImg.Margin = new Padding(8, 4, 0, 0);
            AvatarImg.MouseEnter += (e, s) => {
                Panel.BackColor = System.Drawing.Color.FromArgb(255, 27, 37, 47);
            };
            AvatarImg.MouseLeave += (e, s) => {
                Panel.BackColor = System.Drawing.Color.FromArgb(255, 37, 47, 57);
            };
            AvatarImg.BorderStyle = BorderStyle.FixedSingle;
            AvatarImg.SizeMode = PictureBoxSizeMode.StretchImage;
            AvatarImg.Load("https://minotar.net/avatar/" + Name + "/100.png");

            Label NameLabel = new Label();
            NameLabel.Text = Name;
            NameLabel.Font = new System.Drawing.Font("Arial", 12);
            NameLabel.Height = 48;
            NameLabel.Padding = new Padding(0, 12, 0, 0);
            NameLabel.MouseEnter += (e, s) => {
                Panel.BackColor = System.Drawing.Color.FromArgb(255, 27, 37, 47);
            };
            NameLabel.MouseLeave += (e, s) => {
                Panel.BackColor = System.Drawing.Color.FromArgb(255, 37, 47, 57);
            };
            NameLabel.ForeColor = System.Drawing.Color.White;
            NameLabel.Cursor = Cursors.Hand;

            NameLabel.Click += (e, s) => {
                CreatePlayerData(Name);
            };

            Panel.Controls.AddRange(new Control[] {
                AvatarImg, NameLabel
            });

            Players[Name] = Panel;

            FProgram.form.Invoke(new Action(() => {
                FProgram.form.flowLayoutPanel1.Controls.Add(Panel);
            }));
        }
        public static void RemovePlayer(string Name) {
            FProgram.form.Invoke(new Action(() => {
                try {
                    Players[Name].Dispose();
                    Players.Remove(Name);
                }
                catch { }
            }));
        }

        public static void CreatePlayerData(string Name) {
            FProgram.form.Invoke(new Action(() => {

                if (Name.Length <= 3) {
                    FProgram.form.flowLayoutPanel1.Visible = false;
                    return;
                }

                FProgram.form.flowLayoutPanel1.Visible = true;

                // CREATE DESIGN
                if (Program.MINECRAFTNAME.Length > 0) FProgram.form.button3.Text = "Teleport";

                FProgram.form.label2.Text = Name;
                Console.WriteLine("Loading statistics for " + Name);

                FProgram.form.textBox1.Text = "";
                FProgram.form.textBox1.Visible = false;

                FProgram.form.label11.Text = "";
                FProgram.form.label12.Text = "";
                FProgram.form.label13.Text = "";
                FProgram.form.label14.Text = "";
                FProgram.form.label15.Text = "";
                FProgram.form.label16.Text = "";
                FProgram.form.label17.Text = "";

                FProgram.form.tableLayoutPanel1.Visible = true;
                FProgram.form.pictureBox1.Load("https://minotar.net/armor/body/" + Name + "/100.png");

                WebClient wc = new WebClient();
                string UUID;
                try {
                    UUID = wc.DownloadString($"https://raw.githubusercontent.com/alantr7/minecraft-server-bot/master/uuid/{Name}.txt").Replace("\n", "");
                }
                catch {
                    FProgram.form.label3.Text = "Igrac nije registrovan. Javi Alanu";
                    return;
                }
                FProgram.form.label3.Text = UUID;
                FProgram.Aternos.RunOnAternos(wc, () => {
                    string data = wc.DownloadString("https://aternos.org/files/world/stats/" + UUID + ".json");
                    if (data.Contains("editor"))
                        data = data
                            .Split(new string[] { "<div id=\"editor\" class=\"editor\">" }, StringSplitOptions.None)[1]
                            .Split(new string[] { "</div>" }, StringSplitOptions.None)[0];
                    Console.WriteLine(data);
                    FProgram.form.Invoke(new Action(() => {
                        try {
                            FProgram.form.label11.Text = data.Split(
                                new string[] { "minecraft:play_one_minute\":" }, StringSplitOptions.None)[1].Split(',')[0];
                        }
                        catch { }
                        try {
                            FProgram.form.label12.Text = data.Split(
                                new string[] { "minecraft:player_kills\":" }, StringSplitOptions.None)[1].Split(',')[0];
                        }
                        catch { }
                        try {
                            FProgram.form.label13.Text = data.Split(
                                new string[] { "minecraft:mob_kills\":" }, StringSplitOptions.None)[1].Split(',')[0];
                        }
                        catch { }
                        try {
                            FProgram.form.label14.Text = data.Split(
                                new string[] { "minecraft:raid_win\":" }, StringSplitOptions.None)[1].Split(',')[0];
                        }
                        catch { }
                        try {
                            FProgram.form.label15.Text = data.Split(
                                new string[] { "minecraft:damage_dealt\":" }, StringSplitOptions.None)[1].Split(',')[0];
                        }
                        catch { }
                        try {
                            FProgram.form.label16.Text = data.Split(
                                new string[] { "minecraft:damage_taken\":" }, StringSplitOptions.None)[1].Split(',')[0];
                        }
                        catch { }
                        try {
                            FProgram.form.label17.Text = data.Split(
                                new string[] { "minecraft:deaths\":" }, StringSplitOptions.None)[1].Split(',')[0];
                        }
                        catch { }
                    }));
                }, true);
            }));
        }

    }
}
