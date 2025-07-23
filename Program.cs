// See https://aka.ms/new-console-template for more information
// Example paths:
// C:\Users\jpetsche\Pictures\Fractals
// C:\Users\JoePetsche\Downloads (laptop)
// S:\Pictures\2025_07_06 El Dente and Wilson Peak

using System.Reflection;
using ImageSorter;
using System.IO;
using System.Linq;
using System.Globalization;

public static class Program
{
    static void Main(string[] args)
    {
        bool suppressGoodFiles = true;
        string folderPath = "."; // Default to current directory
        bool audit = false;

        // Parse arguments
        if (args.Length > 0)
        {
            folderPath = args[0];
            if (args.Any(a => a.Equals("audit", StringComparison.OrdinalIgnoreCase)))
            {
                audit = true;
            }
        }
        else
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine($"Version: {version}");
            Console.WriteLine("Enter the folder path containing JPEG files (or press Enter for current directory):");
            string? userInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(userInput))
            {
                folderPath = userInput;
            }
            // Ask for audit if not provided in args
            Console.WriteLine("Output audit CSV? (y/N):");
            string? auditInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(auditInput) && auditInput.Trim().ToLower().StartsWith("y"))
            {
                audit = true;
            }
        }

        // Validate folder path
        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine($"Error: Directory '{folderPath}' does not exist.");
            return;
        }

        // TESTING
        var readme = new FileReadme(folderPath);
        readme.ReadReadme();
        var filters = readme.GetEachFilter();
        var name = readme.SearchFilterReturnName("IMG_1111.jpg");

        // Print the folder being scanned
        Console.WriteLine($"Scanning for JPEG files in: {Path.GetFullPath(folderPath)}");
        Console.WriteLine(new string('=', 80));

        // Get all JPEG files from the specified folder
        string[] jpegFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(
                file => file.ToLower().EndsWith(".jpg") ||
                file.ToLower().EndsWith(".jpeg")
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

        if (jpegFiles.Length == 0 && heicFiles.Length == 0 && movFiles.Length == 0)
        {
            Console.WriteLine("No files found in the specified directory.");
            return;
        }

        // Process files
        Console.WriteLine(
            $"Found " +
            $"{jpegFiles.Length} JPEG file(s), " +
            $"{heicFiles.Length} HEIC file(s), " +
            $"{movFiles.Length} MOV file(s).");
        Console.WriteLine();

        var allResults = new List<FileProcessResult>();
        Console.WriteLine("Processing JPEG files...");
        foreach (string filePath in jpegFiles)
        {
            var result = FileJPEG.ProcessJpegFile(filePath, suppressGoodFiles);
            PrintFileProcessResult(result, suppressGoodFiles);
            allResults.Add(result);
        }

        Console.WriteLine("Processing HEIC files...");
        foreach (string filePath in heicFiles)
        {
            //var result = HEICManipulation.GetMetaDataWithGroupDocs(filePath);
            var result = HEICManipulation.GetDateTakenWithMagickNet(filePath);
            PrintFileProcessResult(result, suppressGoodFiles);
            allResults.Add(result);
        }

        Console.WriteLine("Processing MOV files...");
        foreach (string filePath in movFiles)
        {
            var result = MOVManipulation.ProcessMovFile(filePath);
            PrintFileProcessResult(result, suppressGoodFiles);
            allResults.Add(result);
        }

        // Sort the results by BestDate
        Console.WriteLine("Sorting files...");
        FileManipulation.OrderFiles(allResults, readme);

        Console.WriteLine(new string('=', 80));
        Console.WriteLine("Sorted by BestDate:");
        Console.WriteLine();
        foreach (var result in allResults)
        {
            Console.WriteLine($"{result.FileName,-30} {result.FileNameMod,-30} {result.Author,-10} {result.BestDate:yyyy-MM-dd HH:mm:ss} {result.Status}");
        }
        
        Console.WriteLine(new string('=', 80));

        // Output audit CSV if requested
        if (audit)
        {
            FileCSV.WriteAuditCsv(allResults, "_ImageFileAudit", folderPath);
        }
        //Console.WriteLine("Processing complete. Press any key to exit...");
        //Console.ReadKey();
        //Console.Read(); // For debug console when input isn't really possible. 
        // But the again, this line isn't needed in that case.
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
                Console.WriteLine($"Date Taken: {result.DateTaken:yyyy-MM-dd HH:mm:ss}");
            }
        }
        else if (result.Status == "NoExif")
        {
            Console.Write($"{result.FileName,-40} ");
            Console.WriteLine($"No EXIF date found. File created: {result.FileCreationTime:yyyy-MM-dd HH:mm:ss}");
        }
    }

    // TODO: rename any jpeg and _.* files. could be MOV, could be txt or other.
}