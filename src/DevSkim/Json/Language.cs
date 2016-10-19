// Copyright(C) Microsoft.All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace DevSkim
{
    /// <summary>
    /// Helper class for language based commenting and type converion
    /// </summary>
    public class Language
    {
        private Language()
        {
            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            string fileName = Path.Combine(Path.Combine(dir, "Content"), "Comments.json");
            using (StreamReader file = File.OpenText(fileName))
            {
                Comments = JsonConvert.DeserializeObject<List<Comment>>(file.ReadToEnd());
            }

            fileName = Path.Combine(Path.Combine(dir, "Content"), "ContentTypes.json");
            using (StreamReader file = File.OpenText(fileName))
            {
                ContentTypes = JsonConvert.DeserializeObject<List<ContentTypeRecord>>(file.ReadToEnd());
            }

        }

        /// <summary>
        /// Decorates given string with language specific comments
        /// </summary>
        /// <param name="textToComment">text to be decorated</param>
        /// <param name="contentType">VS Content Type</param>
        /// <returns>Commented string</returns>
        public static string Comment(string textToComment, string contentType)
        {
            string result = string.Empty;

            foreach(Comment comment in _instance.Comments)
            {
                foreach(string ct in comment.ContentTypes)
                {
                    if (ct == contentType)
                    {
                        result = string.Concat(comment.Preffix, textToComment, comment.Suffix);
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(result))
                    break;
            }

            return result;
        }

        /// <summary>
        /// Returns list of languages coresponding to Visual Studio content type
        /// </summary>
        /// <param name="vsContentType">Content Type</param>
        /// <returns>List of programming languages</returns>
        public static string[] ContentTypeToLanguageList(string vsContentType)
        {            
            foreach(ContentTypeRecord record in _instance.ContentTypes)
            {
                if (record.VSType == vsContentType)
                {
                   return record.DSTypes;                    
                }
            }

            return new string[] { };
        }

        private static Language _instance = new Language();

        private List<Comment> Comments;
        private List<ContentTypeRecord> ContentTypes;
    }
}
