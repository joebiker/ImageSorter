using System;
using MetadataExtractor;
using MetadataExtractor.Formats.QuickTime;
using System.Collections.Generic; // Added for List
using System.Linq; // Added for Where and FirstOrDefault

namespace Allegory
{
    public static class FileMOV
    {
        /// <summary>
        /// Processes a MOV file and returns a FileProcessResult.
        /// This is a basic stub; you can add metadata extraction as needed.
        /// </summary>
        public static POCOs.FileProcessResult ProcessMovFile(string filePath)
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
                result.FileCreated = fileInfo.CreationTime; // not as good as quickTimeDir.GetDateTime(MetadataExtractor.Formats.QuickTime.QuickTimeMovieHeaderDirectory.TagCreated);
                result.FileModified = fileInfo.LastWriteTime;

                // Try to extract QuickTime creation date from MOV metadata
                var directories = MetadataExtractor.ImageMetadataReader.ReadMetadata(filePath);
                var quickTimeDir = directories.OfType<MetadataExtractor.Formats.QuickTime.QuickTimeMovieHeaderDirectory>().FirstOrDefault();
                DateTime? creationDate = quickTimeDir.GetDateTime(MetadataExtractor.Formats.QuickTime.QuickTimeMovieHeaderDirectory.TagCreated);
                if (creationDate != null)
                {
                    result.DateTaken = creationDate;
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
        /// Updates MOV files' DateTaken by finding matching JPEG files with the same base name.
        /// For example, IMG_1234.MOV would look for IMG_1234.JPG or IMG_1234.JPEG
        /// </summary>
        /// <param name="files">List of FileProcessResult objects to process</param>
        public static void UpdateMovDatesFromJpeg(List<POCOs.FileProcessResult> files)
        {
            // Get all MOV files
            var movFiles = files.Where(f => 
                System.IO.Path.GetExtension(f.FileName).Equals(".mov", StringComparison.OrdinalIgnoreCase));

            foreach (var movFile in movFiles)
            {
                // Get base name without extension
                string baseName = System.IO.Path.GetFileNameWithoutExtension(movFile.FileName);
                
                // Look for matching JPEG file
                // TODO: add support for HEIC files?
                var matchingJpeg = files.FirstOrDefault(f => 
                    (System.IO.Path.GetExtension(f.FileName).Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                     System.IO.Path.GetExtension(f.FileName).Equals(".jpeg", StringComparison.OrdinalIgnoreCase)) &&
                    System.IO.Path.GetFileNameWithoutExtension(f.FileName).Equals(baseName, StringComparison.OrdinalIgnoreCase));

                // If matching JPEG found and it has DateTaken, copy it
                if (matchingJpeg != null && matchingJpeg.DateTaken.HasValue)
                {
                    movFile.DateTaken = matchingJpeg.DateTaken;
                    movFile.Status = "Jpeg";
                }
            }
        }
    }
} 