using System;
using System.IO;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Model;

namespace Vertica.Integration.Domain.Core
{
    public class ArchiveFoldersStep : Step<MaintenanceWorkItem>
    {
        private readonly IArchiveService _archiver;

        public ArchiveFoldersStep(IArchiveService archiver)
        {
            _archiver = archiver;
        }

        public override Execution ContinueWith(MaintenanceWorkItem workItem, ITaskExecutionContext context)
        {
            if (workItem.Configuration.ArchiveFolders.GetEnabledFolders().Length == 0)
                return Execution.StepOver;

            return Execution.Execute;
        }

        public override void Execute(MaintenanceWorkItem workItem, ITaskExecutionContext context)
        {
            MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder[] folders =
                workItem.Configuration.ArchiveFolders.GetEnabledFolders();

            foreach (MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder folder in folders)
            {
                context.Log.Message("Folder: {0}", folder);

                FileInfo[] filesToArchive = folder.GetFiles() ?? new FileInfo[0];
                DirectoryInfo[] foldersToArchive = folder.GetFolders() ?? new DirectoryInfo[0];

                if (filesToArchive.Length == 0 && foldersToArchive.Length == 0)
                    continue;

                MaintenanceConfiguration.ArchiveFoldersConfiguration.Folder localFolder = folder;

                ArchiveCreated archive = _archiver.Archive(folder.ArchiveOptions.Name, a =>
                {
                    if (localFolder.ArchiveOptions.Expires.HasValue)
                        a.Options.ExpiresOn(localFolder.ArchiveOptions.Expires.Value);

                    a.Options.GroupedBy(localFolder.ArchiveOptions.GroupName);

                    foreach (FileInfo fileToArchive in filesToArchive)
                        a.IncludeFile(fileToArchive);

                    foreach (DirectoryInfo folderToArchive in foldersToArchive)
                        a.IncludeFolder(folderToArchive);
                });

                context.Log.Message("{0} file(s) and {1} folder(s) have been archived and will now be physically deleted. {2}.",
                    filesToArchive.Length, foldersToArchive.Length, archive);

                foreach (FileInfo fileToDelete in filesToArchive)
                {
                    try
                    {
                        fileToDelete.Delete();
                    }
                    catch (Exception ex)
                    {
                        context.Log.Error(folder.Target, $"Unable to delete file '{fileToDelete.FullName}'. Message: {ex.Message}");
                    }
                }

                foreach (DirectoryInfo folderToDelete in foldersToArchive)
                {
                    try
                    {
                        folderToDelete.Delete(recursive: true);
                    }
                    catch (Exception ex)
                    {
                        context.Log.Error(folder.Target, $"Unable to delete folder '{folderToDelete.FullName}'. Message: {ex.Message}");
                    }
                }
            }
        }

        public override string Description => "Archives files/folders based on configuration (MaintenanceConfiguration)";
    }
}