"use strict";
var version = "version 1.30";
var iCount;
var DataSource = [];
var DataURL = [];
var DataBild = [];
var DataName = [];
var DataGenre = [];
var DataDate = [];
var DataFanart = [];
var DataYear = [];
var DataRating = [];
var DataActors = [];
var navilinks_full, navilinks, moviewall, moviewall_full;
var string_navigation_1 = '<li><a href="javascript:{}" onclick="func_ShowDetails(';
var string_navigation_1HD = '<li><a href="javascript:{}" style="color:#A6D3EA; background: transparent url(images/hd.jpg) no-repeat; background-position: right top; padding-right: 27px;" onclick="func_ShowDetails(';
var string_navigation_2 = ')">';
var string_navigation_3 = '</a> </li>';
var string_moviewalllink_1 = ' <a class="thumbs" href="javascript:func_ShowDetails(';
var string_moviewalllink_2 = ')"  onerror="func_DefaultFanart(';
var string_moviewalllink_3 = ')">';
var string_moviewallpic_1 = '<img class="img150" src="';
var string_moviewallpic_2 = '" width="133" height="200" alt="';
var string_moviewallpic_3 = '" title="';
var string_moviewallpic_4 = '"</a>';
//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++//
//---- Function for setting default wallpaper if movie has no fanart-----
//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++//
function func_DefaultFanart(selectedmovie) {
var X;
X = document.getElementById("backgrounddiv" + selectedmovie);
X.innerHTML = '<img src="images/bg.jpg" width="100%" height="100%" alt=""/>';
}
//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++//
//---- Function: Choose fanart of selected movie and activate div -----
//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++//
function func_ShowDetails(selectedmovie) {
var i, Q, Z;
for (i = 1;i <= iCount; i++) // deactivate div before open up new one!
{
Q = document.getElementById("movie" + i);
Q.style.display = 'none';
}
Z = document.getElementById("backgrounddiv" + selectedmovie);
Z.innerHTML = '<img src="' + DataFanart[selectedmovie] + '" width="100%" height="100%" onerror="func_DefaultFanart(' + selectedmovie + ')"; alt="" />';
Q = document.getElementById("movie" + selectedmovie);
Q.style.display = 'block';
}
//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++//
//---- Function for building left and right navigation of site-----
//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++//
function func_BuildNavigation(selectedmovie)
{
var y, Q, V, i, arr2str, result1, result2;
Q = document.getElementById("navigation");
V = document.getElementById("div_moviewall");
var myregexp1 = new RegExp('1080');
var myregexp2 = new RegExp('720');

//execute only for buildung up navigation for ALL movies (PageLoad, Reset-Button)!
if (selectedmovie === 0) {
navilinks_full = "";  // deletes all entries of left sidebar
moviewall_full = ""; // deletes all entries of right sidebar
y = iCount; // this will make sure to add navigation of ALL movies!
func_ShowDetails(1);
}
else // only navigation elements of one specific movie will be added!
{
y = selectedmovie;
}
//Navigation builder for left and right sidebar
for (i = selectedmovie;i <= y; i++)
{
if (i === 0) {
i = 1; }
arr2str = DataSource[i].toString();
result1 = arr2str.search(myregexp1);
result2 = arr2str.search(myregexp2);
if (result1 !== -1 || result2 !== -1) // Found something!
{
navilinks = string_navigation_1HD + i + string_navigation_2 + DataName[i] + string_navigation_3;
}
else
{
navilinks = string_navigation_1 + i + string_navigation_2 + DataName[i] + string_navigation_3;
}
navilinks_full = navilinks_full + navilinks;
moviewall = string_moviewalllink_1 + i + string_moviewalllink_2 + i + string_moviewalllink_3 + string_moviewallpic_1 + DataBild[i] + string_moviewallpic_2 + DataName[i] + string_moviewallpic_3 + DataName[i] + string_moviewallpic_4;
moviewall_full = moviewall_full + moviewall;
}
if (selectedmovie === 0) { // only needed when function not called from another function
selectedmovie = 1;
Q.innerHTML = '<ol class="symbol" id="ol_navigation">' + navilinks_full + '</ol><p style="color:yellow;font-size:0.6em;">' + version + '</p>';
V.innerHTML = moviewall_full;
}
}


