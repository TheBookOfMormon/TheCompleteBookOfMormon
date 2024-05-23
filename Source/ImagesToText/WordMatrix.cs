using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ImagesToText
{
	public partial class WordMatrix : UserControl
	{
		private Point MouseDownLocation { get; set; }
		private Point Offset { get; set; }

		public WordMatrix()
		{
			InitializeComponent();
		}

		public void Update(List<RowData> rowData, int rowIndex, string ocrWord)
		{
			if (rowIndex > 0)
				LabelPreviousWord.Text = rowData[rowIndex - 1].Content;
			else
				LabelPreviousWord.Text = "";

			LabelExpectedWord.Text = rowData[rowIndex].Content;
			LabelOcrWord.Text = ocrWord;

			if (rowIndex + 1 < rowData.Count)
				LabelNextWord.Text = rowData[rowIndex + 1].Content;
			else
				LabelNextWord.Text = "";
		}

		public void Reposition(Rectangle clientRect)
		{
			float yFactor;
			float yPos;
			if (clientRect.Bottom < (Parent.Size.Height / 2))
			{
				yFactor = 1;
				yPos = clientRect.Bottom;
			}
			else
			{
				yFactor = -1.5f;
				yPos = clientRect.Top;
			}

			int newWidth = Math.Max(LabelPreviousWord.Size.Width, LabelNextWord.Size.Width);
			newWidth = Math.Max(newWidth, LabelOcrWord.Location.X + LabelOcrWord.Size.Width);
			Size = new Size(newWidth, Size.Height);
			Point location = new Point(clientRect.Left, clientRect.Top + (clientRect.Height / 2));
			location = new Point(location.X, (int)(yPos + yFactor * (Size.Height / 2)));
			location = new Point(Math.Max(0, location.X), Math.Max(0, location.Y));
			if (location.X + Size.Width > Parent.Width)
				location.X = Parent.Width - Size.Width;
			if (location.Y + Size.Height > Parent.Height)
				location.Y = Parent.Height - Size.Height;

			location.Offset(Offset);
			Location = location;
        }

        private void WordMatrix_MouseDown(object sender, MouseEventArgs e)
        {
			if (e.Button == MouseButtons.Left)
				MouseDownLocation = e.Location;
        }

        private void WordMatrix_MouseMove(object sender, MouseEventArgs e)
        {
			if (e.Button == MouseButtons.Left)
			{
				Left = e.X + Left - MouseDownLocation.X;
				//Top = e.Y + Top - MouseDownLocation.Y;
				Offset = new Point(e.X + Offset.X - MouseDownLocation.X, 0);
			}
		}
    }
}
