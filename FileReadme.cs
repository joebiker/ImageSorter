using System;
using System.IO;
using System.Collections.Generic;
using System.Linq; // Added for .Where() and .Any()

/// <summary>
/// Reads the contents of README.txt in the given directory.
/// The file is expected to be a CSV with two columns:
///     - The first column is a filter that will be used to match the file name.
///     - The second column is the name who took the series of photos.
/// </summary>
/// <param name="directory">The directory to read the README.txt file from.</param>
namespace ImageSorter
{
    public class FileReadme
    {
        /// <summary>
        /// An informative row in the README.txt file.
        /// </summary>
        public class CsvRow
        {
            public string Filter { get; set; }
            public string Name { get; set; }
            public string Prefix { get; set; }

            public CsvRow(string filter, string name, string prefix)
            {
                Filter = filter;
                Name = name;
                Prefix = prefix;
            }

        }

        private string README_FILENAME = "README.txt";
        private string _directory;
        //private string _readme;
        //private string[] _lines;
        private List<CsvRow> _rows= new List<CsvRow>();

        public FileReadme(string directory)
        {
            _directory = directory;
        }
        
        /// <summary>
        /// Reads the contents of README.txt in the given directory. 
        /// When a new line is found it will stop reading the file.
        /// </summary>
        /// <returns>True if the file exists and was read, false otherwise.</returns>
        public bool ReadReadme()
        {
            string readmePath = Path.Combine(_directory, README_FILENAME);
            if (!File.Exists(readmePath))
                return false;

            // Read the file line by line instead of splitting
            using (var reader = new StreamReader(readmePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Stop at first empty line (after trimming)
                    if (string.IsNullOrWhiteSpace(line))
                        break;
                        
                    var parts = line.Split(',');
                    if (parts.Length > 1)
                        _rows.Add(
                            new CsvRow(parts[0].Trim(), 
                            parts[1].Trim(), 
                            FindPrefix(parts[0].Trim()))
                            );
                }
            }
            return true;
        }

        public string[] GetEachFilter()
        {
            //var result = new List<string>();
            var result = new string[_rows.Count];
            int i = 0;
            foreach (var row in _rows)
            {
                result[i] = row.Filter;
                i++;
            }
            return result;
        }

        /// <summary>
        /// Finds the Name associated with the filter.
        /// </summary>
        /// <param name="item">The first column value to search for.</param>
        /// <returns>The second column value if found, empty string otherwise.</returns>
        public string FindSecondCsvValue(string item)
        {
            if (_rows == null)
                return "";

            foreach (var row in _rows)
            {
                if (row.Filter.StartsWith(item, StringComparison.OrdinalIgnoreCase))
                {
                    return row.Name;
                }
            }
            return "";
        }

    
        /// <summary>
        /// Extracts the prefix from the filter.
        /// The prefix is the leading letters up to the first underscore.
        /// If no underscore is found, the prefix is the leading letters.
        /// </summary>
        /// <param name="filter">The filter to extract the prefix from.</param>
        public string FindPrefix(string filter)
        {
            // Extract prefix up to first underscore
            string? prefix = null;
            int underscoreIdx = filter.IndexOf('_');
            if (underscoreIdx > 0)
                prefix = filter.Substring(0, underscoreIdx + 1); // include underscore
            else {
                // fallback: extract leading letters as the prefix
                prefix = new string(filter.TakeWhile(char.IsLetter).ToArray());

            }
            return prefix;
        }

        /// <summary>
        /// Performs a fuzzy search to find the closest matching filter and returns its associated name.
        /// Matches are scored based on:
        /// 1. Matching prefix (e.g., 'IMG_', 'DSC')
        /// 2. Numeric similarity for files with numbers
        /// For example, 'IMG_6800.jpg' would match with 'IMG_6955' better than 'IMG_1970'
        /// </summary>
        /// <param name="filename">The filename to search for (can include extension)</param>
        /// <returns>The name associated with the best matching filter, or empty string if no good match found</returns>
        public string SearchFilterReturnName(string filename)
        {
            if (_rows == null || _rows.Count == 0 || string.IsNullOrWhiteSpace(filename))
                return "";

            // Remove extension if present
            string filenameNoExt = Path.GetFileNameWithoutExtension(filename);
            
            // Get the prefix of the input filename
            string searchPrefix = FindPrefix(filenameNoExt);
            
            // First, find all rows with matching prefix
            var matchingRows = _rows.Where(r => r.Prefix.Equals(searchPrefix, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!matchingRows.Any())
                return "";

            // Try to extract number from filename
            string numberStr = new string(filenameNoExt.Where(char.IsDigit).ToArray());
            if (!int.TryParse(numberStr, out int searchNumber))
                return matchingRows.First().Name; // If no number, just return first prefix match

            // Find the closest matching number among the matching prefixes
            var bestMatch = matchingRows
                .Select(row =>
                {
                    string rowNumber = new string(row.Filter.Where(char.IsDigit).ToArray());
                    int.TryParse(rowNumber, out int num);
                    return new
                    {
                        Row = row,
                        Number = num,
                        Distance = Math.Abs(num - searchNumber)
                    };
                })
                .OrderBy(x => x.Distance)
                .First();

            return bestMatch.Row.Name;
        }

    }
} 