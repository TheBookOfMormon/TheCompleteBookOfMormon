using System;
using System.Drawing;

namespace ImagesToText.Extensions
{
	public static class RectangleExtensions
	{
		public static Rectangle Combine(this Rectangle first, Rectangle second)
		{
			var firstPoint = new Point(Math.Min(first.X, second.X), Math.Min(first.Y, second.Y));
			var secondPoint = new Point(Math.Max(first.Right, second.Right), Math.Max(first.Bottom, second.Bottom));
			var size = new Size(secondPoint.X - firstPoint.X, secondPoint.Y - firstPoint.Y);
			return new Rectangle(firstPoint, size);
		}
		public static Rectangle Exclude(this Rectangle first, Rectangle? second)
		{
			if (second == null)
				return first;
			Point topLeft = first.Location;
			Size size = first.Size;
			int xOffset = Math.Max(0, second.Value.X - first.X);
			return new Rectangle(topLeft.X + xOffset, topLeft.Y, size.Width - xOffset, size.Height);
		}
	}
}
