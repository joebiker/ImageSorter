namespace Allegory.POCOs
{
    public class FileProcessResult
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileNameMod { get; set; } = string.Empty;
        public string FileNameAuthor { get; set; } = string.Empty;
        public string FileNameOrdered { get; set; } = string.Empty;
        public string FileNameUndo { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime BestDate {
            // if DateTaken is available use that, 
            // otherwise, use the older of the two: Created or Modified.
            get { return DateTaken ?? (FileCreated < FileModified ? FileCreated : FileModified); }
        }
        public DateTime? DateTaken { get; set; }
        public DateTime FileCreated { get; set; }
        public DateTime FileModified { get; set; }
        public string Status { get; set; } = string.Empty; // "Exif", "NoExif", "Error"
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
