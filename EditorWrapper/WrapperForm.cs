using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Reflection;
using VelesConflictRemote;
using Editor;

namespace EditorWrapper
{
    public partial class WrapperForm : Form
    {
        

        public WrapperForm()
        {
            InitializeComponent();
            
        }

        private void WrapperForm_Load(object sender, EventArgs e)
        {
            
            Thread wait = new Thread(new ThreadStart(() =>
            {
                while (Program.Game == null)
                {
                    Thread.Sleep(100);
                }
                
                Program.Game.SelectionChanged += new EventHandler<EventArgs>(SelectionChanged);
            }));
            wait.Start();

        }
        void SelectionChanged(object sender, EventArgs e)
        {
            PlanetProperty.SelectedObject = Program.Game.Selected;
        }
        protected override void OnClosed(EventArgs e)
        {
            Program.Game.Exit();
            base.OnClosed(e);
        }

        private void btn_Reset_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you really want to reset the map?", "Warning!", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                ScriptLocation = null;
                Program.Game.Reset();
            }
        }

        private void btn_NewPlanet_Click(object sender, EventArgs e)
        {
            Program.Game.NewPlanet();
        }

        private void btn_Delete_Click(object sender, EventArgs e)
        {
            Program.Game.DeletePlanet(PlanetProperty.SelectedObject as Editor.Planet);
        }

        private void btn_ReflectVerticaly_Click(object sender, EventArgs e)
        {
            Program.Game.ReflectVerticaly();
        }

        private void btn_ReflectHorizontaly_Click(object sender, EventArgs e)
        {
            Program.Game.ReflectHorizontaly();
        }

        private void btn_ReflectDiagonaly_Click(object sender, EventArgs e)
        {
            Program.Game.ReflectDiagonaly();
        }

        private void btn_Load_Click(object sender, EventArgs e)
        {
            Program.Game.Load();
            textBox1.Text = Program.Game.MapWidth.ToString();
            textBox2.Text = Program.Game.MapHeight.ToString();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            int Try;
            if (int.TryParse(textBox1.Text, out Try))
            {
                Program.Game.MapWidth = Try;
                Program.Game.RefreshLine();
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            int Try;
            if (int.TryParse(textBox2.Text, out Try))
            {
                Program.Game.MapHeight= Try;
                Program.Game.RefreshLine();
            }
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            if (ScriptLocation == null)
            {
                Program.Game.Save();
            }
            else
            {
                Program.Game.Save(ScriptLocation);
            }
        }
        string ScriptLocation = null;
        private void btn_SetScript_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog();
            open.CheckPathExists = true;
            open.CheckFileExists = true;
            open.AddExtension = true;
            open.Filter = "XML files (*.xml)|*.xml";
            System.Windows.Forms.DialogResult result = open.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ScriptLocation = open.FileName;
            }
        }

        private void GameRender_SizeChanged(object sender, EventArgs e)
        {
            Program.Game.ApplySizeChange(GameRender.Size.Width, GameRender.Size.Height);
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PhoneSelectForm selectionForm = new PhoneSelectForm();
            selectionForm.ShowDialog();
        }

        private void PlanetProperty_Click(object sender, EventArgs e)
        {

        }

        private void PlanetProperty_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            Planet Selected = PlanetProperty.SelectedObject as Planet;
            if (EditorBase.defaultConnection != null && !EditorBase.defaultConnection.Dead)
            {
                string Command = string.Format("+UPDATE {0} {1} {2} {3} {4} {5} {6} {7}\n", Selected.ID, Selected.Position.X, Selected.Position.Y, Selected.PlanetSize, Selected.Forces, (int)Selected.Owner, Selected.GrowthCounter, Selected.Growth);
                EditorBase.defaultConnection.Write(Command);
            }
        }

        private void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (EditorBase.defaultConnection != null && !EditorBase.defaultConnection.Dead)
            {
                string Command = string.Format("+PLAY\n");
                EditorBase.defaultConnection.Write(Command);
            }
        }
    }
}
