using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Doc
{
    public interface IAssemblyTreeBuilder
    {
        AssemblyTree Build();
    }

    public class AssemblyTreeBuilder : IAssemblyTreeBuilder
    {
        private IList<Assembly> Assemblies { get; set; }
        private Dictionary<string, IEnumerable<string>> assemblyNamespaces { get; set; }

        public AssemblyTreeBuilder(IEnumerable<Assembly> assemblies)
        {
            Assemblies = new List<Assembly>(assemblies);
            assemblyNamespaces = new Dictionary<string, IEnumerable<string>>(Assemblies.Count);

            foreach (var assembly in assemblies)
                assemblyNamespaces.Add(
                    assembly.FullName,
                        assembly.GetTypes().Where(t => !String.IsNullOrEmpty(t.Namespace))
                        .GroupBy(t => t.Namespace)
                        .OrderBy(g => g.Key)
                        .Select(g => g.Key));
        }

        public AssemblyTreeBuilder(Assembly assembly)
            : this (new Assembly[] { assembly }) { }

        public AssemblyTree Build()
        {
            var tree = new AssemblyTree();
            foreach (var assembly in Assemblies)
            {
                ProcessAssembly(assembly, tree.Root as IAssemblyTreeNode);
            }
            return tree;
        }

        private void ProcessAssembly(Assembly assembly, IAssemblyTreeNode root)
        {
            if (root == null) throw new ArgumentNullException("root");
            if (assembly == null) throw new ArgumentNullException("assembly");

            var assemblyNode = new AssemblyAssemblyTreeNode(assembly.GetName().Name, assembly);
            root.AddChild(assemblyNode);

            var namespaceFolderNode = new AssemblyFolderTreeNode("Namespaces");
            assemblyNode.AddChild(namespaceFolderNode);

            foreach (var @namespace in assemblyNamespaces[assembly.FullName])
                ProcessNamespace(assembly, @namespace, namespaceFolderNode);
        }

        private void ProcessNamespace(Assembly assembly, string @namespace, IAssemblyTreeNode root)
        {
            if (root == null) throw new ArgumentNullException("root");
            if (assembly == null) throw new ArgumentNullException("assembly");

            var namespaceNode = new AssemblyFolderTreeNode(@namespace);
            root.AddChild(namespaceNode);
            
            var classFolderNode = new AssemblyFolderTreeNode("Classes");
            namespaceNode.AddChild(classFolderNode);
            foreach (var type in assembly.GetTypes()
                .Where(t => t.IsClass
                    && t.IsVisible
                    && t.Namespace == @namespace
                    && t.BaseType != typeof(MulticastDelegate)
                    && Attribute.GetCustomAttribute(
                            t, typeof(CompilerGeneratedAttribute)) == null)
                .OrderBy(t => t.Name))
            {
                ProcessClass(assembly, type, classFolderNode);
            }

            var interfaceFolderNode = new AssemblyFolderTreeNode("Interfaces");
            namespaceNode.AddChild(interfaceFolderNode);
            foreach (var type in assembly.GetTypes()
                .Where(t => t.IsInterface 
                    && t.IsVisible 
                    && t.Namespace == @namespace)
                .OrderBy(t => t.Name))
            {
                ProcessInterface(assembly, type, interfaceFolderNode);
            }

            var enumFolderNode = new AssemblyFolderTreeNode("Enums");
            namespaceNode.AddChild(enumFolderNode);
            foreach (var type in assembly.GetTypes()
                .Where(t => t.IsEnum 
                    && t.IsVisible 
                    && t.Namespace == @namespace)
                .OrderBy(t => t.Name))
            {
                ProcessEnum(assembly, type, enumFolderNode);
            }

            var delegateFolderNode = new AssemblyFolderTreeNode("Delegates");
            namespaceNode.AddChild(delegateFolderNode);
            foreach (var type in assembly.GetTypes()
                .Where(t => t.IsClass 
                    && t.IsVisible 
                    && t.Namespace == @namespace 
                    && t.BaseType == typeof(MulticastDelegate)
                    && Attribute.GetCustomAttribute(
                        t, typeof(CompilerGeneratedAttribute)) == null)
                .OrderBy(t => t.Name))
            {
                ProcessDelegate(assembly, type, delegateFolderNode);
            }
        }

        private void ProcessClass(Assembly assembly, Type type, IAssemblyTreeNode root)
        {
            if (root == null) throw new ArgumentNullException("root");
            if (assembly == null) throw new ArgumentNullException("assembly");

            var node = new AssemblyClassTreeNode(type.GetFriendlyName(), type);
            root.AddChild(node);
        }

        private void ProcessInterface(Assembly assembly, Type type, IAssemblyTreeNode root)
        {
            if (root == null) throw new ArgumentNullException("root");
            if (assembly == null) throw new ArgumentNullException("assembly");

            var node = new AssemblyInterfaceTreeNode(type.Name, type);
            root.AddChild(node);
        }

        private void ProcessDelegate(Assembly assembly, Type type, IAssemblyTreeNode root)
        {
            if (root == null) throw new ArgumentNullException("root");
            if (assembly == null) throw new ArgumentNullException("assembly");

            var node = new AssemblyDelegateTreeNode(type.Name, type);
            root.AddChild(node);
        }

        private void ProcessEnum(Assembly assembly, Type type, IAssemblyTreeNode root)
        {
            if (root == null) throw new ArgumentNullException("root");
            if (assembly == null) throw new ArgumentNullException("assembly");

            var node = new AssemblyEnumTreeNode(type.Name, type);
            root.AddChild(node);
        }
    }

}
