using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Schweinefutter.Data;
using SQLitePCL;

namespace Schweinefutter.Pages
{
    // SQLite kann keine subquerries

    /// <summary>
    /// Codedatei für die Index Page der GUI
    /// </summary>
    public partial class Index
    {
        /// <summary>
        /// Datenbank Kontext der Anwendung
        /// </summary>
        private DataContext? _context;

        /// <summary>
        /// Lokale Liste der Tiertabelle aus der SQLite Datenbank
        /// </summary>
        public List<TierEntry>? TheTiers { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="MOhrM"></param>
        /// <param name="FeedStart"></param>
        /// <param name="FressStop"></param>
        /// <param name="KGDIFF"></param>
        public record ResultRecord(string? MOhrM, DateTime FeedStart, DateTime FressStop, float KGDIFF);

        public List<ResultRecord>? LeftJoin { get; set; }


        /// <summary>
        /// Map mit den eingetragenen Tier Objekten, als Key wird die Ohrmarke benutzt.
        /// </summary>
        public Dictionary<string, Pig> PigMap { get; set; }


        /// <summary>
        /// Die aktuelle gesuche Ohrmarke. 
        /// </summary>
        public string? SearchString;

        /// <summary>
        /// Das Aktuell in der GUI geladene Schwein.
        /// </summary>
        public Pig? CurrentPig;


        /// <summary>
        /// Event was ausgelöst wird, wenn die Suchleiste nach einem Eintrag sucht. 
        /// </summary>
        public async Task CheckforPig()
        {
            if (SearchString != null && PigMap.ContainsKey(SearchString))
            {
                CurrentPig = PigMap[SearchString];
            }
        }

        /// <summary>
        /// Lädt die GUI Seitenleise und so direkt, damit der User einen Grundblick auf die App bekommt
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            //Load Tier Database for the Sidebar bevor first Render
            await LoadTier();
            await LoadAutoComplete();
        }

        
        protected async Task LoadAutoComplete()
        {
            EarNames = TheTiers.Select(c => c.EarMarker);
        }

        
        /// <summary>
        /// Lädt die restlichen Dinge nach, damit der User früher eine GUI sieht.
        /// </summary>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await InitLeftJoin();

            await FillSchweineMap();
            await BuildFutterArrayForChart();
            base.OnAfterRenderAsync(firstRender);
        }

        /// <summary>
        /// Füllt die Map mit den Schweinobjekten aus den Daten, die aus der Datenbank gezogen wurden.
        /// </summary>
        private async Task FillSchweineMap()
        {
            PigMap = new Dictionary<string, Pig>();

            for (int i = 0; i < TheTiers.Count(); i++)
            {
                PigMap.TryAdd(TheTiers[i].EarMarker, new Pig()
                {
                    InstallDate = TheTiers[i].InstallDate,
                    Birthday = TheTiers[i].BirthDate,
                    FeedingTermins = new List<FeedingTermin>(),
                    EarMarker = TheTiers[i].EarMarker
                });
            }

            //Multithreading nicht möglich, da dieses die chronologische Reinfolge der Einträge entfernt und das sortieren danach aufwändiger ist.
            // Keine Querie um die Zuordnung zum jeweiligen Schwein leichter zu machen
            foreach (var lefti in LeftJoin)
            {
                PigMap.GetValueOrDefault(lefti.MOhrM).FeedingTermins.Add(new FeedingTermin()
                {
                    FeedStart = lefti.FeedStart,
                    FeedStop = lefti.FressStop,
                    KGAmount = lefti.KGDIFF
                });
            }
        }

        /// <summary>
        /// Baut die Arrays für die GUI aus den Schweineobjekten, passiert mit Multitask.
        /// </summary>
        private async Task BuildFutterArrayForChart()
        {
            List<Task>? taskList = new List<Task>();

            foreach (var schwein in PigMap)
            {
                taskList.Add(Task.Run(() =>
                {
                    schwein.Value.RemoveOldDates(schwein.Value.GetNewestDate());
                    schwein.Value.initFeedArray();
                }));
            }

            await Task.WhenAll(taskList);
        }

        
        
        /// <summary>
        /// Erzeugt einen LeftJoin zwischen den beiden Datenbank Tabellen, bzw deren in Code Representation
        /// SQLite kann leider keine Subquerries, daher wird ein reiner LeftJoin erzeugt um
        /// diesen dann in einem Weiteren Schritt auf die Datenstruktur aufzuteilen
        /// </summary>
        public async Task InitLeftJoin()
        {
            _context ??= await DataContextFactory.CreateDbContextAsync();

            LeftJoin = new List<ResultRecord>();
            if (_context != null)
            {
                //Operiert auf der Tier List, um die Zugriffe auf die Datenbank zu reduzieren. Kann man Diskutieren,
                //da gleichzeitig die Futter Tabelle abgefragt wird und daher weitere Zugriffe schwierig sind.
                LeftJoin = TheTiers //_context.Tier //.Where(tier => tier.MOHRM == "48035")
                    .GroupJoin(_context.Futter, tier => tier.ID, p => p.TIER_ID,
                        (tier, grouping) => new { tier, grouping })
                    .SelectMany(t => t.grouping.DefaultIfEmpty(),
                        (tier, p) => new ResultRecord(
                            tier.tier.EarMarker,
                            p.FeedStart,
                            p.FeedStop,
                            p.KGdiff)
                    ).ToList();
            }

            if (_context != null)
            {
                await _context.DisposeAsync();
                _context = null;
            }
        }

        /// <summary>
        /// Lade die Tier Tabelle aus der SQLite Datenbank
        /// </summary>
        public async Task LoadTier()
        {
            _context ??= await DataContextFactory.CreateDbContextAsync();

            if (_context != null)
            {
                TheTiers = await _context.Tier.ToListAsync();
            }

            if (_context != null)
            {
                await _context.DisposeAsync();
                _context = null;
            }
        }
        
        bool smooth = true;
        bool showDataLabels = false;
    
        bool sidebar1Expanded = true;
        
        /// Die Max Anzahl der Einträge, die die Liste am Stück anzeigt.
        const int ListMaxCount = 100;
        
        /// Der Format String für Zeitangaben der Schweine.
        const string DateFormat = "dd.MM.yyyy";
    
        ///Liste der Ohrmarken für die GUI Liste in dem Seitenteil und für die Autovervollständigung der Suchleiste
        IEnumerable<string> EarNames;
    
    

        string FormatAsGram(object value)
        {
            return ((double)value).ToString("F0", CultureInfo.CreateSpecificCulture("de-de_phoneb") )+"g";
    
        }

        string FormatAsMonth(object value)
        {
            if (value != null)
            {
                return Convert.ToDateTime(value).ToString("dd/MM");
            }

            return string.Empty;
        }
    }
}