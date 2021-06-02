using System.IO;

namespace Net5
{
    public class S2930
    {
        private FileStream backing_field;
        public FileStream Prop
        {
            init
            {
                backing_field = new ("", FileMode.Open);
            }
        }
    }
}
