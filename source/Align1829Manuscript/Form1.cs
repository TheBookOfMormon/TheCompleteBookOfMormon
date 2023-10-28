using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Align1829Manuscript
{
    public partial class Form1 : Form
    {
        List<string> Bom1829;
        List<string> Bom1830Alpha;
        List<int> Words;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ReloadText();
        }

        private void ReloadText()
        {
            Bom1829 = File
                .ReadAllLines(@"C:\temp\1829.bom")
                .Where(x => !x.Contains('['))
                .ToList();
            Bom1830Alpha = File
                .ReadAllLines(@"C:\temp\1830.bom")
                .Where(x => !x.Contains('['))
                .ToList();
            UpdateGrid();
        }

        private void UpdateGrid()
        {
            int index = dataGridView1?.CurrentCell?.RowIndex ?? -1;
            int scroll = dataGridView1.FirstDisplayedScrollingRowIndex;
            Words = Enumerable
                .Range(0, Math.Max(Bom1829.Count, Bom1830Alpha.Count) - 1)
                .ToList();
            dataGridView1.DataSource = Words;
            if (index > -1)
            {
                dataGridView1.CurrentCell = dataGridView1.Rows[index].Cells[0];
                dataGridView1.FirstDisplayedScrollingRowIndex = scroll;
            }
            dataGridView1.Refresh();
            Affect1830Checkbox.Checked = false;
        }

        private void AlignFrom(int firstIndex)
        {
            var origCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            Bom1829 =
                Bom1829.Take(firstIndex)
                .Concat(Bom1829.Skip(firstIndex).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray())
                .ToList();

            int lastRowIndex = Math.Min(Bom1829.Count, Bom1830Alpha.Count) - 1;
            for (int index = firstIndex; index <= lastRowIndex; index++)
            {
                if (!IsPunctuation(Bom1829, index) && IsPunctuation(Bom1830Alpha, index))
                    Bom1829.Insert(index, "");
            }

            Cursor.Current = origCursor;
        }

        private readonly static Regex PunctuationRegex = new Regex(@"[\,\.\:\;\(\)\?\!—]", RegexOptions.Compiled);
        private static bool IsPunctuation(List<string> source, int index)
        {
            if (index >= source.Count)
                return false;
            return PunctuationRegex.IsMatch(source[index]);
        }

        private List<string> GetWordsSource() =>
            !Affect1830Checkbox.Checked ? Bom1829 : Bom1830Alpha;

        private static string GetWord(List<string> source, int index) =>
            index < source.Count ? source[index] : "###";

        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
                return;

            Rectangle newRect = new Rectangle(e.CellBounds.X + 1, e.CellBounds.Y + 1,
                e.CellBounds.Width - 4, e.CellBounds.Height - 4);

            int dataRow = e.RowIndex;

            using (
                    Brush gridBrush = new SolidBrush(dataGridView1.GridColor),
                    backColorBrush = new SolidBrush(e.CellStyle.BackColor),
                    mismatchColorBrush = new SolidBrush(Color.FromArgb(255, 192, 192)),
                    textBrush = new SolidBrush(dataGridView1.ForeColor))
            {
                using (Pen gridLinePen = new Pen(gridBrush))
                {
                    string bom1829Word = GetWord(Bom1829, dataRow);
                    string bom1830Word = GetWord(Bom1830Alpha, dataRow);

                    Brush currentBrush = string.Compare(bom1829Word, bom1830Word, true) == 0 || IsPunctuation(Bom1830Alpha, dataRow)
                        ? backColorBrush
                        : mismatchColorBrush;

                    // Erase the cell.
                    e.Graphics.FillRectangle(currentBrush, e.CellBounds);

                    // Draw the grid lines (only the right and bottom lines;
                    // DataGridView takes care of the others).
                    e.Graphics.DrawLine(gridLinePen, e.CellBounds.Left,
                            e.CellBounds.Bottom - 1, e.CellBounds.Right - 1,
                            e.CellBounds.Bottom - 1);
                    e.Graphics.DrawLine(gridLinePen, e.CellBounds.Right - 1,
                            e.CellBounds.Top, e.CellBounds.Right - 1,
                            e.CellBounds.Bottom);

                    // Draw the text content of the cell, ignoring alignment.
                    string word = e.ColumnIndex switch
                    {
                        0 => bom1829Word,
                        1 => bom1830Word,
                        2 => dataRow.ToString(),
                        _ => throw new IndexOutOfRangeException()
                    };
                    e.Graphics.DrawString(word, e.CellStyle.Font,
                        textBrush, e.CellBounds.X + 2,
                        e.CellBounds.Y + 2, StringFormat.GenericDefault);
                    e.Handled = true;
                }
            }

        }

        private void ButtonAlign_Click(object sender, EventArgs e)
        {
            AlignFrom(dataGridView1.CurrentRow.Index);
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            File.WriteAllLines(@"C:\Temp\1829.bom", Bom1829);
            File.WriteAllLines(@"C:\Temp\1830.bom", Bom1830Alpha);
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            GetWordsSource().Insert(dataGridView1.CurrentRow.Index, "");
            UpdateGrid();
        }

        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Delete " + GetWordsSource()[dataGridView1.CurrentRow.Index], "Delete?", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                GetWordsSource().RemoveAt(dataGridView1.CurrentRow.Index);
                UpdateGrid();
            }
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            string keyPressed = e.KeyChar.ToString().ToUpperInvariant();
            if (keyPressed == "\a" && Control.ModifierKeys == Keys.Control)
            {
                e.Handled = true;
                string lineStr = InputForm.ShowDialog("Line number", "Go to line");
                if (int.TryParse(lineStr, out int lineNumber) && lineNumber < Words.Count)
                    dataGridView1.FirstDisplayedScrollingRowIndex = lineNumber;
            }
        }

        private void ButtonReload_Click(object sender, EventArgs e)
        {
            ReloadText();
        }

    }
}
