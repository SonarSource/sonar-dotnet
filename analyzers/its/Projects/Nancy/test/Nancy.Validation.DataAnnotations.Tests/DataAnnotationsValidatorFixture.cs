﻿namespace Nancy.Validation.DataAnnotations.Tests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    using FakeItEasy;

    using Nancy.Tests;

    using Xunit;

    public class DataAnnotationsValidatorFixture
    {
        private readonly DataAnnotationsValidatorFactory factory;
        public readonly IPropertyValidator propertyValidator1;
        public readonly IPropertyValidator propertyValidator2;
        public readonly IValidatableObjectAdapter validatableObjectAdapter;
        public readonly IPropertyValidatorFactory validatorFactory;
        public readonly DataAnnotationsValidator validator;

        public DataAnnotationsValidatorFixture()
        {
            this.propertyValidator1 =
                A.Fake<IPropertyValidator>();

            this.propertyValidator2 =
                A.Fake<IPropertyValidator>();

            this.validatableObjectAdapter =
                A.Fake<IValidatableObjectAdapter>();

            this.validatorFactory =
                A.Fake<IPropertyValidatorFactory>();

            A.CallTo(() => this.validatorFactory.GetValidators(typeof(ModelUnderTest)))
               .Returns(new[] { this.propertyValidator1, this.propertyValidator2 });

            this.validator =
                new DataAnnotationsValidator(typeof(ModelUnderTest), this.validatorFactory, this.validatableObjectAdapter);

            var adapterFactory = new DefaultPropertyValidatorFactory(new IDataAnnotationsValidatorAdapter[]
            {
                new RangeValidatorAdapter(),
                new RegexValidatorAdapter(),
                new RequiredValidatorAdapter(),
                new StringLengthValidatorAdapter(),
                new OopsAdapter()
            });

            var adapter = A.Fake<IValidatableObjectAdapter>();

            this.factory = new DataAnnotationsValidatorFactory(adapterFactory, adapter);
        }

        [Fact]
        public void Should_get_property_validators_from_factory()
        {
            // Given
            var instance = new ModelUnderTest();
            var context = new NancyContext();

            // When
            this.validator.Validate(instance, context);

            // Then
            A.CallTo(() => this.validatorFactory.GetValidators(typeof(ModelUnderTest))).MustHaveHappened();
        }

        [Fact]
        public void Should_invoke_all_validators_returned_by_factory_with_instance_being_validated()
        {
            // Given
            var instance = new ModelUnderTest();
            var context = new NancyContext();

            // When
            this.validator.Validate(instance, context);

            // Then
            A.CallTo(() => this.propertyValidator1.Validate(instance, context)).MustHaveHappened();
            A.CallTo(() => this.propertyValidator2.Validate(instance, context)).MustHaveHappened();
        }

        [Fact]
        public void Should_invoke_validatable_object_adapter_with_instance_being_validated()
        {
            // Given
            var instance = new ModelUnderTest();
            var context = new NancyContext();

            // When
            this.validator.Validate(instance, context);

            // Then
            A.CallTo(() => this.validatableObjectAdapter.Validate(instance, context)).MustHaveHappened();
        }

        [Fact]
        public void Should_contain_validation_results_from_all_validators()
        {
            // Given
            var instance = new ModelUnderTest();
            var context = new NancyContext();

            var result1 = new ModelValidationError("Foo", string.Empty);
            var result2 = new ModelValidationError("Bar", string.Empty);
            var result3 = new ModelValidationError("Baz", string.Empty);

            A.CallTo(() => this.propertyValidator1.Validate(instance, context)).Returns(new[] { result1 });
            A.CallTo(() => this.propertyValidator2.Validate(instance, context)).Returns(new[] { result2, result3 });

            // When
            var results = this.validator.Validate(instance, context);

            // Then
            results.Errors.Count().ShouldEqual(3);
        }

        [Fact]
        public void Should_contain_validation_result_from_validatable_object_adapter()
        {
            // Given
            var instance = new ModelUnderTest();
            var result = new ModelValidationError("Foo", string.Empty);
            var context = new NancyContext();

            A.CallTo(() => this.validatableObjectAdapter.Validate(instance, context)).Returns(new[] { result });

            // When
            var results = this.validator.Validate(instance, context);

            // Then
            results.Errors.Count().ShouldEqual(1);
            results.Errors.Keys.Contains("Foo").ShouldBeTrue();
        }

        [Fact]
        public void Should_return_descriptor_with_rules_from_all_validators()
        {
            // Given
            var rule1 = new ModelValidationRule(string.Empty, s => string.Empty, new[] { "One" });
            var rule2 = new ModelValidationRule(string.Empty, s => string.Empty, new[] { "Two" });
            var rule3 = new ModelValidationRule(string.Empty, s => string.Empty, new[] { "Three" });

            A.CallTo(() => this.propertyValidator1.GetRules()).Returns(new[] { rule1 });
            A.CallTo(() => this.propertyValidator2.GetRules()).Returns(new[] { rule2, rule3 });

            // When
            var descriptor = this.validator.Description;

            // Then
            descriptor.Rules.Count().ShouldEqual(3);
        }

        [Fact]
        public void Should_not_throw_when_no_validation_attributes_exist()
        {
            // Given, When
            var ex = Record.Exception(() => this.factory.Create(typeof(string)));

            // Then
            ex.ShouldBeNull();
        }

        [Fact]
        public void Should_invoke_validation()
        {
            // Given
            var subject = this.factory.Create(typeof(TestModel));
            var instance = new TestModel { Age = "yeah" };

            // When
            var result = subject.Validate(instance, new NancyContext());

            // Then
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldHaveCount(3);
        }

        [Fact]
        public void Description_should_be_correct()
        {
            // Given, When
            var subject = this.factory.Create(typeof(TestModel));

            // Then
            subject.Description.ShouldNotBeNull();
            subject.Description.Rules.SelectMany(r => r.Value).ShouldHaveCount(10);
        }

        [Fact]
        public void Should_read_range_annotation()
        {
            // Given, When
            var subject = this.factory.Create(typeof(TestModel));

            // Then
            subject.Description.Rules.SelectMany(r => r.Value).ShouldHave(r => r.RuleType == "Comparison" && r.MemberNames.Contains("Value"));
            subject.Description.Rules.SelectMany(r => r.Value).ShouldHave(r => r.RuleType == "Comparison" && r.MemberNames.Contains("Value"));
        }

        [Fact]
        public void Should_read_derived_range_annotation()
        {
            // Given, When
            var subject = this.factory.Create(typeof(TestModelWithDerivedDataAnnotations));

            // Then
            subject.Description.Rules.SelectMany(r => r.Value).ShouldHave(r => r.RuleType == "Comparison" && r.MemberNames.Contains("Value"));
            subject.Description.Rules.SelectMany(r => r.Value).ShouldHave(r => r.RuleType == "Comparison" && r.MemberNames.Contains("Value"));
        }

        [Fact]
        public void Should_read_regex_annotation()
        {
            // Given, When
            var subject = this.factory.Create(typeof(TestModel));

            // Then
            subject.Description.Rules.SelectMany(r => r.Value).ShouldHave(r => r.RuleType == "Regex" && r.MemberNames.Contains("Age"));
        }

        [Fact]
        public void Should_read_derived_regex_annotation()
        {
            // Given, When
            var subject = this.factory.Create(typeof(TestModelWithDerivedDataAnnotations));

            // Then
            subject.Description.Rules.SelectMany(r => r.Value).ShouldHave(r => r.RuleType == "Regex" && r.MemberNames.Contains("Age"));
        }

        [Fact]
        public void Should_read_required_annotation()
        {
            // Given, When
            var subject = this.factory.Create(typeof(TestModel));

            // Then
            subject.Description.Rules.SelectMany(r => r.Value).ShouldHave(r => r.RuleType == "NotNull" && r.MemberNames.Contains("FirstName"));
            subject.Description.Rules.SelectMany(r => r.Value).ShouldHave(r => r.RuleType == "NotEmpty" && r.MemberNames.Contains("FirstName"));
        }

        [Fact]
        public void Should_read_derived_required_annotation()
        {
            // Given, When
            var subject = this.factory.Create(typeof(TestModelWithDerivedDataAnnotations));

            // Then
            subject.Description.Rules.SelectMany(r => r.Value).ShouldHave(r => r.RuleType == "NotNull" && r.MemberNames.Contains("FirstName"));
            subject.Description.Rules.SelectMany(r => r.Value).ShouldHave(r => r.RuleType == "NotEmpty" && r.MemberNames.Contains("FirstName"));
        }

        [Fact]
        public void Should_read_string_length_annotation()
        {
            // Given, When
            var subject = this.factory.Create(typeof(TestModel));

            // Then
            subject.Description.Rules.SelectMany(r => r.Value).ShouldHave(r => r.RuleType == "StringLength" && r.MemberNames.Contains("FirstName"));
        }

        [Fact]
        public void Should_read_derived_string_length_annotation()
        {
            // Given, When
            var subject = this.factory.Create(typeof(TestModelWithDerivedDataAnnotations));

            // Then
            subject.Description.Rules.SelectMany(r => r.Value).ShouldHave(r => r.RuleType == "StringLength" && r.MemberNames.Contains("FirstName"));
        }

        [Fact]
        public void Should_use_custom_validator()
        {
            // Given, When
            var subject = this.factory.Create(typeof(TestModel));

            // Then
            subject.Description.Rules.SelectMany(r => r.Value).ShouldHave(r => r.RuleType == "Oops" && r.MemberNames.Contains(string.Empty));
        }

        [Fact]
        public void Should_use_display_attribute()
        {
            // Given
            var subject = this.factory.Create(typeof(TestModel));
            var instance = new TestModel { FirstName = "name", LastName = "a long name", Age = "1" };

            // When
            var result = subject.Validate(instance, new NancyContext());

            // Then
            result.IsValid.ShouldBeFalse();
            result.Errors["LastName"][0].ErrorMessage.ShouldContain("Last Name");
        }

        [Fact]
        public void Should_use_displayname_attribute()
        {
            // Given
            var subject = this.factory.Create(typeof(TestModel));
            var instance = new TestModel { FirstName = "a long name", Age = "1" };

            // When
            var result = subject.Validate(instance, new NancyContext());

            // Then
            result.IsValid.ShouldBeFalse();
            result.Errors["FirstName"][0].ErrorMessage.ShouldContain("First Name");
        }

        [OopsValidation]
        private class TestModel : IValidatableObject
        {
            [DisplayName("First Name")]
            [Required]
            [StringLength(5)]
            public string FirstName { get; set; }

            [Display(Name = "Last Name")]
            [StringLength(5)]
            public string LastName { get; set; }

            [RegularExpression("\\d+")]
            [Required]
            public string Age { get; set; }

            [Range(0, 10)]
            public int Value { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                return Enumerable.Empty<ValidationResult>();
            }
        }

        public class ModelUnderTest
        {
        }

        private class OopsValidationAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                return new ValidationResult("Oops");
            }
        }

        private class OopsAdapter : DataAnnotationsValidatorAdapter
        {
            public OopsAdapter()
                : base("Oops")
            {
            }

            public override bool CanHandle(ValidationAttribute attribute)
            {
                return attribute.GetType() == typeof(OopsValidationAttribute);
            }

            protected override ModelValidationError GetValidationError(ValidationResult result, ValidationContext context, ValidationAttribute attribute)
            {
                return new ModelValidationError(new[] { string.Empty }, result.ErrorMessage);
            }
        }

        private class TestModelWithDerivedDataAnnotations
        {
            [DerivedRequired]
            [DerivedStringLength(5)]
            public string FirstName { get; set; }

            [DerivedRegularExpression("\\d+")]
            [DerivedRequired]
            public string Age { get; set; }

            [DerivedRange(0, 10)]
            public int Value { get; set; }
        }

        public class DerivedRequiredAttribute : RequiredAttribute
        {
        }

        public class DerivedStringLengthAttribute : StringLengthAttribute
        {
            public DerivedStringLengthAttribute(int maximumLength)
                : base(maximumLength)
            {
            }
        }

        public class DerivedRegularExpressionAttribute : RegularExpressionAttribute
        {
            public DerivedRegularExpressionAttribute(string pattern)
                : base(pattern)
            {
            }
        }

        public class DerivedRangeAttribute : RangeAttribute
        {
            public DerivedRangeAttribute(int minimum, int maximum)
                : base(minimum, maximum)
            {
            }

            public DerivedRangeAttribute(double minimum, double maximum)
                : base(minimum, maximum)
            {
            }

            public DerivedRangeAttribute(Type type, string minimum, string maximum)
                : base(type, minimum, maximum)
            {
            }
        }
    }
}