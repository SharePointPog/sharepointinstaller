using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CodePlex.SharePointInstaller.Wrappers;

namespace CodePlex.SharePointInstaller.Controls
{
    public delegate void TreeNodeEventHandler(object sender, TreeViewCancelEventArgs e);

    /// <summary>
    /// Extends the TreeNode class to allow for Populate On Demand and managing checkboxes when a parent/child node's checkbox changes.
    /// </summary>
    public class ExtendedTreeNode : TreeNode
    {
        private bool populateOnDemand;

        private TreeNode onDemandPlaceholderTreeNode;

        private bool preventChildCheck;

        private bool preventParentCheck;

        public event TreeNodeEventHandler TreeNodePopulate;

        private ExtendedTreeNode(string text, bool populateOnDemand, object tag) : base(text)
        {
            Tag = tag;
            this.populateOnDemand = populateOnDemand;
        }

        public static ExtendedTreeNode AddNewExtendedTreeNode(TreeNodeCollection parentNodeCollection, string text)
        {
            return AddNewExtendedTreeNode(parentNodeCollection, text, false, null);
        }

        public static ExtendedTreeNode AddNewExtendedTreeNode(TreeNodeCollection parentNodeCollection, string text, object tag)
        {
            return AddNewExtendedTreeNode(parentNodeCollection, text, false, tag);
        }

        public static ExtendedTreeNode AddNewExtendedTreeNode(TreeNodeCollection parentNodeCollection, string text, bool populateOnDemand)
        {
            return AddNewExtendedTreeNode(parentNodeCollection, text, populateOnDemand, null);
        }

        public static ExtendedTreeNode AddNewExtendedTreeNode(List<TreeNode> parentNodeCollection, string text, bool populateOnDemand, object tag)
        {
            var extendedTreeNode = new ExtendedTreeNode(text, populateOnDemand, tag);
            parentNodeCollection.Add(extendedTreeNode);

            return extendedTreeNode;
        }

        public static ExtendedTreeNode AddNewExtendedTreeNode(TreeNodeCollection parentNodeCollection, string text, bool populateOnDemand, object tag)
        {
            var extendedTreeNode = new ExtendedTreeNode(text, populateOnDemand, tag);
            parentNodeCollection.Add(extendedTreeNode);

            if (extendedTreeNode.TreeView == null)
            {
                throw new InvalidOperationException("You cannot add a new ExtendedTreeNode to a node that is not already a member of a TreeView.");
            }

            extendedTreeNode.Prepare();

            return extendedTreeNode;
        }

        public static ExtendedTreeNode AddNewExtendedTreeNode(TreeNodeCollection parentNodeCollection, string text, bool populateOnDemand, object tag, TreeNodeEventHandler handler)
        {
            var node = AddNewExtendedTreeNode(parentNodeCollection, text, populateOnDemand, tag);

            node.TreeNodePopulate += handler;

            return node;
        }
        
        protected virtual void OnTreeNodePopulate(TreeViewCancelEventArgs e)
        {
            if (TreeNodePopulate != null)
            {
                TreeNodePopulate(this, e);
            }
        }        

        public void SuspendCheckEvent()
        {
            TreeView.AfterCheck -= OnTreeViewAfterCheck;
        }

        public void ResumeCheckEvent()
        {
            TreeView.AfterCheck += OnTreeViewAfterCheck;
        }

        public void Prepare()
        {
            TreeView.AfterCheck += OnTreeViewAfterCheck;

            // Check our flag if we have a parent node (not a top-level node) and the parent is checked
            if (Parent != null)
            {
                var info = Tag as EntityInfo;
                if (info != null && info.Corrupted)
                    Checked = false;
                else
                    Checked = Parent.Checked;
            }

            if (populateOnDemand)
            {
                // Create the placeholder node that should never be visible - we do this so the +/- will show up on our node
                // Note that this should be a TreeNode and not an ExtendedTreeNode to avoid having it watch for check events
                onDemandPlaceholderTreeNode = new TreeNode("...");
                Nodes.Add(onDemandPlaceholderTreeNode);

                // Watch the BeforeExpand event on the treeview so we can replace the placeholder node
                // with the true child nodes of this node
                TreeView.BeforeExpand += OnTreeViewBeforeExpand;
            }
        }

        private void CheckChildNode(TreeNode node, bool value)
        {
            var extendedNode = node as ExtendedTreeNode;
            if (extendedNode != null)
            {
                extendedNode.PreventParentCheck = true;
            }
            try
            {
                node.Checked = value;
            }
            finally
            {
                if (extendedNode != null)
                {
                    extendedNode.PreventParentCheck = false;
                }
            }
        }

        private void CheckParentNode(TreeNode node, bool value)
        {
            var extendedNode = node as ExtendedTreeNode;
            if (extendedNode != null)
            {
                extendedNode.PreventChildCheck = true;
            }
            try
            {
                node.Checked = value;
            }
            finally
            {
                if (extendedNode != null)
                {
                    extendedNode.PreventChildCheck = false;
                }
            }
        }

        private void OnTreeViewBeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            // If our node was expanded and we haven't fired the TreeNodePopulate event yet, go ahead and fire it
            if (!Populated && ReferenceEquals(e.Node, this) && e.Action == TreeViewAction.Expand)
            {
                Populated = true;
                Nodes.Remove(onDemandPlaceholderTreeNode);      
                OnTreeNodePopulate(e);

                if (e.Cancel)
                {
                    Populated = false;
                    Nodes.Add(onDemandPlaceholderTreeNode);
                }
            }
        }

        private void OnTreeViewAfterCheck(object sender, TreeViewEventArgs e)
        {
            //
            // Check/Uncheck parent and child nodes if our node is checked
            // Note that we are careful to set flags preventing circular checking
            // For example, if a child node is checked, it may cause its parent node
            // to be checked, but we don't want that to cause all of its children to be
            // checked.
            //

            if (ReferenceEquals(e.Node, this))
            {
                // Update all child nodes
                if (!preventChildCheck)
                {
                    foreach (TreeNode childNode in Nodes)
                    {
                        if (childNode.Checked != Checked)
                        {
                            CheckChildNode(childNode, Checked);
                        }
                    }
                }

                // Update our parent if necessary
                if (Parent != null && !preventParentCheck)
                {
                    // If we were just checked make sure our parent is checked
                    if (Checked && !Parent.Checked)
                    {
                        CheckParentNode(Parent, true);
                    }

                    // If we were just unchecked make sure our parent is unchecked if all of its children are unchecked
                    if (!Checked)
                    {
                        var uncheckParent = true;
                        foreach (TreeNode peerNode in Parent.Nodes)
                        {
                            if (peerNode.Checked)
                            {
                                uncheckParent = false;
                                break;
                            }
                        }
                        if (uncheckParent)
                        {
                            CheckParentNode(Parent, false);
                        }
                    }
                }
            }
        }

        public bool Populated
        {
            get; 
            private set;
        }

        public bool PreventChildCheck
        {
            get { return preventChildCheck; }
            set { preventChildCheck = value; }
        }

        public bool PreventParentCheck
        {
            get { return preventParentCheck; }
            set { preventParentCheck = value; }
        }
    }
}