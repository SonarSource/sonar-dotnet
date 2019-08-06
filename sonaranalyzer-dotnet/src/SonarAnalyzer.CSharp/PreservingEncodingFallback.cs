using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarAnalyzer
{
    internal class PreservingEncodingBuffer : EncoderFallbackBuffer
    {
        public override int Remaining => buffer.Length - currentChar;

        public override bool Fallback(char charUnknown, int index)
        {
            buffer = String.Format(".{0:X}", (int)charUnknown);
            currentChar = 0;
            return true;
        }

        public override bool Fallback(char charUnknownHigh, char charUnknownLow, int index)
        {
            buffer = String.Format(".{0:X}{1:X}", (int)charUnknownHigh, (int)charUnknownLow);
            currentChar = 0;
            return true;
        }

        public override char GetNextChar()
        {
            return currentChar < buffer.Length ? buffer[currentChar++] : '\u0000';
        }

        public override bool MovePrevious()
        {
            return false;
        }
        private string buffer;
        private int currentChar;
    }

    internal class PreservingEncodingFallback : EncoderFallback
    {
        public override int MaxCharCount => 4;

        public override EncoderFallbackBuffer CreateFallbackBuffer()
        {
            return new PreservingEncodingBuffer();
        }
    }
}
