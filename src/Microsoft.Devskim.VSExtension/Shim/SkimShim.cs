﻿// Copyright (C) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.DevSkim;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Microsoft.DevSkim.VSExtension
{
    /// <summary>
    /// Shim around DevSkim. Parses code applies rules
    /// </summary>
    public class SkimShim
    {
        public SkimShim()
        {
            processor = new RuleProcessor();
            processor.AllowSuppressions = true;

            ruleset = new Ruleset();

            LoadRules();
        }

        #region Public Static Methods

        /// <summary>
        /// Reapllys settings
        /// </summary>
        public static void ApplySettings()
        {
            _instance.LoadRules();            
        }

        /// <summary>
        /// Indicates if there are more than issues on the given line
        /// </summary>
        /// <param name="text">line of code</param>
        /// <param name="contenttype">VS Content Type</param>
        /// <returns>True if more than one issue exists</returns>
        public static bool HasProblems(string text, string contenttype)
        {
            return (_instance.processor.Analyze(text, ContentType.GetLanguages(contenttype)).Length > 1);            
        }

        /// <summary>
        /// Analyze text for issues
        /// </summary>
        /// <param name="text">line of code</param>
        /// <param name="contenttype">VS Content Type</param>
        /// <returns>List of actionable and non-actionable issues</returns>
        public static Problem[] Analyze(string text, string contenttype)
        {
            List<Problem> results = new List<Problem>();
            Issue[] issues = _instance.processor.Analyze(text, ContentType.GetLanguages(contenttype));

            // Add matches for errors
            foreach (Issue issue in issues)
                results.Add(new Problem() { Actionable = true, Issue = issue});
            
            // Get list of IDs on the ignore list
            string pattern = @"\s*DevSkim:\s+ignore\s([^\s]+)(\s+until\s\d{4}-\d{2}-\d{2}|)";
            Regex reg = new Regex(pattern);

            Match match = reg.Match(text);
            if (match.Success)
            {
                int suppressStart = match.Index;
                int suppressLength = match.Length;

                string idString = match.Groups[1].Value.Trim();
                
                // parse Ids.                
                if (idString != "all")
                {
                    int index = match.Groups[1].Index;
                    string[] ids = idString.Split(',');
                    foreach(string id in ids)
                    {
                        Issue issue = new Issue();
                        issue.Index = index;
                        issue.Length = id.Length;
                        issue.Rule = _instance.ruleset.First(x => x.Id == id);

                        results.Add(new Problem() { Actionable = false, Issue = issue });

                        index += id.Length + 1;
                    }
                }
            }

            return results.ToArray();
        }    

        #endregion

        #region Private
        /// <summary>
        /// Reloads rules based on settings
        /// </summary>
        private void LoadRules()
        {
            Settings set = Settings.GetSettings();

            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            dir = Path.Combine(Path.Combine(dir, "Content"), "rules");
            if (set.UseDefaultRules)
                ruleset.AddDirectory(dir, null);

            if (set.UseCustomRules)
                ruleset.AddDirectory(set.CustomRulesPath, "custom");

            processor.Rules = ruleset;

            processor.SeverityLevel = Severity.Critical;

            if (set.EnableImportantRules) processor.SeverityLevel |= Severity.Important;
            if (set.EnableModerateRules) processor.SeverityLevel |= Severity.Moderate;
            if (set.EnableLowRules) processor.SeverityLevel |= Severity.Low;

            if (set.EnableInformationalRules) processor.SeverityLevel |= Severity.Informational;
            if (set.EnableDefenseInDepthRules) processor.SeverityLevel |= Severity.DefenseInDepth;
            if (set.EnableManualReviewRules) processor.SeverityLevel |= Severity.ManualReview;

        }

        private RuleProcessor processor;
        private Ruleset ruleset;

        private static SkimShim _instance = new SkimShim();

        #endregion
    }
}