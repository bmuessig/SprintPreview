using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SprintPreview
{
    /// <summary>
    /// Patcher class.
    /// </summary>
    public static class Patcher
    {
        /// <summary>
        /// Hash 2020/02/11.
        /// </summary>
        private const string HASH_2020_02_11 = "58-8D-0E-E6-06-BC-F6-E9-66-01-80-26-9A-6A-39-DE-34-8F-6D-81";

        /// <summary>
        /// Hash 2021/01/19.
        /// </summary>
        private const string HASH_2021_01_19 = "F1-F8-F0-45-E0-4D-0E-59-A1-48-06-D4-BF-86-47-48-6C-60-A7-66";

        /// <summary>
        /// Returns, whether the file is supported for patching.
        /// </summary>
        /// <param name="modTimeUTC">The file modification time (UTC).</param>
        /// <param name="hash">The sha1 file hash string.</param>
        /// <returns>Whether the file is compatible.</returns>
        public static bool IsCompatible(DateTime modTimeUTC, string hash)
        {
            return modTimeUTC.Year == 2020 && modTimeUTC.Month == 2 && modTimeUTC.Day == 11 && hash == HASH_2020_02_11 ||
                modTimeUTC.Year == 2021 && modTimeUTC.Month == 1 && modTimeUTC.Day == 19 && hash == HASH_2021_01_19;
        }

        /// <summary>
        /// Returns, whether the file is supported for patching.
        /// </summary>
        /// <param name="hash">The sha1 file hash string.</param>
        /// <returns>Whether the file is compatible.</returns>
        public static bool IsCompatible(string hash)
        {
            return hash == HASH_2020_02_11 || hash == HASH_2021_01_19;
        }

        /// <summary>
        /// Patches a file with a given hash.
        /// </summary>
        /// <param name="hash">The hash of the file.</param>
        /// <param name="path">The target file path.</param>
        /// <returns>Whether the patching process succeeded.</returns>
        public static bool Patch(string hash, string path)
        {
            // Check the hash against the currently single version
            if (hash != HASH_2020_02_11 && hash != HASH_2021_01_19)
                return false;

            // Make sure the file exists
            if (!File.Exists(path))
                return false;

            // Try to patch the file
            try
            {
                // Open the file exclusively
                using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Write, FileShare.None))
                {
                    // Seek to the beginning of the first change and make the changes
                    fs.Position = hash == HASH_2020_02_11 ? 0x2b6d92 : 0x2b7dba;
                    fs.Write(new byte[] { 0x43, 0x68, 0x65, 0x63, 0x6b, 0x65, 0x64 }, 0, 7);

                    // Seek to the beginning of the second change and make the changes
                    fs.Position = hash == HASH_2020_02_11 ? 0x2b6df1 : 0x2b7e19;
                    fs.Write(new byte[] { 0x43, 0x68, 0x65, 0x63, 0x6b, 0x65, 0x64 }, 0, 7);

                    if (hash == HASH_2020_02_11)
                    {
                        // Seek to the beginning of the third change and make the changes
                        fs.Position = 0x2c18ce;
                        fs.Write(new byte[] { 0x32, 0x30 }, 0, 2);

                        // Seek to the beginning of the forth change and make the changes
                        fs.Position = 0x316aef;
                        fs.Write(new byte[] { 0x32, 0x30 }, 0, 2);
                    } else {
                        // Seek to the beginning of the third change and make the changes
                        fs.Position = 0x2c2976;
                        fs.Write(new byte[] { 0x32, 0x31 }, 0, 2);

                        // Seek to the beginning of the forth change and make the changes
                        fs.Position = 0x317a9f;
                        fs.Write(new byte[] { 0x32, 0x31 }, 0, 2);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Copies files, preserving date and time.
        /// </summary>
        /// <param name="sourcePath">The source file path.</param>
        /// <param name="targetPath">The target file path.</param>
        /// <returns>Whether the copying succeeded.</returns>
        public static bool Copy(string sourcePath, string targetPath)
        {
            // Check the paths
            if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(targetPath) ||
                !File.Exists(sourcePath))
                return false;

            // Try to copy the file
            try
            {
                File.Copy(sourcePath, targetPath, true);
                DateTime creation = File.GetCreationTimeUtc(sourcePath), modification = File.GetLastWriteTimeUtc(sourcePath);
                File.SetCreationTimeUtc(targetPath, creation);
                File.SetLastWriteTimeUtc(targetPath, modification);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates new support files for the given hash.
        /// </summary>
        /// <param name="hash">The file hash.</param>
        /// <param name="path">The base path for the files.</param>
        /// <returns>Whether the creation succeeded.</returns>
        public static bool CreateFiles(string hash, string path)
        {
            // Check the hash against the currently single version
            if (hash != HASH_2020_02_11 && hash != HASH_2021_01_19)
                return false;

            // Attempt to create the files
            try
            {
                // TODO: Add files
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Removes all support files to the given hash.
        /// </summary>
        /// <param name="hash">The file hash.</param>
        /// <param name="path">The base path for the files.</param>
        /// <returns>Whether the removal succeeded.</returns>
        public static bool RemoveFiles(string hash, string path)
        {
            // Check the hash against the currently single version
            if (hash != HASH_2020_02_11 && hash != HASH_2021_01_19)
                return false;

            // Attempt to remove the files
            try
            {
                // TODO: Add files
                string file = Path.Combine(path, "layout60.bck");
                if (File.Exists(file))
                    File.Delete(file);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
