using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyFramework.Utility.WorkflowEditor
{
    public class LocalName
    {
        public static string IdRef { get; set; } = "WorkflowViewState.IdRef";
        public static string DisplayName { get; set; } = "DisplayName";
        public static string Literal { get; set; } = "Literal";
        public static string CSharpValue { get; set; } = "CSharpValue";
        public static string CSharpReference { get; set; } = "CSharpReference";
        public static string[] Expressions { get; set; } = new string[] { Literal, CSharpValue, CSharpReference};
        public static string ExpressionType { get; set; } = "TypeArguments";
        public static string Variable { get; set; } = "Variable";
        public static string VariableName { get; set; } = "Name";
        public static string VariableType { get; set; } = "TypeArguments";
        public static string ArgumentDefinition { get; set; } = "Property";
        public static string ArgumentDefinitionName { get; set; } = "Name";
        public static string ArgumentDefinitionType { get; set; } = "Type";
        public static string ArgumentWrapperType { get; set; } = "TypeArguments";
        public static string ArgumentValueType { get; set; } = "TypeArguments";
        public static string LiteralValue { get; set; } = "Value";
        public static string Class { get; set; } = "Class";
        public static string Namespaces { get; set; } = "TextExpression.NamespacesForImplementation";
        public static string References { get; set; } = "TextExpression.ReferencesForImplementation";
        public static string Description { get; set; } = "Annotation.AnnotationText";
        public static string EditingPrefix { get; set; } = "LazyFramework_";
        public static string Editing_Path { get; set; } = EditingPrefix + "Path";

    }

    public class NamespaceNames
    {
        public static string X = "{http://schemas.microsoft.com/winfx/2006/xaml}";
        public static string Empty = "{http://schemas.microsoft.com/netfx/2009/xaml/activities}";
    }
}
