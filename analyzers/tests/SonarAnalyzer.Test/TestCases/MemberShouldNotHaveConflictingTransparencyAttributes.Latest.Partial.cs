using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;

namespace Net6Poc.MemberShouldNotHaveConflictingTransparencyAttributes;

public partial class PartialClass
{
    [SecuritySafeCritical]  // FN https://sonarsource.atlassian.net/browse/NET-2717
    public partial PartialClass() { }
}
