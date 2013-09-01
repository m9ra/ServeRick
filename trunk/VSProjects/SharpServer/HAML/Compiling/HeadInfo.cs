using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer.HAML.Compiling
{
    class HeadInfo
    {
        private readonly string Tag;
        private readonly string Id;
        private readonly string[] Classes;

        public string OpeningTag { get; private set; }
        public string ClosingTag { get; private set; }

        public HeadInfo(string tag, string id, string[] classes)
        {
            if (tag == "" && id == "" && classes.Length == 0)
            {
                //empty head
                ClosingTag=OpeningTag = "";
                return;
            }
                       

            Tag = tag=="" ? "div": tag;
            Id = id;
            Classes = classes;

            initializeHead();
        }

        private void initializeHead()
        {
            var classAttrib = "";
            if (Classes.Length > 0)
            {
                classAttrib = string.Format(" class=\"{0}\"", string.Join(" ", Classes));
            }

            var idAttrib = "";
            if (Id != "")
            {
                idAttrib = string.Format(" id='{0}'", Id);
            }

            OpeningTag = string.Format("<{0}{1}{2}>", Tag, idAttrib, classAttrib);
            ClosingTag = string.Format("</{0}>", Tag);
        }
    }
}
