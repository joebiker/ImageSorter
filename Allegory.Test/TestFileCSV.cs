using Allegory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Xunit;

namespace Allegory.Test
{
    public class TestFileCSV
    {
        [Fact]
        public void WriteAuditCsv_CreatesCsvFile_WithCorrectHeaderAndData()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            var results = new List<FileProcessResult>
            {
                new FileProcessResult
                {
                    FilePath = @"C:\Images\img1.jpg",
                    FileName = "img1.jpg",
                    FileNameMod = "img1_mod.jpg",
                    FileNameAuthor = "img1_author.jpg",
                    FileNameOrdered = "img1_ordered.jpg",
                    Author = "John Doe",
                    DateTaken = new DateTime(2024, 1, 2, 3, 4, 5),
                    FileCreated = new DateTime(2024, 1, 1, 0, 0, 0),
                    FileModified = new DateTime(2024, 1, 2, 0, 0, 0),
                    Status = "Exif",
                    ErrorMessage = ""
                }
            };

            string baseFileName = "TestOutput";

            // Act
            FileCSV.WriteAuditCsv(results, baseFileName, tempDir);

            // Assert
            string csvPath = Path.Combine(tempDir, baseFileName + ".csv");
            Assert.True(File.Exists(csvPath));

            var lines = File.ReadAllLines(csvPath);
            Assert.Equal("FilePath,FileName,FileNameMod,FileNameAuthor,FileNameOrdered,Author,BestDate,DateTaken,FileCreated,FileModified,Status,ErrorMessage", lines[0]);
            Assert.Contains("img1.jpg", lines[1]);

            // Cleanup
            Directory.Delete(tempDir, true);
        }

        [Fact]
        public void WriteAuditCsv_AppendsNumber_WhenFileExists()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            string baseFileName = "TestOutput";
            string firstFile = Path.Combine(tempDir, baseFileName + ".csv");
            File.WriteAllText(firstFile, "dummy");

            var results = new List<FileProcessResult>();

            // Act
            FileCSV.WriteAuditCsv(results, baseFileName, tempDir);

            // Assert
            string secondFile = Path.Combine(tempDir, baseFileName + "(1).csv");
            Assert.True(File.Exists(secondFile));

            // Cleanup
            Directory.Delete(tempDir, true);
        }

        [Fact]
        public void WriteAuditCsv_EscapesSpecialCharacters()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            var results = new List<FileProcessResult>
            {
                new FileProcessResult
                {
                    FilePath = "C:\\Images\\img,2.jpg",
                    FileName = "img\"2.jpg",
                    FileNameMod = "img\n2_mod.jpg",
                    FileNameAuthor = "",
                    FileNameOrdered = "",
                    Author = "",
                    DateTaken = null,
                    FileCreated = DateTime.Now,
                    FileModified = DateTime.Now,
                    Status = "",
                    ErrorMessage = ""
                }
            };

            string baseFileName = "TestOutput";

            // Act
            FileCSV.WriteAuditCsv(results, baseFileName, tempDir);

            // Assert
            string csvPath = Path.Combine(tempDir, baseFileName + ".csv");
            var lines = File.ReadAllLines(csvPath);
            Assert.Contains("\"C:\\Images\\img,2.jpg\"", lines[1]);
            Assert.Contains("\"img\"\"2.jpg\"", lines[1]);
            Assert.Contains("\"img\n2_mod.jpg\"", lines[1]);

            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }
}