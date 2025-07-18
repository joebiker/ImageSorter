using System;
using System.IO;
using MetadataExtractor;
using MetadataExtractor.Formats.QuickTime;

namespace ImageSorter
{
    public static class MOVManipulation
    {
        /// <summary>
        /// Processes a MOV file and returns a FileProcessResult.
        /// This is a basic stub; you can add metadata extraction as needed.
        /// </summary>
        public static FileProcessResult ProcessMovFile(string filePath)
        {
            var result = new FileProcessResult();
            result.FileName = Path.GetFileName(filePath);
            try
            {
                // Try to extract QuickTime creation date from MOV metadata
                var directories = MetadataExtractor.ImageMetadataReader.ReadMetadata(filePath);
                var quickTimeDir = directories.OfType<MetadataExtractor.Formats.QuickTime.QuickTimeMovieHeaderDirectory>().FirstOrDefault();
                var creationDate = quickTimeDir.GetDateTime(MetadataExtractor.Formats.QuickTime.QuickTimeMovieHeaderDirectory.TagCreated);
                result.FileCreationTime = File.GetCreationTime(filePath);
                if (creationDate != null)
                {
                    result.DateTaken = creationDate;
                    result.Status = "Exif";
                }
                else
                {
                    result.Status = "NoExif";
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
