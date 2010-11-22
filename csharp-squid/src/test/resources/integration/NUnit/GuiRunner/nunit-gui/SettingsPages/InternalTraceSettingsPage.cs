// ****************************************************************
// Copyright 2010, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org
// ****************************************************************

using System;
using NUnit.Core;

namespace NUnit.Gui.SettingsPages
{
    public partial class InternalTraceSettingsPage : NUnit.UiKit.SettingsPage
    {
        public InternalTraceSettingsPage(string key) : base(key)
        {
            InitializeComponent();
        }

        public override void LoadSettings()
        {
            traceLevelComboBox.SelectedIndex = (int)(InternalTraceLevel)settings.GetSetting("Options.InternalTraceLevel", InternalTraceLevel.Default);
            logDirectoryLabel.Text = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "logs");
        }

        public override void ApplySettings()
        {
           settings.SaveSetting("Options.InternalTraceLevel", (InternalTraceLevel)traceLevelComboBox.SelectedIndex);
        }
    }
}
