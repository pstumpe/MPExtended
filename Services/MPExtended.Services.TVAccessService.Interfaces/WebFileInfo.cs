﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MPExtended.Services.TVAccessService.Interfaces
{
    public class WebFileInfo
    {
        public WebFileInfo()
        {
            LastAccessTime = new DateTime(1970, 1, 1);
            LastModifiedTime = new DateTime(1970, 1, 1);
            Exists = false;
        }

        public WebFileInfo(FileInfo info)
            : this()
        {
            if (info != null)
            {
                IsLocalFile = true;
                Size = info.Length;
                Name = info.Name;
                Path = info.FullName;
                LastAccessTime = info.LastAccessTime;
                LastModifiedTime = info.LastWriteTime;
                Extension = info.Extension;
                IsReadOnly = info.IsReadOnly;
                Exists = true;
            }
        }

        public WebFileInfo(string path)
            : this(File.Exists(path) ? new FileInfo(path) : null)
        {
        }

        public bool IsLocalFile { get; set; }
        public long Size { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime LastAccessTime { get; set; }
        public DateTime LastModifiedTime { get; set; }
        public string Extension { get; set; }
        public bool IsReadOnly { get; set; }
        public bool Exists { get; set; }

        public override string ToString()
        {
            return Path;
        }
    }
}
