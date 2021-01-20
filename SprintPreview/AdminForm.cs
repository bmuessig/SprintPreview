using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;

namespace SprintPreview
{
    public partial class AdminForm : Form
    {
        private bool revertChanges = false;
        private string sourcePath = "", targetPath = "", sourceHash = "", installPath = "";

        public AdminForm()
        {
            InitializeComponent();
        }

        private void AdminForm_Load(object sender, EventArgs e)
        {
            // Determine the location of the SprintLayout installation
            string path = GetLayoutPath(true);
            if (!Directory.Exists(path))
            {
                path = GetLayoutPath(false);
                if (!Directory.Exists(path))
                    path = "";
            }

            // Choose the path
            Choose(path);
        }

        /// <summary>
        /// Chooses a path and determines additional information about the installation.
        /// </summary>
        /// <param name="path"></param>
        private void Choose(string path)
        {
            // Sanitze the input
            if (string.IsNullOrWhiteSpace(path))
                path = "";

            // Set the UI strings
            pathBox.Text = path == "" ? "" : Path.GetFullPath(path);
            pathBox.Select(0, 0);

            // Check, if a backup exists
            revertChanges = File.Exists(Path.Combine(path, "layout60.bck"));

            // Adjust the paths for the application
            installPath = path;
            targetPath = Path.Combine(path, revertChanges ? "layout60.exe" : "layout60.bck");
            sourcePath = Path.Combine(path, revertChanges ? "layout60.bck" : "layout60.exe");

            // Check, if the binary exists
            if (!File.Exists(sourcePath))
            {
                versionLabel.Text = "None";
                patchButton.Text = "Unsupported";
                patchButton.Enabled = false;
                changeButton.Select();
                return;
            }

            // Determine the version by date
            DateTime creationTime = File.GetLastWriteTimeUtc(sourcePath);

            // Calculate the hash
            using (var cryptoProvider = new SHA1CryptoServiceProvider())
                sourceHash = BitConverter.ToString(cryptoProvider.ComputeHash(File.ReadAllBytes(sourcePath)));

            // Make the version label
            versionLabel.Text = string.Format("{0:D4}/{1:D2}/{2:D2} ({3})", creationTime.Year, creationTime.Month, creationTime.Day,
                sourceHash.Length >= 17 ? sourceHash.Substring(0, 17) : "invalid hash");

            // Determine compatibility
            if (!Patcher.IsCompatible(creationTime, sourceHash))
            {
                patchButton.Text = revertChanges ? "Invalid Backup" : "Unsupported";
                patchButton.Enabled = false;
                changeButton.Select();
                return;
            }

            // Activate the button
            patchButton.Text = revertChanges ? "Undo" : "Patch";
            patchButton.Enabled = true;
            patchButton.Select();
        }

        /// <summary>
        /// Returns the possible paths to SprintLayout 6.0.
        /// </summary>
        /// <param name="x86">Whether the x86 path is returned over the x64 path.</param>
        /// <returns>A possible path to SprintLayout.</returns>
        private string GetLayoutPath(bool x86)
        {
            return Path.Combine(Environment.GetFolderPath(x86 ? Environment.SpecialFolder.ProgramFilesX86 : Environment.SpecialFolder.ProgramFiles), "Layout60");
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void changeButton_Click(object sender, EventArgs e)
        {
            // Run the dialog
            if (folderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            // Choose the path
            Choose(folderDialog.SelectedPath);
        }

        private void patchButton_Click(object sender, EventArgs e)
        {
            // Sanity check the state
            if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(targetPath) || string.IsNullOrWhiteSpace(sourceHash) || string.IsNullOrWhiteSpace(installPath))
                return;

            // Check the hash and source file
            if (!File.Exists(sourcePath) || !Patcher.IsCompatible(sourceHash))
                return;

            // Disable the button
            patchButton.Enabled = false;
            Application.DoEvents();

            // Change the text of the cancel button
            cancelButton.Text = "Close";
            cancelButton.Select();

            // Check, if the changes should be reverted
            if (revertChanges)
            {
                // Perform the operations and store the result
                bool resultUndoPatching = Patcher.Copy(sourcePath, targetPath);
                bool resultUndoFiles = Patcher.RemoveFiles(sourceHash, installPath);

                // Check, if this was a success
                if (resultUndoPatching && resultUndoFiles)
                    MessageBox.Show(string.Format("Success, undoing changes in:\n{0}\n\nAll files were reverted/removed successfully!\nYou can use the regular version now!", installPath),
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show(string.Format("At least one error occurred.\n\nRestoring \"{0}\" {1}.\nDeleting files in {2} {3}.",
                        targetPath, resultUndoPatching ? "succeeded (you can use the program as usual)" : "failed (please re-install the software)",
                        installPath, resultUndoFiles ? "succeeded (the folder is clean now)" : "failed (you may have to clean up manually)"),
                        "Failure", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                // Re-do the UI and rescan the path
                Choose(installPath);
                return;
            }

            // Otherwise, make the changes
            // Perform the operations and store the result
            bool resultBackup = Patcher.Copy(sourcePath, targetPath);
            bool resultPatching = Patcher.Patch(sourceHash, sourcePath), resultFiles = Patcher.CreateFiles(sourceHash, installPath);

            // Check, if this was a success
            if (resultBackup && resultPatching && resultFiles)
                MessageBox.Show(string.Format("Success performing changes in:\n{0}\n\nPatching the application, adding all required files and making a backup succeeded!\nYou can use the unlocked version now - have fun!", installPath),
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show(string.Format("At least one error occurred.\n\nBacking up {0}\nPatching \"{1}\" {2}.\nCreating files in {3} {4}.",
                    resultBackup ? "succeeded (you can easily undo the changes" : "failed (you will have to re-install the software)",
                    targetPath, resultPatching ? "succeeded (the patch should mostly work)" : "failed (please retry)",
                    installPath, resultFiles ? "succeeded (please retry)" : "failed (you may want to retry)"),
                    "Failure", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            // Re-do the UI and rescan the path
            Choose(installPath);
        }
    }
}
