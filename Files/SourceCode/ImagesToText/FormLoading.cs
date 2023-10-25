using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ImagesToText
{
	public partial class FormLoading : Form
	{
		public FormLoading()
		{
			InitializeComponent();
		}

		public FormLoading(Form parent, string filename) : this()
		{
			LabelLoadingImage.Text = "Loading image...\r\n" + Path.GetFileNameWithoutExtension(filename);
			int left = parent.Size.Width / 2 - Size.Width / 2;
			int top = parent.Size.Height / 2 - Size.Height / 2;
			Location = new Point(left, top);
		}
	}
}
