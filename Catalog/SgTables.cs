using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SG.Core.Models;
using SG.Core.Sources;
using SG.Core.Storage;
using SG.Core.Sync;

namespace SG.Core.Catalog
{
    /// <summary>
    /// Every table the Salam Gomrok kiosk reads, as one factory method each (table name, column mapping and
    /// offline file name copied from SGLib/Get.cs and the call sites). Replaces the ~20 Get* methods.
    /// Create the returned SyncedTable on the UI thread.
    /// </summary>
    public sealed class SgTables
    {
        private readonly Func<IDbConnection> _connect;
        private readonly ICipher _cipher;
        private readonly string _dir;

        /// <param name="connect">Opens a connection to the Salam Gomrok database (credentials stay in the host app).</param>
        /// <param name="cipher">Cipher for the offline .dat files; pass the same keys the old app used to keep reading old files.</param>
        /// <param name="cacheDirectory">Folder for .dat files; "" = current directory like the old app.</param>
        public SgTables(Func<IDbConnection> connect, ICipher cipher, string cacheDirectory = "")
        {
            if (connect == null) throw new ArgumentNullException("connect");
            _connect = connect; _cipher = cipher; _dir = cacheDirectory ?? "";
        }

        private SyncedTable<T> Make<T>(string table, string file, Func<IDataRecord, T> map, string suffix = "",
                                       Func<IList<T>, IList<T>> post = null)
        {
            return new SyncedTable<T>(
                new DbTableSource<T>(_connect, table, map, "*", suffix),
                new LocalTable<T>(System.IO.Path.Combine(_dir, file), _cipher), post);
        }

        /// <summary>Table "slide1" cached in slide1.dat.</summary>
        public SyncedTable<SGSlide> Slides()
        {
            return Make("slide1", "slide1.dat", r => new SGSlide
            {
                ID = r.Int("id"),
                Subject = r.Str("mozoh"),
                StartDate = r.Str("tarikhshoroh"),
                EndDate = r.Str("tarikhpayan"),
                Image = r.Str("file"),
                Link = r.Str("link")
            }, "",
                rows => { foreach (var s in rows) s.Expired = PersianDate.IsExpired(s.EndDate, DateTime.Today); return rows; });
        }

        /// <summary>Table "news" cached in news.dat.</summary>
        public SyncedTable<SGNews> News()
        {
            return Make("news", "news.dat", r => new SGNews
            {
                ID = r.Int("id"),
                Date = r.Str("tarikh"),
                Organization = r.Str("sazeman"),
                Subject = r.Str("mozoh"),
                Description = r.Str("sharh"),
                User = r.Str("user")
            }, "ORDER BY ID DESC");
        }

        /// <summary>Table "amozesh" cached in amozesh.dat.</summary>
        public SyncedTable<SGEducation> Education()
        {
            return Make("amozesh", "amozesh.dat", r => new SGEducation
            {
                ID = r.Int("id"),
                Chapter = r.Str("fasl"),
                Subject = r.Str("mozoh"),
                Description = r.Str("sharh"),
                Attachment = r.Str("zamaimeh")
            }, "ORDER BY ID DESC");
        }

        /// <summary>Table "PAP" cached in PAP.dat.</summary>
        public SyncedTable<SGQ> Questions()
        {
            return Make("PAP", "PAP.dat", r => new SGQ
            {
                ID = r.Int("id"),
                FromID = r.Int("fromId"),
                Subject = r.Str("mozo"),
                Description = r.Str("tozihat"),
                ImageUrl = r.Str("imgUrl"),
                Date = r.Str("date")
            }, "ORDER BY ID DESC");
        }

        /// <summary>Table "PapCm" cached in PapCm.dat.</summary>
        public SyncedTable<SGA> Answers()
        {
            return Make("PapCm", "PapCm.dat", r => new SGA
            {
                ID = r.Int("id"),
                FromID = r.Int("fromId"),
                ToID = r.Int("toPId"),
                Answer = r.Str("pasokh"),
                Date = r.Str("date")
            });
        }

        /// <summary>Table "bakhshnameh" cached in bakhshnameh.dat.</summary>
        public SyncedTable<SGLaw> Laws()
        {
            return Make("bakhshnameh", "bakhshnameh.dat", r => new SGLaw
            {
                ID = r.Int("id"),
                Code = r.Str("shomarh"),
                Date = r.Str("tarikh"),
                Subject = r.Str("mozoh"),
                Description = r.Str("sharh"),
                Attachment = r.Str("zamimeUrl"),
                From = r.Str("fromUser")
            }, "ORDER BY ID DESC");
        }

