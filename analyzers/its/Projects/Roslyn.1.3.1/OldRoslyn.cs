using System;
using System.Diagnostics;

namespace Roslyn
{
    // This class ensures that the old SE engine is run.
    // S2259 FPs and FNs should not get fixed. If they do, there's probably something wrong with rule activation for the old Roslyn.
    public class OldRoslyn
    {
        private object field;

        public void ThisConditional()
        {
            field = null;
            this?.field.ToString(); // This S2259 is raised only by the old engine. New SE doesn't support this.
        }

        public string DebugAssert()
        {
            object asserted = null;
            Debug.Assert(asserted != null);
            return asserted.ToString();    // This S2259 FP should never get fixed, because the old SE engine does not support Debug.Assert
        }

        public string TryCatch(object arg) {
            try
            {
                return arg?.ToString();
            }
            catch (InvalidOperationException) when (arg != null)
            {
                return arg.ToString();
            }
            catch (FormatException) when (arg == null)
            {
                return arg.ToString();  // This S2259 FP should never get fixed, because the old SE engine does not know that FormatException cannot be throw by the arg?. part.
            }
            catch
            {
                return null;
            }
        }
    }
}
