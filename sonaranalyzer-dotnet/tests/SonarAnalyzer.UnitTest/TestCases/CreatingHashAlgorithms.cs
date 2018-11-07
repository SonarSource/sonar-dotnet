namespace NS
{
    using System.Security.Cryptography;

    public class TestClass
    {
        // RSPEC 4790: https://jira.sonarsource.com/browse/RSPEC-4790
        public void ComputeHash()
        {
            // Review all instantiations of classes that inherit from HashAlgorithm, for example:
            HashAlgorithm hashAlgo = HashAlgorithm.Create();
//                                   ^^^^^^^^^^^^^^^^^^^^^^    {{Make sure that hashing data is safe here.}}
            HashAlgorithm hashAlgo2 = HashAlgorithm.Create("SHA1");
//                                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^    {{Make sure that hashing data is safe here.}}

            SHA1 sha = new SHA1CryptoServiceProvider();
//                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^    {{Make sure that hashing data is safe here.}}

            MD5 md5 = new MD5CryptoServiceProvider();
//                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^    {{Make sure that hashing data is safe here.}}

            // ...
        }

        public void AdditionalTests(SHA1CryptoServiceProvider sha1)
        {
            var myHash = new MyHashAlgorithm();     // Noncompliant
            myHash = new MyHashAlgorithm(123);      // Noncompliant

            myHash = MyHashAlgorithm.Create();      // Noncompliant
            myHash = MyHashAlgorithm.Create(42);    // Noncompliant

            myHash = MyHashAlgorithm.CreateHash();  // compliant - method name is not Create
            myHash = MyHashAlgorithm.DoCreate();    // compliant - method name is not Create

            // Other methods are not checked
            var hash = sha1.ComputeHash((byte[])null);
            hash = sha1.Hash;
            var canReuse = sha1.CanReuseTransform;
            sha1.Clear();
        }
    }

    public class MyHashAlgorithm : HashAlgorithm
//                                 ^^^^^^^^^^^^^
    {
        public MyHashAlgorithm() { }
        public MyHashAlgorithm(int data) { }
        public static MyHashAlgorithm Create() => null;
        public static MyHashAlgorithm Create(int data) => null;

        public static MyHashAlgorithm CreateHash() => null;
        public static MyHashAlgorithm DoCreate() => null;

        public override void Initialize() { /* no-op */ }
        protected override void HashCore(byte[] array, int ibStart, int cbSize) { /* no-op */ }
        protected override byte[] HashFinal()
        {
            throw new System.NotImplementedException();
        }
    }


    // Check reporting on partial classes. Should only report once.
    public partial class ParticalClassAglorithm : NS.MyHashAlgorithm, System.IDisposable
//                                                ^^^^^^^^^^^^^^^^^^
    {
        public void Dispose() { /* no-op */ }
    }

    internal interface IMarker { }
    internal interface IMarker2 { }
    public partial class ParticalClassAglorithm : IMarker, IMarker2
    {
    }

}