        /// <summary>Table "tarefeh" cached in tarefeh.dat.</summary>
        public SyncedTable<SGTariff> Tariffs()
        {
            return Make("tarefeh", "tarefeh.dat", r => new SGTariff
            {
                ID = r.Int("id"),
                HS = r.Str("HS"),
                Description = r.Str("SHARH"),
                Reference = r.Str("MAKHAZ"),
                English = r.Str("EN"),
                Measure = r.Str("SANJESH")
            });
        }

        /// <summary>Table "SharhFasl" cached in SharhFasl.dat.</summary>
        public SyncedTable<SGChapterDescription> ChapterDescriptions()
        {
            return Make("SharhFasl", "SharhFasl.dat", r => new SGChapterDescription
            {
                ID = r.Int("id"),
                HS = r.Str("hs"),
                Description = r.Str("sharh")
            });
        }

        /// <summary>Table "MOJAVEZ1" cached in MOJAVEZ1.dat.</summary>
        public SyncedTable<SGLicense> Licenses()
        {
            return Make("MOJAVEZ1", "MOJAVEZ1.dat", r => new SGLicense
            {
                ID = r.Int("id"),
                HS = r.Str("HS"),
                Licenses = r.Str("mojavez")
            });
        }

        /// <summary>Table "listmojavez" cached in listmojavez.dat.</summary>
        public SyncedTable<SGLicenseList> LicenseList()
        {
            return Make("listmojavez", "listmojavez.dat", r => new SGLicenseList
            {
                ID = r.Int("id"),
                Name = r.Str("name")
            });
        }

        /// <summary>Table "olaviat" cached in olaviat.dat.</summary>
        public SyncedTable<SGPriority> Priorities()
        {
            return Make("olaviat", "olaviat.dat", r => new SGPriority
            {
                ID = r.Int("id"),
                HS = r.Str("hscod"),
                Priority = r.Str("olaviat")
            });
        }

        /// <summary>Table "sherkat" cached in sherkat.dat.</summary>
        public SyncedTable<SGCompany> Companies()
        {
            return Make("sherkat", "sherkat.dat", r => new SGCompany
            {
                ID = r.Int("id"),
                HS = r.Str("hs"),
                Company = r.Str("sherkat"),
                Link = r.Str("link")
            });
        }

        /// <summary>Table "saderatESTANDARD" cached in saderatESTANDARD.dat.</summary>
        public SyncedTable<SGExportStandard> ExportStandards()
        {
            return Make("saderatESTANDARD", "saderatESTANDARD.dat", r => new SGExportStandard
            {
                ID = r.Int("ID"),
                Description = r.Str("sharh"),
                Standard = r.Str("estandard")
            });
        }

        /// <summary>Table "ArzeshSaderat" cached in ArzeshSaderat.dat.</summary>
        public SyncedTable<SGExportPrice> ExportPrices()
        {
            return Make("ArzeshSaderat", "ArzeshSaderat.dat", r => new SGExportPrice
            {
                ID = r.Int("id"),
                HS = r.Str("Hscod"),
                Description = r.Str("sharh"),
                Brand = r.Str("mark"),
                Unit = r.Str("vahed"),
                Currency = r.Str("noarz"),
                Price = r.Str("arzesh"),
                PriceKind = r.Str("noghimat"),
                Paper = r.Str("mosavebeh"),
                PaperDate = r.Str("tarikhmosavebeh")
            });
        }

        /// <summary>Table "TSC" cached in TSC.dat.</summary>
        public SyncedTable<SGTSC> Tsc()
        {
            return Make("TSC", "TSC.dat", r => new SGTSC
            {
                ID = r.Int("id"),
                HS = r.Int("HS"),
                Description = r.Str("SHARH"),
                Price = r.Str("ARZESH"),
                Country = r.Str("KESVAR"),
                Brand = r.Str("BERAND"),
                Model = r.Str("MODEL"),
                Unit = r.Str("VAHED"),
                BuyKind = r.Str("nokharid"),
                Reference = r.Str("makhaz"),
                Identifier = r.Str("SHENASEH1")
            });
        }

