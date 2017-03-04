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
        public bool GroupByObjectType { get; set; }

        private IList<Assembly> Assemblies { get; set; }
        private Dictionary<string, IEnumerable<string>> assemblyNamespaces { get; set; }

        public AssemblyTreeBuilder(IEnumerable<Assembly> assemblies)
        {
            GroupByObjectType = false;

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

            IAssemblyTreeNode namespaceFolderNode;
            if (GroupByObjectType)
            {
                namespaceFolderNode = new AssemblyFolderTreeNode("Namespaces");
                assemblyNode.AddChild(namespaceFolderNode);
            }
            else
            {
                namespaceFolderNode = assemblyNode;
            }

            foreach (var @namespace in assemblyNamespaces[assembly.FullName])                
                ProcessNamespace(assembly, @namespace, namespaceFolderNode);
        }

        private void ProcessNamespace(Assembly assembly, string @namespace, IAssemblyTreeNode root)
        {
            if (root == null) throw new ArgumentNullException("root");
            if (assembly == null) throw new ArgumentNullException("assembly");

            var namespaceNode = new AssemblyNamespaceTreeNode(@namespace);
            root.AddChild(namespaceNode);

            IAssemblyTreeNode classFolderNode, interfaceFolderNode, enumFolderNode, delegateFolderNode;
            if (GroupByObjectType)
            {
                classFolderNode = new AssemblyFolderTreeNode("Classes");
                namespaceNode.AddChild(classFolderNode);

                interfaceFolderNode = new AssemblyFolderTreeNode("Interfaces");
                namespaceNode.AddChild(interfaceFolderNode);

                enumFolderNode = new AssemblyFolderTreeNode("Enums");
                namespaceNode.AddChild(enumFolderNode);

                delegateFolderNode = new AssemblyFolderTreeNode("Delegates");
                namespaceNode.AddChild(delegateFolderNode);
            }
            else
            {
                classFolderNode = namespaceNode;
                interfaceFolderNode = namespaceNode;
                enumFolderNode = namespaceNode;
                delegateFolderNode = namespaceNode;                
            }

            
            foreach (var type in assembly.GetTypes()
                .Where(t => t.IsClass                    
                    && t.Namespace == @namespace
                    && t.IsEnabled()
                    && !t.IsDelegate()
                    && !t.IsCompilerGenerated())
                .OrderBy(t => t.Name))
            {
                ProcessClass(type, classFolderNode);
            }

            
            foreach (var type in assembly.GetTypes()
                .Where(t => t.IsInterface                     
                    && t.Namespace == @namespace
                    && t.IsEnabled())
                .OrderBy(t => t.Name))
            {
                ProcessInterface(type, interfaceFolderNode);
            }

            
            foreach (var type in assembly.GetTypes()
                .Where(t => t.IsEnum                     
                    && t.Namespace == @namespace
                    && t.IsEnabled())
                .OrderBy(t => t.Name))
            {
                ProcessEnum(type, enumFolderNode);
            }

            
            foreach (var type in assembly.GetTypes()
                .Where(t => t.IsClass                     
                    && t.Namespace == @namespace
                    && t.IsEnabled()
                    && t.IsDelegate()
                    && !t.IsCompilerGenerated())
                .OrderBy(t => t.Name))
            {               
                ProcessDelegate(type, delegateFolderNode);
            }
        }

        private void ProcessClass(Type type, IAssemblyTreeNode root)
        {
            if (root == null) throw new ArgumentNullException("root");
            var node = new AssemblyClassTreeNode(type.GetFriendlyName(), type);
            root.AddChild(node);
        }

        private void ProcessInterface(Type type, IAssemblyTreeNode root)
        {
            if (root == null) throw new ArgumentNullException("root");
            var node = new AssemblyInterfaceTreeNode(type.Name, type);
            root.AddChild(node);
        }

        private void ProcessDelegate(Type type, IAssemblyTreeNode root)
        {
            if (root == null) throw new ArgumentNullException("root");
            var node = new AssemblyDelegateTreeNode(type.Name, type);
            root.AddChild(node);
        }

        private void ProcessEnum(Type type, IAssemblyTreeNode root)
        {
            if (root == null) throw new ArgumentNullException("root");
            var node = new AssemblyEnumTreeNode(type.Name, type);
            root.AddChild(node);
        }
    }

}
