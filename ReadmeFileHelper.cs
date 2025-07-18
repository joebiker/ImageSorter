using System;
using System.IO;

namespace ImageSorter
{
    public class ReadmeFileHelper
    {
        public class CsvRow
        {
            public string Filter { get; set; }
            public string Name { get; set; }

            public CsvRow(string filter, string name)
            {
                Filter = filter;
                Name = name;
            }
        }

        private string _directory;
        private string _readme;
        private string[] _lines;
        private List<CsvRow> _rows= new List<CsvRow>();

        public ReadmeFileHelper(string directory)
        {
            _directory = directory;
        }
        /// <summary>
        /// Reads the contents of README.txt in the given directory. Returns null if the file does not exist.
        /// </summary>
        /// <returns>The contents of README.txt, or null if not found.</returns>
        public bool ReadReadme()
        {
            string readmePath = Path.Combine(_directory, "README.txt");
            if (!File.Exists(readmePath))
                return false;
            _readme = File.ReadAllText(readmePath);
            _lines = _readme.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in _lines)
            {
                var parts = line.Split(',');
                if (parts.Length > 1)
                    _rows.Add(new CsvRow(parts[0].Trim(), parts[1].Trim()));
            }
            return true;
        }
        
        public string[] GetEachFileName()
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

        public string FindSecondCsvValue(string firstItem)
        {
            if (_rows == null)
                return "";

            // Extract prefix up to first underscore
            string? prefix = null;
            int underscoreIdx = firstItem.IndexOf('_');
            if (underscoreIdx > 0)
                prefix = firstItem.Substring(0, underscoreIdx + 1); // include underscore
            else
                prefix = firstItem; // fallback: use full string

            foreach (var row in _rows)
            {
                if (row.Filter.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return row.Name;
                }
            }
            return "";
        }
    }
}
