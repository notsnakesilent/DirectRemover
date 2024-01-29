using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Runtime.InteropServices;
using System.Web.UI.WebControls;
using System.Threading;

namespace DirectRemover
{
    public partial class Form1 : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public DateTime now = DateTime.Now;

        public static List<string> infectados = new List<string>();

        public Form1()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.TransparencyKey = Color.IndianRed;
            DriveInfo[] unidades = DriveInfo.GetDrives();

            foreach (DriveInfo unidad in unidades)
            {
                if (unidad.IsReady)
                {
                    string nombreEtiqueta = !string.IsNullOrEmpty(unidad.VolumeLabel)
                        ? $" ({unidad.VolumeLabel})"
                        : "";

                    guna2ComboBox1.Items.Add($"{unidad.Name} {nombreEtiqueta}");
                }
            }

        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            this.WindowState= FormWindowState.Minimized;
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            log.Visible = true;
            panel2.Visible= true;
            var t = new Thread(Deinfect);
            t.Name = "Cleaner";
            t.Priority = ThreadPriority.AboveNormal;
            t.Start();

        }

        private void Deinfect()
        {
            string unidadSeleccionada = guna2ComboBox1.SelectedItem.ToString().Split(' ')[0];
            log.Text = $"[{now.ToString("HH:mm:ss")}] Obteniendo archivos EXE en la unidad {unidadSeleccionada} , esto puede llevar tiempo";
            string[] archivosExe = Directory.GetFiles(unidadSeleccionada, "*.exe", SearchOption.AllDirectories);
            log.Text = "";
            foreach (string exe in archivosExe)
            {
                try
                {
                    var fileInfo = new System.IO.FileInfo(exe);
                    if(fileInfo.Length.ToString().Equals("534016"))
                    {
                        using (var fileStream = new FileStream(exe, FileMode.Open, FileAccess.Read))
                        {

                            var peFile = new PeNet.PeFile(fileStream);

                       if(peFile.ImpHash.Equals("9a06f0024c1694774ae97311608bab5b"))
                        {
                            infectados.Add(exe);
                        }
                       fileStream.Close();
                         }
                    }


                    foreach (string inf in infectados)
                    {
                        try
                        {
                            File.Delete(exe);
                            log.Text = $"{Environment.NewLine}[{now.ToString("HH:mm:ss")}] Se removio {exe} infectado";
                            continue;
                        }
                        catch (Exception ex)
                        {
                            log.Text = $"{Environment.NewLine}[{now.ToString("HH:mm:ss")}] Error {ex.Message} al borrar";
                            continue;
                        }
                    }


                    FileAttributes atributos = File.GetAttributes(exe);
                        bool tieneLosTresAtributos = (atributos & FileAttributes.Hidden) == FileAttributes.Hidden &&
                        (atributos & FileAttributes.System) == FileAttributes.System;
                        if(tieneLosTresAtributos)
                        {
                            atributos &= ~(FileAttributes.Hidden | FileAttributes.ReadOnly | FileAttributes.System);
                            File.SetAttributes(exe, atributos);
                            log.Text = Environment.NewLine+"[" + now.ToString("HH: mm:ss") + "]" + " Se le removieron los artributos a " + exe;
                        }
                      

                
                }
                catch (Exception ex)
                {
                    log.Text = $"{Environment.NewLine}[{now.ToString("HH:mm:ss")}] Error {ex.Message} al parsear";
                    continue;
                }
            }

        }
    }
}
