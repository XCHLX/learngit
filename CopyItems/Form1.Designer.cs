namespace CopyItems
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.lbname1 = new System.Windows.Forms.Label();
            this.lbsize = new System.Windows.Forms.Label();
            this.lbcolcor = new System.Windows.Forms.Label();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.button2 = new System.Windows.Forms.Button();
            this.lbsellder = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbsellderid = new System.Windows.Forms.TextBox();
            this.tbpid = new System.Windows.Forms.TextBox();
            this.tbchildid = new System.Windows.Forms.TextBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(22, 25);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "开始";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lbname1
            // 
            this.lbname1.AutoSize = true;
            this.lbname1.Location = new System.Drawing.Point(88, 62);
            this.lbname1.Name = "lbname1";
            this.lbname1.Size = new System.Drawing.Size(0, 12);
            this.lbname1.TabIndex = 4;
            // 
            // lbsize
            // 
            this.lbsize.AutoSize = true;
            this.lbsize.Location = new System.Drawing.Point(88, 93);
            this.lbsize.Name = "lbsize";
            this.lbsize.Size = new System.Drawing.Size(0, 12);
            this.lbsize.TabIndex = 6;
            // 
            // lbcolcor
            // 
            this.lbcolcor.AutoSize = true;
            this.lbcolcor.Location = new System.Drawing.Point(88, 120);
            this.lbcolcor.Name = "lbcolcor";
            this.lbcolcor.Size = new System.Drawing.Size(0, 12);
            this.lbcolcor.TabIndex = 8;
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.HorizontalScrollbar = true;
            this.listBox1.ItemHeight = 12;
            this.listBox1.Location = new System.Drawing.Point(387, 10);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(610, 436);
            this.listBox1.TabIndex = 9;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(253, 223);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 10;
            this.button2.Text = "重新复制";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // lbsellder
            // 
            this.lbsellder.AutoSize = true;
            this.lbsellder.Location = new System.Drawing.Point(23, 93);
            this.lbsellder.Name = "lbsellder";
            this.lbsellder.Size = new System.Drawing.Size(89, 12);
            this.lbsellder.TabIndex = 12;
            this.lbsellder.Text = "卖家ID(必填)：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 123);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 13;
            this.label1.Text = "任务id:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 156);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 12);
            this.label2.TabIndex = 14;
            this.label2.Text = "单个任务宝贝ID:";
            // 
            // tbsellderid
            // 
            this.tbsellderid.Location = new System.Drawing.Point(121, 89);
            this.tbsellderid.Name = "tbsellderid";
            this.tbsellderid.Size = new System.Drawing.Size(207, 21);
            this.tbsellderid.TabIndex = 16;
            // 
            // tbpid
            // 
            this.tbpid.Location = new System.Drawing.Point(121, 120);
            this.tbpid.Name = "tbpid";
            this.tbpid.Size = new System.Drawing.Size(207, 21);
            this.tbpid.TabIndex = 17;
            // 
            // tbchildid
            // 
            this.tbchildid.Location = new System.Drawing.Point(121, 153);
            this.tbchildid.Name = "tbchildid";
            this.tbchildid.Size = new System.Drawing.Size(207, 21);
            this.tbchildid.TabIndex = 18;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(25, 227);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(72, 16);
            this.checkBox1.TabIndex = 19;
            this.checkBox1.Text = "全部选择";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(121, 62);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(207, 21);
            this.textBox1.TabIndex = 20;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 12);
            this.label3.TabIndex = 21;
            this.label3.Text = "卖家id后缀:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1022, 469);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.tbchildid);
            this.Controls.Add(this.tbpid);
            this.Controls.Add(this.tbsellderid);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbsellder);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.lbcolcor);
            this.Controls.Add(this.lbsize);
            this.Controls.Add(this.lbname1);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "宝贝复制";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lbname1;
        private System.Windows.Forms.Label lbsize;
        private System.Windows.Forms.Label lbcolcor;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label lbsellder;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbsellderid;
        private System.Windows.Forms.TextBox tbpid;
        private System.Windows.Forms.TextBox tbchildid;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label3;
    }
}

