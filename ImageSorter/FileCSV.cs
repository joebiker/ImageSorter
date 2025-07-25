using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

namespace ImageSorter
{
    public static class FileCSV
    {
        /// <summary>
        /// Writes the list of FileProcessResult objects to a CSV file. The file will be named using the provided baseFileName (e.g., 'Fileoutput').
        /// If a file with that name already exists, a number in parentheses will be appended before the extension (e.g., 'Fileoutput(1).csv', 'Fileoutput(2).csv', etc.) until an unused filename is found.
        /// </summary>
        /// <param name="allResults">The list of FileProcessResult objects to write to the CSV.</param>
        /// <param name="baseFileName">The base filename for the CSV output (e.g., 'Fileoutput'). Should be a filename only, no extension.</param>
        /// <param name="folderPath">The folder path where the CSV file will be saved.</param>
        public static void WriteAuditCsv(List<FileProcessResult> allResults, string baseFileName, string folderPath)
        {
            // Determine the filename in the specified folder
            string fileName = Path.Combine(folderPath, baseFileName + ".csv");
            int count = 1;
            while (File.Exists(fileName))
            {
                string numberedFileName = $"{baseFileName}({count}).csv";
                fileName = Path.Combine(folderPath, numberedFileName);
                count++;
            }

            // Write the CSV file
            using (var writer = new StreamWriter(fileName))
            {
                // Write header
                writer.WriteLine("FilePath,FileName,FileNameMod,FileNameAuthor,FileNameOrdered,Author,BestDate,DateTaken,FileCreated,FileModified,Status,ErrorMessage");
                foreach (var result in allResults)
                {
                    writer.WriteLine($"{Escape(result.FilePath)},{Escape(result.FileName)},{Escape(result.FileNameMod)},{Escape(result.FileNameAuthor)},{Escape(result.FileNameOrdered)},{Escape(result.Author)},{result.BestDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)},{NullableDate(result.DateTaken)},{result.FileCreated.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)},{result.FileModified.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)},{Escape(result.Status)},{Escape(result.ErrorMessage)}");
                }
            }
            Console.WriteLine($"Audit CSV written to: {fileName}");
        }

        /// <summary>
        /// Escapes special characters in a string for CSV output.
        /// </summary>
        /// <param name="value">The string to escape.</param>
        /// <returns>The escaped string, suitable for CSV output.</returns>
        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                value = value.Replace("\"", "\"\"");
                return $"\"{value}\"";
            }
            return value;
        }

        /// <summary>
        /// Formats a nullable DateTime for CSV output.
        /// </summary>
        /// <param name="dt">The nullable DateTime to format.</param>
        /// <returns>A formatted date string, or empty string if the date is null.</returns>
        private static string NullableDate(DateTime? dt)
        {
            return dt.HasValue ? dt.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) : "";
        }
    }
}