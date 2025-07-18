namespace ImageSorter
{
    public class FileProcessResult
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string FileNameMod { get; set; }
        public string Author { get; set; }
        public DateTime BestDate {
            get { return DateTaken ?? FileCreationTime; }
        }
        public DateTime? DateTaken { get; set; }
        public DateTime FileCreationTime { get; set; }
        public string Status { get; set; } // "Exif", "NoExif", "Error"
        public string ErrorMessage { get; set; }
    }
}
