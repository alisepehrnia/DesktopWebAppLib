using System;
using System.IO;
using System.Linq;

namespace SG.Core.Media
{
    /// <summary>
    /// Cycles through the files of a folder. Replaces Main.Slide()/CheckSlides(), which re-listed the
    /// directory with ad-hoc index math on every tick.
    /// </summary>
    public sealed class FolderSlideshow
    {
        private readonly string _folder;
        private readonly string _pattern;
        private int _index = -1;

        public FolderSlideshow(string folder, string searchPattern = "*.jpg")
        {
            if (string.IsNullOrWhiteSpace(folder)) throw new ArgumentException("folder required", "folder");
            _folder = folder;
            _pattern = searchPattern;
        }

        public string Folder { get { return _folder; } }

        /// <summary>Path of the slide returned by the last Next(), or null.</summary>
        public string Current { get; private set; }

        /// <summary>File name without extension (the old code used this as the slide ID).</summary>
        public string CurrentName { get { return Current == null ? null : Path.GetFileNameWithoutExtension(Current); } }

        public bool HasItems { get { return List().Length > 0; } }

        /// <summary>Advances and returns the next slide path (wrapping around), or null if the folder is empty.</summary>
        public string Next()
        {
            string[] files = List();
            if (files.Length == 0) { Current = null; _index = -1; return null; }

            _index = (_index + 1) % files.Length;
            Current = files[_index];
            return Current;
        }

        private string[] List()
        {
            if (!Directory.Exists(_folder)) return new string[0];
            return Directory.GetFiles(_folder, _pattern).OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToArray();
        }
    }
}
