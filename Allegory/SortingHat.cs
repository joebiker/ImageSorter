using System.Text.RegularExpressions;

namespace Allegory
{
    public class SortingHat
    {
        /// <summary>
        /// Represents a camera group identified by its naming pattern
        /// </summary>
        public class CameraGroup
        {
            public string FirstFileName { get; set; } = string.Empty;
            public string CameraName { get; set; } = string.Empty;
        }

        /// <summary>
        /// Analyzes filenames in a directory and identifies different camera naming patterns
        /// </summary>
        /// <param name="directoryPath">Path to the directory containing files</param>
        /// <returns>Array of camera groups with their identifier and first filename</returns>
        public static CameraGroup[] IdentifyCameras(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
            }

            // Get all image and video files
            string[] allFiles = Directory.GetFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(file =>
                {
                    string ext = Path.GetExtension(file).ToLower();
                    return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || 
                           ext == ".heic" || ext == ".mov";
                })
                .Select(Path.GetFileName)
                .Where(name => name != null)
                .Cast<string>()
                .ToArray();

            if (allFiles.Length == 0)
            {
                return Array.Empty<CameraGroup>();
            }

            // Dictionary to store pattern groups
            var patternGroups = new Dictionary<string, List<string>>();

            foreach (var filename in allFiles)
            {
                string pattern = ExtractNamingPattern(filename);
                
                if (!patternGroups.ContainsKey(pattern))
                {
                    patternGroups[pattern] = new List<string>();
                }
                patternGroups[pattern].Add(filename);
            }

            // Create camera groups
            var cameraGroups = new List<CameraGroup>();
            int cameraIndex = 1;

            foreach (var group in patternGroups.OrderBy(g => g.Value.First()))
            {
                string firstFile = group.Value.First();
                string manufacturer = GuessManufacturer(firstFile);

                cameraGroups.Add(new CameraGroup
                {
                    FirstFileName = firstFile,
                    CameraName = !string.IsNullOrEmpty(manufacturer) 
                        ? manufacturer
                        : $"Camera{cameraIndex}"
                });
                cameraIndex++;
            }

            return cameraGroups.ToArray();
        }

        /// <summary>
        /// Guesses the camera manufacturer based on the filename pattern
        /// </summary>
        /// <param name="filename">The filename to analyze</param>
        /// <returns>The manufacturer name, or empty string if unknown</returns>
        private static string GuessManufacturer(string filename)
        {
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(filename).ToUpper();

            // Apple/iPhone: IMG_ with date format (IMG_YYYYMMDD_HHMMSS)
            if (nameWithoutExtension.StartsWith("IMG_") || Regex.IsMatch(nameWithoutExtension, @"^IMG_\d{8}_\d{6}"))
                return "Apple";

            // Generic Android/smartphone date format (YYYYMMDD_HHMMSS without prefix)
            if (Regex.IsMatch(nameWithoutExtension, @"^\d{8}_\d{6}$"))
                return "Android";

            // Sony: DSC (Cyber-shot), DSC-RX, DSCF
            if (nameWithoutExtension.StartsWith("DSC"))
                return "Sony";

            // Nikon: DSC_ (some models), _DSC, DSCN
            if (nameWithoutExtension.StartsWith("DSCN") || nameWithoutExtension.StartsWith("_DSC"))
                return "Nikon";

            // Fujifilm: DSCF, DSF_
            if (nameWithoutExtension.StartsWith("DSCF") || nameWithoutExtension.StartsWith("DSF_"))
                return "Fujifilm";

            // Canon: IMG_ (most Canon cameras), CRW_, CR2_, CR3_
            if (nameWithoutExtension.StartsWith("CRW") || 
                nameWithoutExtension.StartsWith("CR2") || nameWithoutExtension.StartsWith("CR3"))
                return "Canon";

            // Panasonic: P followed by 7 digits
            if (Regex.IsMatch(nameWithoutExtension, @"^P\d{7}$"))
                return "Panasonic";

            // Olympus: PA, PB, PC followed by digits
            if (Regex.IsMatch(nameWithoutExtension, @"^P[ABC]\d"))
                return "Olympus";

            // Samsung: SAM_, SAMN
            if (nameWithoutExtension.StartsWith("SAM"))
                return "Samsung";

            // GoPro: GOPR, GP followed by digits
            if (nameWithoutExtension.StartsWith("GOPR") || nameWithoutExtension.StartsWith("GP"))
                return "GoPro";

            // DJI (drones): DJI_, DJIX
            if (nameWithoutExtension.StartsWith("DJI"))
                return "DJI";

            // Unknown manufacturer
            return string.Empty;
        }

