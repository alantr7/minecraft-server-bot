using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlanInterface.Forms {
    class ServerManager {

        public static void SetStatus(string Status, string Ip, string Software, string Version, string Qt, int Qp) {

            FProgram.form.Invoke(new Action(() => {

                if (Ip != null)                             FProgram.form.label_ip.Text = $"{Ip}.aternos.me";
                if (Software != null && Version != null)    FProgram.form.label_version.Text = $"{Software} - {Version}";

                switch (Status) {
                    case "online":
                        FProgram.form.button1.Visible = false;
                        FProgram.form.label18.Visible = true;
                        FProgram.form.label18.Text = "Online";
                        FProgram.form.progressBar1.Visible = false;
                        break;
                    case "offline":
                        FProgram.form.button1.Visible = true;
                        FProgram.form.label18.Visible = false;
                        FProgram.form.progressBar1.Visible = false;
                        break;
                    case "queue":
                        FProgram.form.button1.Visible = false;
                        FProgram.form.label18.Visible = true;
                        FProgram.form.label18.Text = Qt;
                        FProgram.form.progressBar1.Visible = true;
                        FProgram.form.progressBar1.Value = 100 - Qp;
                        break;
                }
            }));
        }
    }
}
