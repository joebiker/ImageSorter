// Example paths:
// C:\Users\jpetsche\Pictures\Fractals
// C:\Users\JoePetsche\Downloads (laptop)
// S:\Pictures\2025_07_06 El Dente and Wilson Peak

using System.Reflection;
using Allegory;
using Allegory.POCOs;

public static class Program
{
    public static int RENAME_SIZE = 3; // other numbers not supported yet

    // TODO: implement intRenameSize(3)
    static void Main(string[] args)
    {
        string folderPath = @"."; // Default to current directory
        string fullPath = Path.GetFullPath(folderPath);
        bool suppressGoodFiles = true;
        bool auditCsvOutput = false;
        bool executeRename = false;
        bool undoRename = false;
        int lineWidth = 80;
        int intRenameSize = 3; // other numbers not supported yet
        
        // Print Version to console
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        Console.WriteLine($"Version: {version}");

        if (args.Any(a => a.Equals("-version", StringComparison.OrdinalIgnoreCase) || a.Equals("--version", StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        // Parse arguments
        if (args.Length > 0)
        {
            folderPath = args[0];
            fullPath = Path.GetFullPath(folderPath);
            if (args.Any(a => a.Equals("-audit", StringComparison.OrdinalIgnoreCase)))
            {
                auditCsvOutput = true;
            }
            // allow go or do or execute
            if (args.Any(a => a.Equals("-go", StringComparison.OrdinalIgnoreCase) || a.Equals("-do", StringComparison.OrdinalIgnoreCase) || a.Equals("-execute", StringComparison.OrdinalIgnoreCase)))
            {
                undoRename = false;
                executeRename = true;
            }
            // -undo or -revert 
            if (args.Any(a => a.Equals("-undo", StringComparison.OrdinalIgnoreCase) || a.Equals("-revert", StringComparison.OrdinalIgnoreCase)))
            {
                undoRename = true;
                // incase two parameters are provided, make sure the rename is not executed
                executeRename = false;
            }
        }
        else
        {
            Console.WriteLine("Enter the folder path containing JPEG files (or press Enter for current directory):");
            string? userInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(userInput))
            {
                folderPath = userInput;
                fullPath = Path.GetFullPath(folderPath);
            }
            // Ask for audit if not provided in args
            Console.WriteLine("Output audit CSV? (y/N):");
            string? auditInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(auditInput) && auditInput.Trim().ToLower().StartsWith("y"))
            {
                auditCsvOutput = true;
            }
            // Ask for rename execution if not provided in args
            Console.WriteLine("Execute file renaming? (y/N):");
            string? renameInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(renameInput) && renameInput.Trim().ToLower().StartsWith("y"))
            {
                executeRename = true;
            }
        }

        // Validate folder path
        if (!Directory.Exists(fullPath))
        {
            Console.WriteLine($"Error: Directory '{fullPath}' does not exist.");
            return;
        }

        // Print the folder being scanned
        Console.WriteLine($"Scanning for image files in: {fullPath}");
        Console.WriteLine(new string('=', lineWidth));

        // the main show
        var allResults = new List<FileProcessResult>();
        FileManipulation.FindFiles(fullPath, allResults, suppressGoodFiles);

        // Update MOV dates from matching JPEGs
        Console.WriteLine("Checking MOV files for matching JPEGs...");
        FileMOV.UpdateMovDatesFromJpeg(allResults);

        // Find similar files
        Console.WriteLine("Finding similar files releated to each image...");
        FileManipulation.FindSimilarFiles(allResults, fullPath);
        
        // Maybe this goes into a class file?
        var readme = new FileReadme(fullPath);
        readme.ReadReadme();

        // Find Author of each file
        Console.WriteLine("Parsing README and finding Authors...");
        FileManipulation.FindAuthors(allResults, readme);

        // Sort the results by BestDate
        Console.WriteLine("Sorting files...");
        FileManipulation.OrderFiles(allResults);

        // Preview the renames (only when not undo or execute)
        Console.WriteLine(new string('=', lineWidth));
        if(!undoRename && !executeRename)
        {
            Console.WriteLine("Previewing file renames:");
            foreach (var result in allResults)
            {
                // space this out so the largest FileName will align -> in the same column
                string fileName = result.FileName;
                string fileNameMod = result.FileNameMod;
                int maxLength = GetLongestFileName(allResults);
                Console.WriteLine($"{fileName.PadRight(maxLength)} -> {fileNameMod}");
            }
        }

        // Rename files only if -go parameter is provided
        if (executeRename)
        {
            Console.WriteLine("Executing file renames...");
            FileManipulation.RenameFiles(allResults);
        }
        else if (!executeRename && !undoRename)
        {
            // If Undo is selected, do not mention rename.
            Console.WriteLine("To execute the file renames, run the command again with the -go parameter.");
        }

        if (undoRename)
        {
            Console.WriteLine("Undoing file renames...");
            FileManipulation.UndoRenameFiles(allResults, intRenameSize, readme);
        }

        Console.WriteLine(new string('=', lineWidth));

        // Output audit CSV if requested at command line
        if (auditCsvOutput && allResults.Count > 0)
        {
            FileCSV.WriteAuditCsv(allResults, "_ImageFileAudit", fullPath);
        }
        
        //Console.WriteLine("Processing complete. (needed for Visual Studio Code to see the output) Press any key to exit...");
        //Console.ReadKey();
        //Console.Read(); // For debug console when input isn't really possible. 
    }

    /// <summary>
    /// Returns the length of the longest FileName in the list.
    /// </summary>
    /// <param name="allResults">The list of FileProcessResult objects to check.</param>
    /// <returns>The length of the longest FileName.</returns>
    private static int GetLongestFileName(List<FileProcessResult> allResults)
    {
        return allResults.Max(r => r.FileName.Length);
    }

}