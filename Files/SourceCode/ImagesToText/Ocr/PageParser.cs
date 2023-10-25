using System;
using System.Drawing;
using Tesseract;
using ImagesToText.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace ImagesToText.Ocr
{
	public static class PageParser
	{
		private const string WordChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-'’&0123456789";

		public static IEnumerable<Capture> Parse(string filePath)
		{
			using var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
			engine.SetVariable("tessedit_char_whitelist", " abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPARSTUVWYXZ.-—':,;!?()");
			using var pixImage = Pix.LoadFromFile(filePath);
			using var pixPage = engine.Process(pixImage);
			using var iter = pixPage.GetIterator();
			iter.Begin();
			return ParsePage(iter).ToArray();
		}

		private static IEnumerable<Capture> ParsePage(ResultIterator iter)
		{
			do
			{
				foreach (Capture capture in ParseBlock(iter))
					yield return capture;
			} while (iter.Next(PageIteratorLevel.Block));
		}

		private static IEnumerable<Capture> ParseBlock(ResultIterator iter)
		{
			do
			{
				foreach (Capture capture in ParseParagraph(iter))
					yield return capture;
			} while (iter.Next(PageIteratorLevel.Block, PageIteratorLevel.Para));
		}

		private static IEnumerable<Capture> ParseParagraph(ResultIterator iter)
		{
			var lines = new List<List<Capture>>();
			do
			{
				lines.Add(ParseLine(iter).ToList());
			} while (iter.Next(PageIteratorLevel.Para, PageIteratorLevel.TextLine));
			
			for (int i = 0; i <= lines.Count - 2; i++)
			{
				List<Capture> thisLine = lines[i];
				List<Capture> nextLine = lines[i + 1];
				if (thisLine.Count == 0 || nextLine.Count == 0)
					continue;

				Capture lastWord = thisLine.Last();
				string lastWordContent = lastWord.Content;
				if (lastWordContent.EndsWith("-"))
				{
					lastWordContent = lastWordContent[0..^1];
					lastWordContent += nextLine.First().Content;
					lastWord.Content = lastWordContent;
					nextLine.RemoveAt(0);
				}
			}
			return lines.SelectMany(x => x).ToArray();
		}

		private static IEnumerable<Capture> ParseLine(ResultIterator iter)
		{
			do
			{
				string word = iter.GetText(PageIteratorLevel.Word);
				if (string.IsNullOrWhiteSpace(word))
					continue;
				foreach (Capture capture in ParseWord(iter))
				{
					if (capture.Content == "J")
						capture.Content = "I";
					yield return capture;
				}
			} while (iter.Next(PageIteratorLevel.TextLine, PageIteratorLevel.Word));
		}

		private static IEnumerable<Capture> ParseWord(ResultIterator iter)
		{
			Rectangle? rect = null;

			string currentWord = "";
			do
			{
				string symbol = iter.GetText(PageIteratorLevel.Symbol);

				if (!iter.TryGetBoundingBox(PageIteratorLevel.Symbol, out Rect symbolBounds))
					throw new Exception($"Got symbol {symbol} but not bounding box");

				if (WordChars.Contains(symbol))
				{
					rect = UpdateWordRect(rect, symbolBounds);
					currentWord += symbol;
				}
				else
				{
					if (currentWord != "")
					{
						yield return new Capture(currentWord, rect.Value);
					}
					currentWord = "";
					rect = null;
					if (!string.IsNullOrWhiteSpace(symbol))
						yield return new Capture(symbol, ToRectangle(symbolBounds).Exclude(rect));
				}
			} while (iter.Next(PageIteratorLevel.Word, PageIteratorLevel.Symbol));
			if (!string.IsNullOrEmpty(currentWord))
				yield return new Capture(currentWord, rect.Value);
		}

		static Rectangle UpdateWordRect(Rectangle? rect, Rect symbolRect)
		{
			var tempRect = ToRectangle(symbolRect);
			if (rect == null)
				rect = tempRect;
			else
				rect = rect.Value.Combine(tempRect);
			return rect.Value;
		}

		static Rectangle ToRectangle(Rect rect) =>
			new Rectangle(rect.X1, rect.Y1, rect.Width, rect.Height);
	}
}
