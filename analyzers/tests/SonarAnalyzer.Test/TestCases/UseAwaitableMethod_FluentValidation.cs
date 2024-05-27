using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

public class FluentValidationTests
{
    public class IntValidator : AbstractValidator<int> { }

    public async Task Validate()
    {
        var validator = new IntValidator();
        validator.Validate(0); // Noncompliant FP
    }
}
