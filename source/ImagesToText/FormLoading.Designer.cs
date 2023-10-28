
namespace ImagesToText
{
	partial class FormLoading
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
			this.LabelLoadingImage = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// LabelLoadingImage
			// 
			this.LabelLoadingImage.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.LabelLoadingImage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelLoadingImage.Font = new System.Drawing.Font("Segoe UI", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.LabelLoadingImage.Location = new System.Drawing.Point(0, 0);
			this.LabelLoadingImage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.LabelLoadingImage.Name = "LabelLoadingImage";
			this.LabelLoadingImage.Size = new System.Drawing.Size(786, 222);
			this.LabelLoadingImage.TabIndex = 0;
			this.LabelLoadingImage.Text = "Loading image...";
			this.LabelLoadingImage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// FormLoading
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 23F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(786, 222);
			this.Controls.Add(this.LabelLoadingImage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "FormLoading";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "FormLoading";
			this.TopMost = true;
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label LabelLoadingImage;
	}
}