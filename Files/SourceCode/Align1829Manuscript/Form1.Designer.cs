
namespace Align1829Manuscript
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Col1829 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Col1830 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnLineNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ButtonReload = new System.Windows.Forms.Button();
            this.Affect1830Checkbox = new System.Windows.Forms.CheckBox();
            this.ButtonDelete = new System.Windows.Forms.Button();
            this.ButtonAdd = new System.Windows.Forms.Button();
            this.ButtonSave = new System.Windows.Forms.Button();
            this.ButtonAlign = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Col1829,
            this.Col1830,
            this.ColumnLineNumber});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.Size = new System.Drawing.Size(1200, 796);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dataGridView1_CellPainting);
            // 
            // Col1829
            // 
            this.Col1829.HeaderText = "1829";
            this.Col1829.Name = "Col1829";
            this.Col1829.ReadOnly = true;
            // 
            // Col1830
            // 
            this.Col1830.HeaderText = "1830";
            this.Col1830.Name = "Col1830";
            this.Col1830.ReadOnly = true;
            // 
            // ColumnLineNumber
            // 
            this.ColumnLineNumber.HeaderText = "Line";
            this.ColumnLineNumber.Name = "ColumnLineNumber";
            this.ColumnLineNumber.ReadOnly = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.ButtonReload);
            this.panel1.Controls.Add(this.Affect1830Checkbox);
            this.panel1.Controls.Add(this.ButtonDelete);
            this.panel1.Controls.Add(this.ButtonAdd);
            this.panel1.Controls.Add(this.ButtonSave);
            this.panel1.Controls.Add(this.ButtonAlign);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 702);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1200, 94);
            this.panel1.TabIndex = 1;
            // 
            // ButtonReload
            // 
            this.ButtonReload.Location = new System.Drawing.Point(603, 30);
            this.ButtonReload.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ButtonReload.Name = "ButtonReload";
            this.ButtonReload.Size = new System.Drawing.Size(112, 41);
            this.ButtonReload.TabIndex = 5;
            this.ButtonReload.Text = "Reload";
            this.ButtonReload.UseVisualStyleBackColor = true;
            this.ButtonReload.Click += new System.EventHandler(this.ButtonReload_Click);
            // 
            // Affect1830Checkbox
            // 
            this.Affect1830Checkbox.AutoSize = true;
            this.Affect1830Checkbox.Location = new System.Drawing.Point(18, 37);
            this.Affect1830Checkbox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Affect1830Checkbox.Name = "Affect1830Checkbox";
            this.Affect1830Checkbox.Size = new System.Drawing.Size(114, 27);
            this.Affect1830Checkbox.TabIndex = 4;
            this.Affect1830Checkbox.Text = "Affect right";
            this.Affect1830Checkbox.UseVisualStyleBackColor = true;
            // 
            // ButtonDelete
            // 
            this.ButtonDelete.Location = new System.Drawing.Point(261, 32);
            this.ButtonDelete.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ButtonDelete.Name = "ButtonDelete";
            this.ButtonDelete.Size = new System.Drawing.Size(112, 41);
            this.ButtonDelete.TabIndex = 3;
            this.ButtonDelete.Text = "Delete";
            this.ButtonDelete.UseVisualStyleBackColor = true;
            this.ButtonDelete.Click += new System.EventHandler(this.ButtonDelete_Click);
            // 
            // ButtonAdd
            // 
            this.ButtonAdd.Location = new System.Drawing.Point(140, 32);
            this.ButtonAdd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ButtonAdd.Name = "ButtonAdd";
            this.ButtonAdd.Size = new System.Drawing.Size(112, 41);
            this.ButtonAdd.TabIndex = 2;
            this.ButtonAdd.Text = "Add";
            this.ButtonAdd.UseVisualStyleBackColor = true;
            this.ButtonAdd.Click += new System.EventHandler(this.ButtonAdd_Click);
            // 
            // ButtonSave
            // 
            this.ButtonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonSave.Location = new System.Drawing.Point(1070, 32);
            this.ButtonSave.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ButtonSave.Name = "ButtonSave";
            this.ButtonSave.Size = new System.Drawing.Size(112, 41);
            this.ButtonSave.TabIndex = 1;
            this.ButtonSave.Text = "Save";
            this.ButtonSave.UseVisualStyleBackColor = true;
            this.ButtonSave.Click += new System.EventHandler(this.ButtonSave_Click);
            // 
            // ButtonAlign
            // 
            this.ButtonAlign.Location = new System.Drawing.Point(482, 30);
            this.ButtonAlign.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ButtonAlign.Name = "ButtonAlign";
            this.ButtonAlign.Size = new System.Drawing.Size(112, 41);
            this.ButtonAlign.TabIndex = 0;
            this.ButtonAlign.Text = "Align";
            this.ButtonAlign.UseVisualStyleBackColor = true;
            this.ButtonAlign.Click += new System.EventHandler(this.ButtonAlign_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 796);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.dataGridView1);
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Form1_KeyPress);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button ButtonSave;
        private System.Windows.Forms.Button ButtonAlign;
        private System.Windows.Forms.Button ButtonAdd;
        private System.Windows.Forms.Button ButtonDelete;
        private System.Windows.Forms.CheckBox Affect1830Checkbox;
        private System.Windows.Forms.DataGridViewTextBoxColumn Col1829;
        private System.Windows.Forms.DataGridViewTextBoxColumn Col1830;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnLineNumber;
        private System.Windows.Forms.Button ButtonReload;
    }
}

