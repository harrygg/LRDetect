using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LRDetect
{
  abstract class Collector
  {
    public static event Action RaiseProgressUpdate;
    protected virtual void OnRaiseProgressUpdate()
    {
      var handler = RaiseProgressUpdate;
      if (handler != null)
        handler();
    }
    
    public static event EventHandler<CollectorStatusEventArgs> CollectionStarted;
    protected virtual void OnCollectionStarted(CollectorStatusEventArgs e)
    {
      Logger.Debug("Executing " + className + "." + MethodBase.GetCurrentMethod().Name);
      EventHandler<CollectorStatusEventArgs> handler = CollectionStarted;
      if (handler != null)
        handler(this, e);
    }

    public static event EventHandler<CollectorStatusEventArgs> CollectionEnded;
    protected virtual void OnCollectionEnded(CollectorStatusEventArgs e)
    {
      Logger.Debug("Executing " + className + "." + MethodBase.GetCurrentMethod().Name);
      EventHandler<CollectorStatusEventArgs> handler = CollectionEnded;
      if (handler != null)
        handler(this, e);
    }


    public Collector()
    {
      try
      {
        className = GetType().Name;
        if (Enabled)
        {
          AddTitle();
          OnCollectionStarted(new CollectorStatusEventArgs(className, "Detecting " + Title.ToLower()));

          Collect();
          OnCollectionEnded(new CollectorStatusEventArgs(className, null));
          RenderHtml();
        }
      }
      catch (Exception ex)
      {
        System.Windows.Forms.MessageBox.Show(ex.ToString());
        Logger.Error(ex.ToString());
      }
    }
    public List<DataHolder> dataHolders = new List<DataHolder>();

    protected virtual string PrependRawContent { get { return ""; } }
    private string title = String.Empty;
    public abstract string Title { get; }

    protected StringBuilder buffer = new StringBuilder();
    protected abstract void Collect();

    protected List<String[]> cells = new List<String[]>();
    public string rawContent;
    int counter = 0;
    public abstract int Order { get; }
    string className;

    protected virtual bool Enabled { get { return true; } }
    protected void AddStringsToDataCells(String[] tds)
    {
      // Move the progress bar only once in 200 times
      // applies mainly to dlls collector where we have > 1900 dlls
      if(counter++ % 200 == 0)
        OnRaiseProgressUpdate();
      cells.Add( tds );
    }
    /// <summary>
    /// Add data pair to the data holder with this title.
    /// Create the data holder if there is it does not exists
    /// </summary>
    /// <param name="title">Title of the data holder boject</param>
    /// <param name="subject"></param>
    /// <param name="value"></param>
    protected void AddDataPair(string title, string subject, string value)
    {
      // Move the progress bar
      OnRaiseProgressUpdate();

      //Check if DataHolder with this title exists
      var dh = dataHolders.Find(d => d.title.ToLower().Equals(title.ToLower()));

      //If it does, then add subject, value to current data pairs
      if (dh != null)
        dh.dataPairs.Add(subject, value);
      else // if not, then create a new data holder object and add it to the data holders collection
      {
        dh = new DataHolder(title);
        dh.dataPairs.Add(subject, value);
        dataHolders.Add(dh);
      }
    }

    protected virtual void RenderHtml()
    {
      Logger.Debug("Starting " + className + "." + MethodBase.GetCurrentMethod().Name);
      buffer.Append(PrependRawContent);
      HtmlTable dataHolder = new HtmlTable(dataHolders);

      buffer.Append(dataHolder.ToString());
      Logger.Debug("Ending " + className + "." + MethodBase.GetCurrentMethod().Name);
    }

    void AddTitle()
    {
      buffer.Append(Html.Heading(Html.HeadingSize.H2, Title));
    }

    public override String ToString()
    {
      return buffer.ToString();
    }

  }

  public class DataHolder
  {
    public string title;
    public Dictionary<string, string> dataPairs = new Dictionary<string, string>();

    public DataHolder(string t)
    {
      title = t;
    }
  }
}
