using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DnnSharp.FaqMaster.Core.DnnSf
{
    public class DateUtils
    {
        public static DateTime GetFileTime(string filePath, bool checkMetadataToo = true)
        {
            DateTime createTime = File.GetCreationTime(filePath);
            DateTime writeTime = File.GetLastWriteTime(filePath);

            var fileTime = writeTime < createTime ? createTime : writeTime;
            if (!checkMetadataToo)
                return fileTime;

            if (File.Exists(filePath + ".sbmetadata")) {
                DateTime metadataTime = DateUtils.GetFileTime(filePath + ".sbmetadata", false);
                if (metadataTime > fileTime)
                    fileTime = metadataTime;
            } else if (File.Exists(filePath + ".metadata")) {
                DateTime metadataTime = DateUtils.GetFileTime(filePath + ".metadata", false);
                if (metadataTime > fileTime)
                    fileTime = metadataTime;
            }

            return fileTime;
        }

    }
}
