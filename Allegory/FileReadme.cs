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
namespace Allegory
{
    public class FileReadme
    {
        /// <summary>
        /// An informative row in the README.txt file.
        /// </summary>
        public class CsvRow
        {
            // Image file filter (e.g., IMG_6955)
            public string Filter { get; set; }
            // Name of the photographer
            public string Name { get; set; }
            // Prefix extracted from the filter (e.g., 'IMG_')
            public string Prefix { get; set; }

            public CsvRow(string filter, string name, string prefix)
            {
                Filter = filter;
                Name = name;
                Prefix = prefix;
            }

        }

        private string README_FILENAME = "_README.txt";
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
            // if file doesn't exist, remove the first char of the filename and try again
            string readmePath = Path.Combine(_directory, README_FILENAME);
            if (!File.Exists(readmePath))
            {
                // remove the first char of the filename and try again
                readmePath = Path.Combine(_directory, README_FILENAME.Substring(1));
                if (!File.Exists(readmePath))
                    return false;
            }

            // Read the file line by line instead of splitting
            using (var reader = new StreamReader(readmePath))
            {
                string? line;
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

        public string[] GetFilters()
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
        /// Extracts the prefix from the filter.
        /// The prefix is the leading letters up to the first underscore.
        /// If no underscore is found, the prefix is the leading letters.
        /// Enhanced to handle camera file patterns like DSC07898, IMG_6955, etc.
        /// </summary>
        /// <param name="filter">The filter to extract the prefix from.</param>
        public string FindPrefix(string filter)
        {
            // First try: Extract prefix up to first underscore (existing logic)
            int underscoreIdx = filter.IndexOf('_');
            if (underscoreIdx > 0 && underscoreIdx < 4)
                return filter.Substring(0, underscoreIdx + 1); // include underscore

            // Enhanced: For camera files without underscore, extract letters + first few digits
            // This handles patterns like DSC07898, where DSC is the camera type
            var letters = filter.TakeWhile(char.IsLetter).ToArray();
            if (letters.Length > 0)
            {
                string letterPart = new string(letters);
                
                // For patterns like DSC07898, we want "DSC" as the prefix
                // For patterns like IMG6955, we want "IMG" as the prefix
                return letterPart;
            }

            // Fallback: return first 3 characters if no clear pattern
            return filter.Length >= 3 ? filter.Substring(0, 3) : filter;
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

        /// <summary>
        /// Creates a new _README.txt file in the specified directory from camera groups.
        /// The file contains one line per camera group in CSV format: FirstFileName,CameraName
        /// </summary>
        /// <param name="directory">The directory where the _README.txt file will be created</param>
        /// <param name="cameraGroups">Array of camera groups to write to the file</param>
        /// <returns>True if the file was created successfully, false otherwise</returns>
        public static bool CreateReadmeFromCameraGroups(string directory, SortingHat.CameraGroup[] cameraGroups)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                Console.WriteLine("Error: Directory path is null or empty.");
                return false;
            }

            if (!Directory.Exists(directory))
            {
                Console.WriteLine($"Error: Directory does not exist: {directory}");
                return false;
            }

            if (cameraGroups == null || cameraGroups.Length == 0)
            {
                Console.WriteLine("Warning: No camera groups provided. Empty _README.txt will be created.");
            }

            string readmePath = Path.Combine(directory, "_README.txt");

            try
            {
                using (var writer = new StreamWriter(readmePath, false))
                {
                    if (cameraGroups != null)
                    {
                        foreach (var group in cameraGroups)
                        {
                            // Extract just the filename without extension for the filter
                            string filterName = Path.GetFileNameWithoutExtension(group.FirstFileName);
                            writer.WriteLine($"{filterName}, {group.CameraName}");
                        }
                    }
                }

                Console.WriteLine($"Successfully created _README.txt at: {readmePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating _README.txt: {ex.Message}");
                return false;
            }
        }

    }
}