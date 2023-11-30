using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlTypes;
using System.Numerics;


//
//https://www.sqlite.org/datatype3.html
//

namespace Schweinefutter.Data
{
    /// <summary>
    /// Code Representation der Futter Tabelle der Datenbank
    /// </summary>
    public class FutterEntry
    {
        [Column("ID")]
        public int ID { get; set; }
        
        
        [Column("TIER_ID")]
        public int TIER_ID { get; set; }
        
        
        [Column("TRANSPONDER")]
        public int  Transponder { get; set; }
        
        
        [Column("FRESSSTART")]
        public DateTime FeedStart { get; set; }
        
        
        [Column("FRESSSTOP")]
        public DateTime FeedStop { get; set; }
        
        [Column("KGBEGINN")]
        public float KGBegin { get; set; }
        
        [Column("KGENDE")]
        public float KGEnd { get; set; }
        
        [Column("KGDIFF")]
        public float KGdiff { get; set; }
        
        [Column("STATION_ID")]
        public int StationID { get; set; }
        
        
        //TODO nochmal lesen https://learn.microsoft.com/en-us/dotnet/csharp/linq/
    }

    /// <summary>
    /// Code Representation der Tier Tabelle der Datenbank
    /// </summary>
    public class TierEntry
    { 
        public int ID { get; set; }
        
        [Column("MOHRM")]
        public string?  EarMarker { get; set; }
        
        
        [Column("IDENTNR")]
        public int IdNumber { get; set; }
        
        
        [Column("GEBURTDT")]
        public DateTime BirthDate { get; set; }
        
        //Installdate wirkt falsch, aber google sagt das ist die Übersetzung
        [Column("EINSTALLDT")]
        public DateTime InstallDate { get; set; }    
    }
}