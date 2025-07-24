namespace ImageSorter
{
    public class FileProcessResult
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string FileNameMod { get; set; }
        public string Author { get; set; }
        public DateTime BestDate {
            // if DateTaken is available use that, 
            // otherwise, use the older of the two: Created or Modified.
            get { return DateTaken ?? (FileCreated < FileModified ? FileCreated : FileModified); }
        }
        public DateTime? DateTaken { get; set; }
        public DateTime FileCreated { get; set; }
        public DateTime FileModified { get; set; }
        public string Status { get; set; } // "Exif", "NoExif", "Error"
        public string ErrorMessage { get; set; }
    }
}
