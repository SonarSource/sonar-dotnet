using System;
using Microsoft.Maui.Controls;

#nullable enable

namespace MemberShouldBeStatic
{
    // https://sonarsource.atlassian.net/browse/NET-4640
    // The XAML-generated partial declaration supplies the base type.
    public partial class NewAppVersionAvailableInfoBanner : Grid
    {
        private partial void MauiElement_Event(object? sender, EventArgs e);
    }

    public partial class NotMauiElement
    {
        private partial void NotMauiElement_Event(object? sender, EventArgs e);
    }
}
