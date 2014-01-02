using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DnnSharp.FaqMaster.Core.DnnSf
{
    public static class DirectoryUtils
    {
        public static bool WasUpdatedSince(this DirectoryInfo dir, DateTime since)
        {
            // let's do the easy part first, check for last write time, which means new or deleted files
            if (since < dir.LastWriteTime)
                return true;

            // now the hard part, we need to check the date of the last modified file
            var lastFile = dir.GetFiles().OrderByDescending(f => f.LastWriteTimeUtc).FirstOrDefault();
            if (lastFile == null)
                return false;

            return since < lastFile.LastWriteTime;
        }

    }
}
