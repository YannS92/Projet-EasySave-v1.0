using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace EasySave_2._0
{
    //calculate the number of files,total size
    static class Information
    {

        //Define the number of file in a specific directory
        private static long filesSize= 0;

        //Define the total size of a specific directory
        private static int nbFiles = 0;


         
        /// Get Files number in a directory
         
        /// <param name="_diSource">DirectoryInfo object of the directory</param>
        /// <returns></returns>
        public static int CompleteFilesNumber(DirectoryInfo _diSource)
        {
            lock (Model.sync)
            {
                int temp = GetFilesNumberInSourceDirectory(_diSource);
                Reset();
                return temp;
            }
        }

        /// Get Size of all files in a directory

        /// <param name="_diSource">DirectoryInfo object of the directory</param>
        /// <returns></returns>
        public static long CompleteSize(DirectoryInfo _diSource)
        {
            lock (Model.sync)
            {
                long temp = GetSizeInSourceDirectory(_diSource);
                Reset();
                return temp;
            }
        }

        /// Calculate the number of file in a directory using recursion (for complete save)

        /// <param name="_diSource">DirectoryInfo object of the directory</param>
        /// <returns></returns>
        static private int GetFilesNumberInSourceDirectory(DirectoryInfo _diSource)
        {
            lock (Model.sync)
            {
                foreach (FileInfo fi in _diSource.GetFiles())
                {
                    nbFiles++;
                }
                foreach (DirectoryInfo diSourceSubDir in _diSource.GetDirectories())
                {
                    GetFilesNumberInSourceDirectory(diSourceSubDir);
                }

                return nbFiles;
            }
        }

         
        /// Calculate the total size of a directory using recursion (for complete save)
         
        /// <param name="_diSource">DirectoryInfo object of the directory</param>
        /// <returns></returns>
        static private long GetSizeInSourceDirectory(DirectoryInfo _diSource)
        {
            lock (Model.sync)
            {
                foreach (FileInfo fi in _diSource.GetFiles())
                {
                    filesSize += fi.Length;
                }
                foreach (DirectoryInfo diSourceSubDir in _diSource.GetDirectories())
                {
                    GetSizeInSourceDirectory(diSourceSubDir);
                }

                return filesSize;
            }
        }

         
        /// Get the number of all files in a directory (following differncial procedure)
         
        /// <param name="_diSource">DirectoryInfo object of the source directory</param>
        /// <param name="_diTarget">DirectoryInfo object of the target directory</param>
        /// <returns></returns>
        public static int DifferencialFilesNumber(DirectoryInfo _diSource, DirectoryInfo _diTarget)
        {
            lock (Model.sync)
            {
                int temp = DifferencialGetFilesNumberInSourceDirectory(_diSource, _diTarget);
                Reset();
                return temp;
            }
        }

        /// Get Size of all files in a directory (following differncial procedure)

        /// <param name="_diSource">DirectoryInfo object of the source directory</param>
        /// <param name="_diTarget">DirectoryInfo object of the target directory<</param>
        /// <returns></returns>
        public static long DifferencialSize(DirectoryInfo _diSource, DirectoryInfo _diTarget)
        {
            lock (Model.sync)
            {
                long temp = DifferencialGetSizeInSourceDirectory(_diSource, _diTarget);
                Reset();
                return temp;
            }
        }

        /// Get number of all files in a directory (following differncial procedure)

        /// <param name="_diSource">DirectoryInfo object of the source directory</param>
        /// <param name="_diTarget">DirectoryInfo object of the target directory</param>
        /// <returns></returns>
        static private int DifferencialGetFilesNumberInSourceDirectory(DirectoryInfo _diSource, DirectoryInfo _diTarget)
        {
            lock (Model.sync)
            {
                Directory.CreateDirectory(_diTarget.FullName);

                // Count each file from the parent directory.
                foreach (FileInfo fi in _diSource.GetFiles())
                {

                    string targetPath = Path.Combine(_diTarget.FullName, fi.Name);

                    if (!File.Exists(targetPath) || fi.LastWriteTime != File.GetLastWriteTime(targetPath))
                    {
                        nbFiles++;
                    }


                }

                // Enter each subdirectory and count the files in them using recursion.
                foreach (DirectoryInfo diSourceSubDir in _diSource.GetDirectories())
                {
                    string targetDirectoryPath = Path.Combine(_diTarget.FullName, diSourceSubDir.Name);

                    if (!Directory.Exists(targetDirectoryPath))
                    {
                        GetFilesNumberInSourceDirectory(diSourceSubDir);
                    }
                    else
                    {
                        DirectoryInfo nextTargetSubDir = new DirectoryInfo(targetDirectoryPath);
                        DifferencialGetFilesNumberInSourceDirectory(diSourceSubDir, nextTargetSubDir);
                    }

                }

                return nbFiles;
            }
        }

         
        /// Calculate the total size of a directory using recursion (for differencial save)
         
        /// <param name="_diSource">DirectoryInfo object of the source directory</param>
        /// <param name="_diTarget">DirectoryInfo object of the target directory</param>
        /// <returns></returns>
        static private long DifferencialGetSizeInSourceDirectory(DirectoryInfo _diSource, DirectoryInfo _diTarget)
        {
            lock (Model.sync)
            {
                Directory.CreateDirectory(_diTarget.FullName);

                //// Count the size of each file from the parent directory.
                foreach (FileInfo fi in _diSource.GetFiles())
                {

                    string targetPath = Path.Combine(_diTarget.FullName, fi.Name);

                    if (!File.Exists(targetPath) || fi.LastWriteTime != File.GetLastWriteTime(targetPath))
                    {
                        filesSize += fi.Length;
                    }


                }

                // Enter each subdirectory and count the size of all the files in them using recursion.
                foreach (DirectoryInfo diSourceSubDir in _diSource.GetDirectories())
                {
                    string targetDirectoryPath = Path.Combine(_diTarget.FullName, diSourceSubDir.Name);

                    if (!Directory.Exists(targetDirectoryPath))
                    {
                        GetSizeInSourceDirectory(diSourceSubDir);
                    }
                    else
                    {
                        DirectoryInfo nextTargetSubDir = new DirectoryInfo(targetDirectoryPath);
                        DifferencialGetSizeInSourceDirectory(diSourceSubDir, nextTargetSubDir);
                    }

                }

                return filesSize;
            }
        }

         
        /// Reset private parameters
         
        private static void Reset()
        {
            lock (Model.sync)
            {
                nbFiles = 0;
                filesSize = 0;
            }
        }


        /// Check if the Sofware is launched.

        /// <param name="_processName">The name of the process you want to check</param>
        /// <returns></returns>
        public static bool CheckIfSoftwareIsLaunched(string _processName)
        {
            lock (Model.sync)
            {
                if (_processName != "")
                {
                    bool softwareIsLaunched;

                    // Check if the Sofware (Calculator for testing purpose) is launched
                    if (Process.GetProcessesByName(_processName).Length == 0)
                    {
                        // The software isn't launched
                        softwareIsLaunched = false;
                    }
                    else
                    {
                        // The software is launched
                        softwareIsLaunched = true;
                    }
                    return softwareIsLaunched;
                }
                else
                {
                    return false;
                }
            }
        }
        /// Return in a string the extension of a specified file.

        /// <param name="targetFile">FileInfo of the target file</param>
        /// <returns></returns>
        public static string GetFileExtension(FileInfo targetFile)
        {
            lock (Model.sync)
            {
                string fileExtension = targetFile.Extension;
                return fileExtension;
            }
        }

    }
}
