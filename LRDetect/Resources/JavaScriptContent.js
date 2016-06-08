$(document).ready(function () {

  $('.ShowMore').click(function (e) {
    e.preventDefault();
    toggleText(this);
    $(this).next().toggle("swing", function (e) { });
  });

});

function toggleText(element) {
  if ($(element).text() == ShowMore)
    $(element).text(ShowLess);
  else
    $(element).text(ShowMore);
}

var sortedOn = 0;
var firstSort = true;

function SortTable(sortOn, id) {
  var table = document.getElementById(id);
  var tbody = table.getElementsByTagName('tbody')[0];
  var trows = tbody.getElementsByTagName('tr');
  var rowArray = new Array();
  for (var i = 0, length = trows.length; i < length; i++) {
    rowArray[i] = new Object;
    rowArray[i].oldIndex = i;
    rowArray[i].value = trows[i].getElementsByTagName('td')[sortOn].firstChild.nodeValue.toLowerCase();
  }
  if ((sortOn == sortedOn) && firstSort == false) {
    rowArray.reverse();
  } else {
    sortedOn = sortOn;
    if (sortedOn == 0) {
      rowArray.sort(RowCompare);
    } else {
      rowArray.sort(RowCompareNumbers);
    }
    firstSort = false;
  }
  var newTbody = document.createElement('tbody');
  for (var i = 0, length = rowArray.length; i < length; i++) {
    var newRow = trows[rowArray[i].oldIndex].cloneNode(true);
    newRow.className = (i % 2 == 0) ? 'even' : 'odd';
    newTbody.appendChild(newRow);
  }
  table.replaceChild(newTbody, tbody);
}

function RowCompare(a, b) {
  var aVal = a.value;
  var bVal = b.value;
  return (aVal == bVal ? 0 : (aVal > bVal ? 1 : -1));
}

function RowCompareNumbers(a, b) {
  var aVal = parseInt(a.value);
  var bVal = parseInt(b.value);
  return (aVal - bVal);
}

function alternateRowColors() {
  var tables = document.getElementsByTagName('table');
  for (var i = 0, len = tables.length; i < len; ++i) {
    if (tables[i].className == 'alternateColors') {
      var tbody = tables[i].getElementsByTagName('tbody')[0];
      var trows = tbody.getElementsByTagName('tr');
      for (var k = 0; k < trows.length; k++) {
        trows[k].className = (k % 2 == 0) ? 'even' : 'odd';
      }
    }
  }
}