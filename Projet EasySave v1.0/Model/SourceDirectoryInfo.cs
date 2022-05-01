using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Projet_EasySave_v1._0
{
    //Static class used to calculate the number of files in directory, or the total size of a directory
    static class SourceDirectoryInfo
    {
        //Define the number of file in a specific directory
        static long filesSize= 0;

        //Define the total size of a specific directory
        static int nbFiles = 0;

        //Calculate the number of file in a directory using recursion (for complete save)
        static public int GetFilesNumberInSourceDirectory(DirectoryInfo _diSource)
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

        //Calculate the total size of a directory using recursion (for complete save)
        static public long GetSizeInSourceDirectory(DirectoryInfo _diSource)
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

        //Calculate the number of file in a directory using recursion (for differencial save)
        static public int DifferencialGetFilesNumberInSourceDirectory(DirectoryInfo _diSource, DirectoryInfo _diTarget)
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

        //Calculate the total size of a directory using recursion (for differencial save)
        static public long DifferencialGetSizeInSourceDirectory(DirectoryInfo _diSource, DirectoryInfo _diTarget)
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
}
