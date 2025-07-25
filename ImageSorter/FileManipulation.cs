using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System;

namespace ImageSorter
{
    public static class FileManipulation
    {
        /// <summary>
        /// Process files, get list of files, find dates, find authors, order files
        /// </summary>
        public static void FindFiles(string folderPath, List<FileProcessResult> allResults, bool suppressGoodFiles)
        {
            // Get all JPEG files from the specified folder
            string[] jpegFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(
                    file => file.ToLower().EndsWith(".jpg") ||
                    file.ToLower().EndsWith(".jpeg")
                    )
                .ToArray();

            string[] pngFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(
                    file => file.ToLower().EndsWith(".png")
                    )
                .ToArray();

            string[] heicFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(
                    file => file.ToLower().EndsWith(".heic") // Added support for HEIC files
                    )
                .ToArray();

            string[] movFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(
                    file => file.ToLower().EndsWith(".mov") // Added support for MOV files
                    )
                .ToArray();

            if (jpegFiles.Length == 0 && pngFiles.Length == 0 && heicFiles.Length == 0 && movFiles.Length == 0)
            {
                Console.WriteLine("No files found in the specified directory.");
                return;
            }

            // Process files
            Console.WriteLine(
                $"Found " +
                $"{jpegFiles.Length} JPEG file(s), " +
                $"{pngFiles.Length} PNG file(s), " +
                $"{heicFiles.Length} HEIC file(s), " +
                $"{movFiles.Length} MOV file(s).");
            Console.WriteLine();

            Console.WriteLine("Processing JPEG files...");
            foreach (string filePath in jpegFiles)
            {
                var result = FileJPEG.ProcessJpegFile(filePath, suppressGoodFiles);
                PrintFileProcessResult(result, suppressGoodFiles);
                allResults.Add(result);
            }

            Console.WriteLine("Processing PNG files...");
            foreach (string filePath in pngFiles)
            {
                var result = FilePNG.ProcessPngFile(filePath, suppressGoodFiles);
                PrintFileProcessResult(result, suppressGoodFiles);
                allResults.Add(result);
            }

            Console.WriteLine("Processing HEIC files...");
            foreach (string filePath in heicFiles)
            {
                var result = FileHEIC.GetDateTakenWithMagickNet(filePath);
                PrintFileProcessResult(result, suppressGoodFiles);
                allResults.Add(result);
            }

            Console.WriteLine("Processing MOV files...");
            foreach (string filePath in movFiles)
            {
                var result = FileMOV.ProcessMovFile(filePath);
                PrintFileProcessResult(result, suppressGoodFiles);
                allResults.Add(result);
            }

        }

        static void PrintFileProcessResult(FileProcessResult result, bool suppressGood = false)
        {
            if (result.Status == "Error")
            {
                Console.WriteLine($"Error processing {result.FileName}: {result.ErrorMessage}");
            }
            else if (result.Status == "Exif")
            {
                if (!suppressGood)
                {
                    Console.Write($"{result.FileName,-40} ");
                    Console.WriteLine($"Date Taken: {result.DateTaken:yyyy-MM-dd HH:mm:ss}, File created: {result.FileCreated:yyyy-MM-dd HH:mm:ss}, File modified: {result.FileModified:yyyy-MM-dd HH:mm:ss}");
                }
            }
            else if (result.Status == "NoExif")
            {
                Console.Write($"{result.FileName,-40} ");
                Console.WriteLine($"No EXIF date found. File created: {result.FileCreated:yyyy-MM-dd HH:mm:ss}, File modified: {result.FileModified:yyyy-MM-dd HH:mm:ss}");
            }
        }
        
        /// <summary>
        /// Assigns authors to files based on patterns in the README file
        /// If no readme is available, this will do nothing.
        /// </summary>
        public static void FindAuthors(List<FileProcessResult> files, FileReadme readme)
        {
            if (files == null || readme == null)
                return;

            if (readme.GetFilters().Length == 0)
                return;

            foreach (var file in files)
            {
                if (string.IsNullOrWhiteSpace(file.FileName))
                    continue;

                string author = readme.SearchFilterReturnName(file.FileName);
                file.Author = string.IsNullOrWhiteSpace(author) ? "" : author;

                // Setup the FileNameAuthor
                if (!string.IsNullOrWhiteSpace(file.Author))
                {
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
                    string extension = Path.GetExtension(file.FileName);
                    file.FileNameAuthor = $"{fileNameWithoutExt}_{file.Author}{extension}";
                }
                else {
                    // Must write something for the Ordering function.
                    file.FileNameAuthor = file.FileName;
                }
            }
        }

