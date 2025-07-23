using System.Text.RegularExpressions;
using GroupDocs.Metadata;
using ImageMagick;

namespace ImageSorter
{
    public static class HEICManipulation
    {

        /// <summary>
        /// Extracts relevant metadata from a HEIC file and returns a FileProcessResult.
        /// JVP Note: Did not pick up every file Date Taken, unknown why.
        /// From: https://products.groupdocs.com/metadata/net/extract/heic/
        /// Limited to 15 files for evaluation. 
        /// </summary>
        public static FileProcessResult GetMetaDataWithGroupDocs(string filePath)
        {
            var result = new FileProcessResult
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                Status = "NoExif"
            };

            // Always get file creation time
            FileInfo fileInfo = new FileInfo(filePath);
            result.FileCreationTime = fileInfo.CreationTime;

            try
            {
                using (var metadata = new Metadata(filePath))
                {
                    // Try to get a datetime property (preferably named with 'date')
                    const string pattern = @"date";
                    var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                    var dateProperty = metadata.FindProperties(
                        p => regex.IsMatch(p.Name) &&
                        p.Value.Type == GroupDocs.Metadata.Common.MetadataPropertyType.DateTime)
                        .FirstOrDefault();

                    if (dateProperty != null)
                    {
                        result.DateTaken = dateProperty.Value.ToStruct(DateTime.MinValue);
                        result.Status = "Exif";
                    }
                    else
                    {
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

        /// <summary>
        /// Retrieves the 'Date Taken' (EXIF DateTimeOriginal) metadata using Magick.NET with error handling.
        /// Returns null if not found or on error.
        /// </summary>
        public static FileProcessResult GetDateTakenWithMagickNet(string path)
        {
            var result = new FileProcessResult
            {
                FilePath = path,
                FileName = System.IO.Path.GetFileName(path),
                Status = "NoExif"
            };

            // Always get file creation time
            var fileInfo = new System.IO.FileInfo(path);
            result.FileCreationTime = fileInfo.CreationTime;

            try
            {
                using (var image = new MagickImage(path))
                {
                    var exifProfile = image.GetExifProfile();
                    if (exifProfile == null)
                    {
                        Console.WriteLine($"No EXIF profile found in file: {path}");
                        result.Status = "NoExif";
                        return result;
                    }
                    var dateTakenValue = exifProfile.GetValue(ImageMagick.ExifTag.DateTimeOriginal);
                    if (dateTakenValue == null || string.IsNullOrWhiteSpace(dateTakenValue.Value))
                    {
                        Console.WriteLine($"No DateTimeOriginal tag found in file: {path}");
                        result.Status = "NoExif";
                        return result;
                    }
                    string dateTakenString = dateTakenValue.Value;
                    // EXIF date format is usually "yyyy:MM:dd HH:mm:ss"
                    if (DateTime.TryParseExact(dateTakenString, "yyyy:MM:dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out DateTime dateTaken))
                    {
                        result.DateTaken = dateTaken;
                        result.Status = "Exif";
                    }
                    else
                    {
                        Console.WriteLine($"Failed to parse DateTimeOriginal '{dateTakenString}' in file: {path}");
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
    }
}
