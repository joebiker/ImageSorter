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

        // Print Version to console
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        Console.WriteLine($"Version: {version}");

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

        // Print the folder being scanned
        Console.WriteLine($"Scanning for image files in: {Path.GetFullPath(folderPath)}");
        Console.WriteLine(new string('=', 80));

        // the main show
        var allResults = new List<FileProcessResult>();
        FileManipulation.FindFiles(folderPath, allResults, suppressGoodFiles);

        // Update MOV dates from matching JPEGs
        Console.WriteLine("Checking MOV files for matching JPEGs...");
        FileMOV.UpdateMovDatesFromJpeg(allResults);

        // Find similar files
        Console.WriteLine("Finding similar files releated to each image...");
        FileManipulation.FindSimilarFiles(allResults, folderPath);
        
        // Maybe this goes into a class file?
        var readme = new FileReadme(folderPath);
        readme.ReadReadme();

        // Find Author of each file
        Console.WriteLine("Parsing README and finding Authors...");
        FileManipulation.FindAuthors(allResults, readme);

        // Sort the results by BestDate
        Console.WriteLine("Sorting files...");
        FileManipulation.OrderFiles(allResults, readme);

        // Rename files.
        Console.WriteLine("Renaming files...");
        FileManipulation.RenameFiles(allResults);

        Console.WriteLine(new string('=', 80));
/*
        // Print the results to screen (debugging purposes)
        Console.WriteLine("Sorted by BestDate:");
        Console.WriteLine();
        foreach (var result in allResults)
        {
            Console.WriteLine($"{result.FileName,-30} {result.FileNameMod,-30} {result.Author,-10} {result.BestDate:yyyy-MM-dd HH:mm:ss} {result.Status}");
        }
        Console.WriteLine(new string('=', 80));
*/
        // Output audit CSV if requested at command line
        if (audit)
        {
            FileCSV.WriteAuditCsv(allResults, "_ImageFileAudit", folderPath);
        }
        
        //Console.WriteLine("Processing complete. Press any key to exit...");
        //Console.ReadKey();
        //Console.Read(); // For debug console when input isn't really possible. 
        // But the again, this line isn't needed in that case.
    }

    // TODO: rename any jpeg and _.* files. could be MOV, could be txt or other.
}