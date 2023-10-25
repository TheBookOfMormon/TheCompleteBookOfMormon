using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImagesToText
{
	public partial class FormMain : Form
	{
		public FormMain()
		{
			InitializeComponent();
		}

		private void EnableProofReadButton()
		{
			ButtonProofRead.Enabled =
				!string.IsNullOrEmpty(TextBoxOcrTextFilePath.Text)
				&& !string.IsNullOrEmpty(TextBoxImagesFolderPath.Text);
		}

		private void ButtonSelectOcrTextFile_Click(object sender, EventArgs e)
		{
			DialogResult openFileResult = OpenFileDialogText.ShowDialog();
			if (openFileResult != DialogResult.OK)
				return;

			TextBoxOcrTextFilePath.Text = OpenFileDialogText.FileName;
			EnableProofReadButton();
		}

		private void ButtonProofRead_Click(object sender, EventArgs e)
		{
			using var form = new FormProofRead(TextBoxOcrTextFilePath.Text, TextBoxImagesFolderPath.Text);
			form.ShowDialog();
		}

		private void ButtonSelectImagesFolder_Click(object sender, EventArgs e)
		{
			DialogResult selectResult = FolderBrowserDialogImages.ShowDialog();
			if (selectResult != DialogResult.OK)
				return;

			TextBoxImagesFolderPath.Text = FolderBrowserDialogImages.SelectedPath;
			EnableProofReadButton();
		}
	}
}
