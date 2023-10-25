using ImagesToText.Ocr;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ImagesToText
{
	public partial class FormProofRead : Form
	{
		private int SkipToImageIndex;
		private string OcrTextFilePath;
		private string[] ImageFilePaths;
		private string[] ImageFileNames;
		private Dictionary<int, int> OcrPageIndexToLineNumber;
		private List<RowData> OcrText;
		private BindingList<RowData> OcrTextBindingList;
		private IEnumerator<Capture> Captures;
		private Capture CurrentCapture;
		private Image CurrentPageImage;
		private Graphics CurrentPageCanvas;
		private WordMatrix WordMatrix;
		private DateTime NextPermittedFindTime;

		private bool _modified;
		private bool Modified
		{
			get => _modified;
			set
			{
				_modified = value;
				string indicator = _modified ? "(Unsaved) " : "";
				Text = indicator + "Proof read";
			}
		}

		public FormProofRead(string ocrTextFilePath, string imagesFolderPath)
		{
			if (!File.Exists(ocrTextFilePath))
				throw new ArgumentException("OCR text file does not exist", nameof(ocrTextFilePath));
			if (!Directory.Exists(imagesFolderPath))
				throw new ArgumentException("OCR images folder does not exist", nameof(imagesFolderPath));

			OcrTextFilePath = ocrTextFilePath;
			ImageFilePaths = Directory.GetFiles(imagesFolderPath, "*.png").Union(Directory.GetFiles(imagesFolderPath, "*.jpg")).ToArray();
			if (ImageFilePaths.Length == 0)
				throw new ArgumentException("OCR images folder contains no PNG or JPG files");
			ImageFileNames = ImageFilePaths.Select(x => Path.GetFileNameWithoutExtension(x)).ToArray();

			OcrText = File.ReadAllLines(ocrTextFilePath)
				.Select(x => new RowData(x))
				.ToList();
			if (OcrText.Count == 0)
				throw new ArgumentException("OCR text file contains no lines");
			OcrTextBindingList = new BindingList<RowData>(OcrText);

			InitializeComponent();

			DataGridViewOcrText.AutoGenerateColumns = false;
			DataGridViewOcrText.DataSource = OcrTextBindingList;
			ComboBoxSkipToImage.Items.AddRange(ImageFileNames);

			OcrPageIndexToLineNumber = new Dictionary<int, int>();
			var pageNumberRegex = new System.Text.RegularExpressions.Regex(@"^\[File\:(\d{3})\]$");
			int pageNumberIndex = 0;
			for (int ocrLineNumber = 0; ocrLineNumber < OcrText.Count; ocrLineNumber++)
			{
				string content = OcrText[ocrLineNumber].Content;
				System.Text.RegularExpressions.Match match = pageNumberRegex.Match(content);
				if (match.Success)
				{
					ComboBoxSkipToOcrPage.Items.Add(match.Groups[1].Value);
					OcrPageIndexToLineNumber[pageNumberIndex++] = ocrLineNumber;
				}
			}
		}

		public IEnumerable<Capture> CreateCapturesIterator()
		{
			foreach (string imageFilePath in ImageFilePaths.Skip(SkipToImageIndex).OrderBy(x => x))
				foreach (Capture capture in GetPageCaptures(imageFilePath))
					yield return CurrentCapture = capture;
		}

		private IEnumerable<Capture> GetPageCaptures(string imageFilePath)
		{
			IEnumerable<Capture> result = null;
			using (var loadingForm = new FormLoading(this, imageFilePath))
			{
				loadingForm.Show();
				loadingForm.Refresh();
				LabelCurrentImageName.Text = Path.GetFileNameWithoutExtension(imageFilePath);
				CurrentPageCanvas?.Dispose();
				CurrentPageImage?.Dispose();
				using var loadedImage = Image.FromFile(imageFilePath);
				CurrentPageImage = (Image)loadedImage.Clone();
				CurrentPageCanvas = Graphics.FromImage(CurrentPageImage);
				result = PageParser.Parse(imageFilePath).ToArray()
					.Append(new Capture($"<<END OF PAGE: {Path.GetFileNameWithoutExtension(imageFilePath)}>>", new Rectangle(0, 100, 1, 1)));
			}
			return result;
		}

		private void Find()
		{
			if (DateTime.UtcNow < NextPermittedFindTime)
				return;

			NextPermittedFindTime = DateTime.UtcNow.AddSeconds(0.5);
			if (Captures == null)
			{
				Captures = CreateCapturesIterator().GetEnumerator();
				DataGridViewOcrText.CurrentCell = DataGridViewOcrText.Rows[0].Cells[1];
			}
			int gridRow = DataGridViewOcrText.CurrentRow.Index;
			bool pauseIterator = false;
			if (Control.ModifierKeys != Keys.Control)
				gridRow++;
			if (Control.ModifierKeys == Keys.Alt)
				pauseIterator = true;

			while (pauseIterator || Captures.MoveNext())
			{
				pauseIterator = false;
				Capture capture = Captures.Current;
				string cellValue = (string)DataGridViewOcrText.Rows[gridRow].Cells[1].Value;
				string captureContent = capture.Content;
				if (captureContent.Length == 1 && "1{}|[]".IndexOf(captureContent) >= 0)
					captureContent = "I";
				if (captureContent != cellValue)
				{
					DataGridViewOcrText.CurrentCell = DataGridViewOcrText.Rows[gridRow].Cells[1];
					HighlightCapture(capture);
					UpdateWordComparison();
					DataGridViewOcrText.Focus();
					if (gridRow == OcrText.Count - 1)
						MessageBox.Show("Finished");
					return;
				}
				int x = CurrentCapture.Rectangle.Width / 2 + CurrentCapture.Rectangle.Left;
				int y = CurrentCapture.Rectangle.Height / 2 + CurrentCapture.Rectangle.Top;
				var ellipseRect = new Rectangle(x, y, 0, 0);
				ellipseRect.Inflate(8, 8);
				using var matchedBrush = new SolidBrush(Color.FromArgb(128, 0, 255, 0));
				CurrentPageCanvas.FillEllipse(matchedBrush, ellipseRect);
				gridRow++;
			}
		}

		private void UpdateWordComparison()
		{
			if (CurrentCapture == null)
				return;

			int gridRow = DataGridViewOcrText.CurrentRow.Index;
			WordMatrix.Update(OcrText, gridRow, CurrentCapture.Content);
			Rectangle clientRect = CurrentCapture.Rectangle;
			clientRect = PanZoomPictureBoxPage.Transformation.ConvertToPb(clientRect);
			WordMatrix.Reposition(clientRect);
		}

		private void HighlightCapture(Capture capture)
		{
			PanZoomPictureBoxPage.Image?.Dispose();
			var previewImage = (Image)CurrentPageImage.Clone();
			using var previewCanvas = Graphics.FromImage(previewImage);
			Rectangle highlightRect = new Rectangle(capture.Rectangle.Location, capture.Rectangle.Size);
			highlightRect.Inflate(32, 32);
			previewCanvas.DrawRectangle(Pens.Red, highlightRect);
			highlightRect.Inflate(1, 1);
			previewCanvas.DrawRectangle(Pens.Green, highlightRect);
			PanZoomPictureBoxPage.Image = previewImage;
		}

		private void ComboBoxSkipToImage_SelectedIndexChanged(object sender, EventArgs e)
		{
			SkipToImageIndex = ComboBoxSkipToImage.SelectedIndex;
			Captures?.Dispose();
			Captures = null;
			DataGridViewOcrText.Focus();
			Find();
		}

		private void ButtonCloneWord_Click(object sender, EventArgs e)
		{
			int wordIndex = DataGridViewOcrText.CurrentCell.RowIndex;
			OcrTextBindingList.Insert(wordIndex, new RowData(OcrText[wordIndex].Content));
			Modified = true;
			DataGridViewOcrText.Focus();
			DataGridViewOcrText.CurrentCell = DataGridViewOcrText.Rows[wordIndex].Cells[1];
		}

		private void DataGridViewOcrText_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
		{
			if (e.ColumnIndex != 0 || e.RowIndex < 0)
				return;

			Rectangle newRect = new Rectangle(e.CellBounds.X + 1, e.CellBounds.Y + 1,
				e.CellBounds.Width - 4, e.CellBounds.Height - 4);

			using (
					Brush gridBrush = new SolidBrush(DataGridViewOcrText.GridColor),
					backColorBrush = new SolidBrush(e.CellStyle.BackColor),
					textBrush = new SolidBrush(DataGridViewOcrText.ForeColor))
			{
				using (Pen gridLinePen = new Pen(gridBrush))
				{
					// Erase the cell.
					e.Graphics.FillRectangle(backColorBrush, e.CellBounds);

					// Draw the grid lines (only the right and bottom lines;
					// DataGridView takes care of the others).
					e.Graphics.DrawLine(gridLinePen, e.CellBounds.Left,
							e.CellBounds.Bottom - 1, e.CellBounds.Right - 1,
							e.CellBounds.Bottom - 1);
					e.Graphics.DrawLine(gridLinePen, e.CellBounds.Right - 1,
							e.CellBounds.Top, e.CellBounds.Right - 1,
							e.CellBounds.Bottom);

					// Draw the text content of the cell, ignoring alignment.
					string lineNumber = e.RowIndex.ToString("D7");
					e.Graphics.DrawString(lineNumber, e.CellStyle.Font,
						textBrush, e.CellBounds.X + 2,
						e.CellBounds.Y + 2, StringFormat.GenericDefault);
					e.Handled = true;
				}
			}
		}

		private void ButtonDeleteWord_Click(object sender, EventArgs e)
		{
			int wordIndex = DataGridViewOcrText.CurrentCell.RowIndex;
			if (wordIndex < 0 || OcrText.Count == 0)
				return;

			DialogResult result = MessageBox.Show("Delete word " + OcrText[wordIndex].Content, "Delete?", MessageBoxButtons.YesNo);
			if (result != DialogResult.Yes)
				return;

			OcrTextBindingList.RemoveAt(wordIndex);
			Modified = true;
			DataGridViewOcrText.Focus();
			Rectangle buttonFindRect = ButtonFind.RectangleToScreen(ButtonFind.ClientRectangle);
			buttonFindRect.Offset(ButtonFind.Width / 2, ButtonFind.Height / 2);
			Cursor.Position = buttonFindRect.Location;
		}

		private void ButtonSave_Click(object sender, EventArgs e)
		{
			File.WriteAllLines(OcrTextFilePath, OcrText.Select(x => x.Content).ToArray());
			Modified = false;
		}

		private void DataGridViewOcrText_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			Modified = true;
		}

		private void ComboBoxSkipToOcrPage_SelectedIndexChanged(object sender, EventArgs e)
		{
			int ocrLineNumber = OcrPageIndexToLineNumber[ComboBoxSkipToOcrPage.SelectedIndex] + 1;
			DataGridViewOcrText.CurrentCell = DataGridViewOcrText.Rows[ocrLineNumber].Cells[1];
			DataGridViewOcrText.Focus();
		}

		private void PanZoomPictureBoxPage_MouseUp(object sender, MouseEventArgs e)
		{
			DataGridViewOcrText.Focus();
		}

		private void ButtonFind_Click(object sender, EventArgs e)
		{
			Find();
		}

		private void FormProofRead_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Down && e.Control)
				e.Handled = true;
			if (e.KeyCode == Keys.Up && e.Control)
				e.Handled = true;
			if (e.KeyCode == Keys.X && e.Control)
			{
				e.Handled = true;
				DataGridViewOcrText.CurrentCell.Value = CurrentCapture.Content;
				WordMatrix.Update(OcrText, DataGridViewOcrText.CurrentCell.RowIndex, CurrentCapture.Content);
				NextPermittedFindTime = DateTime.UtcNow.AddSeconds(1);
			}
		}

		private void PanZoomPictureBoxPage_Paint(object sender, PaintEventArgs e)
		{
			UpdateWordComparison();
		}

		private void DataGridViewOcrText_CellEnter(object sender, DataGridViewCellEventArgs e)
		{
			UpdateWordComparison();
			ButtonUp.Enabled = e.RowIndex > 0;
			ButtonDown.Enabled = e.RowIndex < DataGridViewOcrText.Rows.Count - 1;
		}

		private void FormProofRead_Load(object sender, EventArgs e)
		{
			WordMatrix = new WordMatrix();
			PanelCenter.Controls.Add(WordMatrix);
			PanelCenter.Controls.SetChildIndex(WordMatrix, 0);
		}

		private void ButtonDown_Click(object sender, EventArgs e)
		{
			DataGridViewOcrText.CurrentCell = DataGridViewOcrText.Rows[DataGridViewOcrText.CurrentRow.Index + 1].Cells[1];
		}

		private void ButtonUp_Click(object sender, EventArgs e)
		{
			DataGridViewOcrText.CurrentCell = DataGridViewOcrText.Rows[DataGridViewOcrText.CurrentRow.Index - 1].Cells[1];
		}

		private void DataGridViewOcrText_RowHeightInfoNeeded(object sender, DataGridViewRowHeightInfoNeededEventArgs e)
		{
			e.MinimumHeight = 48;
			e.Height = 48;
		}
	}

	public class RowData
	{
		public string Content { get; set; }

		public RowData(string content)
		{
			Content = content;
		}
	}
}
