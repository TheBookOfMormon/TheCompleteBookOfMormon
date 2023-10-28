
namespace ImagesToText
{
	partial class FormProofRead
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
			components = new System.ComponentModel.Container();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProofRead));
			PanelLeft = new System.Windows.Forms.Panel();
			DataGridViewOcrText = new System.Windows.Forms.DataGridView();
			LineNumberColum = new System.Windows.Forms.DataGridViewTextBoxColumn();
			DataGridViewTextBoxColumnWord = new System.Windows.Forms.DataGridViewTextBoxColumn();
			PanelControls = new System.Windows.Forms.Panel();
			ButtonUp = new System.Windows.Forms.Button();
			ImageListIcons = new System.Windows.Forms.ImageList(components);
			ButtonDown = new System.Windows.Forms.Button();
			ComboBoxSkipToOcrPage = new System.Windows.Forms.ComboBox();
			LabelSkipToOcrFile = new System.Windows.Forms.Label();
			ButtonSave = new System.Windows.Forms.Button();
			ButtonDeleteWord = new System.Windows.Forms.Button();
			ButtonCloneWord = new System.Windows.Forms.Button();
			ComboBoxSkipToImage = new System.Windows.Forms.ComboBox();
			LabelSkipToImage = new System.Windows.Forms.Label();
			ButtonFind = new System.Windows.Forms.Button();
			PanelCenter = new System.Windows.Forms.Panel();
			PanZoomPictureBoxPage = new Controls.PanZoomPictureBox();
			LabelCurrentImageName = new System.Windows.Forms.Label();
			PanelLeft.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)DataGridViewOcrText).BeginInit();
			PanelControls.SuspendLayout();
			PanelCenter.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)PanZoomPictureBoxPage).BeginInit();
			SuspendLayout();
			// 
			// PanelLeft
			// 
			PanelLeft.Controls.Add(DataGridViewOcrText);
			PanelLeft.Controls.Add(PanelControls);
			PanelLeft.Dock = System.Windows.Forms.DockStyle.Left;
			PanelLeft.Location = new System.Drawing.Point(0, 0);
			PanelLeft.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
			PanelLeft.Name = "PanelLeft";
			PanelLeft.Size = new System.Drawing.Size(954, 1230);
			PanelLeft.TabIndex = 0;
			// 
			// DataGridViewOcrText
			// 
			DataGridViewOcrText.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			DataGridViewOcrText.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { LineNumberColum, DataGridViewTextBoxColumnWord });
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Consolas", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			DataGridViewOcrText.DefaultCellStyle = dataGridViewCellStyle1;
			DataGridViewOcrText.Dock = System.Windows.Forms.DockStyle.Fill;
			DataGridViewOcrText.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnF2;
			DataGridViewOcrText.Location = new System.Drawing.Point(0, 0);
			DataGridViewOcrText.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
			DataGridViewOcrText.Name = "DataGridViewOcrText";
			DataGridViewOcrText.RowHeadersWidth = 102;
			DataGridViewOcrText.RowTemplate.Height = 25;
			DataGridViewOcrText.Size = new System.Drawing.Size(954, 882);
			DataGridViewOcrText.TabIndex = 1;
			DataGridViewOcrText.CellEndEdit += DataGridViewOcrText_CellEndEdit;
			DataGridViewOcrText.CellEnter += DataGridViewOcrText_CellEnter;
			DataGridViewOcrText.CellPainting += DataGridViewOcrText_CellPainting;
			DataGridViewOcrText.RowHeightInfoNeeded += DataGridViewOcrText_RowHeightInfoNeeded;
			// 
			// LineNumberColum
			// 
			LineNumberColum.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			LineNumberColum.HeaderText = "Line";
			LineNumberColum.MinimumWidth = 12;
			LineNumberColum.Name = "LineNumberColum";
			LineNumberColum.ReadOnly = true;
			LineNumberColum.Width = 250;
			// 
			// DataGridViewTextBoxColumnWord
			// 
			DataGridViewTextBoxColumnWord.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			DataGridViewTextBoxColumnWord.DataPropertyName = "Content";
			DataGridViewTextBoxColumnWord.HeaderText = "Word";
			DataGridViewTextBoxColumnWord.MinimumWidth = 12;
			DataGridViewTextBoxColumnWord.Name = "DataGridViewTextBoxColumnWord";
			// 
			// PanelControls
			// 
			PanelControls.Controls.Add(ButtonUp);
			PanelControls.Controls.Add(ButtonDown);
			PanelControls.Controls.Add(ComboBoxSkipToOcrPage);
			PanelControls.Controls.Add(LabelSkipToOcrFile);
			PanelControls.Controls.Add(ButtonSave);
			PanelControls.Controls.Add(ButtonDeleteWord);
			PanelControls.Controls.Add(ButtonCloneWord);
			PanelControls.Controls.Add(ComboBoxSkipToImage);
			PanelControls.Controls.Add(LabelSkipToImage);
			PanelControls.Controls.Add(ButtonFind);
			PanelControls.Dock = System.Windows.Forms.DockStyle.Bottom;
			PanelControls.Location = new System.Drawing.Point(0, 882);
			PanelControls.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
			PanelControls.Name = "PanelControls";
			PanelControls.Size = new System.Drawing.Size(954, 348);
			PanelControls.TabIndex = 0;
			// 
			// ButtonUp
			// 
			ButtonUp.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			ButtonUp.ImageIndex = 0;
			ButtonUp.ImageList = ImageListIcons;
			ButtonUp.Location = new System.Drawing.Point(608, 16);
			ButtonUp.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
			ButtonUp.Name = "ButtonUp";
			ButtonUp.Size = new System.Drawing.Size(62, 62);
			ButtonUp.TabIndex = 9;
			ButtonUp.UseVisualStyleBackColor = true;
			ButtonUp.Click += ButtonUp_Click;
			// 
			// ImageListIcons
			// 
			ImageListIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			ImageListIcons.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("ImageListIcons.ImageStream");
			ImageListIcons.TransparentColor = System.Drawing.Color.Transparent;
			ImageListIcons.Images.SetKeyName(0, "arrow-up.png");
			ImageListIcons.Images.SetKeyName(1, "arrow-down.png");
			// 
			// ButtonDown
			// 
			ButtonDown.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			ButtonDown.ImageIndex = 1;
			ButtonDown.ImageList = ImageListIcons;
			ButtonDown.Location = new System.Drawing.Point(682, 16);
			ButtonDown.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
			ButtonDown.Name = "ButtonDown";
			ButtonDown.Size = new System.Drawing.Size(62, 62);
			ButtonDown.TabIndex = 8;
			ButtonDown.UseVisualStyleBackColor = true;
			ButtonDown.Click += ButtonDown_Click;
			// 
			// ComboBoxSkipToOcrPage
			// 
			ComboBoxSkipToOcrPage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			ComboBoxSkipToOcrPage.FormattingEnabled = true;
			ComboBoxSkipToOcrPage.Location = new System.Drawing.Point(264, 173);
			ComboBoxSkipToOcrPage.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
			ComboBoxSkipToOcrPage.Name = "ComboBoxSkipToOcrPage";
			ComboBoxSkipToOcrPage.Size = new System.Drawing.Size(669, 49);
			ComboBoxSkipToOcrPage.TabIndex = 7;
			ComboBoxSkipToOcrPage.TabStop = false;
			ComboBoxSkipToOcrPage.SelectedIndexChanged += ComboBoxSkipToOcrPage_SelectedIndexChanged;
			// 
			// LabelSkipToOcrFile
			// 
			LabelSkipToOcrFile.AutoSize = true;
			LabelSkipToOcrFile.Location = new System.Drawing.Point(28, 180);
			LabelSkipToOcrFile.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
			LabelSkipToOcrFile.Name = "LabelSkipToOcrFile";
			LabelSkipToOcrFile.Size = new System.Drawing.Size(225, 41);
			LabelSkipToOcrFile.TabIndex = 6;
			LabelSkipToOcrFile.Text = "Skip to OCR file";
			// 
			// ButtonSave
			// 
			ButtonSave.Location = new System.Drawing.Point(757, 251);
			ButtonSave.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
			ButtonSave.Name = "ButtonSave";
			ButtonSave.Size = new System.Drawing.Size(181, 62);
			ButtonSave.TabIndex = 5;
			ButtonSave.Text = "&Save";
			ButtonSave.UseVisualStyleBackColor = true;
			ButtonSave.Click += ButtonSave_Click;
			// 
			// ButtonDeleteWord
			// 
			ButtonDeleteWord.Location = new System.Drawing.Point(155, 16);
			ButtonDeleteWord.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
			ButtonDeleteWord.Name = "ButtonDeleteWord";
			ButtonDeleteWord.Size = new System.Drawing.Size(111, 62);
			ButtonDeleteWord.TabIndex = 4;
			ButtonDeleteWord.Text = "&Del";
			ButtonDeleteWord.UseVisualStyleBackColor = true;
			ButtonDeleteWord.Click += ButtonDeleteWord_Click;
			// 
			// ButtonCloneWord
			// 
			ButtonCloneWord.Location = new System.Drawing.Point(28, 16);
			ButtonCloneWord.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
			ButtonCloneWord.Name = "ButtonCloneWord";
			ButtonCloneWord.Size = new System.Drawing.Size(111, 62);
			ButtonCloneWord.TabIndex = 3;
			ButtonCloneWord.Text = "&Add";
			ButtonCloneWord.UseVisualStyleBackColor = true;
			ButtonCloneWord.Click += ButtonCloneWord_Click;
			// 
			// ComboBoxSkipToImage
			// 
			ComboBoxSkipToImage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			ComboBoxSkipToImage.FormattingEnabled = true;
			ComboBoxSkipToImage.Location = new System.Drawing.Point(264, 91);
			ComboBoxSkipToImage.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
			ComboBoxSkipToImage.Name = "ComboBoxSkipToImage";
			ComboBoxSkipToImage.Size = new System.Drawing.Size(669, 49);
			ComboBoxSkipToImage.TabIndex = 2;
			ComboBoxSkipToImage.TabStop = false;
			ComboBoxSkipToImage.SelectedIndexChanged += ComboBoxSkipToImage_SelectedIndexChanged;
			// 
			// LabelSkipToImage
			// 
			LabelSkipToImage.AutoSize = true;
			LabelSkipToImage.Location = new System.Drawing.Point(28, 98);
			LabelSkipToImage.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
			LabelSkipToImage.Name = "LabelSkipToImage";
			LabelSkipToImage.Size = new System.Drawing.Size(200, 41);
			LabelSkipToImage.TabIndex = 1;
			LabelSkipToImage.Text = "Skip to image";
			// 
			// ButtonFind
			// 
			ButtonFind.Location = new System.Drawing.Point(757, 16);
			ButtonFind.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
			ButtonFind.Name = "ButtonFind";
			ButtonFind.Size = new System.Drawing.Size(181, 62);
			ButtonFind.TabIndex = 0;
			ButtonFind.Text = "&Find";
			ButtonFind.UseVisualStyleBackColor = true;
			ButtonFind.Click += ButtonFind_Click;
			// 
			// PanelCenter
			// 
			PanelCenter.Controls.Add(PanZoomPictureBoxPage);
			PanelCenter.Controls.Add(LabelCurrentImageName);
			PanelCenter.Dock = System.Windows.Forms.DockStyle.Fill;
			PanelCenter.Location = new System.Drawing.Point(954, 0);
			PanelCenter.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
			PanelCenter.Name = "PanelCenter";
			PanelCenter.Size = new System.Drawing.Size(990, 1230);
			PanelCenter.TabIndex = 1;
			// 
			// PanZoomPictureBoxPage
			// 
			PanZoomPictureBoxPage.Dock = System.Windows.Forms.DockStyle.Fill;
			PanZoomPictureBoxPage.Location = new System.Drawing.Point(0, 128);
			PanZoomPictureBoxPage.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
			PanZoomPictureBoxPage.Name = "PanZoomPictureBoxPage";
			PanZoomPictureBoxPage.Size = new System.Drawing.Size(990, 1102);
			PanZoomPictureBoxPage.TabIndex = 0;
			PanZoomPictureBoxPage.TabStop = false;
			PanZoomPictureBoxPage.Paint += PanZoomPictureBoxPage_Paint;
			PanZoomPictureBoxPage.MouseUp += PanZoomPictureBoxPage_MouseUp;
			// 
			// LabelCurrentImageName
			// 
			LabelCurrentImageName.BackColor = System.Drawing.SystemColors.ActiveCaption;
			LabelCurrentImageName.Dock = System.Windows.Forms.DockStyle.Top;
			LabelCurrentImageName.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			LabelCurrentImageName.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			LabelCurrentImageName.Location = new System.Drawing.Point(0, 0);
			LabelCurrentImageName.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
			LabelCurrentImageName.Name = "LabelCurrentImageName";
			LabelCurrentImageName.Size = new System.Drawing.Size(990, 128);
			LabelCurrentImageName.TabIndex = 1;
			// 
			// FormProofRead
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(240F, 240F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			ClientSize = new System.Drawing.Size(1944, 1230);
			Controls.Add(PanelCenter);
			Controls.Add(PanelLeft);
			KeyPreview = true;
			Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
			Name = "FormProofRead";
			Text = "Proof read";
			WindowState = System.Windows.Forms.FormWindowState.Maximized;
			Load += FormProofRead_Load;
			KeyDown += FormProofRead_KeyDown;
			PanelLeft.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)DataGridViewOcrText).EndInit();
			PanelControls.ResumeLayout(false);
			PanelControls.PerformLayout();
			PanelCenter.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)PanZoomPictureBoxPage).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Panel PanelLeft;
		private System.Windows.Forms.DataGridView DataGridViewOcrText;
		private System.Windows.Forms.Panel PanelControls;
		private System.Windows.Forms.Panel PanelCenter;
		private Controls.PanZoomPictureBox PanZoomPictureBoxPage;
		private System.Windows.Forms.Button ButtonFind;
		private System.Windows.Forms.Label LabelCurrentImageName;
		private System.Windows.Forms.ComboBox ComboBoxSkipToImage;
		private System.Windows.Forms.Label LabelSkipToImage;
		private System.Windows.Forms.Button ButtonCloneWord;
		private System.Windows.Forms.DataGridViewTextBoxColumn LineNumberColum;
		private System.Windows.Forms.DataGridViewTextBoxColumn DataGridViewTextBoxColumnWord;
		private System.Windows.Forms.Button ButtonDeleteWord;
		private System.Windows.Forms.Button ButtonSave;
		private System.Windows.Forms.ComboBox ComboBoxSkipToOcrPage;
		private System.Windows.Forms.Label LabelSkipToOcrFile;
        private System.Windows.Forms.Button ButtonDown;
        private System.Windows.Forms.ImageList ImageListIcons;
        private System.Windows.Forms.Button ButtonUp;
    }
}