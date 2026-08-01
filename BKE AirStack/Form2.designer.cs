namespace BKE_Air_Stack
{
    partial class Form2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form2));
            sched_label = new System.Windows.Forms.Label();
            tBAnchor1 = new System.Windows.Forms.TextBox();
            dataGV = new System.Windows.Forms.DataGridView();
            InsertBtn = new System.Windows.Forms.Button();
            UpdateBtn = new System.Windows.Forms.Button();
            DeleteBtn = new System.Windows.Forms.Button();
            button5 = new System.Windows.Forms.Button();
            lblAnchor1 = new System.Windows.Forms.Label();
            lblAnchor2 = new System.Windows.Forms.Label();
            tBAnchor2 = new System.Windows.Forms.TextBox();
            lblAnchor3 = new System.Windows.Forms.Label();
            tBAnchor3 = new System.Windows.Forms.TextBox();
            lblBedding = new System.Windows.Forms.Label();
            tBBedding = new System.Windows.Forms.TextBox();
            lblOBB = new System.Windows.Forms.Label();
            tBOBB = new System.Windows.Forms.TextBox();
            lblLogo = new System.Windows.Forms.Label();
            tBLogo = new System.Windows.Forms.TextBox();
            label7 = new System.Windows.Forms.Label();
            lblprogram = new System.Windows.Forms.Label();
            tBProgram = new System.Windows.Forms.TextBox();
            sqlupBtn = new System.Windows.Forms.Button();
            sqldownBtn = new System.Windows.Forms.Button();
            sqLiteCommand1 = new System.Data.SQLite.SQLiteCommand();
            lblBG = new System.Windows.Forms.Label();
            tBBG = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            tBCBB = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)dataGV).BeginInit();
            SuspendLayout();
            // 
            // sched_label
            // 
            sched_label.Anchor = System.Windows.Forms.AnchorStyles.Top;
            sched_label.AutoSize = true;
            sched_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            sched_label.ForeColor = System.Drawing.Color.FromArgb(235, 42, 83);
            sched_label.Location = new System.Drawing.Point(301, 10);
            sched_label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            sched_label.Name = "sched_label";
            sched_label.Size = new System.Drawing.Size(82, 25);
            sched_label.TabIndex = 0;
            sched_label.Text = "SCHED";
            // 
            // tBAnchor1
            // 
            tBAnchor1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            tBAnchor1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            tBAnchor1.Location = new System.Drawing.Point(83, 66);
            tBAnchor1.Margin = new System.Windows.Forms.Padding(4);
            tBAnchor1.Name = "tBAnchor1";
            tBAnchor1.Size = new System.Drawing.Size(168, 26);
            tBAnchor1.TabIndex = 2;
            // 
            // dataGV
            // 
            dataGV.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dataGV.BackgroundColor = System.Drawing.Color.FromArgb(24, 22, 34);
            dataGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGV.Location = new System.Drawing.Point(266, 36);
            dataGV.Margin = new System.Windows.Forms.Padding(4);
            dataGV.Name = "dataGV";
            dataGV.RowHeadersVisible = false;
            dataGV.RowHeadersWidth = 51;
            dataGV.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dataGV.Size = new System.Drawing.Size(507, 455);
            dataGV.TabIndex = 20;
            dataGV.CellClick += dataGV_CellClick;
            dataGV.CellContentClick += dataGV_CellContentClick;
            dataGV.ColumnAdded += dataGV_ColumnAdded;
            // 
            // InsertBtn
            // 
            InsertBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            InsertBtn.BackColor = System.Drawing.Color.FromArgb(24, 22, 34);
            InsertBtn.FlatAppearance.BorderSize = 0;
            InsertBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            InsertBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            InsertBtn.ForeColor = System.Drawing.Color.LightGray;
            InsertBtn.Location = new System.Drawing.Point(21, 445);
            InsertBtn.Margin = new System.Windows.Forms.Padding(4);
            InsertBtn.Name = "InsertBtn";
            InsertBtn.Size = new System.Drawing.Size(72, 46);
            InsertBtn.TabIndex = 8;
            InsertBtn.Text = "INSERT";
            InsertBtn.UseVisualStyleBackColor = false;
            InsertBtn.Click += InsertBtn_Click;
            // 
            // UpdateBtn
            // 
            UpdateBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            UpdateBtn.BackColor = System.Drawing.Color.FromArgb(24, 22, 34);
            UpdateBtn.FlatAppearance.BorderSize = 0;
            UpdateBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            UpdateBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            UpdateBtn.ForeColor = System.Drawing.Color.LightGray;
            UpdateBtn.Location = new System.Drawing.Point(100, 445);
            UpdateBtn.Margin = new System.Windows.Forms.Padding(4);
            UpdateBtn.Name = "UpdateBtn";
            UpdateBtn.Size = new System.Drawing.Size(72, 46);
            UpdateBtn.TabIndex = 8;
            UpdateBtn.Text = "UPDATE";
            UpdateBtn.UseVisualStyleBackColor = false;
            UpdateBtn.Click += UpdateBtn_Click;
            // 
            // DeleteBtn
            // 
            DeleteBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            DeleteBtn.BackColor = System.Drawing.Color.FromArgb(24, 22, 34);
            DeleteBtn.FlatAppearance.BorderSize = 0;
            DeleteBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            DeleteBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            DeleteBtn.ForeColor = System.Drawing.Color.LightGray;
            DeleteBtn.Location = new System.Drawing.Point(178, 445);
            DeleteBtn.Margin = new System.Windows.Forms.Padding(4);
            DeleteBtn.Name = "DeleteBtn";
            DeleteBtn.Size = new System.Drawing.Size(72, 46);
            DeleteBtn.TabIndex = 8;
            DeleteBtn.Text = "DELETE";
            DeleteBtn.UseVisualStyleBackColor = false;
            DeleteBtn.Click += DeleteBtn_Click;
            // 
            // button5
            // 
            button5.FlatAppearance.BorderSize = 0;
            button5.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(235, 42, 83);
            button5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            button5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            button5.ForeColor = System.Drawing.Color.LightGray;
            button5.Image = (System.Drawing.Image)resources.GetObject("button5.Image");
            button5.Location = new System.Drawing.Point(0, 0);
            button5.Margin = new System.Windows.Forms.Padding(4);
            button5.Name = "button5";
            button5.Size = new System.Drawing.Size(29, 29);
            button5.TabIndex = 7;
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // lblAnchor1
            // 
            lblAnchor1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblAnchor1.AutoSize = true;
            lblAnchor1.ForeColor = System.Drawing.Color.FromArgb(235, 42, 83);
            lblAnchor1.Location = new System.Drawing.Point(19, 71);
            lblAnchor1.Name = "lblAnchor1";
            lblAnchor1.Size = new System.Drawing.Size(58, 15);
            lblAnchor1.TabIndex = 8;
            lblAnchor1.Text = "Anchor 1:";
            // 
            // lblAnchor2
            // 
            lblAnchor2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblAnchor2.AutoSize = true;
            lblAnchor2.ForeColor = System.Drawing.Color.FromArgb(235, 42, 83);
            lblAnchor2.Location = new System.Drawing.Point(19, 102);
            lblAnchor2.Name = "lblAnchor2";
            lblAnchor2.Size = new System.Drawing.Size(58, 15);
            lblAnchor2.TabIndex = 10;
            lblAnchor2.Text = "Anchor 2:";
            // 
            // tBAnchor2
            // 
            tBAnchor2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            tBAnchor2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            tBAnchor2.Location = new System.Drawing.Point(83, 96);
            tBAnchor2.Margin = new System.Windows.Forms.Padding(4);
            tBAnchor2.Name = "tBAnchor2";
            tBAnchor2.Size = new System.Drawing.Size(168, 26);
            tBAnchor2.TabIndex = 3;
            // 
            // lblAnchor3
            // 
            lblAnchor3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblAnchor3.AutoSize = true;
            lblAnchor3.ForeColor = System.Drawing.Color.FromArgb(235, 42, 83);
            lblAnchor3.Location = new System.Drawing.Point(19, 131);
            lblAnchor3.Name = "lblAnchor3";
            lblAnchor3.Size = new System.Drawing.Size(58, 15);
            lblAnchor3.TabIndex = 12;
            lblAnchor3.Text = "Anchor 3:";
            // 
            // tBAnchor3
            // 
            tBAnchor3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            tBAnchor3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            tBAnchor3.Location = new System.Drawing.Point(83, 126);
            tBAnchor3.Margin = new System.Windows.Forms.Padding(4);
            tBAnchor3.Name = "tBAnchor3";
            tBAnchor3.Size = new System.Drawing.Size(168, 26);
            tBAnchor3.TabIndex = 4;
            // 
            // lblBedding
            // 
            lblBedding.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblBedding.AutoSize = true;
            lblBedding.ForeColor = System.Drawing.Color.FromArgb(235, 42, 83);
            lblBedding.Location = new System.Drawing.Point(19, 222);
            lblBedding.Name = "lblBedding";
            lblBedding.Size = new System.Drawing.Size(54, 15);
            lblBedding.TabIndex = 18;
            lblBedding.Text = "Bedding:";
            // 
            // tBBedding
            // 
            tBBedding.AllowDrop = true;
            tBBedding.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            tBBedding.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            tBBedding.Location = new System.Drawing.Point(83, 216);
            tBBedding.Margin = new System.Windows.Forms.Padding(4);
            tBBedding.Name = "tBBedding";
            tBBedding.Size = new System.Drawing.Size(168, 26);
            tBBedding.TabIndex = 7;
            tBBedding.DragDrop += tBBedding_DragDrop;
            tBBedding.DragEnter += tBLogo_DragEnter;
            // 
            // lblOBB
            // 
            lblOBB.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblOBB.AutoSize = true;
            lblOBB.ForeColor = System.Drawing.Color.FromArgb(235, 42, 83);
            lblOBB.Location = new System.Drawing.Point(39, 192);
            lblOBB.Name = "lblOBB";
            lblOBB.Size = new System.Drawing.Size(33, 15);
            lblOBB.TabIndex = 16;
            lblOBB.Text = "OBB:";
            // 
            // tBOBB
            // 
            tBOBB.AllowDrop = true;
            tBOBB.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            tBOBB.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            tBOBB.Location = new System.Drawing.Point(83, 186);
            tBOBB.Margin = new System.Windows.Forms.Padding(4);
            tBOBB.Name = "tBOBB";
            tBOBB.Size = new System.Drawing.Size(168, 26);
            tBOBB.TabIndex = 6;
            tBOBB.DragDrop += tBOBB_DragDrop;
            tBOBB.DragEnter += tBLogo_DragEnter;
            // 
            // lblLogo
            // 
            lblLogo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblLogo.AutoSize = true;
            lblLogo.ForeColor = System.Drawing.Color.FromArgb(235, 42, 83);
            lblLogo.Location = new System.Drawing.Point(37, 163);
            lblLogo.Name = "lblLogo";
            lblLogo.Size = new System.Drawing.Size(37, 15);
            lblLogo.TabIndex = 14;
            lblLogo.Text = "Logo:";
            // 
            // tBLogo
            // 
            tBLogo.AllowDrop = true;
            tBLogo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            tBLogo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            tBLogo.Location = new System.Drawing.Point(83, 156);
            tBLogo.Margin = new System.Windows.Forms.Padding(4);
            tBLogo.Name = "tBLogo";
            tBLogo.Size = new System.Drawing.Size(168, 26);
            tBLogo.TabIndex = 5;
            tBLogo.DragDrop += tBLogo_DragDrop;
            tBLogo.DragEnter += tBLogo_DragEnter;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new System.Drawing.Font("Calibri", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label7.ForeColor = System.Drawing.Color.White;
            label7.Location = new System.Drawing.Point(10, 348);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(152, 60);
            label7.TabIndex = 19;
            label7.Text = "Note: Anchor Name and Position is separated\r\n          by Comma ( ,  symbol) IE.\r\n          Juan Dela Cruz,Magician\r\n\r\nNote: You can drag file in the Logo, OBB,\r\n          Bedding and Background";
            // 
            // lblprogram
            // 
            lblprogram.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblprogram.AutoSize = true;
            lblprogram.ForeColor = System.Drawing.Color.FromArgb(235, 42, 83);
            lblprogram.Location = new System.Drawing.Point(19, 42);
            lblprogram.Name = "lblprogram";
            lblprogram.Size = new System.Drawing.Size(56, 15);
            lblprogram.TabIndex = 21;
            lblprogram.Text = "Program:";
            // 
            // tBProgram
            // 
            tBProgram.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            tBProgram.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            tBProgram.Location = new System.Drawing.Point(83, 36);
            tBProgram.Margin = new System.Windows.Forms.Padding(4);
            tBProgram.Name = "tBProgram";
            tBProgram.Size = new System.Drawing.Size(168, 26);
            tBProgram.TabIndex = 1;
            // 
            // sqlupBtn
            // 
            sqlupBtn.BackColor = System.Drawing.Color.FromArgb(24, 22, 34);
            sqlupBtn.FlatAppearance.BorderSize = 0;
            sqlupBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            sqlupBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            sqlupBtn.ForeColor = System.Drawing.Color.LightGray;
            sqlupBtn.Location = new System.Drawing.Point(204, 352);
            sqlupBtn.Margin = new System.Windows.Forms.Padding(4);
            sqlupBtn.Name = "sqlupBtn";
            sqlupBtn.Size = new System.Drawing.Size(46, 27);
            sqlupBtn.TabIndex = 25;
            sqlupBtn.Text = "↑";
            sqlupBtn.UseVisualStyleBackColor = false;
            sqlupBtn.Visible = false;
            sqlupBtn.Click += sqlupBtn_Click;
            // 
            // sqldownBtn
            // 
            sqldownBtn.BackColor = System.Drawing.Color.FromArgb(24, 22, 34);
            sqldownBtn.FlatAppearance.BorderSize = 0;
            sqldownBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            sqldownBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            sqldownBtn.ForeColor = System.Drawing.Color.LightGray;
            sqldownBtn.Location = new System.Drawing.Point(204, 382);
            sqldownBtn.Margin = new System.Windows.Forms.Padding(4);
            sqldownBtn.Name = "sqldownBtn";
            sqldownBtn.Size = new System.Drawing.Size(46, 27);
            sqldownBtn.TabIndex = 26;
            sqldownBtn.Text = "↓";
            sqldownBtn.UseVisualStyleBackColor = false;
            sqldownBtn.Visible = false;
            sqldownBtn.Click += sqldownBtn_Click;
            // 
            // sqLiteCommand1
            // 
            sqLiteCommand1.CommandText = null;
            // 
            // lblBG
            // 
            lblBG.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblBG.AutoSize = true;
            lblBG.ForeColor = System.Drawing.Color.FromArgb(235, 42, 83);
            lblBG.Location = new System.Drawing.Point(2, 254);
            lblBG.Name = "lblBG";
            lblBG.Size = new System.Drawing.Size(74, 15);
            lblBG.TabIndex = 28;
            lblBG.Text = "Background:";
            // 
            // tBBG
            // 
            tBBG.AllowDrop = true;
            tBBG.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            tBBG.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            tBBG.Location = new System.Drawing.Point(83, 246);
            tBBG.Margin = new System.Windows.Forms.Padding(4);
            tBBG.Name = "tBBG";
            tBBG.Size = new System.Drawing.Size(168, 26);
            tBBG.TabIndex = 8;
            tBBG.DragDrop += tBBG_DragDrop;
            tBBG.DragEnter += tBLogo_DragEnter;
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            label1.AutoSize = true;
            label1.ForeColor = System.Drawing.Color.FromArgb(235, 42, 83);
            label1.Location = new System.Drawing.Point(37, 285);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(32, 15);
            label1.TabIndex = 30;
            label1.Text = "CBB:";
            // 
            // tBCBB
            // 
            tBCBB.AllowDrop = true;
            tBCBB.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            tBCBB.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            tBCBB.Location = new System.Drawing.Point(82, 278);
            tBCBB.Margin = new System.Windows.Forms.Padding(4);
            tBCBB.Name = "tBCBB";
            tBCBB.Size = new System.Drawing.Size(168, 26);
            tBCBB.TabIndex = 29;
            tBCBB.DragDrop += tBCBB_DragDrop;
            tBCBB.DragEnter += tBLogo_DragEnter;
            // 
            // Form2
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(32, 30, 45);
            ClientSize = new System.Drawing.Size(798, 513);
            Controls.Add(label1);
            Controls.Add(tBCBB);
            Controls.Add(lblBG);
            Controls.Add(tBBG);
            Controls.Add(sqldownBtn);
            Controls.Add(sqlupBtn);
            Controls.Add(lblprogram);
            Controls.Add(tBProgram);
            Controls.Add(label7);
            Controls.Add(lblBedding);
            Controls.Add(tBBedding);
            Controls.Add(lblOBB);
            Controls.Add(tBOBB);
            Controls.Add(lblLogo);
            Controls.Add(tBLogo);
            Controls.Add(lblAnchor3);
            Controls.Add(tBAnchor3);
            Controls.Add(lblAnchor2);
            Controls.Add(tBAnchor2);
            Controls.Add(lblAnchor1);
            Controls.Add(button5);
            Controls.Add(DeleteBtn);
            Controls.Add(UpdateBtn);
            Controls.Add(InsertBtn);
            Controls.Add(dataGV);
            Controls.Add(tBAnchor1);
            Controls.Add(sched_label);
            Margin = new System.Windows.Forms.Padding(4);
            Name = "Form2";
            Text = "Form2";
            Load += Form2_Load;
            ((System.ComponentModel.ISupportInitialize)dataGV).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label sched_label;
        private System.Windows.Forms.TextBox tBAnchor1;
        private System.Windows.Forms.DataGridView dataGV;
        private System.Windows.Forms.Button InsertBtn;
        private System.Windows.Forms.Button UpdateBtn;
        private System.Windows.Forms.Button DeleteBtn;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Label lblAnchor1;
        private System.Windows.Forms.Label lblAnchor2;
        private System.Windows.Forms.TextBox tBAnchor2;
        private System.Windows.Forms.Label lblAnchor3;
        private System.Windows.Forms.TextBox tBAnchor3;
        private System.Windows.Forms.Label lblBedding;
        private System.Windows.Forms.TextBox tBBedding;
        private System.Windows.Forms.Label lblOBB;
        private System.Windows.Forms.TextBox tBOBB;
        private System.Windows.Forms.Label lblLogo;
        private System.Windows.Forms.TextBox tBLogo;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblprogram;
        private System.Windows.Forms.TextBox tBProgram;
        private System.Windows.Forms.Button sqlupBtn;
        private System.Windows.Forms.Button sqldownBtn;
        private System.Data.SQLite.SQLiteCommand sqLiteCommand1;
        private System.Windows.Forms.Label lblBG;
        private System.Windows.Forms.TextBox tBBG;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tBCBB;
    }
}