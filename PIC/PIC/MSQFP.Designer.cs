namespace PIC
{
    partial class MSQFP
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
            this.btnBegin = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.txtToolCount = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnBegin
            // 
            this.btnBegin.Location = new System.Drawing.Point(651, 31);
            this.btnBegin.Name = "btnBegin";
            this.btnBegin.Size = new System.Drawing.Size(75, 47);
            this.btnBegin.TabIndex = 0;
            this.btnBegin.Text = "开启";
            this.btnBegin.UseVisualStyleBackColor = true;
            this.btnBegin.Click += new System.EventHandler(this.button1_Click);
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 15;
            this.listBox1.Location = new System.Drawing.Point(47, 126);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(679, 259);
            this.listBox1.TabIndex = 1;
            // 
            // txtToolCount
            // 
            this.txtToolCount.Location = new System.Drawing.Point(243, 31);
            this.txtToolCount.Name = "txtToolCount";
            this.txtToolCount.Size = new System.Drawing.Size(100, 25);
            this.txtToolCount.TabIndex = 2;
            this.txtToolCount.Text = "1";
            // 
            // MSQFP
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.txtToolCount);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.btnBegin);
            this.Name = "MSQFP";
            this.Text = "违禁词检测分配";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnBegin;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.TextBox txtToolCount;
    }
}