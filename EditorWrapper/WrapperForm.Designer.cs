namespace EditorWrapper
{
    partial class WrapperForm
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
            this.GameRender = new System.Windows.Forms.PictureBox();
            this.PlanetProperty = new System.Windows.Forms.PropertyGrid();
            this.btn_Reset = new System.Windows.Forms.Button();
            this.btn_NewPlanet = new System.Windows.Forms.Button();
            this.btn_Delete = new System.Windows.Forms.Button();
            this.btn_ReflectVerticaly = new System.Windows.Forms.Button();
            this.btn_ReflectHorizontaly = new System.Windows.Forms.Button();
            this.btn_ReflectDiagonaly = new System.Windows.Forms.Button();
            this.btn_Save = new System.Windows.Forms.Button();
            this.btn_Load = new System.Windows.Forms.Button();
            this.mapSettings = new System.Windows.Forms.GroupBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_SetScript = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.phoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disonnectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.GameRender)).BeginInit();
            this.mapSettings.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // GameRender
            // 
            this.GameRender.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GameRender.Location = new System.Drawing.Point(0, 0);
            this.GameRender.Name = "GameRender";
            this.GameRender.Size = new System.Drawing.Size(800, 480);
            this.GameRender.TabIndex = 0;
            this.GameRender.TabStop = false;
            this.GameRender.SizeChanged += new System.EventHandler(this.GameRender_SizeChanged);
            // 
            // PlanetProperty
            // 
            this.PlanetProperty.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PlanetProperty.Location = new System.Drawing.Point(806, 12);
            this.PlanetProperty.Name = "PlanetProperty";
            this.PlanetProperty.Size = new System.Drawing.Size(190, 468);
            this.PlanetProperty.TabIndex = 1;
            this.PlanetProperty.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.PlanetProperty_PropertyValueChanged);
            this.PlanetProperty.Click += new System.EventHandler(this.PlanetProperty_Click);
            // 
            // btn_Reset
            // 
            this.btn_Reset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_Reset.Location = new System.Drawing.Point(13, 487);
            this.btn_Reset.Name = "btn_Reset";
            this.btn_Reset.Size = new System.Drawing.Size(75, 23);
            this.btn_Reset.TabIndex = 2;
            this.btn_Reset.Text = "Reset";
            this.btn_Reset.UseVisualStyleBackColor = true;
            this.btn_Reset.Click += new System.EventHandler(this.btn_Reset_Click);
            // 
            // btn_NewPlanet
            // 
            this.btn_NewPlanet.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_NewPlanet.Location = new System.Drawing.Point(507, 489);
            this.btn_NewPlanet.Name = "btn_NewPlanet";
            this.btn_NewPlanet.Size = new System.Drawing.Size(75, 23);
            this.btn_NewPlanet.TabIndex = 3;
            this.btn_NewPlanet.Text = "New Planet";
            this.btn_NewPlanet.UseVisualStyleBackColor = true;
            this.btn_NewPlanet.Click += new System.EventHandler(this.btn_NewPlanet_Click);
            // 
            // btn_Delete
            // 
            this.btn_Delete.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_Delete.Location = new System.Drawing.Point(426, 489);
            this.btn_Delete.Name = "btn_Delete";
            this.btn_Delete.Size = new System.Drawing.Size(75, 23);
            this.btn_Delete.TabIndex = 4;
            this.btn_Delete.Text = "Delete";
            this.btn_Delete.UseVisualStyleBackColor = true;
            this.btn_Delete.Click += new System.EventHandler(this.btn_Delete_Click);
            // 
            // btn_ReflectVerticaly
            // 
            this.btn_ReflectVerticaly.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_ReflectVerticaly.Location = new System.Drawing.Point(426, 518);
            this.btn_ReflectVerticaly.Name = "btn_ReflectVerticaly";
            this.btn_ReflectVerticaly.Size = new System.Drawing.Size(75, 37);
            this.btn_ReflectVerticaly.TabIndex = 5;
            this.btn_ReflectVerticaly.Text = "Vertical Reflect";
            this.btn_ReflectVerticaly.UseVisualStyleBackColor = true;
            this.btn_ReflectVerticaly.Click += new System.EventHandler(this.btn_ReflectVerticaly_Click);
            // 
            // btn_ReflectHorizontaly
            // 
            this.btn_ReflectHorizontaly.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_ReflectHorizontaly.Location = new System.Drawing.Point(507, 516);
            this.btn_ReflectHorizontaly.Name = "btn_ReflectHorizontaly";
            this.btn_ReflectHorizontaly.Size = new System.Drawing.Size(75, 39);
            this.btn_ReflectHorizontaly.TabIndex = 6;
            this.btn_ReflectHorizontaly.Text = "Horizontal Reflect";
            this.btn_ReflectHorizontaly.UseVisualStyleBackColor = true;
            this.btn_ReflectHorizontaly.Click += new System.EventHandler(this.btn_ReflectHorizontaly_Click);
            // 
            // btn_ReflectDiagonaly
            // 
            this.btn_ReflectDiagonaly.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_ReflectDiagonaly.Location = new System.Drawing.Point(426, 561);
            this.btn_ReflectDiagonaly.Name = "btn_ReflectDiagonaly";
            this.btn_ReflectDiagonaly.Size = new System.Drawing.Size(156, 23);
            this.btn_ReflectDiagonaly.TabIndex = 7;
            this.btn_ReflectDiagonaly.Text = "Diagonal Reflect";
            this.btn_ReflectDiagonaly.UseVisualStyleBackColor = true;
            this.btn_ReflectDiagonaly.Click += new System.EventHandler(this.btn_ReflectDiagonaly_Click);
            // 
            // btn_Save
            // 
            this.btn_Save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Save.Location = new System.Drawing.Point(920, 487);
            this.btn_Save.Name = "btn_Save";
            this.btn_Save.Size = new System.Drawing.Size(75, 23);
            this.btn_Save.TabIndex = 8;
            this.btn_Save.Text = "Save";
            this.btn_Save.UseVisualStyleBackColor = true;
            this.btn_Save.Click += new System.EventHandler(this.btn_Save_Click);
            // 
            // btn_Load
            // 
            this.btn_Load.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Load.Location = new System.Drawing.Point(839, 487);
            this.btn_Load.Name = "btn_Load";
            this.btn_Load.Size = new System.Drawing.Size(75, 23);
            this.btn_Load.TabIndex = 9;
            this.btn_Load.Text = "Load";
            this.btn_Load.UseVisualStyleBackColor = true;
            this.btn_Load.Click += new System.EventHandler(this.btn_Load_Click);
            // 
            // mapSettings
            // 
            this.mapSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.mapSettings.Controls.Add(this.textBox2);
            this.mapSettings.Controls.Add(this.textBox1);
            this.mapSettings.Controls.Add(this.label2);
            this.mapSettings.Controls.Add(this.label1);
            this.mapSettings.Location = new System.Drawing.Point(13, 618);
            this.mapSettings.Name = "mapSettings";
            this.mapSettings.Size = new System.Drawing.Size(200, 87);
            this.mapSettings.TabIndex = 10;
            this.mapSettings.TabStop = false;
            this.mapSettings.Text = "Map Settings";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(51, 37);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(143, 20);
            this.textBox2.TabIndex = 3;
            this.textBox2.Text = "480";
            this.textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(51, 14);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(143, 20);
            this.textBox1.TabIndex = 2;
            this.textBox1.Text = "800";
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Height";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Width:";
            // 
            // btn_SetScript
            // 
            this.btn_SetScript.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_SetScript.Location = new System.Drawing.Point(13, 516);
            this.btn_SetScript.Name = "btn_SetScript";
            this.btn_SetScript.Size = new System.Drawing.Size(75, 23);
            this.btn_SetScript.TabIndex = 11;
            this.btn_SetScript.Text = "Set Script";
            this.btn_SetScript.UseVisualStyleBackColor = true;
            this.btn_SetScript.Click += new System.EventHandler(this.btn_SetScript_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.phoneToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 708);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1001, 24);
            this.menuStrip1.TabIndex = 12;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // phoneToolStripMenuItem
            // 
            this.phoneToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.playToolStripMenuItem,
            this.connectToolStripMenuItem,
            this.disonnectToolStripMenuItem});
            this.phoneToolStripMenuItem.Name = "phoneToolStripMenuItem";
            this.phoneToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.phoneToolStripMenuItem.Text = "Phone";
            // 
            // playToolStripMenuItem
            // 
            this.playToolStripMenuItem.Name = "playToolStripMenuItem";
            this.playToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.playToolStripMenuItem.Text = "Play";
            this.playToolStripMenuItem.Click += new System.EventHandler(this.playToolStripMenuItem_Click);
            // 
            // connectToolStripMenuItem
            // 
            this.connectToolStripMenuItem.Name = "connectToolStripMenuItem";
            this.connectToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.connectToolStripMenuItem.Text = "Connect";
            this.connectToolStripMenuItem.Click += new System.EventHandler(this.connectToolStripMenuItem_Click);
            // 
            // disonnectToolStripMenuItem
            // 
            this.disonnectToolStripMenuItem.Name = "disonnectToolStripMenuItem";
            this.disonnectToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.disonnectToolStripMenuItem.Text = "Disonnect";
            // 
            // WrapperForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1001, 732);
            this.Controls.Add(this.btn_SetScript);
            this.Controls.Add(this.mapSettings);
            this.Controls.Add(this.btn_Load);
            this.Controls.Add(this.btn_Save);
            this.Controls.Add(this.btn_ReflectDiagonaly);
            this.Controls.Add(this.btn_ReflectHorizontaly);
            this.Controls.Add(this.btn_ReflectVerticaly);
            this.Controls.Add(this.btn_Delete);
            this.Controls.Add(this.btn_NewPlanet);
            this.Controls.Add(this.btn_Reset);
            this.Controls.Add(this.PlanetProperty);
            this.Controls.Add(this.GameRender);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "WrapperForm";
            this.Text = "Veles Conflict Editor";
            this.Load += new System.EventHandler(this.WrapperForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.GameRender)).EndInit();
            this.mapSettings.ResumeLayout(false);
            this.mapSettings.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.PictureBox GameRender;
        private System.Windows.Forms.PropertyGrid PlanetProperty;
        private System.Windows.Forms.Button btn_Reset;
        private System.Windows.Forms.Button btn_NewPlanet;
        private System.Windows.Forms.Button btn_Delete;
        private System.Windows.Forms.Button btn_ReflectVerticaly;
        private System.Windows.Forms.Button btn_ReflectHorizontaly;
        private System.Windows.Forms.Button btn_ReflectDiagonaly;
        private System.Windows.Forms.Button btn_Save;
        private System.Windows.Forms.Button btn_Load;
        private System.Windows.Forms.GroupBox mapSettings;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_SetScript;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem phoneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disonnectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem playToolStripMenuItem;

    }
}

