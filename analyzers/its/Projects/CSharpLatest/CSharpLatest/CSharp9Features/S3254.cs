﻿namespace CSharpLatest.CSharp9Features;

public class S3254
{
    Record r = new(1, 5);

    record Record
    {
        public Record(int x, int y = 5) { }
    }
}
