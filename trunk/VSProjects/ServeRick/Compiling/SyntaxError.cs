using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parsing.Source;

namespace ServeRick.Compiling
{
    /// <summary>
    /// Representation of syntax error
    /// </summary>
    public class SyntaxError
    {
        /// <summary>
        /// Error location
        /// </summary>
        public readonly SourceContext Location;

        /// <summary>
        /// Error description
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// Preview around error location
        /// </summary>        
        /// <returns>Preview representation</returns>
        public string NearPreview
        {
            get
            {
                //find start index
                var startIndex = Location.Index - 1;
                while (startIndex > 0)
                {
                    var ch = Location.Text[startIndex];
                    if (ch == '\n')
                        break;

                    --startIndex;
                }

                //find end index
                var endIndex = Location.Index + 1;
                while (endIndex < Location.Text.Length)
                {
                    var ch = Location.Text[endIndex];
                    if (ch == '\n')
                        break;

                    ++endIndex;
                }

                startIndex = Math.Max(0, startIndex);
                endIndex = Math.Min(Location.Text.Length, endIndex);

                return Location.Text.Substring(startIndex, endIndex - startIndex);

            }
        }

        /// <summary>
        /// Initialize syntax error object
        /// </summary>
        /// <param name="location">Context where error is located</param>
        /// <param name="description">Description of error</param>
        public SyntaxError(SourceContext location, string description)
        {
            Description = description;
            Location = location;
        }
    }
}
