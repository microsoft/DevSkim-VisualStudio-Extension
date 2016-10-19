// Copyright (C) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;

namespace DevSkim
{
    /// <summary>
    /// DevSkim specifig tag
    /// </summary>
    class DevSkimTag : IErrorTag
    {        
        public DevSkimTag(Rule rule)
        {
            Rule = rule;
            ErrorType = PredefinedErrorTypeNames.OtherError;
            ToolTipContent = new DevSkimToolTip(Rule);
        }

        public string ErrorType { get; set; }

        public object ToolTipContent { get; set; }

        public Rule Rule { get; set; }
    }
}
