/* return "Some code at the start of a file".Trim(); */
/*
{
    int SingleLine() { return 42; }
}
*/
class Fixes
{
    int SingleLine() { return 42; }/* return 42; */

    int SingleLineWithSpacing() { return 42; }              /* return 42; */

    /*  Multi lines with a mix like
     *  return 42;
     *  are not removed.
     */

    int SeperateLine() { return 42; }
    /* return 42;*/

    int MultipleLines() { return 42; }
    /* 
     {
         return 42;
     }
     */
}
/* Console.WriteLine("End Of file"); */
