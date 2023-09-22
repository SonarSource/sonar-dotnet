using Point = (int x, int y);

namespace Net8.features
{
    using PointDictionary = Dictionary<string, Point>;

    internal class AliasAnyType
    {
        private PointDictionary pointDictionary;

        Point Copy(Point source)
        {
            return new Point(source.x, source.y);
        }
    }
}
