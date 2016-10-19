// Copyright(C) Microsoft.All rights reserved.
// Licensed under the MIT License.See LICENSE.txt in the project root for license information.

using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace DevSkim
{
    /// <summary>
    /// Interaction logic for DevSkimToolTip.xaml
    /// </summary>
    public partial class DevSkimToolTip : UserControl
    {
        public DevSkimToolTip(Rule rule)
        {
            InitializeComponent();
            System.Drawing.Color textColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolboxContentTextColorKey);
            System.Drawing.Color linkColor = VSColorTheme.GetThemedColor(ThemedDialogColors.HyperlinkColorKey);
            System.Drawing.Color subColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolboxContentTextColorKey);

            this.TitleBox.Text = rule.Name;

            this.MessageBox.Foreground = new SolidColorBrush(Color.FromRgb(textColor.R, textColor.G, textColor.B));                        
            this.MessageBox.Text = rule.Description;

            this.SeverityBox.Foreground = new SolidColorBrush(Color.FromRgb(subColor.R, subColor.G, subColor.B));
            this.SeverityBox.Text = string.Format("Severity: {0}",rule.Severity);            

            this.Url.Foreground = new SolidColorBrush(Color.FromRgb(linkColor.R, linkColor.G, linkColor.B));
            this.Url.NavigateUri = new Uri(rule.RuleInfo);
            
        }

        private void Url_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            IVsWindowFrame ppFrame;
            IVsWebBrowsingService browserService;
            browserService = Package.GetGlobalService(typeof(SVsWebBrowsingService)) as IVsWebBrowsingService;

            browserService.Navigate(this.Url.NavigateUri.AbsoluteUri, 0, out ppFrame);
        }
    }
}
