// Copyright (C) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DevSkim
{
    internal class SuppressSuggestedAction : ISuggestedAction
    {
        private readonly ITrackingSpan _span;
        private readonly ITextSnapshot _snapshot;
        private readonly Rule _rule;
        private readonly DateTime _suppDate = DateTime.MinValue;
        private readonly string _code;
        private readonly string _display = string.Empty;

        public SuppressSuggestedAction(ITrackingSpan span, Rule rule) : this(span, rule, -1)
        {

        }

        public SuppressSuggestedAction(ITrackingSpan span, Rule rule, int days)
        {
            _rule = rule;
            _span = span;
            _snapshot = span.TextBuffer.CurrentSnapshot;
            _code = span.GetText(_snapshot);
            
            if (days > 0)
            {
                _display = string.Format(" ({0} days)", days);
                _suppDate = DateTime.Now.AddDays(days);
            }
            
            if (_rule == null)
            {
                _display = _display.Insert(0, "Suppress all issues");
            }
            else
            {                
                _display = _display.Insert(0, "Suppress this issue");
            }
        }
        public string DisplayText
        {
            get
            {
                return _display;
            }
        }

        public string IconAutomationText
        {
            get
            {
                return null;
            }
        }

        ImageMoniker ISuggestedAction.IconMoniker
        {
            get
            {
                return default(ImageMoniker);
            }
        }

        public string InputGestureText
        {
            get
            {
                return null;
            }
        }

        public bool HasActionSets
        {
            get
            {
                return false;
            }
        }

        public Task<IEnumerable<SuggestedActionSet>> GetActionSetsAsync(CancellationToken cancellationToken)
        {
            return null;
        }

        public bool HasPreview
        {
            get
            {
                return false;
            }
        }

        public Task<object> GetPreviewAsync(CancellationToken cancellationToken)
        {
            return null;
        }

        public void Dispose()
        {
        }

        public void Invoke(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            string fixedCode = string.Empty;
            Suppressor supp = new Suppressor(_code, _snapshot.ContentType.TypeName);
            if (_rule == null)
            {
                fixedCode = supp.SuppressAll(_suppDate);
            }
            else
            {
                fixedCode = supp.SuppressRule(_rule.Id, _suppDate);
            }

            _span.TextBuffer.Replace(_span.GetSpan(_snapshot), fixedCode);
        }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            telemetryId = Guid.Empty;
            return false;
        }
    }
}
