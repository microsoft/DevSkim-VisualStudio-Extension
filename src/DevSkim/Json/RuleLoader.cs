// Copyright (C) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace DevSkim
{
    /// <summary>
    /// Provides functionality for loading the rules
    /// </summary>
    class RuleLoader
    {
        /// <summary>
        /// Parse a directory with rules files and loads the rules
        /// </summary>
        /// <param name="path">Path to rules folder</param>
        /// <returns>Return list of Rules objects</returns>
        public static List<Rule> ParseDirectory(string path)
        {                        
            List<Rule> result = new List<Rule>();

            if (Directory.Exists(path))
            {
                foreach (string fileName in Directory.EnumerateFileSystemEntries(path, "*.json", SearchOption.AllDirectories))
                {
                    List<Rule> ruleList = new List<Rule>();
                    using (StreamReader file = File.OpenText(fileName))
                    {
                        ruleList = JsonConvert.DeserializeObject<List<Rule>>(file.ReadToEnd());
                        foreach (Rule r in ruleList)
                        {
                            r.File = fileName;
                            foreach (PatternRecord p in r.Patterns)
                            {
                                if (p.Type == PatternType.Regex_Word || p.Type == PatternType.String)
                                {
                                    p.Type = PatternType.Regex;
                                    p.Pattern = string.Format(@"\b{0}\b", p.Pattern);
                                }
                            }
                        }

                        result.AddRange(ruleList);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Expands lists of AppliesTo.
        /// Replaces extensions variables (ALL_SOURCE, DOTNET etc) with the actual extensions
        /// </summary>
        /// <param name="list">List of AppliesTo to extensions with variables</param>
        /// <returns>Extended list of AppliesTo extensions</returns>
        private static string[] ExpandAppliesTo(string[] list)
        {
            List<string> result = new List<string>();
            //result.AddRange(list);

            foreach (string line in list)
            {
                result.AddRange(AppliesToStringToArray(
                                ReplaceExtensionVariables(
                                line)));
            }

            return result.ToArray();
        }

        /// <summary>
        /// Replaces extensions variables (ALL_SOURCE, DOTNET etc) with the actual extensions
        /// </summary>
        /// <param name="rawfield"></param>
        /// <returns></returns>
        private static string ReplaceExtensionVariables(string rawfield)
        {
            string result = string.Empty;

            if (rawfield != null)
            {
                result = rawfield;
                result = result.Replace("ALL_SOURCE", "*");
                result = result.Replace("$JAVASCRIPT", "*.js,*.cshtml,*.html,*.htm,*.coffee,*.ts,*.dart,*.ls");
                result = result.Replace("$JAVA", "*.java");
                result = result.Replace("$HTML", "*.html,*.htm,*.cshtml,*.asp,*.aspx,*.asmx");
                result = result.Replace("$DOTNET", "*.cs,*.cshtml,*.fs,*.asp,*.aspx,*.asmx");
                result = result.Replace("$CPP", "*.c,*.h,*.cpp,*.hpp");
                result = result.Replace("$IOS", "*.m,*.h,*.swift");
                result = result.Replace("$PHP", "*.php");
                result = result.Replace("$POWERSHELL", "*.ps1,*.psd1,*.psm1,*.sp1xml,*.clixml,*.psc1,*.pssc");
                result = result.Replace("$WEBAPP", "*.cshtml,*.asp,*.aspx,*.asmx,*.py,*.rb,*.js");
            }
            return result;
        }

        /// <summary>
        /// Splits comma separated list of extensions to array
        /// </summary>
        /// <param name="appliesTo">comma sepparated list</param>
        /// <returns>List of extensions</returns>
        private static string[] AppliesToStringToArray(string appliesTo)
        {
            string[] result = appliesTo.Split(',');
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = result[i].Replace("*", "");
                result[i] = result[i].Trim();
            }

            return result;
        }
    }
}
