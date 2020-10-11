﻿using DarkUI.Forms;
using ModioX.Extensions;
using ModioX.Models.Resources;
using System;
using System.IO;
using System.Windows.Forms;

namespace ModioX.Forms
{
    public partial class BackupFilesWindow : DarkForm
    {
        public BackupFilesWindow()
        {
            InitializeComponent();
        }

        private void BackupManagerForm_Load(object sender, EventArgs e)
        {
            LoadBackupFiles();
        }

        private void LoadBackupFiles()
        {
            DgvBackups.Rows.Clear();

            foreach (BackupFile backupFile in MainWindow.SettingsData.BackupFiles)
            {
                string gameId = string.IsNullOrEmpty(backupFile.CategoryId) ? "n/a" : backupFile.CategoryId;

                if (File.Exists(backupFile.LocalPath))
                {
                    FileInfo fileInfo = new FileInfo(backupFile.LocalPath);
                    _ = DgvBackups.Rows.Add(backupFile.Name, MainWindow.Categories.GetCategoryById(gameId).Title, backupFile.FileName, fileInfo.Length.ToString("#,##0") + " bytes", string.Format("{0:g}", backupFile.CreatedDate));
                }
                else
                {
                    _ = DgvBackups.Rows.Add(backupFile.Name, MainWindow.Categories.GetCategoryById(gameId).Title, backupFile.FileName, "No File Exists", backupFile.CreatedDate.ToString());
                }
            }
        }

        private void DgvBackupFiles_SelectionChanged(object sender, EventArgs e)
        {
            if (DgvBackups.CurrentRow != null)
            {
                BackupFile backupFile = MainWindow.SettingsData.BackupFiles[DgvBackups.CurrentRow.Index];

                LabelName.Text = backupFile.Name;
                LabelGameTitle.Text = MainWindow.Categories.GetCategoryById(backupFile.CategoryId).Title;
                LabelCreatedDate.Text = string.Format("{0:g}", backupFile.CreatedDate);
                LabelFileName.Text = backupFile.FileName;
                LabelLocalPath.Text = backupFile.LocalPath;
                LabelConsolePath.Text = backupFile.InstallPath;

                if (!File.Exists(backupFile.LocalPath))
                {
                    LabelName.Text += " (File Doesn't Exist)";
                    _ = DarkMessageBox.Show(this, string.Format("Local file for {0} can't be found at path {1}.\n\nIf you have moved this file then edit the backup and choose the local file again, otherwise re-install your game update and re-backup the orginal game file.", backupFile.Name, backupFile.LocalPath), "No Local File", MessageBoxIcon.Warning);
                }
            }

            ToolItemEditBackup.Enabled = DgvBackups.CurrentRow != null;
            ToolItemDeleteBackup.Enabled = DgvBackups.CurrentRow != null;
            ToolItemBackupFile.Enabled = DgvBackups.CurrentRow != null && MainWindow.IsConsoleConnected;
            ToolItemRestoreFile.Enabled = DgvBackups.CurrentRow != null && MainWindow.IsConsoleConnected;
        }

        private void ToolItemEditBackup_Click(object sender, EventArgs e)
        {
            BackupFile backupFileIndex = MainWindow.SettingsData.BackupFiles[DgvBackups.CurrentRow.Index];
            BackupFile backupFile = DialogExtensions.ShowBackupFileDetails(this, backupFileIndex);

            if (backupFile != null)
            {
                backupFileIndex = backupFile;
            }

            LoadBackupFiles();
        }

        private void ToolItemDeleteBackup_Click(object sender, EventArgs e)
        {
            MainWindow.SettingsData.BackupFiles.RemoveAt(DgvBackups.CurrentRow.Index);
            LoadBackupFiles();
        }

        private void ToolItemBackupFile_Click(object sender, EventArgs e)
        {
            BackupFile backupFile = MainWindow.SettingsData.BackupFiles[DgvBackups.CurrentRow.Index];
            BackupGameFile(backupFile);
        }

        private void ToolItemRestoreFile_Click(object sender, EventArgs e)
        {
            BackupFile backupFile = MainWindow.SettingsData.BackupFiles[DgvBackups.CurrentRow.Index];
            RestoreGameFile(backupFile);
        }

        public void BackupGameFile(BackupFile backupFile)
        {
            try
            {
                FtpExtensions.DownloadFile(backupFile.LocalPath, backupFile.InstallPath);
                _ = DarkMessageBox.Show(this, string.Format("Successfully backed up {0} for file {1} from {2}", backupFile.Name, backupFile.FileName, backupFile.InstallPath), "Backup File Restored", MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Program.Log.Error("There was an issue attempting to backup game file.", ex);
                _ = DarkMessageBox.Show(this, string.Format("There was an issue when attempting to restore file to console. Make sure the local file path exists on your computer and that there isn't a typos", Path.GetFileName(backupFile.LocalPath), backupFile.InstallPath), "Backup File Restored", MessageBoxIcon.Error);
            }
        }

        public void RestoreGameFile(BackupFile backupFile)
        {
            try
            {
                if (!File.Exists(backupFile.LocalPath))
                {
                    _ = DarkMessageBox.Show(this, "This file backup doesn't exist on your computer. If your game doesn't have mods installed, then I would suggest you backup the original files.", "No Backup File", MessageBoxIcon.Information);
                    return;
                }

                FtpExtensions.UploadFile(backupFile.LocalPath, backupFile.InstallPath);
                _ = DarkMessageBox.Show(this, string.Format("Successfully restored {0} for file {1} to {2}", backupFile.Name, backupFile.FileName, backupFile.InstallPath), "Backup File Restored", MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Program.Log.Error("There was an issue attempting to restore game file.", ex);
                _ = DarkMessageBox.Show(this, string.Format("There was an issue attempting to restore game file. Make sure the local file path exists on your computer and that there isn't a typos", Path.GetFileName(backupFile.LocalPath), backupFile.InstallPath), "Backup File Restored", MessageBoxIcon.Error);
            }
        }
    }
}