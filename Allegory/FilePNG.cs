using System;
using ImageMagick;

namespace Allegory
{
    public static class FilePNG
    {
        public static POCOs.FileProcessResult ProcessPngFile(string filePath, bool suppressPrintMessageOfGood = false)
        {
            var result = new POCOs.FileProcessResult
            {
                FilePath = filePath,
                FileName = System.IO.Path.GetFileName(filePath),
                Status = "NoExif"
            };

            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                result.FileCreated = fileInfo.CreationTime;
                result.FileModified = fileInfo.LastWriteTime;

                // Try to get the date taken using MagickImage
                DateTime? dateTaken = GetDateTakenWithMagick(filePath);
                result.DateTaken = dateTaken;

                if (dateTaken.HasValue)
                {
                    result.Status = "Exif";
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
        /// Gets the DateTaken from metadata using MagickImage
        /// </summary>
        public static DateTime? GetDateTakenWithMagick(string filePath)
        {
            try
            {
                using (var image = new MagickImage(filePath))
                {
                    var exifProfile = image.GetExifProfile();
                    if (exifProfile == null)
                    {
                        return null;
                    }

                    // Try DateTimeOriginal first (when photo was taken)
                    var dateTakenValue = exifProfile.GetValue(ExifTag.DateTimeOriginal);
                    if (dateTakenValue != null && !string.IsNullOrWhiteSpace(dateTakenValue.Value))
                    {
                        string dateTakenString = dateTakenValue.Value;
                        if (DateTime.TryParseExact(dateTakenString, "yyyy:MM:dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out DateTime dateTaken))
                        {
                            return dateTaken;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // If any error occurs reading metadata, return null
                return null;
            }

            return null;
        }
    }
}
