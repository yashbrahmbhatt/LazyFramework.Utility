using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LazyFramework.Utility.WorkflowEditor
{    public class Argument
    {
        public string Class { get; private set; }
        public string Name
        {
            get
            {
                return XDocumentHelpers.GetAttribute(PropertyElement, LocalName.ArgumentDefinitionName).Value;
            }
        }
        public string? Description
        {
            get
            {
                return XDocumentHelpers.GetAttribute(PropertyElement, LocalName.Description).Value;
            }
        }
        public string? DefaultValue
        {
            get
            {
                if (Argument_ExpressionElement == null) return null;
                if (Argument_ExpressionElement is XAttribute) return "\"" + ((XAttribute)Argument_ExpressionElement).Value + "\"";
                if (Argument_ExpressionElement is XElement)
                {
                    var element = (XElement)Argument_ExpressionElement;
                    switch (element.Name.LocalName)
                    {
                        case "CSharpValue":
                            return element.Value;
                            break;
                        case "Literal":
                            return "\"" + XDocumentHelpers.GetAttribute(element, LocalName.LiteralValue).Value + "\"";
                        default:
                            throw new NotSupportedException("Unkonwn element type");
                    }
                }
                throw new InvalidOperationException("Unsupported Expression Type");
            }
            set
            {
                // No default value
                if (value == null)
                {
                    if (ArgumentObject is XAttribute) ((XAttribute)ArgumentObject).Remove();
                    if (ArgumentObject is XElement) ((XElement)ArgumentObject).Remove();
                }

                // Value is Empty String (ie. "\"\"")
                // Convert to Literal Child on Root
                // If it was an attribute or null before, we have to create the whole branch
                // Root -> this:Class + Name -> In/Out/InOutArgument -> Literal
                // Otherwise we just replace the CSharpValue with the Literal
                else if (value == "\"\"")
                {
                    if (Type != "x:String") return;
                    if (ArgumentObject == null || ArgumentObject is XAttribute)
                    {
                        var newArgumentElement = new XElement(
                            "{clr-namespace:}" + Class + "." + Name,
                            new XElement(
                                NamespaceNames.Empty + string.Format("{0}Argument", Direction),
                                new XAttribute(NamespaceNames.X + LocalName.ArgumentValueType, Type),
                                new XElement(
                                    NamespaceNames.Empty + LocalName.Literal,
                                    new XAttribute(NamespaceNames.X + LocalName.ArgumentValueType, Type),
                                    new XAttribute(LocalName.LiteralValue, "")
                                )
                                )
                            );

                        if (ArgumentObject is XAttribute) ((XAttribute)ArgumentObject).Remove();
                        PropertyElement.Document.Root.Elements().First().AddAfterSelf(newArgumentElement);
                    }
                    if (ArgumentObject is XElement)
                    {
                        var element = (XElement)Argument_ExpressionElement;

                        switch (element.Name.LocalName)
                        {
                            case "CSharpValue":
                                var newExpressionElement = new XElement(
                                     NamespaceNames.Empty + LocalName.Literal,
                                    new XAttribute(NamespaceNames.X + LocalName.ArgumentValueType, Type),
                                    new XAttribute(LocalName.LiteralValue, "")
                                );
                                Argument_DirectionWrapperElement.Add(newExpressionElement);
                                element.Remove();
                                break;
                            case "Literal":
                                var attribute = XDocumentHelpers.GetAttribute((XElement)Argument_ExpressionElement, LocalName.LiteralValue);
                                attribute.Value = "";
                                return;
                            default:
                                throw new NotSupportedException("Unkonwn element type");
                        }
                    }
                }

                // Value is Non-Empty String (ie. \""Hello World\"")
                // Convert to Attribute on Root
                // If Its an attribute, just set value
                // Otherwise we have to create the attribute
                // If it was an Empty String or CSharpValue before, we have to remove that element as well.
                else if (value.StartsWith("\"") && value.EndsWith("\""))
                {
                    if (ArgumentObject is XAttribute)
                    {
                        ((XAttribute)Argument_ExpressionElement).Value = value.Remove(value.Length - 1).Remove(0, 1);
                        return;
                    }
                    else
                    {
                        if (ArgumentObject != null)
                        {
                            ((XElement)ArgumentObject).Remove();
                        }
                        PropertyElement.Document.Root.Add(new XAttribute("{clr-namespace:}" + Class + "." + Name, value.Remove(value.Length - 1).Remove(0, 1)));
                    }

                }

                // Value is Code String (ie. "new DateTime().ToString()"
                // Convert to Child of Root
                // Same thing as Literals but change the local name and mapping of Value
                else
                {

                    if (ArgumentObject == null || ArgumentObject is XAttribute)
                    {
                        var newArgumentElement = new XElement(
                            "{clr-namespace:}" + Class + "." + Name,
                            new XElement(
                                NamespaceNames.Empty + string.Format("{0}Argument", Direction),
                                new XAttribute(NamespaceNames.X + LocalName.ArgumentValueType, Type),
                                new XElement(
                                    NamespaceNames.Empty + LocalName.CSharpValue,
                                    new XAttribute(NamespaceNames.X + LocalName.ArgumentValueType, Type),
                                    value
                                )
                                )
                            );


                        
                        if (ArgumentObject is XAttribute) ((XAttribute)ArgumentObject).Remove();
                        PropertyElement.Document.Root.Elements().ElementAt(0).AddAfterSelf(newArgumentElement);
                        return;
                    }
                    if (ArgumentObject is XElement)
                    {
                        var element = (XElement)Argument_ExpressionElement;

                        switch (element.Name.LocalName)
                        {
                            case "CSharpValue":
                                element.Value = value;
                                return;
                            case "Literal":
                                var newExpressionElement = new XElement(
                                     NamespaceNames.Empty + LocalName.CSharpValue,
                                    new XAttribute(NamespaceNames.X + LocalName.ArgumentValueType, Type),
                                    value
                                );
                                element.Remove();
                                Argument_DirectionWrapperElement.Add(newExpressionElement);
                                return;

                            default:
                                throw new NotSupportedException("Unkonwn element type");
                        }
                    }

                }
            }
        }
        public string Direction
        {
            get
            {
                return Property_TypeAttribute.Value.Split("(").First().Replace("Argument", "");
            }
            set
            {
                var oldSplit = Property_TypeAttribute.Value.Split("(");
                oldSplit[0] = value + "Argument";
                var output = string.Join("(", oldSplit);
                Property_TypeAttribute.Value = output;
            }
        }
        public string Type
        {
            get
            {
                var split = Property_TypeAttribute.Value.Split("(").ToList();
                split.RemoveAt(0);
                split[split.Count - 1] = split[split.Count - 1].Remove(split[split.Count - 1].Length - 1);
                return string.Join("(", split);
            }
            set
            {
                var split = Property_TypeAttribute.Value.Split("(").ToList();
                Property_TypeAttribute.Value = split[0] + "(" + value + ")";

                if (Argument_DirectionWrapper_TypeAttribute != null)
                {
                    Argument_DirectionWrapper_TypeAttribute.Value = value;
                }
                if (Argument_Expression_TypeElement != null)
                {
                    Argument_Expression_TypeElement.Value = value;
                }
            }
        }

        private XElement PropertyElement { get; set; }
        private XObject? ArgumentObject
        {
            get
            {
                XObject valueElement = PropertyElement.Document.Descendants().FirstOrDefault(d => d.Name.LocalName == Class + "." + Name && d.Parent == PropertyElement.Document.Root, null);
                if (valueElement == null)
                {
                    valueElement = XDocumentHelpers.GetAttribute(PropertyElement.Document.Root, Class + "." + Name);
                }
                return valueElement;
            }
        }
        private XAttribute Property_TypeAttribute
        {
            get
            {
                return XDocumentHelpers.GetAttribute(PropertyElement, LocalName.ArgumentDefinitionType);
            }
        }
        private XElement? Argument_DirectionWrapperElement
        {
            get
            {
                if (ArgumentObject == null) return null;
                if (ArgumentObject is XAttribute)
                {
                    return null;
                }
                else
                {
                    return ((XElement)ArgumentObject).Elements().First();
                }
            }
        }
        private XAttribute? Argument_DirectionWrapper_TypeAttribute
        {
            get
            {

                if (ArgumentObject == null) return null;
                if (ArgumentObject is XAttribute)
                {
                    return null;
                }
                else
                {
                    return XDocumentHelpers.GetAttribute(Argument_DirectionWrapperElement, LocalName.ArgumentWrapperType);
                }
            }
        }

        private XObject? Argument_ExpressionElement
        {
            get
            {
                if (ArgumentObject == null) return null;
                if (ArgumentObject is XAttribute)
                {
                    return ArgumentObject;
                }
                else
                {
                    return ((XElement)ArgumentObject).Descendants().First(ved => LocalName.Expressions.Contains(ved.Name.LocalName));
                }
            }
        }

        private XAttribute? Argument_Expression_TypeElement
        {
            get
            {
                if (Argument_ExpressionElement == null) return null;
                if (Argument_ExpressionElement is XAttribute) return null;

                return XDocumentHelpers.GetAttribute((XElement)Argument_ExpressionElement, LocalName.ArgumentValueType);
            }
        }


        public Argument(XElement argumentDefinition, string className)
        {
            PropertyElement = argumentDefinition;
            Class = className;
        }
    }
    public class Activity
    {
        public string Name
        {
            get
            {
                return NameElement.Value;
            }
            set
            {
                NameElement.Value = value;
            }
        }
        public string Description
        {
            get
            {
                return DescriptionElement.Value;
            }
            set
            {
                DescriptionElement.Value = value;
            }
        }
        public XElement ActivityElement { get; set; }
        public string Type
        {
            get
            {
                return ActivityElement.Name.LocalName;
            }
        }


        private XAttribute DescriptionElement
        {
            get
            {
                return XDocumentHelpers.GetAttribute(ActivityElement, LocalName.Description);
            }
        }
        private XAttribute NameElement
        {
            get
            {
                return XDocumentHelpers.GetAttribute(ActivityElement, LocalName.DisplayName);
            }
        }
        public Activity(XElement activity)
        {
            ActivityElement = activity;
        }
    }
    public class Variable
    {
        public string Name
        {
            get
            {
                return Variable_NameElement.Value;
            }
            set
            {
                Variable_NameElement.Value = value;
            }
        }
        public string Type
        {
            get
            {
                return Variable_TypeElement.Value;
            }
            set
            {
                Variable_TypeElement.Value = value;
                if (Variable_Default_Expression_Type != null) Variable_Default_Expression_Type.Value = value;
            }
        }
        public string? Description
        {
            get
            {
                if (Variable_DescriptionElement == null) return null;
                return Variable_DescriptionElement.Value;
            }
            set
            {
                if (value == null)
                {
                    if (Variable_DescriptionElement != null) Variable_DescriptionElement.Remove();
                }
                else
                {
                    if (Variable_DescriptionElement != null)
                    {
                        Variable_DescriptionElement.Value = value;
                    }
                    else
                    {
                        VariableElement.Add(new XAttribute(NamespaceNames.X + LocalName.Description, value));
                    }
                }
            }
        }

        public string ScopeName
        {
            get
            {
                return Scope_NameElement.Value;
            }
        }

        public string? DefaultValue
        {
            get
            {
                if (Variable_Default_ExpressionElement == null) return null;
                if(Variable_Default_ExpressionElement is XAttribute)
                {
                    return "\"" + ((XAttribute)Variable_Default_ExpressionElement).Value + "\"";
                } else
                {
                    var element = (XElement)Variable_Default_ExpressionElement;
                    if (element.Name.LocalName == LocalName.Literal)
                    {
                        var ValueAttribute = XDocumentHelpers.GetAttribute(element, LocalName.LiteralValue);
                        if (ValueAttribute == null) return "\"" + element.Value + "\"";
                        return "\"" + ValueAttribute.Value + "\"";
                    }
                    else
                    {
                        return element.Value;
                    }
                }

            }
            set
            {
                if (value == null)
                {
                    if (Variable_DefaultElement is XAttribute) ((XAttribute)Variable_DefaultElement).Remove();
                    if (Variable_DefaultElement is XElement) ((XElement)Variable_DefaultElement).Remove();
                }
                else if(value == "\"\"")
                {
                    if (Variable_Default_ExpressionElement == null || Variable_DefaultElement is XAttribute)
                    {
                        var DefaultElement = new XElement(
                            NamespaceNames.Empty + "Variable.Default",
                            new XElement(
                                NamespaceNames.Empty + LocalName.Literal,
                                new XAttribute(NamespaceNames.X + LocalName.VariableType, Type),
                                new XAttribute(LocalName.LiteralValue, "")
                        ));
                        if (Variable_Default_ExpressionElement is XAttribute) ((XAttribute)Variable_DefaultElement).Remove();

                        VariableElement.Add(DefaultElement);
                    }
                    else
                    {
                        var element = (XElement)Variable_Default_ExpressionElement;
                        switch (element.Name.LocalName)
                        {
                            case "Literal":
                                element.Value = "";
                                break;
                            case "CSharpValue":
                                var newElement = new XElement(
                                    NamespaceNames.Empty + LocalName.Literal,
                                    new XAttribute(NamespaceNames.X + LocalName.VariableType, Type),
                                    new XAttribute(LocalName.LiteralValue, "")
                                );
                                element.Remove();
                                ((XElement)Variable_DefaultElement).Add(newElement);

                                break;
                            default:
                                throw new Exception("Unsupported type");
                        }
                    }
                }
                else if (value.StartsWith("\"") && value.EndsWith("\""))
                {

                    if (Variable_Default_ExpressionElement == null)
                    {
                        var DefaultElement = new XElement(
                            NamespaceNames.Empty + "Variable.Default",
                            new XElement(
                                NamespaceNames.Empty + LocalName.Literal,
                                new XAttribute(NamespaceNames.X + LocalName.VariableType, Type),
                                new XAttribute(LocalName.LiteralValue, value.Remove(value.Length - 1).Remove(0,1))
                        ));
                        VariableElement.Add(DefaultElement);
                    }
                    else if (Variable_Default_ExpressionElement is XAttribute)
                    {
                        ((XAttribute)Variable_Default_ExpressionElement).Value = value.Remove(value.Length - 1).Remove(0, 1);
                    }
                    else
                    {
                        var element = (XElement)Variable_Default_ExpressionElement;
                        switch (element.Name.LocalName)
                        {
                            case "Literal":
                                var valueElement = XDocumentHelpers.GetAttribute(element, LocalName.LiteralValue);
                                valueElement.Value = value.Remove(value.Length - 1).Remove(0,1);
                                break;
                            case "CSharpValue":
                                var newElement = new XElement(
                                    NamespaceNames.Empty + LocalName.Literal,
                                    new XAttribute(NamespaceNames.X + LocalName.VariableType, Type),
                                    new XAttribute(LocalName.LiteralValue, value)
                                );
                                element.Remove();
                                ((XElement)Variable_DefaultElement).Add(newElement);



                                break;
                            default:
                                throw new Exception("Unsupported type");
                        }
                    }
                }
                else
                {
                    if (Variable_Default_ExpressionElement == null || Variable_Default_ExpressionElement is XAttribute)
                    {
                        var DefaultElement = new XElement(
                            NamespaceNames.Empty + "Variable.Default",
                            new XElement(
                                NamespaceNames.Empty + LocalName.CSharpValue,
                                new XAttribute(NamespaceNames.X + LocalName.VariableType, Type),
                                value
                        ));
                        if (Variable_Default_ExpressionElement is XAttribute) ((XAttribute)Variable_Default_ExpressionElement).Remove();
                        VariableElement.Add(DefaultElement);
                    }
                    else
                    {
                        var element = (XElement)Variable_Default_ExpressionElement;
                        switch (element.Name.LocalName)
                        {
                            case "Literal":
                                var newElement = new XElement(
                                    NamespaceNames.Empty + LocalName.CSharpValue,
                                    new XAttribute(NamespaceNames.X + LocalName.VariableType, Type),
                                    value
                                );
                                ((XElement)Variable_DefaultElement).Add(newElement);
                                element.Remove();
                                break;
                            case "CSharpValue":
                                element.Value = value.Remove(value.Length - 1).Remove(0);
                                break;
                            default:
                                throw new Exception("Unsupported type");
                        }
                    }
                }

            }
        }
        private XElement VariableElement { get; set; }
        private XAttribute Variable_TypeElement
        {
            get
            {
                return XDocumentHelpers.GetAttribute(VariableElement, LocalName.VariableType);
            }
        }
        private XAttribute Variable_NameElement
        {
            get
            {
                return XDocumentHelpers.GetAttribute(VariableElement, LocalName.VariableName);
            }
        }

        private XAttribute? Variable_DescriptionElement
        {
            get
            {
                return XDocumentHelpers.GetAttribute(VariableElement, LocalName.Description);
            }
        }
        private XElement ScopeElement
        {
            get
            {
                return XDocumentHelpers.GetClosestParentWithAttribute(VariableElement, LocalName.DisplayName);
            }
        }
        private XAttribute Scope_NameElement
        {
            get
            {
                return XDocumentHelpers.GetAttribute(ScopeElement, LocalName.DisplayName);
            }
        }
        private XObject? Variable_DefaultElement
        {
            get
            {
                var attributeDefault = XDocumentHelpers.GetAttribute(VariableElement, "Default");
                if (VariableElement.HasElements)
                {
                    return VariableElement.Elements().First();
                }
                else if (attributeDefault != null)
                {
                    return attributeDefault;
                }
                else
                {
                    return null;
                }
            }
        }
        private XObject? Variable_Default_ExpressionElement
        {
            get
            {
                if (Variable_DefaultElement == null)
                {
                    var defaultElement = XDocumentHelpers.GetAttribute(VariableElement, "Default");
                    if (defaultElement != null) return defaultElement;
                    return null;
                }
                else if (Variable_DefaultElement is XAttribute)
                {
                    return Variable_DefaultElement;
                } 
                else if (Variable_DefaultElement is XElement) {
                    return ((XElement)Variable_DefaultElement).Elements().First();
                } else
                {
                    throw new Exception("Unexpected Value");
                }
            }
        }
        private XAttribute? Variable_Default_Expression_Type
        {
            get
            {
                if (!(Variable_Default_ExpressionElement is XElement)) return null;
                return XDocumentHelpers.GetAttribute((XElement)Variable_Default_ExpressionElement, LocalName.VariableType);
            }
        }
        public Variable(XElement variable)
        {
            VariableElement = variable;
        }
    }
    public class Expression
    {
        public string? Type
        {
            get
            {
                if (Expression_TypeElement == null) return null;
                return Expression_TypeElement.Value;
            }
        }

        public string Path
        {
            get
            {
                var PathValue = ExpressionElement
                    .Ancestors().Reverse().Select(a => a.Name.LocalName + "[" + a.ElementsBeforeSelf().Count() + "]");
                return string.Join("/", PathValue);
            }
        }

        public string Value
        {
            get
            {
                return ExpressionElement.Value;
            }
            set
            {
                if(value == null)
                {
                    ExpressionElement.Value = "";
                    return;
                }
                ExpressionElement.Value = value;
            }
        }
        public string? ActivityType
        {
            get
            {
                if (ActivityElement == null) return null;
                return ActivityElement.Name.LocalName;
            }
        }
        public string? ActivityName
        {
            get
            {
                if (Activity_NameElement == null) return null;
                return Activity_NameElement.Value;
            }
        }
        private XElement? ActivityElement
        {
            get
            {
                return XDocumentHelpers.GetClosestParentWithAttribute(ExpressionElement, LocalName.DisplayName);
            }
        }
        private XAttribute? Activity_NameElement
        {
            get
            {
                return XDocumentHelpers.GetAttribute(ActivityElement, LocalName.DisplayName);
            }
        }
        private XElement ExpressionElement { get; set; }
        private XAttribute? Expression_TypeElement
        {
            get
            {
                return XDocumentHelpers.GetAttribute(ExpressionElement, LocalName.ExpressionType);
            }
        }
        public Expression(XElement element)
        {
            ExpressionElement = element;
        }
    }
    public class References
    {
        private List<XElement> ReferencesElementList
        {
            get
            {
                return Document.Root.Elements().First().Elements().ToList();

            }
        }
        public List<string> Values
        {
            get
            {
                return ReferencesElementList.Select(r => r.Value).ToList();
            }
            set
            {
                if (value != null)
                {
                    var ReferenceParent = Document.Root.Elements().First();
                    ReferenceParent.RemoveNodes();
                    foreach (var namespaceVal in value)
                    {
                        var newElement = new XElement(
                            NamespaceNames.Empty + "AssemblyReference",
                            namespaceVal
                        );
                        ReferenceParent.Add(newElement);
                    }
                }
            }
        }

        private XDocument Document { get; set; }
        public References(XDocument element)
        {
            Document = element;
        }
    }
    public class Namespaces
    {
        private List<XElement> NamespacesElementList
        {
            get
            {
                return Document.Root.Elements().First().Elements().ToList();

            }
        }
        public List<string> Values
        {
            get
            {
                return NamespacesElementList.Select(r => r.Value).ToList();
            }
            set
            {
                if (value != null)
                {
                    var ReferenceParent = Document.Root.Elements().First();
                    ReferenceParent.RemoveNodes();
                    foreach (var namespaceVal in value)
                    {
                        var newElement = new XElement(
                            NamespaceNames.X + "String",
                            namespaceVal
                        );
                        ReferenceParent.Add(newElement);
                    }
                }
            }
        }

        private XDocument Document { get; set; }
        public Namespaces(XDocument element)
        {
            Document = element;
        }
    }

}
