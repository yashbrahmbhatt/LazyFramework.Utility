using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LazyFramework.Utility
{
    public class XDocumentHelpers
    {
        public static XElement? GetClosestParentWithAttribute(XElement? node, string attribute)
        {
            if (node == null) return null;
            while (node != null)
            {
                node = node.Parent;
                if (node == null) return null;
                if (node.Attributes().Any(a => a.Name.LocalName == attribute))
                {
                    return node;
                }
            }
            return node;
        }


        public static XAttribute? GetAttribute(XElement element, string attribute)
        {
            if (element == null) return null;
            var query = element.Attributes().Where(a => a.Name.LocalName == attribute);
            if (query.Count() != 0)
            {
                return query.First();
            }
            return null;
        }

        public static int GetIndexPosition(XObject xObject)
        {
            if (xObject == null)
            {
                throw new ArgumentNullException("element");
            }
            if (xObject.Parent == null)
            {
                return -1;
            }

            int i = 1;
            if (xObject is XElement element)
            {
                foreach (var sibling in element.Parent.Elements(element.Name))
                {
                    if (sibling == element) return i;
                    i++;
                }
            }
            if (xObject is XAttribute attribute)
            {
                foreach (var sibling in attribute.Parent.Attributes())
                {
                    if (sibling == attribute) return i;
                    i++;
                }
            }

            throw new InvalidOperationException("Element has been removed from its parent.");
        }

        public static string GetRelativeXPath(XObject xObject)
        {
            int index = GetIndexPosition(xObject);
            string name;
            if (xObject is XElement element)
            {
                name = element.Name.LocalName;
            }
            else if (xObject is XAttribute attribute)
            {
                name = attribute.Name.LocalName;
            }
            else throw new InvalidOperationException("XObject is not an XElement or XAttribute");


            return index == -1 ? "/" + name : string.Format("{0}/[{1}]", name, index.ToString());
        }

        public static string GetAbsoluteXPath(XObject xObject)
        {
            if (xObject == null)
            {
                throw new ArgumentNullException("element");
            }
            List<string> ancestors = new List<string>();
            if (xObject is XElement element)
            {
                ancestors = element.Ancestors().Select(a => GetRelativeXPath(a)).ToList();
            }
            else if (xObject is XAttribute attribute)
            {
                ancestors.Add(GetRelativeXPath(attribute));
            }
            else throw new InvalidOperationException("XObject is not an XElement or XAttribute");
            ancestors.Reverse();
            return string.Concat(ancestors.ToArray()) + GetRelativeXPath(xObject);
        }

    }
}
