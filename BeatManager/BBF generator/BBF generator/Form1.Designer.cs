namespace BBF_generator
{
    partial class frmGenerator
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
            this.noOfLayers = new System.Windows.Forms.NumericUpDown();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.txtFile0 = new System.Windows.Forms.TextBox();
            this.txtFile1 = new System.Windows.Forms.TextBox();
            this.txtFile2 = new System.Windows.Forms.TextBox();
            this.txtFile3 = new System.Windows.Forms.TextBox();
            this.txtFile4 = new System.Windows.Forms.TextBox();
            this.btnBrowse0 = new System.Windows.Forms.Button();
            this.btnBrowse1 = new System.Windows.Forms.Button();
            this.btnBrowse4 = new System.Windows.Forms.Button();
            this.btnBrowse2 = new System.Windows.Forms.Button();
            this.btnBrowse3 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.txtMP3 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.noOfLayers)).BeginInit();
            this.SuspendLayout();
            // 
            // noOfLayers
            // 
            this.noOfLayers.Location = new System.Drawing.Point(160, 12);
            this.noOfLayers.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.noOfLayers.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.noOfLayers.Name = "noOfLayers";
            this.noOfLayers.ReadOnly = true;
            this.noOfLayers.Size = new System.Drawing.Size(44, 20);
            this.noOfLayers.TabIndex = 0;
            this.noOfLayers.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.noOfLayers.ValueChanged += new System.EventHandler(this.noOfLayers_ValueChanged);
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(116, 194);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(146, 62);
            this.btnGenerate.TabIndex = 1;
            this.btnGenerate.Text = "Create BFF";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtFile0
            // 
            this.txtFile0.Location = new System.Drawing.Point(51, 38);
            this.txtFile0.Name = "txtFile0";
            this.txtFile0.ReadOnly = true;
            this.txtFile0.Size = new System.Drawing.Size(246, 20);
            this.txtFile0.TabIndex = 2;
            // 
            // txtFile1
            // 
            this.txtFile1.Location = new System.Drawing.Point(51, 64);
            this.txtFile1.Name = "txtFile1";
            this.txtFile1.ReadOnly = true;
            this.txtFile1.Size = new System.Drawing.Size(246, 20);
            this.txtFile1.TabIndex = 3;
            this.txtFile1.Visible = false;
            // 
            // txtFile2
            // 
            this.txtFile2.Location = new System.Drawing.Point(51, 90);
            this.txtFile2.Name = "txtFile2";
            this.txtFile2.ReadOnly = true;
            this.txtFile2.Size = new System.Drawing.Size(246, 20);
            this.txtFile2.TabIndex = 4;
            this.txtFile2.Visible = false;
            // 
            // txtFile3
            // 
            this.txtFile3.Location = new System.Drawing.Point(51, 116);
            this.txtFile3.Name = "txtFile3";
            this.txtFile3.ReadOnly = true;
            this.txtFile3.Size = new System.Drawing.Size(246, 20);
            this.txtFile3.TabIndex = 5;
            this.txtFile3.Visible = false;
            // 
            // txtFile4
            // 
            this.txtFile4.Location = new System.Drawing.Point(51, 142);
            this.txtFile4.Name = "txtFile4";
            this.txtFile4.ReadOnly = true;
            this.txtFile4.Size = new System.Drawing.Size(246, 20);
            this.txtFile4.TabIndex = 6;
            this.txtFile4.Visible = false;
            // 
            // btnBrowse0
            // 
            this.btnBrowse0.Location = new System.Drawing.Point(316, 39);
            this.btnBrowse0.Name = "btnBrowse0";
            this.btnBrowse0.Size = new System.Drawing.Size(61, 19);
            this.btnBrowse0.TabIndex = 7;
            this.btnBrowse0.Text = "Browse";
            this.btnBrowse0.UseVisualStyleBackColor = true;
            this.btnBrowse0.Click += new System.EventHandler(this.btnBrowse0_Click);
            // 
            // btnBrowse1
            // 
            this.btnBrowse1.Location = new System.Drawing.Point(316, 64);
            this.btnBrowse1.Name = "btnBrowse1";
            this.btnBrowse1.Size = new System.Drawing.Size(61, 19);
            this.btnBrowse1.TabIndex = 8;
            this.btnBrowse1.Text = "Browse";
            this.btnBrowse1.UseVisualStyleBackColor = true;
            this.btnBrowse1.Visible = false;
            this.btnBrowse1.Click += new System.EventHandler(this.btnBrowse1_Click);
            // 
            // btnBrowse4
            // 
            this.btnBrowse4.Location = new System.Drawing.Point(316, 142);
            this.btnBrowse4.Name = "btnBrowse4";
            this.btnBrowse4.Size = new System.Drawing.Size(61, 19);
            this.btnBrowse4.TabIndex = 9;
            this.btnBrowse4.Text = "Browse";
            this.btnBrowse4.UseVisualStyleBackColor = true;
            this.btnBrowse4.Visible = false;
            this.btnBrowse4.Click += new System.EventHandler(this.btnBrowse4_Click);
            // 
            // btnBrowse2
            // 
            this.btnBrowse2.Location = new System.Drawing.Point(316, 92);
            this.btnBrowse2.Name = "btnBrowse2";
            this.btnBrowse2.Size = new System.Drawing.Size(61, 19);
            this.btnBrowse2.TabIndex = 9;
            this.btnBrowse2.Text = "Browse";
            this.btnBrowse2.UseVisualStyleBackColor = true;
            this.btnBrowse2.Visible = false;
            this.btnBrowse2.Click += new System.EventHandler(this.btnBrowse2_Click);
            // 
            // btnBrowse3
            // 
            this.btnBrowse3.Location = new System.Drawing.Point(316, 117);
            this.btnBrowse3.Name = "btnBrowse3";
            this.btnBrowse3.Size = new System.Drawing.Size(61, 19);
            this.btnBrowse3.TabIndex = 10;
            this.btnBrowse3.Text = "Browse";
            this.btnBrowse3.UseVisualStyleBackColor = true;
            this.btnBrowse3.Visible = false;
            this.btnBrowse3.Click += new System.EventHandler(this.btnBrowse3_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(316, 169);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(61, 19);
            this.button1.TabIndex = 12;
            this.button1.Text = "Browse";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // txtMP3
            // 
            this.txtMP3.Location = new System.Drawing.Point(51, 168);
            this.txtMP3.Name = "txtMP3";
            this.txtMP3.ReadOnly = true;
            this.txtMP3.Size = new System.Drawing.Size(246, 20);
            this.txtMP3.TabIndex = 11;
            // 
            // frmGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(401, 266);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtMP3);
            this.Controls.Add(this.btnBrowse3);
            this.Controls.Add(this.btnBrowse2);
            this.Controls.Add(this.btnBrowse4);
            this.Controls.Add(this.btnBrowse1);
            this.Controls.Add(this.btnBrowse0);
            this.Controls.Add(this.txtFile4);
            this.Controls.Add(this.txtFile3);
            this.Controls.Add(this.txtFile2);
            this.Controls.Add(this.txtFile1);
            this.Controls.Add(this.txtFile0);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.noOfLayers);
            this.Name = "frmGenerator";
            this.Text = "BFF Generator";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.noOfLayers)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown noOfLayers;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.TextBox txtFile0;
        private System.Windows.Forms.TextBox txtFile1;
        private System.Windows.Forms.TextBox txtFile2;
        private System.Windows.Forms.TextBox txtFile3;
        private System.Windows.Forms.TextBox txtFile4;
        private System.Windows.Forms.Button btnBrowse0;
        private System.Windows.Forms.Button btnBrowse1;
        private System.Windows.Forms.Button btnBrowse4;
        private System.Windows.Forms.Button btnBrowse2;
        private System.Windows.Forms.Button btnBrowse3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtMP3;

    }
}

