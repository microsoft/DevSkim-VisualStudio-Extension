// Copyright (C) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DevSkim
{
    /// <summary>
    /// Heart of DevSkim. Parses code applies rules
    /// </summary>
    public class RuleProcessor
    {
        /// <summary>
        /// Creates instance of RuleProcessor
        /// Used only internaly, as rule processor acts like a singleton
        /// </summary>
        private RuleProcessor()
        {
            Settings set = Settings.GetSettings();

            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            _rules = new List<Rule>();            
            if (set.UseDefaultRules)
                _rules.AddRange(RuleLoader.ParseDirectory(Path.Combine(Path.Combine(dir, "Content"), "rules")));

            if (set.UseCustomRules)
                _rules.AddRange(RuleLoader.ParseDirectory(set.CustomRulesPath));

            _rulesCache = new Dictionary<string, List<Rule>>();
        }       

        #region Public Static Methods

        /// <summary>
        /// Test given source code line for issues
        /// </summary>
        /// <param name="text">Source code line</param>
        /// <param name="index">Position in text where to start the scan</param>
        /// <param name="contenttype">Visual Studio content type</param>
        /// <returns>MatchRecord with infomartion of identified issue</returns>
        public static MatchResult IsMatch(string text, int index, string contenttype)
        {
            _instance._textLine = text;
            MatchResult result = _instance.FindMatch(text.Substring(index), contenttype);
            if (result.Location > -1)
                result.Location += index;

            return result;
        }

        /// <summary>
        /// Applies given fix on the provided source code line
        /// </summary>
        /// <param name="text">Source code line</param>
        /// <param name="fixRecord">Fix record to be applied</param>
        /// <returns>Fixed source code line</returns>
        public static string Fix(string text, FixRecord fixRecord)
        {
            Regex regex = new Regex(fixRecord.Search);
            return regex.Replace(text, fixRecord.Replace);
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Test given text for issues
        /// </summary>
        /// <param name="text">Source code</param>
        /// <param name="contenttype">Visual Studio content type</param>
        /// <returns>MatchRecord with infomartion of identified issue</returns>
        private MatchResult FindMatch(string text, string contenttype)
        {
            // Get rules for the given content type
            IEnumerable<Rule> rules = GetRulesForContentType(contenttype);
            MatchResult result = new MatchResult() { Success = false };

            // Go through each rule
            foreach(Rule r in rules)
            {
                // Go through each matching pattern of the rule
                foreach(PatternRecord p in r.Patterns)
                {
                    // Type == Substring 
                    if (p.Type == PatternType.Substring)
                    {
                        result.Location = text.ToLower().IndexOf(p.Pattern.ToLower());
                        result.Length = p.Pattern.Length;
                        if (result.Location > -1)
                        {
                            result.Success = true;
                            result.Rule = r;
                            break; // from pattern loop
                        }
                    }
                    // Type == Regex
                    else if (p.Type == PatternType.Regex)
                    {
                        RegexOptions reopt = RegexOptions.None;
                        if (p.Modifiers != null)
                        {
                            reopt |= (p.Modifiers.Contains("ignorecase")) ? RegexOptions.IgnoreCase : RegexOptions.None;
                            reopt |= (p.Modifiers.Contains("multiline")) ? RegexOptions.Multiline : RegexOptions.None;                            
                        }
                        
                        Regex patRegx = new Regex(p.Pattern, reopt);
                        Match m = patRegx.Match(text);
                        if (m.Success)
                        {
                            result.Success = true;
                            result.Rule = r;
                            result.Location = m.Index;
                            result.Length = m.Length;
                            break; // from pattern loop                 
                        }
                    }                    
                }

                // We got matching rule. Let's see if we have a supression on the line
                if (result.Success)
                {
                    Suppressor supp = new Suppressor(_textLine, contenttype);
                    // If rule is being suppressed then clear the MatchResult
                    if (supp.IsRuleSuppressed(result.Rule.Id))
                    {
                        result = new MatchResult();
                    }
                    // Otherwise break out of the loop as we found an issue.
                    // So, no need to scan for more.
                    else
                    {                        
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Filters the rules for those matching the content type.
        /// Resolves all the overrides
        /// </summary>
        /// <param name="vsContentType">Visual Studio content type</param>
        /// <returns>List of rules</returns>
        private IEnumerable<Rule> GetRulesForContentType(string vsContentType)
        {
            // Do we have the ruleset alrady in cache? If so return it
            if (_rulesCache.ContainsKey(vsContentType))
                return _rulesCache[vsContentType];

            // Otherwise preprare the rules for the content type and store it in cache.
            string[] languageList = Language.ContentTypeToLanguageList(vsContentType);
            List<Rule> filteredRules = new List<Rule>();
            
            foreach (Rule r in _rules)
            {
                if (r.AppliesTo != null && ArrayContains(r.AppliesTo, languageList))
                {
                    // Put rules with defined contenty type (AppliesTo) on top
                    filteredRules.Insert(0, r);
                }
                else if (r.AppliesTo == null)
                {
                    foreach (PatternRecord p in r.Patterns)
                    {
                        // If applies to is defined and matching put those rules first
                        if (p.AppliesTo != null && ArrayContains(p.AppliesTo, languageList))
                        {
                            filteredRules.Insert(0, r);
                            break;
                        }
                        // Generic rules goes to the end of the list
                        if (p.AppliesTo == null)
                        {
                            filteredRules.Add(r);
                            break;
                        }
                    }
                }
            }

            // Now deal with rule overrides. 
            List<string> idsToRemove = new List<string>();
            foreach(Rule rule in filteredRules)
            {
                if (rule.Overrides != null)
                {
                    foreach (string r in rule.Overrides)
                    {
                        // Mark every rule that is overriden
                        if (!idsToRemove.Contains(r))
                            idsToRemove.Add(r);
                    }
                }
            }

            // Remove marked rules
            foreach (string id in idsToRemove)
            {
                filteredRules.Remove(filteredRules.Find(x => x.Id == id));
            }

            // Add the list to the cache so we safe time on the next call
            _rulesCache.Add(vsContentType, filteredRules);

            return filteredRules;
        }

        /// <summary>
        /// Tests if array contains given elements
        /// </summary>
        /// <param name="source">Source array</param>
        /// <param name="comps">List of elements to look for</param>
        /// <returns>True if source array contains element from comps array</returns>
        private bool ArrayContains(string[] source, string[] comps)
        {            
            foreach(string c in comps)
            {
                if (source.Contains(c))
                    return true;
            }

            return false;
        }

        #endregion

        #region Fields 

        private List<Rule> _rules;

        /// <summary>
        /// Property for storing complete line of code that is being scanned
        /// </summary>
        private string _textLine;

        /// <summary>
        /// Cache for rules filtered by content type
        /// </summary>
        private Dictionary<string, List<Rule>> _rulesCache;

        /// <summary>
        /// Singleton instance
        /// </summary>
        private static RuleProcessor _instance = new RuleProcessor();
        #endregion
    }
}
