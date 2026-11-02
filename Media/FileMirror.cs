using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace SG.Core.Media
{
    public sealed class RemoteFile
    {
        /// <summary>Local file name, e.g. "12.jpg".</summary>
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public sealed class MirrorResult
    {
        public int Downloaded, AlreadyPresent, Deleted, Failed;
    }

    /// <summary>
    /// Makes a local folder match a list of remote files. Replaces the two copies of the WebClient
    /// download loop in Main (Worker2_DoWork and Worker2_RunWorkerCompleted).
    /// </summary>
    public static class FileMirror
    {
        /// <summary>
        /// Downloads missing files and (optionally) removes local files that are no longer wanted.
        /// Unlike "delete everything, then download", the old slides stay on screen if the download fails.
        /// Each file is downloaded to a temp name first, so a dropped connection never leaves a broken image.
        /// </summary>
        public static MirrorResult Sync(string folder, IEnumerable<RemoteFile> wanted, bool deleteStale = true, string userAgent = "SG.Core")
        {
            var result = new MirrorResult();
            Directory.CreateDirectory(folder);

            var list = (wanted ?? Enumerable.Empty<RemoteFile>()).Where(f => !string.IsNullOrWhiteSpace(f.Name)).ToList();
            var keep = new HashSet<string>(list.Select(f => f.Name), StringComparer.OrdinalIgnoreCase);

            using (var client = new WebClient())
            {
                client.Headers.Add("user-agent", userAgent);

                foreach (RemoteFile f in list)
                {
                    string target = Path.Combine(folder, Path.GetFileName(f.Name)); // Path.GetFileName blocks "../" tricks
                    if (File.Exists(target)) { result.AlreadyPresent++; continue; }

                    string tmp = target + ".part";
                    try
                    {
                        client.DownloadFile(f.Url, tmp);
                        File.Move(tmp, target);
                        result.Downloaded++;
                    }
                    catch (Exception ex)
                    {
                        CoreLog.Write("FileMirror download failed: " + f.Url, ex);
                        try { if (File.Exists(tmp)) File.Delete(tmp); } catch { }
                        result.Failed++;
                    }
                }
            }

            if (deleteStale)
            {
                foreach (string path in Directory.GetFiles(folder))
                {
                    string name = Path.GetFileName(path);
                    if (name.EndsWith(".part", StringComparison.OrdinalIgnoreCase) || keep.Contains(name)) continue;
                    try { File.Delete(path); result.Deleted++; }
                    catch (Exception ex) { CoreLog.Write("FileMirror delete failed: " + path, ex); }
                }
            }
            return result;
        }
    }
}