        /// <summary>
        /// Orders files by their best available date
        /// </summary>
        public static void OrderFiles(List<FileProcessResult> allResults)
        {
            // must contain more than 1 element
            if (allResults.Count < 2)
                return;

            // Basic sort by BestDate
            allResults.Sort((a, b) => a.BestDate.CompareTo(b.BestDate));

            // Create a new filename
            int i = 1; // 001, 002, 003, etc.
            var lastResult = allResults.First();
            var lastPrefix = i; // Store the last used prefix number

            foreach (var result in allResults)
            {
                // Check if this file has the same name (ignoring extension) as the last file
                bool isSameBaseFile = Path.GetFileNameWithoutExtension(result.FileName)
                    .Equals(Path.GetFileNameWithoutExtension(lastResult.FileName), 
                        StringComparison.OrdinalIgnoreCase);

                if (!isSameBaseFile)
                {
                    // find difference in BestDate from lastResult
                    var diff = result.BestDate - lastResult.BestDate;

                    // if diff is greater than 2 hours, increment i to the next 10s multiple
                    if (diff.TotalHours > 2)
                    {
                        i = (i / 10) * 10 + 10;
                    }
                    else
                    {
                        i++; // Only increment if it's a different base filename
                    }
                    lastPrefix = i;
                }
                else
                {
                    i = lastPrefix; // Use the same prefix for files with same base name
                }
                // Always save to the ordered filename
                result.FileNameOrdered = $"{i:D3}_{result.FileName}";

                // if FileNameAuthor is set, use it, otherwise use the original filename
                if (!string.IsNullOrWhiteSpace(result.FileNameAuthor))
                {
                    // uses three digits in front of the filename
                    result.FileNameMod = $"{i:D3}_{result.FileNameAuthor}";
                }
                else
                {
                    // Set to FileNameOrdered because we don't have an author
                    result.FileNameMod = $"{i:D3}_{result.FileName}";
                }
                lastResult = result;
            }
        }

        /// <summary>
        /// Here we want to rename meta files along with similarly named files.
        /// Search the folderPath for files with the same name, but different extensions, and not already processed in allResults.
        /// Add them to allResults with the Status "Similar". Set the DateTaken to the same as the original file, and all other dates.
        /// </summary>
        /// <param name="allResults">List of already processed files</param>
        /// <param name="folderPath">Directory to search for similar files</param>
        public static void FindSimilarFiles(List<FileProcessResult> allResults, string folderPath)
        {
            if (allResults == null || allResults.Count == 0)
            {
                Console.WriteLine("No files to process for similar names.");
                return;
            }

            // Iterate through each file in allResults
            foreach (var result in allResults.ToList()) // Create a copy to avoid modifying while enumerating
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(result.FileName);
                
                // Search for files with same name but different extensions in the folder
                string[] similarFiles = Directory.GetFiles(
                    folderPath,
                    $"{fileNameWithoutExtension}.*",
                    SearchOption.TopDirectoryOnly)
                    .Where(file => !allResults.Any(r => r.FilePath.Equals(file, StringComparison.OrdinalIgnoreCase)))
                    .ToArray();

                // Process each similar file found
                foreach (var similarFilePath in similarFiles)
                {
                    var similarResult = new FileProcessResult
                    {
                        FilePath = similarFilePath,
                        FileName = Path.GetFileName(similarFilePath),
                        Status = "Similar",
                        // Copy all dates from the original file
                        DateTaken = result.DateTaken,
                        FileCreated = result.FileCreated,
                        FileModified = result.FileModified
                    };

                    allResults.Add(similarResult);
                }
            }
        }
        
        /// <summary>
        /// Renames files on the filesystem using FileNameMod and appends the author name before the extension.
        /// The new filename format will be: {FileNameMod_without_ext}_{Author}{extension}
        /// For example: 001_IMG_1234_JohnDoe.jpg
        /// </summary>
        /// <param name="allResults">List of files to be renamed</param>
        public static void RenameFiles(List<FileProcessResult> allResults)
        {
            if (allResults == null || allResults.Count == 0)
            {
                Console.WriteLine("No files to rename.");
                return;
            }

            foreach (var result in allResults)
            {
                if (string.IsNullOrWhiteSpace(result.FileNameMod))
                {
                    Console.WriteLine($"Skipping renaming for {result.FileName} due to missing modified filename.");
                    continue;
                }

                // Get directory and split filename into name and extension
                string directory = Path.GetDirectoryName(result.FilePath) ?? string.Empty;
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(result.FileNameMod);
                string extension = Path.GetExtension(result.FileNameMod);
                
                // Create new filename with author before extension
                string newFileName = fileNameWithoutExt;
                if (!string.IsNullOrWhiteSpace(result.Author))
                {
                    newFileName = $"{newFileName}_{result.Author}";
                }
                newFileName = $"{newFileName}{extension}";

                string newFilePath = Path.Combine(directory, newFileName);

                try
                {
                    if (File.Exists(newFilePath))
                    {
                        Console.WriteLine($"File already exists: {newFilePath}. Skipping rename for {result.FileName}");
                        continue;
                    }

                    File.Move(result.FilePath, newFilePath);
                    Console.WriteLine($"Renamed {result.FileName} to {newFileName}");
                    result.FilePath = newFilePath; // Update the file path in the result
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error renaming {result.FileName}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Undoes file renames on the filesystem.
        /// </summary>
        /// <param name="allResults">List of files to revert renames</param>
        public static void UndoRenameFiles(List<FileProcessResult> allResults)
        {
            // Implementation goes here
        }
    }
}
