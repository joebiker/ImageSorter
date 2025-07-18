using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace ImageSorter
{
    public static class FileManipulation
    {
        public static FileProcessResult ProcessJpegFile(string filePath, bool suppressPrintMessageOfGood = false)
        {
            var result = new FileProcessResult();
            result.FileName = Path.GetFileName(filePath);
            try
            {
                using (var image = new Bitmap(filePath))
                {
                    // Try to get the date taken from EXIF data
                    DateTime? dateTaken = GetDateTaken(image);
                    result.DateTaken = dateTaken;

                    if (dateTaken.HasValue)
                    {
                        
                        result.Status = "Exif";
                    }
                    else
                    {
                        // Fall back to file creation/modification date
                        FileInfo fileInfo = new FileInfo(filePath);
                        
                        result.FileCreationTime = fileInfo.CreationTime;
                        result.Status = "NoExif";
                    }
                }
            }
            catch (Exception ex)
            {
                result.Status = "Error";
                result.ErrorMessage = ex.Message;
            }
            return result;
        }

        static DateTime? GetDateTaken(Image image)
        {
            try
            {
                // EXIF property ID for DateTimeOriginal (when photo was taken)
                const int DateTimeOriginalPropertyId = 0x9003;

                // Try DateTimeOriginal first (most accurate)
                if (image.PropertyIdList.Contains(DateTimeOriginalPropertyId))
                {
                    PropertyItem propItem = image.GetPropertyItem(DateTimeOriginalPropertyId);
                    string dateString = Encoding.ASCII.GetString(propItem.Value).TrimEnd('\0');

                    if (DateTime.TryParseExact(dateString, "yyyy:MM:dd HH:mm:ss",
                        null, System.Globalization.DateTimeStyles.None, out DateTime result))
                    {
                        return result;
                    }
                }

                // Fall back to DateTime (when image was last modified)
                const int DateTimePropertyId = 0x0132;
                if (image.PropertyIdList.Contains(DateTimePropertyId))
                {
                    PropertyItem propItem = image.GetPropertyItem(DateTimePropertyId);
                    string dateString = Encoding.ASCII.GetString(propItem.Value).TrimEnd('\0');

                    if (DateTime.TryParseExact(dateString, "yyyy:MM:dd HH:mm:ss",
                        null, System.Globalization.DateTimeStyles.None, out DateTime result))
                    {
                        return result;
                    }
                }

                // Fall back to DateTimeDigitized (when image was digitized)
                const int DateTimeDigitizedPropertyId = 0x9004;
                if (image.PropertyIdList.Contains(DateTimeDigitizedPropertyId))
                {
                    PropertyItem propItem = image.GetPropertyItem(DateTimeDigitizedPropertyId);
                    string dateString = Encoding.ASCII.GetString(propItem.Value).TrimEnd('\0');

                    if (DateTime.TryParseExact(dateString, "yyyy:MM:dd HH:mm:ss",
                        null, System.Globalization.DateTimeStyles.None, out DateTime result))
                    {
                        return result;
                    }
                }
            }
            catch (Exception)
            {
                // If any error occurs reading EXIF data, return null
                return null;
            }

            return null;
        }

        public static void OrderFiles(List<FileProcessResult> allResults, ReadmeFileHelper readme)
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

        public static void FindAuthor(List<FileProcessResult> allResults, ReadmeFileHelper readme)
        {
            foreach (var result in allResults)
            {
                var prefix = readme.FindSecondCsvValue(result.FileName);
                result.Author = prefix;
            }
        }
    }
}
