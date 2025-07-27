using System.Text.RegularExpressions;
using ImageMagick;

namespace Allegory
{
    public static class FileHEIC
    {

        /// <summary>
        /// Retrieves the 'Date Taken' (EXIF DateTimeOriginal) metadata using Magick.NET with error handling.
        /// Returns null if not found or on error.
        /// </summary>
        public static POCOs.FileProcessResult GetDateTakenWithMagickNet(string path)
        {
            var result = new POCOs.FileProcessResult
            {
                FilePath = path,
                FileName = System.IO.Path.GetFileName(path),
                Status = "NoExif"
            };

            try
            {
                FileInfo fileInfo = new FileInfo(path);
                result.FileCreated = fileInfo.CreationTime;
                result.FileModified = fileInfo.LastWriteTime;
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