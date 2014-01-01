using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VelesConflictRemote;
using Editor;

namespace EditorWrapper
{
    public partial class PhoneSelectForm : Form
    {
        public PhoneSelectForm()
        {
            InitializeComponent();
        }

        private void PhoneSelectForm_Load(object sender, EventArgs e)
        {
            lock(EditorBase.remoteWorker.PhoneConnections)
            {
                foreach (PhoneConnection phone in EditorBase.remoteWorker.PhoneConnections)
                {
                    list_Phones.Items.Add(string.Format("{0} {1}",phone.Name == null ? "No Name" : phone.Name, phone.DeviceId == null ? "No Id" : phone.DeviceId));
                }
            }
            
        }

        private void btn_Refresh_Click(object sender, EventArgs e)
        {
            int Index = list_Phones.SelectedIndex;
            list_Phones.Items.Clear();
            lock (EditorBase.remoteWorker.PhoneConnections)
            {
                foreach (PhoneConnection phone in EditorBase.remoteWorker.PhoneConnections)
                {
                    list_Phones.Items.Add(string.Format("{0} {1}", phone.Name == null ? "No Name" : phone.Name, phone.DeviceId == null ? "No Id" : phone.DeviceId));
                }
            }
            list_Phones.SelectedIndex = Index;
        }

        private void list_Phones_SelectedIndexChanged(object sender, EventArgs e)
        {
            lock (EditorBase.remoteWorker.PhoneConnections)
            {
                if (list_Phones.SelectedIndex == -1)
                    return;
                text_Name.Text = EditorBase.remoteWorker.PhoneConnections[list_Phones.SelectedIndex].Name;
            }
        }

        private void text_Name_TextChanged(object sender, EventArgs e)
        {
            lock (EditorBase.remoteWorker.PhoneConnections)
            {
                if (list_Phones.SelectedIndex == -1)
                    return;
                EditorBase.remoteWorker.PhoneConnections[list_Phones.SelectedIndex].Name = text_Name.Text;
                btn_Refresh_Click(null, null);
            }
        }

        private void btn_Select_Click(object sender, EventArgs e)
        {
            if (list_Phones.SelectedIndex == -1)
                return;
            EditorBase.defaultConnection = EditorBase.remoteWorker.PhoneConnections[list_Phones.SelectedIndex];
            Close();
        }
    }
}
