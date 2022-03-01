using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DevExpress.UserSkins;
using DevExpress.Skins;
using DevExpress.LookAndFeel;

using System.IO;

namespace GTA_5_json_editor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            BonusSkins.Register();
            SkinManager.EnableFormSkins();
            UserLookAndFeel.Default.SetSkinStyle("DevExpress Style");

            //Stop PS3Lib infection
            if (new FileInfo("PS3Lib.dll").Length > 80000)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("PS3Lib.dll is unusually large a genuine PS3Lib file is around 70kb");
                Environment.Exit(0);
            }

            Application.Run(new mainForm());
        }
    }
}
