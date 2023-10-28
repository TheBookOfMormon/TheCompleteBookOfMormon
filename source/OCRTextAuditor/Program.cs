using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OCRTextAuditor
{
	class Program
	{
		//Something random
		private const string DotPlaceholder = "EBLGPTBMGKABUECVSZNVZGJFTBJXBWAH";
		private const string DashPlaceholder = "YTBKRXFTKSCHXYSZLSBXEWLWSRFAJARN";
		private readonly static Regex PunctuationRegex = new Regex(@"([:\.;,?!—\-\(\)]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

		static void Main(string[] args)
		{
			do
			{
				Console.WriteLine("Press ENTER to process");
				Console.ReadLine();
				Console.Clear();
				ProcessData();
			} while (true);
		}

		private static void ProcessData()
		{
			string[] folderPaths = Directory
				.GetDirectories(@"C:\Data\Mine\Web\TheCompleteBookOfMormon\Files\2-AuditText")
				.OrderByDescending(x => x)
				.ToArray();
			foreach (string folderPath in folderPaths)
			{
				string folderName = folderPath.Split("\\").Last();
				string combinedTextFilePath = Path.Combine(folderPath, "..", folderName + ".bom");
				if (File.Exists(combinedTextFilePath))
				{
					Console.WriteLine("Skipping " + folderName);
					continue;
				}

				var textBuilder = new StringBuilder();
				Console.WriteLine(folderName);
				string[] filePaths = Directory
					.GetFiles(folderPath, "*.txt", SearchOption.TopDirectoryOnly)
					.OrderBy(x => x).ToArray();

				if (filePaths.Any())
				{
					foreach (string filePath in filePaths)
					{
						string fileNumber = Path.GetFileNameWithoutExtension(filePath);
						Console.Write($"{fileNumber} ");

						string content = File.ReadAllText(filePath);
						content = RemoveSpecialCharacters(content);

						content = PreserveDottedWords(content);
						content = PreserveDashedWords(content);
						content = content
							.Replace("\r\n", "\n")
							.Replace("-\n", "")
							.Replace("- \n", "")
							.Replace("\n", " ")
							.Trim();

						while (true)
						{
							string oldContent = content;
							content = oldContent.Replace("  ", " ");
							if (content.Length == oldContent.Length)
								break;
						}

						content = PunctuationRegex.Replace(content, x => $" {x.Groups[1].Value} ");

						content = content
							.Replace(" ", "\n");

						string[] indexedWords = content
							.Split("\n")
							.Where(x => !string.IsNullOrWhiteSpace(x))
							.Select(RestoreSpecialCharacters)
							//.Select((string value, int index) => $"[File:{fileNumber}][Word:{index + 1}]\t{value}")
							.ToArray();

						content = string.Join('\n', indexedWords);
						content = content.Replace("\n", "\r\n");

						textBuilder.AppendLine($"[File:{fileNumber}]");
						textBuilder.AppendLine(content);
					}

					File.WriteAllText(combinedTextFilePath, textBuilder.ToString());
				}
				Console.WriteLine();
				Console.WriteLine();
			}
		}

		private static string RemoveSpecialCharacters(string content)
		=> content
			.Replace("é", "e")
			.Replace("«", "")
			.Replace("»", "")
			.Replace("*", "")
			.Replace("‘", "")
			.Replace("“", "")
			.Replace("°", "")
			.Replace("", "")
			.Replace("¢", "")
			.Replace("|", "")
			.Replace("\"", "")
			.Replace(" ’", " ")
			.Replace("–", "-")
			.Replace("’", "'");

		private static string PreserveDottedWords(string content)
		=> content
			.Replace("B.C.", $"B{DotPlaceholder}C{DotPlaceholder}", StringComparison.OrdinalIgnoreCase)
			.Replace("A.D.", $"B{DotPlaceholder}C{DotPlaceholder}", StringComparison.OrdinalIgnoreCase);

		private static string PreserveDashedWords(string content)
		=> content
			.Replace("to-day", $"to{DashPlaceholder}day")
			.Replace("judgment-seat", $"judgment{DashPlaceholder}seat");

		private static string RestoreSpecialCharacters(string content) =>
			content
			.Replace(DotPlaceholder, ".")
			.Replace(DashPlaceholder, "-");

	}
}