/* ****************************************************************************** * // CREDIT: Quaese, Quelle: http://www.tutorials.de/javascript-ajax/283260-javascript-zweidimensionales-array-sortieren.html
 * Arrayobjekt um Methode erweitern - Array mit Quicksort nach Spalten sortieren
 *
 * Parameter: intLower - Untergrenze des Teilbereichs (beim Start i.A. 0)
 *            intUpper - Obergrenze des Teilbereichs (beim Start i.A. Array.length-1)
 *            intCol   - Spalte, nach der sortiert werden soll (beginnend bei 0)
 * ****************************************************************************** */
Array.prototype.quicksortCol = function(intLower, intUpper, intCol){
  var i = intLower, j = intUpper;
  var varHelp = new Array();
  // Teilen des Bereiches und Vergleichswert ermitteln
  var varX = this[parseInt(Math.floor(intLower+intUpper)/2)][intCol];
 
  // Teilbereiche bearbeiten bis:
  // - "linker" Bereich enthält alle "kleineren" Werte
  // - "rechter" Bereich enthält alle "grösseren" Werte
  do{
    // Solange Wert im linken Teil kleiner ist -> Grenzeindex inkrementieren
    while(this[i][intCol] < varX) i++;
    // Solange Wert im rechten Teil grösser ist -> Grenzindex dekrementieren
    while(varX < this[j][intCol]) j--;
 
    // Untergrenze kleiner als Obergrenze -> Tausch notwendig
    if(i<=j){
      var varHelp = this[i];
      this[i] = this[j];
      this[j] = varHelp;
      i++;
      j--;
    }
  }while(i<j);
 
  // Quicksort rekursiv aufrufen
  if(intLower < j) this.quicksortCol(intLower, j, intCol);
  if(i < intUpper)  this.quicksortCol(i, intUpper, intCol);
}

 //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++//
//---- Function: Custom Search [Date]-----
//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++//
function func_SearchDate() {
var todaydate  = new Date(); //Date Object
var tt   = todaydate.getDate(); //today: day
var mm   = todaydate.getMonth() + 1; //today: month
var jjjj = todaydate.getFullYear(); //today: year
var timestamp_now = Date.UTC(jjjj, mm, tt); // today: timestamp
var timestamp_movie = new Array(iCount); 
timestamp_movie[0] = new Array(2);
var doOnce = 0;
var Q = document.getElementById("navigation");
var V = document.getElementById("div_moviewall");
var i;
navilinks_full = "";  // deletes all entries of left sidebar
moviewall_full = ""; // deletes all entries of right sidebar

// Circle through all entries and search for dates lower than "selectedday"
for (i = 1;i <= iCount; i++){
var moviedate = DataDate[i].split("."); // i.e. moviedate[0]=24; moviedate[1]=12; moviedate[2]=2003;
timestamp_movie[i-1] = [i,Date.UTC(moviedate[2], moviedate[1], moviedate[0])]; // Timestamp in ms
}

// Array sorted with Quicksort function/ ascending
timestamp_movie.quicksortCol(0, (timestamp_movie.length-1), 1);

for (i = iCount;i > 0; i--){

var z = timestamp_movie[i-1][0];
func_BuildNavigation(z);
}

Q.innerHTML = '<ol id="ol_navigation">' + navilinks_full + '</ol>';
V.innerHTML = moviewall_full;
}

//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++//
//---- Function: Custom Search [Year]-----
//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++//
function func_SearchYear() {
var doOnce = 0;
var Q = document.getElementById("navigation");
var V = document.getElementById("div_moviewall");
var i;
var year_movie = new Array(iCount); 
year_movie[0] = new Array(2);
navilinks_full = "";  // deletes all entries of left sidebar
moviewall_full = ""; // deletes all entries of right sidebar

for (i = 1;i <= iCount; i++){
year_movie[i-1] = [i,DataYear[i]]; // Timestamp in ms
}

// Array sorted with Quicksort function/ ascending
year_movie.quicksortCol(0, (year_movie.length-1), 1);

for (i = iCount;i > 0; i--){

var z = year_movie[i-1][0];
func_BuildNavigation(z);
}

Q.innerHTML = '<ol id="ol_navigation">' + navilinks_full + '</ol>';
V.innerHTML = moviewall_full;
}

