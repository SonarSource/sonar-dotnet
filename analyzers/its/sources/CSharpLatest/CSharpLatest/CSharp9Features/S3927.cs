using System;
using System.Runtime.Serialization;

namespace CSharpLatest.CSharp9Features;

[Serializable]
public class S3927
{
    public void WithLocalFunction()
    {
        [OnSerializing]
        void OnSerializing(StreamingContext context)
        {
            // This will never be called
        }
    }
}
