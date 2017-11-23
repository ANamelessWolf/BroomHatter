using Nameless.Libraries.Yggdrasil.Aerith.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Nameless.Libraries.Yggdrasil.Asuna;
using System.Xml.Linq;
using Nameless.Libraries.Yggdrasil;
using Nameless.Libraries.Yggdrasil.Exceptions;
using Nameless.Libraries.Yggdrasil.Lilith;
using System.Text.RegularExpressions;

namespace Nameless.Apps.Broom
{
    public class SolutionFilter : AerithFilter
    {
        /// <summary>
        /// Defines the solution path
        /// </summary>
        public String SolutionPath;
        /// <summary>
        /// Get access to the option file
        /// </summary>
        public XNpc OptionsFile
        {
            get
            {
                string path = System.Reflection.Assembly.GetAssembly(typeof(SolutionFilter)).Location;
                path = Path.GetDirectoryName(path);
                return new XNpc("filter", new FileInfo(Path.Combine(path, "Filter.xml")));
            }
        }
        /// <summary>
        /// The list of unwanted paths on the solution
        /// </summary>
        public IEnumerable<string> UnWantedPaths;
        /// <summary>
        /// The list of unwanted extensions
        /// </summary>
        public IEnumerable<string> UnWantedExtensions;
        /// <summary>
        /// The list of unwanted extensions
        /// </summary>
        public IEnumerable<string> MatchExpressions;
        /// <summary>
        /// Creates a new Solution Filter
        /// </summary>
        /// <param name="solutionPath">The solution path</param>
        public SolutionFilter(string solutionPath)
        {
            try
            {
                this.SolutionPath = solutionPath;
                var data = this.OptionsFile;
                this.UnWantedPaths = this.GetValues(data.Document, "path_contains").Select(x => x.ToUpper());
                this.UnWantedExtensions = this.GetValues(data.Document, "extension_is").Select(x => x.ToUpper());
                this.MatchExpressions = this.GetValues(data.Document, "reg_exp_match");
            }
            catch (Exception)
            {
                throw new NamelessException("Invalid Xml file filter.");
            }
        }
        /// <summary>
        /// Check if the directory is inside of an Unwanted path
        /// </summary>
        /// <param name="directory">The directory to validate</param>
        /// <returns>True if the directory is inside an unwanted path</returns>
        public override bool IsDirectoryValid(DirectoryInfo directory)
        {
            if (directory.FullName.Contains(".git"))
                return false;
            else
                return this.UnWantedPaths.Count(x => directory.FullName.ToUpper().Contains(x)) > 0;
        }
        /// <summary>
        /// Check if the file is inside of an Unwanted path
        /// </summary>
        /// <param name="file">The file to validate</param>
        /// <returns>True if the directory is inside an unwanted path</returns>
        public override bool IsFileValid(FileInfo file)
        {
            string ext = file.Extension.Replace(".", "").ToUpper();
            string lastExp = "";
            Regex reg;
            try
            {
                foreach (var exp in this.MatchExpressions)
                {
                    reg = new Regex(exp);
                    if (reg.Match(file.Name).Success)
                        return true;
                }
            }
            catch (Exception exp)
            {
                exp.CreateNamelessException<NamelessException>("La expresión {0} no es válida", lastExp);
            }
            return this.UnWantedExtensions.Count(x => x == ext) > 0 || IsDirectoryValid(file.Directory);
        }
        /// <summary>
        /// Gets the values extracted from the given node
        /// </summary>
        /// <param name="document">The XML document</param>
        /// <param name="node">The element node name</param>
        /// <returns>The entry values</returns>
        private IEnumerable<string> GetValues(XDocument document, string node)
        {
            var root = document.Elements().FirstOrDefault();
            return root.Element(node).Elements().Select(x => x.Attribute("value").Value);
        }
    }
}
