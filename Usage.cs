// How a host app (or a new app) uses SG.Core. Nothing here is required by the library.
using System;
using System.Data;
using MySql.Data.MySqlClient;                // the host app owns the database driver and credentials
using SG.Core.Catalog;
using SG.Core.Licensing;
using SG.Core.Media;
using SG.Core.Security;
using SG.Core.Storage;
using SG.Core.Sync;

static class Usage
{
    static void Demo(byte[] key1, byte[] key2)
    {
        // 1. wiring (once, on the UI thread)
        CoreLog.Sink = (msg, ex) => Console.WriteLine(msg + " " + ex);
        Func<IDbConnection> connect = () => new MySqlConnection("Server=...;Database=...;Uid=...;Pwd=...");
        var cipher = new DelegateCipher(s => Cryptography.ENC(s, key1, key2), s => Cryptography.DEC(s, key1, key2));
        var tables = new SgTables(connect, cipher);

        // 2. offline-first data
        var news = tables.News();
        news.LoadCache();                                  // instant, from news.dat
        news.Refreshed += (s, e) => Console.WriteLine(news.Items.Count + " news rows");
        news.BeginRefresh();                               // background fetch, saves news.dat

        // 3. slideshow
        var slides = tables.Slides();
        slides.LoadCache();
        slides.Refreshed += (s, e) =>
            FileMirror.Sync("slides", System.Linq.Enumerable.Select(
                System.Linq.Enumerable.Where(slides.Items, x => !x.Expired),
                x => new RemoteFile { Name = x.ID + ".jpg", Url = x.Image }));
        var player = new FolderSlideshow("slides");
        string nextImage = player.Next();                  // call from a Timer tick

        // 4. licensing
        var license = new LicenseManager("data.dat", new LicenseKeys(key2, key1), new DbSubscriberDirectory(connect));
        LicenseCheck check = license.CheckOffline();
        Console.WriteLine(check.Status + " " + check.Name);
    }
}
