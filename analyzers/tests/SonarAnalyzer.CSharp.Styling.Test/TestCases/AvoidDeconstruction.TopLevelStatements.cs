object value = null;

_ = value is System.Exception { Message: var message }; // Noncompliant

_ = value is System.Exception { Message: var used }     // Noncompliant FP, we use this for coverage and don't support top-level statements
    && used is not null
    && used is not null
    && used is not null;