        /// <summary>
        /// Extracts a naming pattern from a filename to identify camera style
        /// </summary>
        /// <param name="filename">The filename to analyze</param>
        /// <returns>A pattern string representing the naming convention</returns>
        private static string ExtractNamingPattern(string filename)
        {
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

            // Pattern 1: IMG_XXXX (e.g., IMG_1234.jpg)
            if (Regex.IsMatch(nameWithoutExtension, @"^IMG_\d{4}$", RegexOptions.IgnoreCase))
            {
                return "IMG_####";
            }

            // Pattern 2: DSC_XXXX (e.g., DSC_0001.jpg)
            if (Regex.IsMatch(nameWithoutExtension, @"^DSC_\d{4}$", RegexOptions.IgnoreCase))
            {
                return "DSC_####";
            }

            // Pattern 3: DCIM date format (e.g., 20231125_123456.jpg)
            if (Regex.IsMatch(nameWithoutExtension, @"^\d{8}_\d{6}$"))
            {
                return "YYYYMMDD_HHMMSS";
            }

            // Pattern 4: Date with dashes (e.g., 2023-11-25_12-34-56.jpg)
            if (Regex.IsMatch(nameWithoutExtension, @"^\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}$"))
            {
                return "YYYY-MM-DD_HH-MM-SS";
            }

            // Pattern 5: Prefix + numbers (e.g., Photo_001.jpg, Picture123.jpg)
            var prefixMatch = Regex.Match(nameWithoutExtension, @"^([a-zA-Z]+)[-_]?\d+$", RegexOptions.IgnoreCase);
            if (prefixMatch.Success)
            {
                return $"{prefixMatch.Groups[1].Value.ToUpper()}_###";
            }

            // Pattern 6: Date stamp format (e.g., IMG_20231125_123456.jpg)
            if (Regex.IsMatch(nameWithoutExtension, @"^[a-zA-Z]+_\d{8}_\d{6}$", RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(nameWithoutExtension, @"^([a-zA-Z]+)_\d{8}_\d{6}$", RegexOptions.IgnoreCase);
                return $"{match.Groups[1].Value.ToUpper()}_YYYYMMDD_HHMMSS";
            }

            // Pattern 7: UUID or long alphanumeric (e.g., a1b2c3d4e5f6.jpg)
            if (Regex.IsMatch(nameWithoutExtension, @"^[a-fA-F0-9]{8,}$"))
            {
                return "UUID/HEX";
            }

            // Pattern 8: Appears to have author prefix (contains underscore or dash with text)
            if (nameWithoutExtension.Contains("_") || nameWithoutExtension.Contains("-"))
            {
                var parts = nameWithoutExtension.Split(new[] { '_', '-' }, 2);
                if (parts.Length > 0 && !string.IsNullOrEmpty(parts[0]))
                {
                    return $"{parts[0]}_PATTERN";
                }
            }

            // Default: Use the actual filename as pattern if no pattern matches
            return $"CUSTOM_{nameWithoutExtension.Length}chars";
        }

        /// <summary>
        /// Analyzes filenames in a directory and identifies different authors based on naming patterns and numeric ranges.
        /// Files with similar patterns but large numeric gaps (>1000) are considered different authors.
        /// </summary>
        /// <param name="directoryPath">Path to the directory containing files</param>
        /// <param name="numericGapThreshold">The numeric gap threshold to consider different authors (default: 1000)</param>
        /// <returns>Array of camera groups with their identifier and first filename</returns>
        public static CameraGroup[] IdentifyAuthors(string directoryPath, int numericGapThreshold = 1000)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
            }

