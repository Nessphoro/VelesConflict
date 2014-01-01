using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Editor;

namespace EditorWrapper
{
    static class Program
    {
        internal static EditorBase Game;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            WrapperForm wf = new WrapperForm();
            wf.Show();
            using (Game = new EditorBase(wf.GameRender.Handle))
            {
                Game.Run();
            }
        }
    }
}