        /// <summary>Table "yadaki" cached in yadaki.dat.</summary>
        public SyncedTable<SGSpare> Spares()
        {
            return Make("yadaki", "yadaki.dat", r => new SGSpare
            {
                ID = r.Int("id"),
                Spare1ID = r.Str("yadaki1_id"),
                Spare2ID = r.Str("yadaki2_id"),
                Spare3ID = r.Str("yadaki3_id"),
                PartNumber = r.Str("part_no"),
                PartName = r.Str("part_name"),
                Description = r.Str("part_p"),
                Price = r.Str("arzesh")
            });
        }

        /// <summary>Table "yadaki_1" cached in yadaki_1.dat.</summary>
        public SyncedTable<SGSpare1> Spares1()
        {
            return Make("yadaki_1", "yadaki_1.dat", r => new SGSpare1
            {
                Spare1ID = r.Int("id"),
                Type = r.Str("no")
            });
        }

        /// <summary>Table "yadaki_2" cached in yadaki_2.dat.</summary>
        public SyncedTable<SGSpare2> Spares2()
        {
            return Make("yadaki_2", "yadaki_2.dat", r => new SGSpare2
            {
                Spare1ID = r.Int("id1"),
                Spare2ID = r.Int("id"),
                Type = r.Str("no")
            });
        }

        /// <summary>Table "yadaki_3" cached in yadaki_3.dat.</summary>
        public SyncedTable<SGSpare3> Spares3()
        {
            return Make("yadaki_3", "yadaki_3.dat", r => new SGSpare3
            {
                Spare1ID = r.Int("ID1"),
                Spare2ID = r.Int("ID2"),
                Spare3ID = r.Int("ID"),
                Type = r.Str("NO")
            });
        }

        /// <summary>Table "ARZESHKHODRO" cached in ARZESHKHODRO.dat.</summary>
        public SyncedTable<SGVehiclePrice> VehiclePrices()
        {
            return Make("ARZESHKHODRO", "ARZESHKHODRO.dat", r => new SGVehiclePrice
            {
                ID = r.Int("id"),
                HS = r.Int("HS"),
                Description = r.Str("SHARH"),
                Price = r.Str("ARZESH"),
                Country = r.Str("KESVAR"),
                Brand = r.Str("BERAND"),
                Model = r.Str("MODEL"),
                Unit = r.Str("VAHED"),
                BuyKind = r.Str("nokharid"),
                Reference = r.Str("makhaz"),
                Identifier = r.Str("SHENASEH1")
            });
        }

        /// <summary>Table "Currency" (+ validity dates from table "Date"), cached in Currency.dat.</summary>
        public SyncedTable<SGCurrency> Currencies()
        {
            Func<IList<SGCurrency>> fetch = () =>
            {
                var rows = new DbTableSource<SGCurrency>(_connect, "Currency", r => new SGCurrency
                {
                    ID = r.Int("id"),
                    DateID = r.Int("date_id"),
                    Name = r.Str("name"),
                    Price = r.Int("price")
                }).Fetch();

                var dates = new DbTableSource<KeyValuePair<int, string[]>>(_connect, "Date",
                    r => new KeyValuePair<int, string[]>(r.Int("id"), new[] { r.Str("date_start"), r.Str("date_end") })).Fetch();
                var byId = new Dictionary<int, string[]>();
                foreach (var d in dates) byId[d.Key] = d.Value;

                foreach (SGCurrency c in rows)
                {
                    string[] d;
                    if (byId.TryGetValue(c.DateID, out d))
                    {
                        c.StartDate = d[0];
                        c.EndDate = d[1];
                        c.Date = " تاریخ اعتبار از " + d[0] + " لغایت " + d[1];
                    }
                }
                return rows;
            };
            return new SyncedTable<SGCurrency>(new DelegateSource<SGCurrency>(fetch),
                new LocalTable<SGCurrency>(System.IO.Path.Combine(_dir, "Currency.dat"), _cipher));
        }

        /// <summary>RSS feed cached in the given file (the app used RSSNews.dat and BankRSS.dat).</summary>
        public SyncedTable<RSSNews> Rss(string url, string cacheFile)
        {
            return new SyncedTable<RSSNews>(
                new RssSource<RSSNews>(url, i => new RSSNews { Subject = i.Title, Summary = i.Description, Link = i.Link }),
                new LocalTable<RSSNews>(System.IO.Path.Combine(_dir, cacheFile), _cipher));
        }
    }
}
