using System;
using System.IO;
using System.Text.RegularExpressions;

namespace RenumberPages
{
    class Program
    {
        const string FilePath = @"C:\Data\Mine\Web\TheCompleteBookOfMormon\Files\3-DataFiles\BookOfMormon1837Kirtland (Audited).bom";
        static int PageNumber = 4;

        static void Main(string[] args)
        {
            var regex = new Regex(@"^\[File.*?\]$");
            string[] lines = File.ReadAllLines(FilePath);
            for(int i = 0; i < lines.Length; i++)
            {
                if (regex.IsMatch(lines[i]))
                {
                    string newLineNumber = $"[File:{PageNumber++:000}]";
                    lines[i] = newLineNumber;
                }
            }
            File.WriteAllLines(FilePath, lines);
        }
    }
}
