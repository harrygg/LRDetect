using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
  class HtmlTable
  {
    StringBuilder buf = new StringBuilder(10000);
    private int cells = 0;
    public String id = String.Empty;
    public String tableClass;
    String tBody = String.Empty;
    String tHeader = String.Empty;
    public int Cells 
    {
      get { return (cells == 0) ? cells = 2 : cells; }
      set { cells = value; }
    }

    public HtmlTable(String tableId = "", int nColumns = 2, String tClass = "")
    {
      Cells = nColumns;
      id = tableId;
      if(tClass != "")
        tableClass = tClass;
    }

    public HtmlTable(List<DataHolder> dataHolder)
    {
      tableClass = "section";
      StringBuilder output = new StringBuilder();
      foreach (DataHolder data in dataHolder)
      {
        output.Append(TableRow(new List<String> { data.title }));
        foreach (var subjectValuePairs in data.dataPairs)
        {
          //foreach (var cell in subjectValuePairs.Value.ToString())
          //{
          Logger.Debug(String.Format("Key {0}, Value: {1}", subjectValuePairs.Key.ToString(), subjectValuePairs.Value.ToString()));
          output.Append(TableRow(new List<String> { subjectValuePairs.Key.ToString(), subjectValuePairs.Value.ToString() }));
          //}
        }
      }
      tBody = output.ToString();
    }
    
    public HtmlTable(List<String[]> Data)
    {
      AddTableBody(Data);
    }

    public void AddTableHead(List<String> ths, bool sortable = false)
    {
      StringBuilder output = new StringBuilder();
      output.Append(openTHead + openTR);
      int i = 0;
      foreach (var th in ths)
      {
        if (sortable)
          output.Append(String.Format("{0}<a onclick=\"SortTable({1}, '{2}')\" href=\"javascript:void(0);\">Click to sort</a> {3}{4} ", openTH, i, id, th, closeTH));
        else
          output.Append(String.Format("{0}{1}{2} ", openTH, th, closeTH));
        i++;
      }
      output.Append(closeTR + closeTHead);
      tHeader = output.ToString();
    }
    private String openTHead = "\n\t<thead>";
    private String closeTHead = "\n\t</thead>";
    private String openTH = "\n\t\t\t<th>";
    private String closeTH = "\n\t\t\t</th>";

    public void AddTableRow(List<String> tds)
    {
      buf.Append(openTR);
      AddTDs(tds);
      buf.Append(closeTR);
    }

    public String TableRow(List<String> tds)
    {
      return openTR + TDs(tds) + closeTR;
    }

    protected void AddTDs(List<String> tds)
    {
      if (tds.Count == 1)
      {
        AddTD(tds[0], 2);
      }
      else
      {
        foreach (var td in tds)
          AddTD(td);
      }
    }

    protected String TDs(List<String> tds)
    {
      StringBuilder output = new StringBuilder();
      if (tds.Count == 1)
      {
        output.Append(TD(tds[0], 2));
      }
      else
      {
        foreach (var td in tds)
          output.Append(TD(td));
      }
      return output.ToString();
    }

    protected void AddTD(String td, int colspan = 1)
    {
      String tag = colspan == 1 ? openTD : openTDColspan2; 
      buf.Append(tag + td + closeTD);
    }

    protected String TD(String td, int colspan = 1)
    {
      String tag = colspan == 1 ? openTD : openTDColspan2;
      return tag + td + closeTD;
    }



    public void AddTableBody(Dictionary<String, String> body)
    {
      StringBuilder output = new StringBuilder();
      foreach(var tRow in body)
      {
        var list = new List<String> {tRow.Key.ToString(), tRow.Value.ToString()};
        output.Append(TableRow(list));
      }
      tBody = output.ToString();
    }

    public void AddTableBody(List<String[]> tds)
    {
      StringBuilder output = new StringBuilder();
      foreach (var tRow in tds)
      {
        output.Append(TableRow(tRow.ToList()));
      }
      tBody = output.ToString();
    }

    private String openTR = "\n\t<tr>";
    private String closeTR = "\n\t</tr>";
    private String openTD = "\n\t\t<td>";
    private String openTDColspan2 = "\n\t\t<td colspan=\"2\" class=\"colspan\">";
    private String closeTD = "</td>";
    private String tableHeader = "";
    private String OpenTable
    {
      get { return String.Format("<table id=\"{0}\" class=\"{1}\">", id, tableClass); }
      set { tableHeader = value; }
    }

    private String closeTable = "\n</table>";

    public override String ToString()
    {
      StringBuilder output = new StringBuilder(10000);
      output.Append(OpenTable);
      output.Append(tHeader);
      output.Append(tBody);
      output.Append(closeTable);
      return output.ToString();
    }
  }
}
