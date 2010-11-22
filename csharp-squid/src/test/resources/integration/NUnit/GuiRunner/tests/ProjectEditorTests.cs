// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org
// ****************************************************************
using System;
using System.IO;
using System.Windows.Forms;
using NUnit.Framework;
using NUnit.Util;
using NUnit.TestUtilities;

namespace NUnit.Gui.Tests
{
	/// <summary>
	/// Summary description for ProjectEditorTests.
	/// </summary>
	[TestFixture]
	public class ProjectEditorTests : FormTester
	{
		private NUnitProject project;
		private ProjectEditor editor;

		[SetUp]
		public void CreateFixtureObjects()
		{
			project = new NUnitProject( "temp.nunit" );
			project.Configs.Add( "Debug" );
			project.Configs.Add( "Release" );

			editor = new ProjectEditor( project );
			
			this.Form = editor;
		}

		[TearDown]
		public void Close()
		{
            //if ( editor != null )
            //    editor.Close();
		}
		[Test]
		public void CheckControls()
		{
			AssertControlExists( "projectPathLabel" );
			AssertControlExists( "projectBaseTextBox" );
			AssertControlExists( "groupBox1" );
			AssertControlExists( "closeButton" );

			ControlTester tester = new ControlTester( Controls["groupBox1"] );
			tester.AssertControlExists( "configComboBox" );
			tester.AssertControlExists( "editConfigsButton" );
			tester.AssertControlExists( "projectTabControl" );
		}

        [Test, Platform(Exclude="Net-1.0", Reason="Sporadic NullReferenceException in SafeNativeMethods.ShowWindow")]
        public void InitialFieldValues()
		{
			editor.Show();
            Assert.AreEqual( Path.GetFullPath( "temp.nunit" ), GetText( "projectPathLabel" ) );
            Assert.AreEqual( Environment.CurrentDirectory, GetText( "projectBaseTextBox" ) );
		}

		[Test]
		public void SetProjectBase()
		{
			editor.Show();
            TextBox textBox = TextBoxes["projectBaseTextBox"];
            textBox.Focus();
			string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            textBox.Text = appDataPath;
            Buttons["closeButton"].Focus();
            Assert.AreEqual( appDataPath, project.BasePath );
		}
	}
}