            // Get all image and video files
            string[] allFiles = Directory.GetFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(file =>
                {
                    string ext = Path.GetExtension(file).ToLower();
                    return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || 
                           ext == ".heic" || ext == ".mov";
                })
                .Select(Path.GetFileName)
                .Where(name => name != null)
                .Cast<string>()
                .ToArray();

            if (allFiles.Length == 0)
            {
                return Array.Empty<CameraGroup>();
            }

            // Group files by pattern first
            var patternGroups = new Dictionary<string, List<string>>();

            foreach (var filename in allFiles)
            {
                string pattern = ExtractNamingPattern(filename);
                
                if (!patternGroups.ContainsKey(pattern))
                {
                    patternGroups[pattern] = new List<string>();
                }
                patternGroups[pattern].Add(filename);
            }

            // Now split each pattern group by numeric ranges
            var authorGroups = new List<CameraGroup>();
            int authorIndex = 1;

            foreach (var patternGroup in patternGroups.OrderBy(g => g.Value.First()))
            {
                string pattern = patternGroup.Key;
                var files = patternGroup.Value;

                // Extract numeric values and sort files by them
                var filesWithNumbers = files.Select(f => new
                {
                    FileName = f,
                    Number = ExtractNumericValue(f)
                })
                .OrderBy(x => x.Number)
                .ToList();

                // Group files by numeric ranges
                var numericGroups = new List<List<string>>();
                
                if (filesWithNumbers.Count > 0)
                {
                    List<string> currentGroup = new List<string> { filesWithNumbers[0].FileName };
                    int lastNumber = filesWithNumbers[0].Number;

                    for (int i = 1; i < filesWithNumbers.Count; i++)
                    {
                        var file = filesWithNumbers[i];
                        int gap = Math.Abs(file.Number - lastNumber);
                        
                        if (gap <= numericGapThreshold)
                        {
                            currentGroup.Add(file.FileName);
                        }
                        else
                        {
                            // Start a new group due to large numeric gap
                            numericGroups.Add(new List<string>(currentGroup));
                            currentGroup.Clear();
                            currentGroup.Add(file.FileName);
                        }
                        lastNumber = file.Number;
                    }

                    // Add the last group
                    if (currentGroup.Count > 0)
                    {
                        numericGroups.Add(currentGroup);
                    }
                }

                // Create author groups for each numeric range
                foreach (var numericGroup in numericGroups)
                {
                    string firstFile = numericGroup.First();
                    string manufacturer = GuessManufacturer(firstFile);

                    authorGroups.Add(new CameraGroup
                    {
                        FirstFileName = firstFile,
                        CameraName = !string.IsNullOrEmpty(manufacturer) 
                            ? $"{manufacturer}{authorIndex}"
                            : $"Author{authorIndex}"
                    });
                    authorIndex++;
                }
            }

            return authorGroups.ToArray();
        }

        /// <summary>
        /// Extracts the numeric value from a filename for comparison purposes
        /// </summary>
        /// <param name="filename">The filename to analyze</param>
        /// <returns>The numeric value, or 0 if no number found</returns>
        private static int ExtractNumericValue(string filename)
        {
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
            
            // Extract all digits from the filename
            string numberStr = new string(nameWithoutExtension.Where(char.IsDigit).ToArray());
            
            if (int.TryParse(numberStr, out int number))
            {
                return number;
            }
            
            return 0;
        }
    }
}
