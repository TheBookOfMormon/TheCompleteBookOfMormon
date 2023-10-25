
namespace ImagesToText
{
	partial class FormMain
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
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.OpenFileDialogImage = new System.Windows.Forms.OpenFileDialog();
			this.LabelOcrTextFilePath = new System.Windows.Forms.Label();
			this.TextBoxOcrTextFilePath = new System.Windows.Forms.TextBox();
			this.ButtonSelectOcrTextFile = new System.Windows.Forms.Button();
			this.ButtonSelectImagesFolder = new System.Windows.Forms.Button();
			this.TextBoxImagesFolderPath = new System.Windows.Forms.TextBox();
			this.LabelImagesFolderPath = new System.Windows.Forms.Label();
			this.ButtonProofRead = new System.Windows.Forms.Button();
			this.OpenFileDialogText = new System.Windows.Forms.OpenFileDialog();
			this.FolderBrowserDialogImages = new System.Windows.Forms.FolderBrowserDialog();
			this.SuspendLayout();
			// 
			// OpenFileDialogImage
			// 
			this.OpenFileDialogImage.Filter = "Image files|*.png";
			// 
			// LabelOcrTextFilePath
			// 
			this.LabelOcrTextFilePath.AutoSize = true;
			this.LabelOcrTextFilePath.Location = new System.Drawing.Point(12, 16);
			this.LabelOcrTextFilePath.Name = "LabelOcrTextFilePath";
			this.LabelOcrTextFilePath.Size = new System.Drawing.Size(74, 15);
			this.LabelOcrTextFilePath.TabIndex = 1;
			this.LabelOcrTextFilePath.Text = "OCR Text file";
			// 
			// TextBoxOcrTextFilePath
			// 
			this.TextBoxOcrTextFilePath.Location = new System.Drawing.Point(97, 12);
			this.TextBoxOcrTextFilePath.Name = "TextBoxOcrTextFilePath";
			this.TextBoxOcrTextFilePath.ReadOnly = true;
			this.TextBoxOcrTextFilePath.Size = new System.Drawing.Size(610, 23);
			this.TextBoxOcrTextFilePath.TabIndex = 2;
			// 
			// ButtonSelectOcrTextFile
			// 
			this.ButtonSelectOcrTextFile.Location = new System.Drawing.Point(713, 11);
			this.ButtonSelectOcrTextFile.Name = "ButtonSelectOcrTextFile";
			this.ButtonSelectOcrTextFile.Size = new System.Drawing.Size(75, 23);
			this.ButtonSelectOcrTextFile.TabIndex = 3;
			this.ButtonSelectOcrTextFile.Text = "...";
			this.ButtonSelectOcrTextFile.UseVisualStyleBackColor = true;
			this.ButtonSelectOcrTextFile.Click += new System.EventHandler(this.ButtonSelectOcrTextFile_Click);
			// 
			// ButtonSelectImagesFolder
			// 
			this.ButtonSelectImagesFolder.Location = new System.Drawing.Point(713, 41);
			this.ButtonSelectImagesFolder.Name = "ButtonSelectImagesFolder";
			this.ButtonSelectImagesFolder.Size = new System.Drawing.Size(75, 23);
			this.ButtonSelectImagesFolder.TabIndex = 6;
			this.ButtonSelectImagesFolder.Text = "...";
			this.ButtonSelectImagesFolder.UseVisualStyleBackColor = true;
			this.ButtonSelectImagesFolder.Click += new System.EventHandler(this.ButtonSelectImagesFolder_Click);
			// 
			// TextBoxImagesFolderPath
			// 
			this.TextBoxImagesFolderPath.Location = new System.Drawing.Point(97, 41);
			this.TextBoxImagesFolderPath.Name = "TextBoxImagesFolderPath";
			this.TextBoxImagesFolderPath.ReadOnly = true;
			this.TextBoxImagesFolderPath.Size = new System.Drawing.Size(610, 23);
			this.TextBoxImagesFolderPath.TabIndex = 5;
			// 
			// LabelImagesFolderPath
			// 
			this.LabelImagesFolderPath.AutoSize = true;
			this.LabelImagesFolderPath.Location = new System.Drawing.Point(12, 44);
			this.LabelImagesFolderPath.Name = "LabelImagesFolderPath";
			this.LabelImagesFolderPath.Size = new System.Drawing.Size(79, 15);
			this.LabelImagesFolderPath.TabIndex = 4;
			this.LabelImagesFolderPath.Text = "Images folder";
			// 
			// ButtonProofRead
			// 
			this.ButtonProofRead.Enabled = false;
			this.ButtonProofRead.Location = new System.Drawing.Point(16, 89);
			this.ButtonProofRead.Name = "ButtonProofRead";
			this.ButtonProofRead.Size = new System.Drawing.Size(75, 23);
			this.ButtonProofRead.TabIndex = 7;
			this.ButtonProofRead.Text = "Proof read";
			this.ButtonProofRead.UseVisualStyleBackColor = true;
			this.ButtonProofRead.Click += new System.EventHandler(this.ButtonProofRead_Click);
			// 
			// FormMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 120);
			this.Controls.Add(this.ButtonProofRead);
			this.Controls.Add(this.ButtonSelectImagesFolder);
			this.Controls.Add(this.TextBoxImagesFolderPath);
			this.Controls.Add(this.LabelImagesFolderPath);
			this.Controls.Add(this.ButtonSelectOcrTextFile);
			this.Controls.Add(this.TextBoxOcrTextFilePath);
			this.Controls.Add(this.LabelOcrTextFilePath);
			this.Name = "FormMain";
			this.Text = "Images to text";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.OpenFileDialog OpenFileDialogImage;
		private System.Windows.Forms.Label LabelOcrTextFilePath;
		private System.Windows.Forms.TextBox TextBoxOcrTextFilePath;
		private System.Windows.Forms.Button ButtonSelectOcrTextFile;
		private System.Windows.Forms.Button ButtonSelectImagesFolder;
		private System.Windows.Forms.TextBox TextBoxImagesFolderPath;
		private System.Windows.Forms.Label LabelImagesFolderPath;
		private System.Windows.Forms.Button ButtonProofRead;
		private System.Windows.Forms.OpenFileDialog OpenFileDialogText;
		private System.Windows.Forms.FolderBrowserDialog FolderBrowserDialogImages;
	}
}

