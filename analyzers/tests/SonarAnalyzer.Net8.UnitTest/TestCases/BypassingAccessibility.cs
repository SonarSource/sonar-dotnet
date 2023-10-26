// https://github.com/SonarSource/sonar-dotnet/issues/8153
namespace Repro_8153
{
    using System.Runtime.CompilerServices;
    using static System.Runtime.CompilerServices.UnsafeAccessorKind;
    using UnsafeAccessorAttributeAlias = System.Runtime.CompilerServices.UnsafeAccessorAttribute;
    using UnsafeAccessorKindAlias = System.Runtime.CompilerServices.UnsafeAccessorKind;

    class ZeroOverheadMemberAccess
    {
        [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "aPrivateStaticField")]        // FN
        extern static ref string M1(UserData obj);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = nameof(UserData.aPublicField))]      // Compliant, the field is public.
                                                                                              // A new rule is needed for this. See: https://github.com/SonarSource/sonar-dotnet/issues/8258
        extern static ref string M2(UserData obj);

        [UnsafeAccessorAttribute(UnsafeAccessorKind.Field, Name = "aProtectedInternalField")] // FN
        extern static ref string M3(UserData obj);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "aProtectedField")]                  // FN
        extern static ref string M4(UserData obj);

        [UnsafeAccessor(UnsafeAccessorKindAlias.Field, Name = "aPrivateProtectedField")]      // FN
        extern static ref string M5(UserData obj);

        [UnsafeAccessorAttributeAlias(UnsafeAccessorKind.Field, Name = "aPrivateField")]      // FN
        extern static ref string M6(UserData obj);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "<APrivateProperty>k__BackingField")]               // FN
        extern static ref string M7(UserData obj);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "<APublicPropertyWithPrivateGet>k__BackingField")]  // FN
        extern static ref string M8(UserData obj);

        [UnsafeAccessor(UnsafeAccessorKind.Constructor)] // FN
        extern static UserData CallPrivateConstructor();

        [UnsafeAccessor(UnsafeAccessorKind.Constructor)] // FN
        extern static UserData CallDifferentPrivateConstructor(string s);

        [UnsafeAccessor(UnsafeAccessorKind.Constructor)] // FN
        extern static UserData CallProtectedConstructor(int i);

        class UserData
        {
            private static int aPrivateStaticField;

            public int aPublicField;
            protected internal int aProtectedInternalField;
            protected int aProtectedField;
            private protected int aPrivateProtectedField;
            private int aPrivateField;
            private int APrivateProperty { get; }

            private UserData() { }
            private UserData(string s) { }
            protected UserData(int i) { }

            public int APublicPropertyWithPrivateGet { private get; set; }
        }
    }
}
