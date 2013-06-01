using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
    public class Html
    {
        private string title = "LR Detect Tool Report";
        private string style
        {
            get
            {
                return "\n\t\tbody {font:1.2em/1.4em Verdana;}"
                + "\n\t\ttable.main { empty-cells: show; width:972px;}" 
                + "\n\t\ttable#processes {display:none;}"
                + "\n\t\ttable tr {height:20px;}"
                + "\n\t\ttable td {padding: 3px auto;word-wrap:normal; font: .8em Verdana, Arial; }"
                + "table th {width:310px; padding: 5px; word-wrap:normal; font: .9em Verdana, Arial; background-color: #eee; text-align:left;}"
		        + "table th a {font-size: .7em ; color:#000; float:right; text-decoration:none; }"
                + "\n\t\th1 { color: #669; font: 1.8em/1.8em Trebuchet MS, Arial; margin-bottom:0; }"
                + "\n\t\th2 { font: bold 1.2em Trebuchet MS, Arial; width: 960px; padding: 5px; background: #ccc;}"
                + "\n\t\ttr.even {background-color: #fff}"
                + "\n\t\ttr.odd {background-color: #eee}"
                + "\n\t\t.text {}"
                + "\n\t\thr {height:1px; border:1px solid #ddd;}"
                + "\n\t\t.gray {width:300px; background-color: #eee; font: bold 0.8em/1.1em Verdana, Arial }"
                + "\n\t\ttd.colspan {font-weight:bold; background-color: #ddd;}"
                + "\n\t\t.c669 { background-color: #669 }"
                + "\n\t\tpre {font-size:1.2em}"
                + "\n\t\t.notice {color: green;}"
                + "\n\t\t.warning {color: #9F6000;}"
                + "\n\t\t.error {color: #D8000C;}"
                + "\n\t\t.dontShow {display:none;font-family:Courier; font-size:.9em; padding-top:10px;}"
                + "\n\t\t.dontShow b {display:inline-block;padding-top:5px;}"
                + "";
            }
        }
        //private string errors;
        //private string body;
        private StringBuilder body = new StringBuilder(32000);
        public static string br = "<br />\n";
        public static string hr = "<hr />\n";

        public Html()
        {
        }
        
        public static string B(string innerHTML)
        {
            return "<b>" + innerHTML + "</b>";
        }

        public static string I(string innerHTML)
        {
            return "<span style=\"font-style:italic;\">" + innerHTML + "</span>";
        }

        public static string U(string innerHTML)
        {
            return "<span style=\"text-decoration:underline;\">" + innerHTML + "</span>";
        }

        public static string Pre(string innerHTML)
        {
            return "<pre>" + innerHTML + "</pre>";
        }

        // method to add content to the html buffer
        public void AddRawContent(string content)
        {
            body.Append(content);
        }

        public void OpenTable(string id = "", string className = "main")
        {
            //<colgroup><col span=\"30%\"><col id=\"format-me-specially\"></colgroup>
            body.Append("\n\t\t<table class=\"" + className +"\" id=\"" + id + "\"> \n");
        }
        public void CloseTable()
        {
            body.Append("\n\t\t</table>");
        }
        // method to add TR element to the html buffer
        public void AddTableRow(string td1, string td2)
        {
            body.Append("\n\t\t\t<tr>"
                + "\n\t\t\t\t<td class=\"gray\">" + td1 + "</td>"
                + "\n\t\t\t\t<td>" + td2 + "</td>"
                + "\n\t\t\t</tr>");
        }
        // method to add TR element to the html buffer
        /// <summary>
        /// Add plain tr with td no classes
        /// </summary>
        /// <param name="td1">Content of td1</param>
        /// <param name="td2">Content of td1</param>
        public void AddTableRowPlain(string td1, string td2)
        {
            body.Append("\n\t\t\t<tr>"
                + "\n\t\t\t\t<td>" + td1 + "</td>"
                + "\n\t\t\t\t<td>" + td2 + "</td>"
                + "\n\t\t\t</tr>");
        }
        public void AddTableRowPlain(string td1, string td2, string td3)
        {
            body.Append("\n\t\t\t<tr>"
                + "\n\t\t\t\t<td>" + td1 + "</td>"
                + "\n\t\t\t\t<td>" + td2 + "</td>"
                + "\n\t\t\t\t<td>" + td3 + "</td>"
                + "\n\t\t\t</tr>");
        }
        // method to add TR element to the html buffer
        public void AddTableRow(string td1, int colspan)
        {
            body.Append("\n\t\t\t<tr>"
                + "\n\t\t\t\t<td colspan=\"" + colspan + "\" class=\"colspan\">" + td1 + "</td>"
                + "\n\t\t\t</tr>");
        }

        // method to add TR element to the html buffer
        public void AddTableRow(string td1, string td2, string td3 = null)
        {
            body.Append("\n\t\t\t<tr>"
                + "\n\t\t\t\t<td>" + td1 + "</td>"
                + "\n\t\t\t\t<td>" + td2 + "</td>"
                + "\n\t\t\t\t<td>" + td3 + "</td>"
                + "\n\t\t\t</tr>");
        }
        // method to add TH element to the html buffer
        public void AddTableHead(string td1, string td2, string td3 = null)
        {
            body.Append("\n\t\t\t<thead>"
                + "\n\t\t\t\t<tr>"
                + "\n\t\t\t\t\t<th><a onclick=\"SortTable(0)\" href=\"javascript:void(0);\">Click to sort</a>" + td1 + "</th>"
                + "\n\t\t\t\t\t<th><a onclick=\"SortTable(1)\" href=\"javascript:void(0);\">Click to sort</a>" + td2 + "</th>"
                + "\n\t\t\t\t\t<th><a onclick=\"SortTable(2)\" href=\"javascript:void(0);\">Click to sort</a>" + td3 + "</th>"
                + "\n\t\t\t\t</tr>"
                + "\n\t\t\t</thead>");
        }

        public void OpenTableBody()
        {
            body.Append("\n\t\t\t<tbody>");
        }

        public void CloseTableBody()
        {
            body.Append("\n\t\t\t<t/body>");
        }

        public static string Link(string link, string text)
        {
            return "<a href=\"" + link + "\" target=\"_blank\">" + text + "</a>";
        }
        
        public static string LinkShowContent(string elementId)
        {
            return "<a href=\"javascript:void(0);\" onClick=\"showContent('" + 
                elementId + "');\" id=\"link_"+ elementId + "\">Click to show content</a>";
        }

        public static string Notice(string innerHTML)
        {
            return "<span class=\"notice\">" + innerHTML + "</span>";
        }

        public static string Warning(string innerHTML)
        {
            return "<span class=\"warning\">" + innerHTML + "</span>";
        }
        public static string Error(string innerHTML)
        {
            return "<span class=\"error\">" + innerHTML + "</span>";
        }

        //private string DisplayErrors()
        //{
        //    return "<div id=\"errors\">" + this.errors + "</div>";
        //}

        public static string semicolon2br(string content)
        {
            return content.Replace(";", "<br />\n");
        }

        public static string Bool2Text(bool value)
        {
            return value ? "Yes" : "No";
        }

        /// <summary>
        /// < becomes &lt;
        /// > becomes &gt;
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string EscapeSpecialChars(string input)
        { 
            return input.Replace("<", "&lt;").Replace(">", "&gt;");
        }

        // TODO check if this is useless
        public static string ErrorMsg(int errorNumber = 0)
        {
            string[] errors = new String[] { "Unable to detect!", "Unknown error!"};
            return Html.Error(errors[errorNumber]);
        }
        /// <summary>
        /// Method to clear the content of the body
        /// in case we run the tool twice
        /// TODO use Clear() in .NET 4
        /// </summary>
        public void Clear()
        {
            body.Length = 0;
            body.Capacity = 0;
        }

        public override string ToString()
        {
            StringBuilder content = new StringBuilder(15000);
            content.Append("<!doctype html>\r\n");
            content.Append("<!-- saved from url=(0016)http://localhost -->\r\n");
            content.Append("<html>\r\n\t<head>");
            content.Append("\n\t<title>" + title + "</title>");
            content.Append("\n\t<style type=\"text/css\">" + style + "\n\t</style>");
            content.Append("\n\t<meta http-equiv=\"content-type\" content=\"text/html; charset=utf-8\" />");
            content.Append("\n\t<meta name=\"Author\" content=\"Hristo Genev, hristo.genev@hp.com\" />");
            content.Append("\n\t<script type=\"text/javascript\">" 
                + "\n\t\tfunction showContent(elementId) {" 
                + "\n\t\t\tvar style = document.getElementById(elementId).style;" 
                + "\n\t\t\tvar link = document.getElementById('link_' + elementId);"
                + "\n\t\t\tif(style.display=='none' || style.display=='') {"
                + "\n\t\t\t\tstyle.display='block'; \n\t\t\t\tlink.innerHTML = 'Click to hide content';" 
                + "\n\t\t\t} else {\n\t\t\t\tstyle.display='none';\n\t\t\t\tlink.innerHTML = 'Click to show content'\n\t\t\t}\n\t\t}\n\t"
                + "\n\tvar sortedOn = 0; var firstSort = true; function SortTable(sortOn){ var table = document.getElementById('processes'); var tbody = table.getElementsByTagName('tbody')[0]; var trows = tbody.getElementsByTagName('tr');  var rowArray = new Array(); for (var i = 0, length = trows.length; i < length; i++) { rowArray[i] = new Object; rowArray[i].oldIndex = i; rowArray[i].value = trows[i].getElementsByTagName('td')[sortOn].firstChild.nodeValue;}if ((sortOn == sortedOn) && firstSort == false) {rowArray.reverse(); }  else {  sortedOn = sortOn;   if (sortedOn == 0) {  rowArray.sort(RowCompare);  }else {rowArray.sort(RowCompareNumbers);  }firstSort = false;}  var newTbody = document.createElement('tbody');  for (var i = 0, length = rowArray.length ; i < length; i++) {var newRow = trows[rowArray[i].oldIndex].cloneNode(true);newRow.className = (i%2==0) ? 'even' : 'odd';newTbody.appendChild(newRow);}  table.replaceChild(newTbody, tbody);  }"
                + "\n\tfunction RowCompare(a, b) {var aVal = a.value;  var bVal = b.value;  return (aVal == bVal ? 0 : (aVal > bVal ? 1 : -1)); }"
                + "\n\tfunction RowCompareNumbers(a, b) {var aVal = parseInt(a.value);var bVal = parseInt(b.value);  return (aVal - bVal); } "
                + "\n\tfunction alternateRowColors(){var tables=document.getElementsByTagName('table'); for(var i = 0, len = tables.length; i < len; ++i) {if (tables[i].className == 'alternateColors'){var tbody = tables[i].getElementsByTagName('tbody')[0];var trows = tbody.getElementsByTagName('tr'); for (var k = 0, len = trows.length; k < len; k++) {trows[k].className = (k%2==0) ? 'even' : 'odd';} } } }"
                + "</script>");
            content.Append("\n\t</head>\n\n\t<body onload=\"alternateRowColors()\">");
            content.Append("\n\n\t\t<h1>" + title + "</h1>");
            content.Append("\n\n\t\t<small>Generated on " + DateTime.Now.ToLocalTime() + " for <b>" + Environment.MachineName + "</b></small>");
            //content.Append(DisplayErrors());
            content.Append(body.ToString());
            content.Append("\n\t</body>\n</html>");

            return content.ToString();
        }
    }
}
