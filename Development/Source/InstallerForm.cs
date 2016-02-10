/**********************************************************************/
/*                                                                    */
/*                   SharePoint Solution Installer                    */
/*             http://www.codeplex.com/sharepointinstaller            */
/*                                                                    */
/*               (c) Copyright 2007 Lars Fastrup Nielsen.             */
/*                                                                    */
/*  This source is subject to the Microsoft Permissive License.       */
/*  http://www.codeplex.com/sharepointinstaller/Project/License.aspx  */
/*                                                                    */
/* KML: Changed InstallerForm to own InstallerOperation               */
/* KML: Minor update to the location and size of the subtitle         */
/*                                                                    */
/**********************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace CodePlex.SharePointInstaller
{
  public partial class InstallerForm : Form
  {
    private readonly InstallerControlList contentControls;
    private readonly InstallOptions options;
    private InstallOperation operation = InstallOperation.Install;
    private InstallerControl currentContentControl;
    private int currentContentControlIndex = 0;

    public InstallerForm()
    {
      this.contentControls = new InstallerControlList();
      this.options = new InstallOptions();
      InitializeComponent();

      this.Load += new EventHandler(InstallerForm_Load);
    }

    #region Event Handlers

    private void InstallerForm_Load(object sender, EventArgs e)
    {
      string bannerImageFile = InstallConfiguration.BannerImage;
      if (!String.IsNullOrEmpty(bannerImageFile))
      {
        this.titlePanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
        if (bannerImageFile != "Default")
        {
          this.titlePanel.BackgroundImage = LoadImage(bannerImageFile);
        } else
        {
          this.titlePanel.BackgroundImage = global::CodePlex.SharePointInstaller.Properties.Resources.Banner;
        }
      }

      string logoImageFile = InstallConfiguration.LogoImage;
      if (!String.IsNullOrEmpty(logoImageFile))
      {
        if (logoImageFile != "Default")
        {
          this.logoPicture.BackgroundImage = LoadImage(logoImageFile);
        }
      }

      ReplaceContentControl(0);
    }

    private void nextButton_Click(object sender, EventArgs e)
    {
      currentContentControlIndex++;
      ReplaceContentControl(currentContentControlIndex);
    }

    private void prevButton_Click(object sender, EventArgs e)
    {
      currentContentControlIndex--;
      ReplaceContentControl(currentContentControlIndex);
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
      if (currentContentControl != null)
      {
        currentContentControl.RequestCancel();
      }
      else
      {
        Application.Exit();
      }
    }

    #endregion

    #region Public Properties

    public InstallerControlList ContentControls
    {
      get { return contentControls; }
    }

    public Button AbortButton
    {
      get
      {
        return cancelButton;
      }
    }

    public Button PrevButton
    {
      get
      {
        return prevButton;
      }
    }

    public Button NextButton
    {
      get
      {
        return nextButton;
      }
    }

    public InstallOperation Operation
    {
      get { return operation; }
      set { operation = value; }
    }

    #endregion

    #region Public Methods
    public void SetProductLabel(string label)
    {
        productLabel.Text = label;
    }
    public void SetTitle(string title)
    {
      titleLabel.Text = title;
    }

    public void SetSubTitle(string title)
    {
      subTitleLabel.Text = title;
    }

      /// <summary>
      /// Store Next Title to be used for most recently added control
      /// </summary>
      /// <param name="title"></param>
    public void StoreNextTitle(string title)
    {
        if (ContentControls.Count > 0)
        {
            InstallerControl control = ContentControls[ContentControls.Count - 1];
            control.StoreNextTitle(title);
            if (currentContentControl == control)
            {
                DisplayNextTitle(title);
            }
        }
    }
    public void DisplayNextTitle(string nextTitle)
    {
        this.NextTextLabel.Text = nextTitle;
        this.NextLabel.Visible = !String.IsNullOrEmpty(this.NextTextLabel.Text);
    }

    #endregion

    #region Private Methods

    private void ReplaceContentControl(int index)
    {
      if (currentContentControl != null)
      {
        currentContentControl.CloseControl(options);
      }

      if (index == 0)
      {
        prevButton.Enabled = false;
        nextButton.Enabled = true;
      } else if (index == (contentControls.Count - 1))
      {
        prevButton.Enabled = true;
        nextButton.Enabled = false;
      } else
      {
        prevButton.Enabled = true;
        nextButton.Enabled = true;
      }

      InstallerControl newContentControl = contentControls[index];
      newContentControl.Dock = DockStyle.Fill;

      titleLabel.Text = newContentControl.Title;
      subTitleLabel.Text = newContentControl.SubTitle;

      contentPanel.Controls.Clear();
      contentPanel.Controls.Add(newContentControl);

      newContentControl.OpenControl(options);

      currentContentControl = newContentControl;
    }

    private Image LoadImage(string filename)
    {
      try
      {
        return Image.FromFile(filename);
      }

      catch (IOException)
      {
        return null;
      }
    }

    #endregion
  }
}