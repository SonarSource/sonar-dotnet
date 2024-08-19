namespace CSharpLatest.CSharp12Features;

internal class CollectionExpressions
{
    void Examples()
    {
        // Create an array:
        int[] a = [1, 2, 3, 4, 5, 6, 7, 8];

        // Create a span
        Span<int> b = ['a', 'b', 'c', 'd', 'e', 'f', 'h', 'i'];

        // Create a 2 D array:
        int[][] twoD = [[1, 2, 3], [4, 5, 6], [7, 8, 9]];

        // create a 2 D array from variables:
        int[] row0 = [1, 2, 3];
        int[] row1 = [4, 5, 6];
        int[] row2 = [7, 8, 9];
        int[][] twoDFromVariables = [row0, row1, row2];
        int[] single = [.. row0, .. row1, .. row2];
    }
}