//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++//
//---- Function: Custom Search [Genre]-----
//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++//
function func_SearchGenre(selectedgenre) {
var myregexp = new RegExp(selectedgenre); //  NEW RegeExp Object
var doOnce = 0;
var Q = document.getElementById("navigation");
var V = document.getElementById("div_moviewall");
var i;
navilinks_full = "";  // deletes all entries of left sidebar
moviewall_full = ""; // deletes all entries of right sidebar

// Circle through all entries and test if movie genre contains "selectedgenre"
for (i = 1;i <= iCount; i++)
{
var arr2str = DataGenre[i].toString();
var result = arr2str.search(myregexp);

if (result !== -1) // Found something!
{
doOnce = 1;
func_BuildNavigation(i); // Calls LoadPage Function and add match to left and right navigation string variable!
}
}
if(doOnce === 1) // only when at least 1 match found refresh and show left and right sidebar
{
Q.innerHTML = '<ol id="ol_navigation">' + navilinks_full + '</ol>';
V.innerHTML = moviewall_full;
}
}
//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++//
//---- Function: Custom Search [Wertung]----- Erweiterung von User furryhamster Vielen Dank!!
//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++//
function func_SearchWertung() {
var doOnce = 0;
var Q = document.getElementById("navigation");
var V = document.getElementById("div_moviewall");
var i;
var rating_movie = new Array(iCount); 
rating_movie[0] = new Array(2);
navilinks_full = "";  // deletes all entries of left sidebar
moviewall_full = ""; // deletes all entries of right sidebar

for (i = 1;i <= iCount; i++){
rating_movie[i-1] = [i,DataRating[i]]; // Timestamp in ms
}

// Array sorted with Quicksort function/ ascending
rating_movie.quicksortCol(0, (rating_movie.length-1), 1);

for (i = iCount;i > 0; i--){

var z = rating_movie[i-1][0];
func_BuildNavigation(z);
}

Q.innerHTML = '<ol id="ol_navigation">' + navilinks_full + '</ol>';
V.innerHTML = moviewall_full;
}

//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++//
//---- Function: Custom Search [Schauspieler]----- Erweiterung von User furryhamster Vielen Dank!!
//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++//
function func_SearchActor(actor) {
var myregexp = new RegExp(actor); //  NEW RegeExp Object
var doOnce = 0;
var Q = document.getElementById("navigation");
var V = document.getElementById("div_moviewall");
var i;
navilinks_full = "";  // deletes all entries of left sidebar
moviewall_full = ""; // deletes all entries of right sidebar

// Circle through all entries and test if movie actors contains "actor"
for (i = 1;i <= iCount; i++)
{
var arr2str = DataActors[i].toString();
var result = arr2str.search(myregexp);

var arr2strLower = arr2str.toLowerCase();
var result2 = arr2strLower.search(myregexp);

if (result !== -1 || result2 !== -1) // Found something!
{
doOnce = 1;
func_BuildNavigation(i); // Calls LoadPage Function and add match to left and right navigation string variable!
}
}
if(doOnce === 1) // only when at least 1 match found refresh and show left and right sidebar
{
Q.innerHTML = '<ol id="ol_navigation">' + navilinks_full + '</ol>';
V.innerHTML = moviewall_full;
}
}

//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++//
//---- Function: Open Movie Folder -----
//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++//
function func_OpenFolder(Counter) {
var Adresse = window.location.href;
var searchURL = new RegExp('file:'); //  NEW RegeExp Object
var result = Adresse.search(searchURL);

if (result !== -1) // Lokal
{
window.open (DataURL[Counter] + '/');
}
else // Seite ist im Internet
{
alert( 'Kein Zugriff auf Ordner möglich!' );
}
}