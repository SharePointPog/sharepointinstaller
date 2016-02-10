using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace CodePlex.SharePointInstaller
{
  /// <summary>
  /// Extends the TreeNode class to allow for Populate On Demand and managing checkboxes when a parent/child node's checkbox changes.
  /// </summary>
  public class ExtendedTreeNode : TreeNode
  {
    #region Static Methods

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

    public static ExtendedTreeNode AddNewExtendedTreeNode(TreeNodeCollection parentNodeCollection, string text, bool populateOnDemand, object tag)
    {
      ExtendedTreeNode extendedTreeNode = new ExtendedTreeNode(text, populateOnDemand, tag);
      parentNodeCollection.Add(extendedTreeNode);

      if (extendedTreeNode.TreeView == null)
      {
        throw new InvalidOperationException("You cannot add a new ExtendedTreeNode to a node that is not already a member of a TreeView.");
      }

      extendedTreeNode.Prepare();

      return extendedTreeNode;
    }

    #endregion

    #region Member Variables

    private bool populateOnDemand;
    private bool populated;
    private TreeNode onDemandPlaceholderTreeNode;
    private bool preventChildCheck;
    private bool preventParentCheck;

    #endregion

    #region Events

    public event TreeNodeEventHandler TreeNodePopulate;
    //public delegate void TreeNodeEventHandler(object sender, TreeViewEventArgs e);
    public delegate void TreeNodeEventHandler(object sender, TreeViewCancelEventArgs e);

    //protected virtual void OnTreeNodePopulate(TreeViewEventArgs e)
    protected virtual void OnTreeNodePopulate(TreeViewCancelEventArgs e)
    {
      if (TreeNodePopulate != null)
      {
        TreeNodePopulate(this, e);
      }
    }

    #endregion

    #region Constructor

    private ExtendedTreeNode(string text, bool populateOnDemand, object tag) : base(text)
    {
      base.Tag = tag;
      this.populateOnDemand = populateOnDemand;
    }

    #endregion

    #region Public Properties

    public bool Populated
    {
      get { return populated; }
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

    #endregion

    #region Public Methods

    public void SuspendCheckEvent()
    {
      this.TreeView.AfterCheck -= new TreeViewEventHandler(TreeView_AfterCheck);
    }

    public void ResumeCheckEvent()
    {
      this.TreeView.AfterCheck += new TreeViewEventHandler(TreeView_AfterCheck);
    }

    #endregion

    #region Private Methods

    private void Prepare()
    {
      this.TreeView.AfterCheck += new TreeViewEventHandler(TreeView_AfterCheck);

      // Check our flag if we have a parent node (not a top-level node) and the parent is checked
      if (this.Parent != null)
      {
        this.Checked = this.Parent.Checked;
      }

      if (populateOnDemand)
      {
        // Create the placeholder node that should never be visible - we do this so the +/- will show up on our node
        // Note that this should be a TreeNode and not an ExtendedTreeNode to avoid having it watch for check events
        onDemandPlaceholderTreeNode = new TreeNode("...");
        this.Nodes.Add(onDemandPlaceholderTreeNode);

        // Watch the BeforeExpand event on the treeview so we can replace the placeholder node
        // with the true child nodes of this node
        this.TreeView.BeforeExpand += new TreeViewCancelEventHandler(TreeView_BeforeExpand);
      }
    }

    private void CheckChildNode(TreeNode node, bool value)
    {
      ExtendedTreeNode extendedNode = node as ExtendedTreeNode;
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
      ExtendedTreeNode extendedNode = node as ExtendedTreeNode;
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

    #endregion

    #region Event Handlers

    private void TreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
    {
      // If our node was expanded and we haven't fired the TreeNodePopulate event yet, go ahead and fire it
      if (!populated && ReferenceEquals(e.Node, this) && e.Action == TreeViewAction.Expand)
      {
        populated = true;
        this.Nodes.Remove(onDemandPlaceholderTreeNode);
        //TreeViewEventArgs treeViewEventArgs = new TreeViewEventArgs(this, e.Action);
        //OnTreeNodePopulate(treeViewEventArgs);
        OnTreeNodePopulate(e);

        if (e.Cancel)
        {
          // It was cancelled
          populated = false;
          this.Nodes.Add(onDemandPlaceholderTreeNode);
        }
      }
    }

    void TreeView_AfterCheck(object sender, TreeViewEventArgs e)
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
          foreach (TreeNode childNode in this.Nodes)
          {
            if (childNode.Checked != this.Checked)
            {
              CheckChildNode(childNode, this.Checked);
            }
          }
        }

        // Update our parent if necessary
        if (this.Parent != null && !preventParentCheck)
        {
          // If we were just checked make sure our parent is checked
          if (this.Checked && !this.Parent.Checked)
          {
            CheckParentNode(this.Parent, true);
          }

          // If we were just unchecked make sure our parent is unchecked if all of its children are unchecked
          if (!this.Checked)
          {
            bool uncheckParent = true;
            foreach (TreeNode peerNode in this.Parent.Nodes)
            {
              if (peerNode.Checked)
              {
                uncheckParent = false;
                break;
              }
            }
            if (uncheckParent)
            {
              CheckParentNode(this.Parent, false);
            }
          }
        }
      }
    }

    #endregion
  }
}
