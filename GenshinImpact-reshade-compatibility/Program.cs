using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;

namespace GenshinImpact_reshade_compatibility
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.SetCompatibleTextRenderingDefault(false);

            var app = new AppController();
            app.Run(args);
        }

        class AppController : WindowsFormsApplicationBase
        {
            public AppController() : base(AuthenticationMode.Windows)
            {
                this.IsSingleInstance = true;
                this.ShutdownStyle = ShutdownMode.AfterMainFormCloses;
                this.EnableVisualStyles = true;
            }

            protected override void OnCreateMainForm()
            {
                this.MainForm = new Form1();
            }
        }
    }
}
