using System;
using System.Drawing;

namespace ImagesToText.Ocr
{
	public class Capture
	{
		public string Content { get; set; }
		public Rectangle Rectangle { get; set; }

		public Capture(string content, Rectangle rectangle)
		{
			if (string.IsNullOrWhiteSpace(content))
				throw new ArgumentException($"'{nameof(content)}' cannot be null or whitespace.", nameof(content));

			Content = content;
			Rectangle = rectangle;
		}

		public Capture Clone() =>
			new Capture(Content, new Rectangle(Rectangle.Location, Rectangle.Size));

	}
}
