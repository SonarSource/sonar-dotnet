﻿namespace Nancy.Tests.Unit.Conventions
{
    using System;
    using System.Linq;
    using FakeItEasy;
    using Nancy.Conventions;

    using Xunit;

    public class DefaultAcceptHeaderCoercionConventionsFixture
    {

        private readonly NancyConventions conventions;
        private readonly DefaultAcceptHeaderCoercionConventions acceptHeaderConventions;

        public DefaultAcceptHeaderCoercionConventionsFixture()
        {
            var typeCatalog = A.Fake<ITypeCatalog>();
            A.CallTo(() => typeCatalog.GetTypesAssignableTo(A<Type>._, A<TypeResolveStrategy>._)).Returns(ArrayCache.Empty<Type>());

            this.conventions = new NancyConventions(typeCatalog);
            this.acceptHeaderConventions = new DefaultAcceptHeaderCoercionConventions();
        }

        [Fact]
        public void Should_not_be_valid_when_conventions_is_null()
        {
            // Given
            this.conventions.AcceptHeaderCoercionConventions = null;

            // When
            var result = this.acceptHeaderConventions.Validate(this.conventions);

            // Then
            result.Item1.ShouldBeFalse();
        }
        [Fact]
        public void Should_return_correct_error_message_when_not_be_valid_because_conventions_is_null()
        {
            // Given
            this.conventions.AcceptHeaderCoercionConventions = null;

            // When
            var result = this.acceptHeaderConventions.Validate(this.conventions);

            // Then
            result.Item2.ShouldEqual("The accept header coercion conventions cannot be null.");
        }

        [Fact]
        public void Should_add_conventions_when_initialised()
        {
            // Given, When
            this.acceptHeaderConventions.Initialise(this.conventions);

            // Then
            this.conventions.AcceptHeaderCoercionConventions.Count.ShouldBeGreaterThan(0);
        }
    }
}