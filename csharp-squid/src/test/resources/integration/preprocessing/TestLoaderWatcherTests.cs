// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org.
// ****************************************************************

using System;
using NUnit.Framework;
using NUnit.Tests.Assemblies;

namespace NUnit.Util.Tests
{
	[TestFixture]
	public class TestLoaderWatcherTests
	{
		private readonly string assembly = MockAssembly.AssemblyPath;
		private MockAssemblyWatcher2 mockWatcher;
		private ITestLoader testLoader;
		private const string ReloadOnChangeSetting = "Options.TestLoader.ReloadOnChange";

		[SetUp]
		public void PreprareTestLoader()
		{
			// arrange
			mockWatcher = new MockAssemblyWatcher2();
			testLoader = new TestLoader(mockWatcher);
			testLoader.LoadProject(assembly);
		}

		[TearDown]
		public void CleanUpSettings()
		{
			Services.UserSettings.RemoveSetting(ReloadOnChangeSetting);
		}

		private void AssertWatcherIsPrepared()
		{
			Assert.IsTrue(mockWatcher.IsWatching);
			CollectionAssert.AreEquivalent(new string[] { assembly }, mockWatcher.AssembliesToWatch);
		}

		[Test]
		public void LoadShouldStartWatcher()
		{
			// act
			testLoader.LoadTest();

			// assert
			AssertWatcherIsPrepared();
		}

		[Test]
		public void ReloadShouldStartWatcher()
		{
			// arrange
			testLoader.LoadTest();
			mockWatcher.AssembliesToWatch = null;
			mockWatcher.IsWatching = false;

			// act
			testLoader.ReloadTest();

			// assert
			AssertWatcherIsPrepared();
		}

		[Test]
		public void UnloadShouldStopWatcherAndFreeResources()
		{
			// act
			testLoader.LoadTest();
			testLoader.UnloadTest();

			// assert
			Assert.IsFalse(mockWatcher.IsWatching);
			Assert.IsTrue(mockWatcher.AreResourcesFreed);
		}

		[Test]
		public void LoadShouldStartWatcherDepedningOnSettings()
		{
			// arrange
			Services.UserSettings.SaveSetting(ReloadOnChangeSetting, false);
			testLoader.LoadTest();

			// assert
			Assert.IsFalse(mockWatcher.IsWatching);
		}

		[Test]
		public void ReloadShouldStartWatcherDepedningOnSettings()
		{
			// arrange
			Services.UserSettings.SaveSetting(ReloadOnChangeSetting, false);
			testLoader.LoadTest();
			testLoader.ReloadTest();

			// assert
			Assert.IsFalse(mockWatcher.IsWatching);
		}
	}

	internal class MockAssemblyWatcher2 : IAssemblyWatcher
	{
		public bool IsWatching;
#if NET_2_0 || NET_4_0
        public System.Collections.Generic.IList<string> AssembliesToWatch;
#else
		public System.Collections.IList AssembliesToWatch;
#endif
		public bool AreResourcesFreed;

		public void Stop()
		{
			IsWatching = false;
		}

		public void Start()
		{
			IsWatching = true;
		}

#if NET_2_0 || NET_4_0
		public void Setup(int delayInMs, System.Collections.Generic.IList<string> assemblies)
#else
        public void Setup(int delayInMs, System.Collections.IList assemblies)
#endif
		{
			AssembliesToWatch = assemblies;
		}

		public void Setup(int delayInMs, string assemblyFileName)
		{
			Setup(delayInMs, new string[] {assemblyFileName});
		}

		public void FreeResources()
		{
			AreResourcesFreed = true;
		}

		public event AssemblyChangedHandler AssemblyChanged;
    }
}