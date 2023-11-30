namespace Schweinefutter.Pages;

/// <summary>
/// Hilfsklasse für die Diagramm Darstellung.
/// </summary>
public class DataItem
{
    public string? Date { get; set; } = "1-1-1980";
    public double? FeedAmount { get; set; } = 0;
}
public class Pig
{
    /// <summary>
    /// Die Tage, die dargestellt werden sollen, in diesem Fall 14 Tage
    /// </summary>
    public const int TimeFrame = 14;
    
    public DateTime Birthday { set; get; }
    
    public DateTime InstallDate { get; set; }
    
    /// <summary>
    /// Die Fresstermin vom Tier als Objekt Liste.
    /// </summary>
    public List<FeedingTermin>? FeedingTermins { get; set; }
    
    /// <summary>
    /// Array für die GUI Diagramm
    /// </summary>
    public DataItem[] FeedArray;//= new int[14];

    /// <summary>
    /// Die Ohrmarke des Tieres
    /// </summary>
   public string? EarMarker { get; set; }
    

    //TODO obsolet machen, in dem man die sortierten einträge abläuft, direkt verarbeitet und dann beim rest abricht.
    
    public void RemoveOldDates(DateTime LastTime)
    {
        FeedingTermins.RemoveAll((x) => x.GetAverageTime().Ticks < LastTime.AddDays(-(TimeFrame +1)).Ticks);
        
    }

    /// <summary>
    /// Erzeugt ein simples Array mit den Fressterminen für die GUI.
    /// </summary>
    public void initFeedArray()
    {

        FeedArray = new DataItem[TimeFrame+1];
        for (var i = 0; i < FeedArray.Length; ++i)
        {
            FeedArray[i] = new DataItem();
        }

        DateTime newest = GetNewestDate();
        
        foreach (var futter in FeedingTermins)
        {
           
            var temp = new DateTime((newest.Ticks - futter.GetAverageTime().Ticks)).DayOfYear;

           
            FeedArray[(TimeFrame +1 - temp)].Date = futter.GetAverageTime().ToString();
            FeedArray[(TimeFrame+1 - temp)].FeedAmount += futter.KGAmount;

            //FeedArray[14 - (int)((newest - futter.GetAverageTime()).TotalDays)].Date = futter.GetAverageTime().ToString();
            //FeedArray[14 - (int)((newest - futter.GetAverageTime()).TotalDays)].FeedAmount += futter.KGAmount;
        }
    }


    /// <summary>
    /// Gibt das neueste aufgenommende durchschnittliche Fressdatum des Tieres zurück
    /// </summary>
    /// <returns>die Halbzeit des neusten aufgenommenen Fresstermins.</returns>
    public DateTime GetNewestDate()
    {
   
        return FeedingTermins.MaxBy((x) => x.GetAverageTime()).GetAverageTime(); 
      
    }
}