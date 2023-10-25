
namespace ImagesToText
{
	partial class WordMatrix
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.panel1 = new System.Windows.Forms.Panel();
            this.LabelNextWord = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.LabelOcrWord = new System.Windows.Forms.Label();
            this.LabelExpectedWord = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.LabelPreviousWord = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.LabelNextWord);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 187);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1770, 28);
            this.panel1.TabIndex = 13;
            this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WordMatrix_MouseDown);
            this.panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.WordMatrix_MouseMove);
            // 
            // LabelNextWord
            // 
            this.LabelNextWord.AutoSize = true;
            this.LabelNextWord.BackColor = System.Drawing.Color.Green;
            this.LabelNextWord.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LabelNextWord.Font = new System.Drawing.Font("Consolas", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.LabelNextWord.Location = new System.Drawing.Point(0, 0);
            this.LabelNextWord.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelNextWord.Name = "LabelNextWord";
            this.LabelNextWord.Size = new System.Drawing.Size(129, 28);
            this.LabelNextWord.TabIndex = 3;
            this.LabelNextWord.Text = "Next word";
            this.LabelNextWord.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.LabelOcrWord);
            this.panel2.Controls.Add(this.LabelExpectedWord);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 29);
            this.panel2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1770, 158);
            this.panel2.TabIndex = 14;
            // 
            // LabelOcrWord
            // 
            this.LabelOcrWord.AutoSize = true;
            this.LabelOcrWord.BackColor = System.Drawing.Color.Transparent;
            this.LabelOcrWord.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LabelOcrWord.Font = new System.Drawing.Font("Consolas", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.LabelOcrWord.Location = new System.Drawing.Point(1016, 0);
            this.LabelOcrWord.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelOcrWord.MinimumSize = new System.Drawing.Size(0, 58);
            this.LabelOcrWord.Name = "LabelOcrWord";
            this.LabelOcrWord.Size = new System.Drawing.Size(116, 58);
            this.LabelOcrWord.TabIndex = 14;
            this.LabelOcrWord.Text = "OCR word";
            this.LabelOcrWord.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LabelOcrWord.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WordMatrix_MouseDown);
            this.LabelOcrWord.MouseMove += new System.Windows.Forms.MouseEventHandler(this.WordMatrix_MouseMove);
            // 
            // LabelExpectedWord
            // 
            this.LabelExpectedWord.AutoSize = true;
            this.LabelExpectedWord.BackColor = System.Drawing.Color.YellowGreen;
            this.LabelExpectedWord.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LabelExpectedWord.Dock = System.Windows.Forms.DockStyle.Left;
            this.LabelExpectedWord.Font = new System.Drawing.Font("Consolas", 99.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.LabelExpectedWord.Location = new System.Drawing.Point(0, 0);
            this.LabelExpectedWord.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelExpectedWord.Name = "LabelExpectedWord";
            this.LabelExpectedWord.Size = new System.Drawing.Size(1016, 157);
            this.LabelExpectedWord.TabIndex = 13;
            this.LabelExpectedWord.Text = "Expected word";
            this.LabelExpectedWord.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LabelExpectedWord.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WordMatrix_MouseDown);
            this.LabelExpectedWord.MouseMove += new System.Windows.Forms.MouseEventHandler(this.WordMatrix_MouseMove);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.LabelPreviousWord);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1770, 29);
            this.panel3.TabIndex = 15;
            this.panel3.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WordMatrix_MouseDown);
            this.panel3.MouseMove += new System.Windows.Forms.MouseEventHandler(this.WordMatrix_MouseMove);
            // 
            // LabelPreviousWord
            // 
            this.LabelPreviousWord.AutoEllipsis = true;
            this.LabelPreviousWord.AutoSize = true;
            this.LabelPreviousWord.BackColor = System.Drawing.Color.Green;
            this.LabelPreviousWord.Dock = System.Windows.Forms.DockStyle.Top;
            this.LabelPreviousWord.Font = new System.Drawing.Font("Consolas", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.LabelPreviousWord.Location = new System.Drawing.Point(0, 0);
            this.LabelPreviousWord.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelPreviousWord.Name = "LabelPreviousWord";
            this.LabelPreviousWord.Size = new System.Drawing.Size(181, 28);
            this.LabelPreviousWord.TabIndex = 1;
            this.LabelPreviousWord.Text = "Previous word";
            this.LabelPreviousWord.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // WordMatrix
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel3);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "WordMatrix";
            this.Size = new System.Drawing.Size(1770, 215);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WordMatrix_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.WordMatrix_MouseMove);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label LabelNextWord;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label LabelOcrWord;
		private System.Windows.Forms.Label LabelExpectedWord;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Label LabelPreviousWord;
	}
}
