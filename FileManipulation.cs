using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace ImageSorter
{
    public static class FileManipulation
    {
        public static void OrderFiles(List<FileProcessResult> allResults, FileReadme readme)
        {
            // must contain more than 1 element
            if (allResults.Count < 2)
                return;
            
            // Basic sort by BestDate
            allResults.Sort((a, b) => a.BestDate.CompareTo(b.BestDate));

            // Create a new filename
            int i = 1; // 001, 002, 003, etc.
            var lastResult = allResults.First(); 
            foreach (var result in allResults)
            {
                // find difference in BestDate from lastResult
                var diff = result.BestDate - lastResult.BestDate;
                
                // print diff in hours
                //Console.WriteLine($"{result.FileName} {diff.TotalHours}");

                // if diff is greater than 2 hours, increment i to the next 10s multiple
                if (diff.TotalHours > 2)
                {
                    i = (i / 10) * 10 + 10;
                }
                
                // uses three digits in front of the filename
                result.FileNameMod = $"{i:D3}_{result.FileName}";
                i++;
                lastResult = result;
            }
        }

        public static void FindAuthor(List<FileProcessResult> allResults, FileReadme readme)
        {
            foreach (var result in allResults)
            {
                var prefix = readme.FindSecondCsvValue(result.FileName);
                result.Author = prefix;
            }
        }
    }
}
