using System;
using System.IO;
using System.IO.Compression;

namespace PageObjectModelCsharp.Util.Helpers
{
    /// <summary>
    /// Handles safe zipping of report folders for email delivery or archival.
    /// </summary>
    public static class ReportZipper
    {
        /// <summary>
        /// Zips the contents of the specified report folder into a timestamped archive.
        /// </summary>
        /// <param name="sourceFolder">The folder containing report files.</param>
        /// <returns>Full path to the created zip file.</returns>
        public static string ZipReportFolder(string sourceFolder)
        {
            if (!Directory.Exists(sourceFolder))
            {
                Console.WriteLine($"❌ Source folder not found: {sourceFolder}");
                return string.Empty;
            }

            string tempFolder = Path.Combine(Path.GetTempPath(), $"ReportTemp_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempFolder);

            try
            {
                // ✅ Copy files to temp folder
                foreach (var file in Directory.GetFiles(sourceFolder))
                {
                    string destFile = Path.Combine(tempFolder, Path.GetFileName(file));
                    File.Copy(file, destFile, true);
                }

                // ✅ Create zip with timestamp
                string zipName = $"ExecutionReport_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
                string zipPath = Path.Combine(sourceFolder, zipName);

                ZipFile.CreateFromDirectory(tempFolder, zipPath);
                Console.WriteLine($"📦 Created zip: {zipPath}");

                return zipPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to zip report folder: {ex.Message}");
                return string.Empty;
            }
            finally
            {
                // ✅ Clean up temp folder
                try
                {
                    Directory.Delete(tempFolder, true);
                }
                catch (Exception cleanupEx)
                {
                    Console.WriteLine($"⚠️ Failed to clean temp folder: {cleanupEx.Message}");
                }
            }
        }
    }
}