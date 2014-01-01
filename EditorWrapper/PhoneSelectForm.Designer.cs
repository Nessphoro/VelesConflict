namespace EditorWrapper
{
    partial class PhoneSelectForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.list_Phones = new System.Windows.Forms.ListBox();
            this.btn_Select = new System.Windows.Forms.Button();
            this.lbl_Name = new System.Windows.Forms.Label();
            this.text_Name = new System.Windows.Forms.TextBox();
            this.btn_Refresh = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // list_Phones
            // 
            this.list_Phones.FormattingEnabled = true;
            this.list_Phones.Location = new System.Drawing.Point(12, 12);
            this.list_Phones.Name = "list_Phones";
            this.list_Phones.Size = new System.Drawing.Size(682, 95);
            this.list_Phones.TabIndex = 0;
            this.list_Phones.SelectedIndexChanged += new System.EventHandler(this.list_Phones_SelectedIndexChanged);
            // 
            // btn_Select
            // 
            this.btn_Select.Location = new System.Drawing.Point(274, 169);
            this.btn_Select.Name = "btn_Select";
            this.btn_Select.Size = new System.Drawing.Size(75, 23);
            this.btn_Select.TabIndex = 1;
            this.btn_Select.Text = "Select";
            this.btn_Select.UseVisualStyleBackColor = true;
            this.btn_Select.Click += new System.EventHandler(this.btn_Select_Click);
            // 
            // lbl_Name
            // 
            this.lbl_Name.AutoSize = true;
            this.lbl_Name.Location = new System.Drawing.Point(451, 113);
            this.lbl_Name.Name = "lbl_Name";
            this.lbl_Name.Size = new System.Drawing.Size(72, 13);
            this.lbl_Name.TabIndex = 2;
            this.lbl_Name.Text = "Phone Name:";
            // 
            // text_Name
            // 
            this.text_Name.Location = new System.Drawing.Point(529, 110);
            this.text_Name.Name = "text_Name";
            this.text_Name.Size = new System.Drawing.Size(165, 20);
            this.text_Name.TabIndex = 3;
            this.text_Name.TextChanged += new System.EventHandler(this.text_Name_TextChanged);
            // 
            // btn_Refresh
            // 
            this.btn_Refresh.Location = new System.Drawing.Point(355, 169);
            this.btn_Refresh.Name = "btn_Refresh";
            this.btn_Refresh.Size = new System.Drawing.Size(75, 23);
            this.btn_Refresh.TabIndex = 4;
            this.btn_Refresh.Text = "Refresh";
            this.btn_Refresh.UseVisualStyleBackColor = true;
            this.btn_Refresh.Click += new System.EventHandler(this.btn_Refresh_Click);
            // 
            // PhoneSelectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(706, 204);
            this.Controls.Add(this.btn_Refresh);
            this.Controls.Add(this.text_Name);
            this.Controls.Add(this.lbl_Name);
            this.Controls.Add(this.btn_Select);
            this.Controls.Add(this.list_Phones);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "PhoneSelectForm";
            this.Text = "Phone Selection";
            this.Load += new System.EventHandler(this.PhoneSelectForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox list_Phones;
        private System.Windows.Forms.Button btn_Select;
        private System.Windows.Forms.Label lbl_Name;
        private System.Windows.Forms.TextBox text_Name;
        private System.Windows.Forms.Button btn_Refresh;
    }
}