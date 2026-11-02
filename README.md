# 🗂️ DesktopWebAppLib (`SG.Core`) – Offline-First Core Library for Desktop Web Apps

A **.NET class library** that provides the reusable core of a **desktop kiosk / web-app hybrid**: offline‑first data tables, hardware‑locked licensing, media mirroring, and cryptography.  
It was extracted from the **Salam Gomrok kiosk** application and is intended to be referenced by a host app (WinForms/WPF) that supplies the UI and database connection.

> **Target frameworks:** `net40` and `net48` (C# 7.3)  
> **Assembly name:** `SG.Core` · **Root namespace:** `SG.Core`

---

## 📖 Overview

`SG.Core` solves the problems that every desktop kiosk repeats by hand:

- **“Show the cached copy now, refresh in the background”** — implemented once as `SyncedTable`.
- **“Is this copy licensed to this machine, and has the subscription expired?”** — implemented as `LicenseManager` with offline and online checks.
- **“Keep a local folder of images in sync with a remote list”** — implemented as `FileMirror`.
- **“Encrypt the offline cache and license file”** — implemented as `Cryptography` (AES + HMAC) and `LocalTable`.

The library is **UI‑agnostic**: it marshals events back to the creating `SynchronizationContext`, so a host form can bind directly to the results without writing `Invoke` calls [reference:0].

---

## ✨ Features

- **Offline‑first synced tables** – load the local cache instantly, refresh from a source in the background, save the fresh copy [reference:1].
- **Pluggable table sources** – `ITableSource` can be backed by a database, RSS, REST, or an arbitrary delegate [reference:2].
- **Encrypted local cache** – `LocalTable` writes/reads `.dat` files through an `ICipher` [reference:3].
- **Hardware‑locked licensing** – `LicenseManager` combines a hardware fingerprint, a trusted clock, and a subscriber directory to authorise, re‑issue, and expire licences [reference:4].
- **Offline + online licence checks** – `CheckOffline()` validates the fingerprint without network; `CheckOnline()` re‑validates against the server and deletes expired licences [reference:5].
- **Media mirroring** – `FileMirror.Sync` downloads missing files atomically (`.part` → rename), keeps stale files until a successful download, and optionally deletes files that are no longer wanted [reference:6].
- **Cryptography helpers** – SHA‑256 hashing/fingerprinting and AES‑then‑HMAC encryption/decryption [reference:7].
- **Persian date utilities** – expiry checks for Jalali‑formatted dates [reference:8].
- **Connectivity helper** – `Connectivity.IsOnline` checks whether any network interface is up [reference:9].
- **Shell helper** – `Shell.cs` provides OS integration utilities [reference:10].

---

## 🏗️ Architecture

```text
SG.Core
├── Catalog/           SgTables – factory methods for every kiosk table
├── Licensing/         LicenseManager, HardwareFingerprint, trusted clock, directory
├── Media/             FileMirror, FolderSlideshow
├── Models/            Types.cs – entities + AuthStatus enum
├── Security/          Cryptography – SHA-256, AES+HMAC
├── Sources/           ITableSource, DbTableSource, RssSource, DelegateSource
├── Storage/           ICipher, LocalTable – encrypted .dat cache
├── Sync/              SyncedTable, UserRefresh – offline-first refresh pattern
├── Util/              Connectivity, CoreLog, PersianDate, Shell
└── SG.Core.csproj     net40;net48, C# 7.3
```

## 🗂️ Repository Structure

```text
DesktopWebAppLib/
├── Catalog/
│   └── SgTables.cs                 # Factory for every table (Slides, News, Education, …)
├── Licensing/
│   ├── Abstractions.cs             # ISubscriberDirectory, ITrustedClock, IHardwareProvider, LicenseKeys
│   ├── DbSubscriberDirectory.cs    # Directory backed by a database query
│   ├── HardwareFingerprint.cs      # WMI-based hardware provider
│   ├── HttpDateClock.cs            # Trusted clock via HTTP Date header
│   └── LicenseManager.cs           # SignIn, Forget, CheckOffline, CheckOnline, Save, Read, Delete
├── Media/
│   ├── FileMirror.cs               # RemoteFile, MirrorResult, Sync()
│   └── FolderSlideshow.cs          # Folder-based slideshow helper
├── Models/
│   └── Types.cs                    # AuthStatus, SGSlide, SGNews, RSSNews, SGCurrency, SGEducation,
│                                   #   SGQ, SGA, SGLaw, SGSubscribe, SubscribeFile, …
├── Security/
│   └── Cryptography.cs             # Fingerprint, Hash, XHash, ENC, DEC (AES-256 + HMAC-SHA256)
├── Sources/
│   ├── DbRecordExtensions.cs       # Int(), Str(), … helpers on IDataRecord
│   ├── DbTableSource.cs            # ITableSource backed by a SQL query
│   ├── ITableSource.cs             # Fetch() contract + DelegateSource
│   └── RssSource.cs                # ITableSource backed by an RSS feed
├── Storage/
│   ├── ICipher.cs                  # Encrypt/Decrypt contract
│   └── LocalTable.cs               # Load/Save encrypted .dat cache
├── Sync/
│   ├── SyncedTable.cs              # LoadCache(), BeginRefresh(), Refreshed/RefreshFailed
│   └── UserRefresh.cs              # User-triggered refresh helper
├── Util/
│   ├── Connectivity.cs             # IsOnline (NetworkInterface)
│   ├── CoreLog.cs                  # Write(message, exception)
│   ├── PersianDate.cs              # IsExpired(dateString, today)
│   └── Shell.cs                    # OS/shell helpers
├── obj/Debug/                      # Build artefacts (generated)
├── SG.Core.csproj                  # net40;net48, C# 7.3
├── .gitattributes
└── .gitignore
```
---
## 🧩 Core Components

### 1. `SyncedTable` – Offline‑First Refresh Pattern

`SyncedTable` is the heart of the library. It replaces the hand‑written `BackgroundWorker` + `List` + `Load/Update_Click/RunWorkerCompleted` trio that every page used to repeat .[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Sync/SyncedTable.cs)

**Lifecycle:**

1. **`LoadCache()`** reads the encrypted local copy synchronously and returns a `CacheLoadResult` (`Loaded`, `Missing`, …) .[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Sync/SyncedTable.cs)
    
2. **`BeginRefresh()`** starts a background fetch, runs an optional post‑process step, saves the fresh list, and replaces `Items` atomically .[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Sync/SyncedTable.cs)
    
3. **`Refreshed`** is raised on the UI context after a successful refresh; **`RefreshFailed`** is raised on failure, keeping the existing items .[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Sync/SyncedTable.cs)
    
4. **`IsRefreshing`** exposes whether a refresh is in flight .[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Sync/SyncedTable.cs)
    

**Usage:**

```csharp
var table = new SyncedTable(
    new DbTableSource(connect, "news", r => new SGNews { … }, "ORDER BY ID DESC"),
    new LocalTable("news.dat", cipher));
table.Refreshed += (s, e) => listBox.DataSource = table.Items;
table.RefreshFailed += (s, e) => MessageBox.Show(e.Error.Message);
table.LoadCache();      // instant offline view
table.BeginRefresh();   // background update
```

### 2. `SgTables` – Table Catalog

`SgTables` is a factory that returns a `SyncedTable` for each kiosk table, with the table name, column mapping, and offline file name copied from the legacy application .[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Catalog/SgTables.cs)

|Method|Source table|Cache file|Model|
|---|---|---|---|
|`Slides()`|`slide1`|`slide1.dat`|`SGSlide` (with `Expired` flag)[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Catalog/SgTables.cs)|
|`News()`|`news`|`news.dat`|`SGNews`[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Catalog/SgTables.cs)|
|`Education()`|`amozesh`|`amozesh.dat`|`SGEducation`[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Catalog/SgTables.cs)|
|`Questions()`|`PAP`|`PAP.dat`|`SGQ`[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Catalog/SgTables.cs)|
|`Answers()`|`PapCm`|`PapCm.dat`|`SGA`[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Catalog/SgTables.cs)|
|`Laws()`|`bakhshnameh`|`bakhshnameh.dat`|`SGLaw`[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Catalog/SgTables.cs)|

Each factory method accepts a `Func<IDbConnection>` (the host keeps the credentials) and an `ICipher` for the offline files .[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Catalog/SgTables.cs)

### 3. `LicenseManager` – Hardware‑Locked Licensing

`LicenseManager` replaces the legacy `Subscribe.Auth` / `Subscribe.IO` classes . All I/O is injected (`ISubscriberDirectory`, `ITrustedClock`, `IHardwareProvider`, `LicenseKeys`), so it is testable and UI‑free .[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Licensing/LicenseManager.cs)

|Method|Purpose|
|---|---|
|`SignIn(user, password)`|Checks password, hardware, and expiry on the server[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Licensing/LicenseManager.cs)|
|`Forget(user)`|Re‑issues a lost licence on the same machine without a password[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Licensing/LicenseManager.cs)|
|`CheckOffline()`|No network: validates the file’s fingerprint against this machine[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Licensing/LicenseManager.cs)|
|`CheckOnline()`|Re‑validates the saved licence against the server; deletes it when expired[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Licensing/LicenseManager.cs)|
|`Save(check)`|Writes the encrypted licence file[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Licensing/LicenseManager.cs)|
|`Read()` / `Delete()`|Read or remove the licence file|

**Result type:** `LicenseCheck` exposes `Status` (an `AuthStatus` enum), `Name`, and `IsAuthorized` .[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Licensing/LicenseManager.cs)

**`AuthStatus` values:** `Authorized`, `TimeExpired`, `FingerprintNotMatch`, `WrongUserPass`, `UserNotFound`, `FileNotFound` .[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Models/Types.cs)

### 4. `FileMirror` – Media Synchronisation

`FileMirror.Sync(folder, wanted, deleteStale, userAgent)` makes a local folder match a list of `RemoteFile` objects .[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Media/FileMirror.cs)

- **Atomic downloads** — each file is written to `.part` and then moved into place, so a dropped connection never leaves a broken image .[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Media/FileMirror.cs)
    
- **Stale files are kept until a successful download** — the old slides stay on screen if the download fails .[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Media/FileMirror.cs)
    
- **Optional cleanup** — `deleteStale` removes local files that are no longer in the wanted list .[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Media/FileMirror.cs)
    
- **Path‑traversal safe** — `Path.GetFileName` blocks `../` tricks .[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Media/FileMirror.cs)
    

**Result:** `MirrorResult` reports `Downloaded`, `AlreadyPresent`, `Deleted`, and `Failed` counts .[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Media/FileMirror.cs)

### 5. `Cryptography` – Hashing & Encryption

|Method|Purpose|
|---|---|
|`Hash(input)`|SHA‑256 hex string[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Security/Cryptography.cs)|
|`XHash(input)`|SHA‑256 with null/empty guard[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Security/Cryptography.cs)|
|`Fingerprint(obj)`|JSON‑serialise an object, then SHA‑256[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Security/Cryptography.cs)|
|`ENC(secret, cryptKey, authKey, payload?)`|AES‑256 encryption then HMAC‑SHA256 authentication, Base64 output[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Security/Cryptography.cs)|
|`DEC(encrypted, cryptKey, authKey, payloadLength?)`|HMAC verification then AES decryption[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Security/Cryptography.cs)|

Constants: `BlockBitSize = 128`, `KeyBitSize = 256` .[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Security/Cryptography.cs)

### 6. `LocalTable` – Encrypted Offline Cache

`LocalTable` loads and saves a `List<T>` to a `.dat` file through an `ICipher` .[](https://github.com/alisepehrnia/DesktopWebAppLib/tree/main/Storage)  
`Load(out list)` returns a `CacheLoadResult` (e.g. `Loaded`, `Missing`), and `Save(list)` writes the encrypted file.

### 7. `ITableSource` – Pluggable Data Sources

`ITableSource.Fetch()` is the only contract: a blocking method, called on a background thread, that returns a list of rows .[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Sources/ITableSource.cs)

Built‑in implementations:

- **`DbTableSource`** – runs a SQL query and maps each `IDataRecord` with a `Func<IDataRecord, T>` .[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Catalog/SgTables.cs)
    
- **`RssSource`** – reads an RSS feed .[](https://github.com/alisepehrnia/DesktopWebAppLib/tree/main/Sources)
    
- **`DelegateSource`** – wraps any `Func<IList<T>>` for one‑off sources .[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Sources/ITableSource.cs)
    

### 8. Utilities (`Util/`)

|Class|Purpose|
|---|---|
|`Connectivity`|`IsOnline` – true when any network interface is up[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Util/Connectivity.cs)|
|`CoreLog`|`Write(message, exception)` – central logging hook[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Media/FileMirror.cs)|
|`PersianDate`|`IsExpired(dateString, today)` – Jalali date expiry check[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Catalog/SgTables.cs)|
|`Shell`|OS/shell helpers[](https://github.com/alisepehrnia/DesktopWebAppLib/tree/main/Util)|

---
## 🛠️ Technologies

|Item|Details|
|---|---|
|**Language**|C# 7.3|
|**Target frameworks**|`net40`, `net48`[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/SG.Core.csproj)|
|**JSON**|Newtonsoft.Json (`LicenseManager`, `Cryptography`)[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Licensing/LicenseManager.cs)[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Security/Cryptography.cs)|
|**Cryptography**|`System.Security.Cryptography` (SHA‑256, AES, HMAC)[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Security/Cryptography.cs)|
|**Data access**|`System.Data` (`IDbConnection`, `IDataRecord`)[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Catalog/SgTables.cs)|
|**Networking**|`System.Net` (`WebClient` in `FileMirror`; `NetworkInterface` in `Connectivity`)[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Media/FileMirror.cs)[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Util/Connectivity.cs)|
|**UI marshalling**|`SynchronizationContext`[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Sync/SyncedTable.cs)|
|**Project file**|`SG.Core.csproj` (`net40;net48`, C# 7.3)[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/SG.Core.csproj)|

---
## 🚀 Getting Started

### Prerequisites

- **.NET Framework 4.0 or 4.8** (the library multi‑targets both).
    
- **Visual Studio 2019/2022** with the **.NET desktop development** workload.
    
- **Newtonsoft.Json** (referenced by `LicenseManager` and `Cryptography`; add it via NuGet in the host app).
    

### Build the library

```bash
git clone https://github.com/alisepehrnia/DesktopWebAppLib.git
cd DesktopWebAppLib
dotnet build SG.Core.csproj
```

> Because the project targets `net40` and `net48`, building on a modern SDK may require the **.NET Framework targeting packs**. Visual Studio handles this automatically.

### Reference it from a host app

**Project reference:**

```xml
<ItemGroup>
  <ProjectReference Include="..\DesktopWebAppLib\SG.Core.csproj" />
</ItemGroup>
```

**DLL reference:**

```xml
<ItemGroup>
  <Reference Include="SG.Core">
    <HintPath>path\to\SG.Core.dll</HintPath>
  </Reference>
</ItemGroup>
```

### Minimal host wiring

```csharp
using SG.Core;
using SG.Core.Catalog;
using SG.Core.Licensing;
using SG.Core.Security;
using SG.Core.Storage;
// 1. Offline tables
var cipher = new MyCipher(); // implement ICipher
var tables = new SgTables(() => OpenSqlConnection(), cipher, @"C:\Kiosk\Cache");
var news = tables.News();
news.LoadCache();
news.BeginRefresh();
// 2. Licensing
var licence = new LicenseManager(
    @"C:\Kiosk\license.dat",
    new LicenseKeys(cryptKey, authKey),
    new DbSubscriberDirectory(OpenSqlConnection));
var check = licence.CheckOffline();
if (!check.IsAuthorized)
    check = licence.CheckOnline();
// 3. Media mirror
var result = FileMirror.Sync(
    @"C:\Kiosk\Slides",
    new[] { new RemoteFile { Name = "12.jpg", Url = "https://…/12.jpg" } });
```

---
## 📝 Notes & Caveats

- **No README or LICENSE in the repository** — this document was written from the source code. The repository contains no `README.md` and no licence file.
    
- **`net40` / `net48` only** — the library does not target .NET Core / .NET 5+. Migrating requires replacing `WebClient` (deprecated) and reviewing `SynchronizationContext` usage.
    
- **Host supplies credentials** — `SgTables` takes a `Func<IDbConnection>`; database credentials are intentionally kept in the host app .[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Catalog/SgTables.cs)
    
- **`ICipher` is a contract only** — you must provide an implementation (e.g. backed by `Cryptography.ENC`/`DEC`) to encrypt the `.dat` caches.
    
- **`SgTables` is specific to the Salam Gomrok schema** — table names (`slide1`, `news`, `amozesh`, `PAP`, `PapCm`, `bakhshnameh`) and column names (`mozoh`, `tarikhshoroh`, …) are hard‑coded . Reusing it for another schema means editing `SgTables.cs`.[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Catalog/SgTables.cs)
    
- **`LicenseManager` is testable by design** — inject a fake `ISubscriberDirectory`, `ITrustedClock`, and `IHardwareProvider` to unit‑test the auth flow without a server or WMI .[](https://raw.githubusercontent.com/alisepehrnia/DesktopWebAppLib/main/Licensing/LicenseManager.cs)
    
- **`FileMirror` uses `WebClient`** — fine for .NET Framework, but consider `HttpClient` if you port the library forward.
    
- **No unit tests** — the repository contains no test project.
    
- **`obj/Debug` is committed** — build artefacts are present in the repository and can be ignored.
    

---
## 🤝 Contributing

Issues and pull requests are welcome — e.g. porting to .NET 8, replacing `WebClient` with `HttpClient`, adding unit tests for `SyncedTable` and `LicenseManager`, or extracting the Salam Gomrok table mapping into a configuration file.

---
## 📄 License

Feel free to use
