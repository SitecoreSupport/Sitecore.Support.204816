using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Form.Core.Configuration;
using Sitecore.Form.Core.Utility;
using Sitecore.Forms.Shell.UI.Controls;
using Sitecore.Globalization;
using Sitecore.Layouts;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;
using Sitecore.WFFM.Abstractions.Dependencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sitecore.Support.Forms.Shell.UI
{
  public class InsertFormWizard : Sitecore.Forms.Shell.UI.CreateFormWizard
  {
    protected PlaceholderList Placeholders;

    protected WizardDialogBaseXmlControl SelectPlaceholder;

    protected WizardDialogBaseXmlControl FormName;

    protected Radiobutton InsertForm;

    private string currentItemUri;

    public bool IsCalledFromPageEditor
    {
      get
      {
        return Sitecore.Web.WebUtil.GetQueryString("pe", "0") == "1";
      }
    }

    public string DeviceID
    {
      get
      {
        return Sitecore.Web.WebUtil.GetQueryString("deviceid");
      }
    }

    public string Layout
    {
      get
      {
        return StringUtil.GetString(base.ServerProperties["LayoutCurrent"]);
      }
      set
      {
        Assert.ArgumentNotNull(value, "value");
        base.ServerProperties["LayoutCurrent"] = value;
      }
    }

    public string ListValue
    {
      get
      {
        return this.Placeholders.SelectedPlaceholder;
      }
    }

    public string Mode
    {
      get
      {
        return Sitecore.Web.WebUtil.GetQueryString("mode");
      }
    }

    public string Placeholder
    {
      get
      {
        return Sitecore.Web.WebUtil.GetQueryString("placeholder");
      }
    }

    protected override Item FormsRoot
    {
      get
      {
        Item item = Database.GetItem(ItemUri.Parse(this.currentItemUri));
        return SiteUtils.GetFormsRootItemForItem(item);
      }
    }

    protected override bool RenderConfirmationFormSection
    {
      get
      {
        return !this.InsertForm.Checked;
      }
    }

    public Item GetCurrentItem()
    {
      string queryString = Sitecore.Web.WebUtil.GetQueryString("id");
      string queryString2 = Sitecore.Web.WebUtil.GetQueryString("la");
      string queryString3 = Sitecore.Web.WebUtil.GetQueryString("vs");
      string queryString4 = Sitecore.Web.WebUtil.GetQueryString("db");
      ItemUri uri = new ItemUri(queryString, Language.Parse(queryString2), Sitecore.Data.Version.Parse(queryString3), queryString4);
      return Database.GetItem(uri);
    }

    protected override bool ActivePageChanging(string page, ref string newpage)
    {
      bool result = true;
      if (!this.AnalyticsSettings.IsAnalyticsAvailable && newpage == "AnalyticsPage")
      {
        newpage = "ConfirmationPage";
      }
      if (!base.CheckGoalSettings(page, ref newpage))
      {
        return result;
      }
      if (this.InsertForm.Checked && page == "CreateForm" && newpage == "FormName")
      {
        newpage = "SelectForm";
      }
      if (this.InsertForm.Checked && page == "ConfirmationPage" && newpage == "AnalyticsPage")
      {
        newpage = "SelectPlaceholder";
      }
      if (this.InsertForm.Checked && page == "SelectForm" && newpage == "FormName")
      {
        newpage = "CreateForm";
      }
      if ((page == "CreateForm" || page == "FormName") && newpage == "SelectForm")
      {
        if (this.EbFormName.Value == string.Empty)
        {
          Context.ClientPage.ClientResponse.Alert(DependenciesManager.ResourceManager.Localize("EMPTY_FORM_NAME"));
          newpage = ((page == "CreateForm") ? "CreateForm" : "FormName");
          return result;
        }
        if (this.FormsRoot.Database.GetItem(this.FormsRoot.Paths.ContentPath + "/" + this.EbFormName.Value) != null)
        {
          StringBuilder stringBuilder = new StringBuilder();
          stringBuilder.AppendFormat("'{0}' ", this.EbFormName.Value);
          stringBuilder.Append(DependenciesManager.ResourceManager.Localize("IS_NOT_UNIQUE_NAME"));
          Context.ClientPage.ClientResponse.Alert(stringBuilder.ToString());
          newpage = ((page == "CreateForm") ? "CreateForm" : "FormName");
          return result;
        }
        if (!Regex.IsMatch(this.EbFormName.Value, Sitecore.Configuration.Settings.ItemNameValidation, RegexOptions.ECMAScript))
        {
          StringBuilder stringBuilder2 = new StringBuilder();
          stringBuilder2.AppendFormat("'{0}' ", this.EbFormName.Value);
          stringBuilder2.Append(DependenciesManager.ResourceManager.Localize("IS_NOT_VALID_NAME"));
          Context.ClientPage.ClientResponse.Alert(stringBuilder2.ToString());
          newpage = ((page == "CreateForm") ? "CreateForm" : "FormName");
          return result;
        }
        if (this.CreateBlankForm.Checked)
        {
          newpage = ((!string.IsNullOrEmpty(this.Placeholder)) ? "ConfirmationPage" : "SelectPlaceholder");
          if (this.AnalyticsSettings.IsAnalyticsAvailable && newpage == "ConfirmationPage")
          {
            newpage = "AnalyticsPage";
          }
        }
      }
      if (page == "SelectForm" && (newpage == "SelectPlaceholder" || newpage == "ConfirmationPage" || newpage == "AnalyticsPage"))
      {
        string selected = this.multiTree.Selected;
        Item item = StaticSettings.GlobalFormsRoot.Database.GetItem(selected);
        if (selected == null || item == null)
        {
          Context.ClientPage.ClientResponse.Alert(DependenciesManager.ResourceManager.Localize("PLEASE_SELECT_FORM"));
          newpage = "SelectForm";
          return result;
        }
        if (item.TemplateID != IDs.FormTemplateID)
        {
          StringBuilder stringBuilder3 = new StringBuilder();
          stringBuilder3.AppendFormat("'{0}' ", item.Name);
          stringBuilder3.Append(DependenciesManager.ResourceManager.Localize("IS_NOT_FORM"));
          Context.ClientPage.ClientResponse.Alert(stringBuilder3.ToString());
          newpage = "SelectForm";
          return result;
        }
      }
      if (newpage == "SelectPlaceholder" && page == "AnalyticsPage")
      {
        newpage = (string.IsNullOrEmpty(this.Placeholder) ? "SelectPlaceholder" : "SelectForm");
      }
      if (newpage == "SelectPlaceholder" && page == "SelectForm" && !this.InsertForm.Checked)
      {
        newpage = (string.IsNullOrEmpty(this.Placeholder) ? "SelectPlaceholder" : ((!this.AnalyticsSettings.IsAnalyticsAvailable) ? "ConfirmationPage" : "AnalyticsPage"));
      }
      if (newpage == "SelectPlaceholder" && page == "SelectForm" && this.InsertForm.Checked)
      {
        newpage = (string.IsNullOrEmpty(this.Placeholder) ? "SelectPlaceholder" : "ConfirmationPage");
      }
      if (page == "ConfirmationPage" && newpage == "ConfirmationPage" && !this.AnalyticsSettings.IsAnalyticsAvailable)
      {
        newpage = (string.IsNullOrEmpty(this.Placeholder) ? "SelectPlaceholder" : "SelectForm");
      }
      if (page == "ConfirmationPage" && (newpage == "SelectPlaceholder" || newpage == "AnalyticsPage"))
      {
        if (newpage != "AnalyticsPage")
        {
          newpage = (string.IsNullOrEmpty(this.Placeholder) ? "SelectPlaceholder" : "SelectForm");
        }
        this.NextButton.Disabled = false;
        this.BackButton.Disabled = false;
        this.CancelButton.Header = "Cancel";
        this.NextButton.Header = "Next >";
      }
      if (page == "SelectPlaceholder" && (newpage == "ConfirmationPage" || newpage == "AnalyticsPage"))
      {
        if (string.IsNullOrEmpty(this.ListValue))
        {
          newpage = "SelectPlaceholder";
          Context.ClientPage.ClientResponse.Alert(DependenciesManager.ResourceManager.Localize("SELECT_MUST_SELECT_PLACEHOLDER"));
        }
        if (this.InsertForm.Checked)
        {
          newpage = "ConfirmationPage";
        }
      }
      if ((((page == "ConfirmationPage" || page == "AnalyticsPage") && newpage == "SelectForm") || (page == "SelectPlaceholder" && newpage == "SelectForm")) && this.CreateBlankForm.Checked)
      {
        newpage = "CreateForm";
      }
      if (newpage == "ConfirmationPage")
      {
        this.ChoicesLiteral.Text = this.RenderSetting();
      }
      return result;
    }

    protected override string GenerateItemSetting()
    {
      string text = this.ListValue ?? this.Placeholder;
      string value = this.EbFormName.Value;
      Item item = Database.GetItem(ItemUri.Parse(this.currentItemUri));
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("<p>");
      Item formsRootItemForItem = SiteUtils.GetFormsRootItemForItem(item);
      stringBuilder.AppendFormat(DependenciesManager.ResourceManager.Localize("FORM_ADDED_MESSAGE"), new object[]
      {
        item.Name,
        text,
        formsRootItemForItem.Paths.FullPath,
        value
      });
      stringBuilder.Append("</p>");
      return stringBuilder.ToString();
    }

    protected override void Localize()
    {
      base.Localize();
      this.SelectPlaceholder["Header"] = DependenciesManager.ResourceManager.Localize("SELECT_PLACEHOLDER");
      this.SelectPlaceholder["Text"] = DependenciesManager.ResourceManager.Localize("FORM_WILL_BE_INSERTED_INTO_PLACEHOLDER");
      this.InsertForm.Header = DependenciesManager.ResourceManager.Localize("INSERT_FORM");
      this.CreateForm["Header"] = DependenciesManager.ResourceManager.Localize("INSERT_FORM_HEADER");
      this.CreateForm["Text"] = DependenciesManager.ResourceManager.Localize("INSERT_FORM_TEXT");
      this.FormName["Header"] = DependenciesManager.ResourceManager.Localize("ENTER_FORM_NAME_HEADER");
      this.FormName["Text"] = DependenciesManager.ResourceManager.Localize("ENTER_FORM_NAME_TEXT");
    }

    private ID GetRendering(Item currentItem)
    {
      LayoutDefinition definition = LayoutDefinition.Parse(LayoutField.GetFieldValue(currentItem.Fields[FieldIDs.FinalLayoutField]));
      return StaticSettings.GetRendering(definition);
    }

    protected override void OnLoad(EventArgs e)
    {
      if (!Context.ClientPage.IsEvent)
      {
        Item currentItem = this.GetCurrentItem();
        this.currentItemUri = currentItem.Uri.ToString();
        this.Localize();
      }
      base.OnLoad(e);
      if (!Context.ClientPage.IsEvent)
      {
        Item currentItem2 = this.GetCurrentItem();
        this.EbFormName.Value = base.GetUniqueName(currentItem2.Name);
        this.Layout = currentItem2[FieldIDs.FinalLayoutField];
        this.Placeholders.DeviceID = this.DeviceID;
        this.Placeholders.ShowDeviceTree = string.IsNullOrEmpty(this.Mode);
        this.Placeholders.ItemUri = this.currentItemUri;
        this.Placeholders.AllowedRendering =this.GetRendering(currentItem2).ToString();
        return;
      }
      this.currentItemUri = (base.ServerProperties["forms_current_item"] as string);
    }

    protected override void OnPreRender(EventArgs e)
    {
      base.OnPreRender(e);
      base.ServerProperties["forms_current_item"] = this.currentItemUri;
    }

    protected override void ActivePageChanged(string page, string oldPage)
    {
      base.ActivePageChanged(page, oldPage);
      if (page == "ConfirmationPage" && this.InsertForm.Checked)
      {
        this.CancelButton.Header = "Cancel";
        this.NextButton.Header = "Insert";
      }
    }

    protected override void OnNext(object sender, EventArgs formEventArgs)
    {
      if (this.NextButton.Header == "Create" || this.NextButton.Header == "Insert")
      {
        this.SaveForm();
        SheerResponse.SetModified(false);
      }
      base.Next();
    }

    protected override void SaveForm()
    {
      string deviceID = this.Placeholders.DeviceID;
      Item form;
      if (!this.InsertForm.Checked)
      {
        base.SaveForm();
        form = Database.GetItem(ItemUri.Parse((string)base.ServerProperties[this.newFormUri]));
      }
      else
      {
        string queryString = Sitecore.Web.WebUtil.GetQueryString("la");
        Language contentLanguage = Context.ContentLanguage;
        if (!string.IsNullOrEmpty(queryString))
        {
          Language.TryParse(Sitecore.Web.WebUtil.GetQueryString("la"), out contentLanguage);
        }
        form = this.FormsRoot.Database.GetItem(this.CreateBlankForm.Checked ? string.Empty : this.multiTree.Selected, contentLanguage);
      }
      if (this.Mode != StaticSettings.DesignMode && this.Mode != "edit")
      {
        Item item = Database.GetItem(ItemUri.Parse(this.currentItemUri));
        LayoutDefinition layoutDefinition = LayoutDefinition.Parse(LayoutField.GetFieldValue(item.Fields[FieldIDs.FinalLayoutField]));
        RenderingDefinition rendering = new RenderingDefinition();
        string listValue = this.ListValue;
        ID rendering3 = StaticSettings.GetRendering(layoutDefinition);
        rendering.ItemID = rendering3.ToString();
        if (rendering.ItemID == IDs.FormInterpreterID.ToString())
        {
          rendering.Parameters = "FormID=" + form.ID;
        }
        else
        {
          rendering.Datasource = form.ID.ToString();
        }
        rendering.Placeholder = listValue;
        DeviceDefinition device = layoutDefinition.GetDevice(deviceID);
        List<RenderingDefinition> renderings = device.GetRenderings(rendering.ItemID);
        if (rendering3 != IDs.FormMvcInterpreterID && renderings.Any((RenderingDefinition x) => (x.Parameters != null && x.Parameters.Contains(rendering.Parameters)) || (x.Datasource != null && x.Datasource.Contains(form.ID.ToString()))))
        {
          Context.ClientPage.ClientResponse.Alert(DependenciesManager.ResourceManager.Localize("FORM_CANT_BE_INSERTED"));
          return;
        }
        item.Editing.BeginEdit();
        device.AddRendering(rendering);
        if (item.Name != "__Standard Values")
        {
          LayoutField.SetFieldValue(item.Fields[FieldIDs.FinalLayoutField], layoutDefinition.ToXml());
        }
        else
        {
          item[FieldIDs.FinalLayoutField] = layoutDefinition.ToXml();
        }
        item.Editing.EndEdit();
        return;
      }
      else
      {
        Item item2 = Database.GetItem(ItemUri.Parse(this.currentItemUri));
        LayoutDefinition layoutDefinition2 = LayoutDefinition.Parse(LayoutField.GetFieldValue(item2.Fields[FieldIDs.FinalLayoutField]));
        RenderingDefinition rendering = new RenderingDefinition();
        string listValue2 = this.ListValue;
        ID rendering2 = StaticSettings.GetRendering(layoutDefinition2);
        rendering.ItemID = rendering2.ToString();
        rendering.Parameters = "FormID=" + form.ID;
        rendering.Datasource = form.ID.ToString();
        rendering.Placeholder = listValue2;
        DeviceDefinition device2 = layoutDefinition2.GetDevice(deviceID);
        List<RenderingDefinition> renderings2 = device2.GetRenderings(rendering.ItemID);
        if (rendering2 != IDs.FormMvcInterpreterID && renderings2.Any((RenderingDefinition x) => (x.Parameters != null && x.Parameters.Contains(rendering.Parameters)) || (x.Datasource != null && x.Datasource.Contains(form.ID.ToString()))))
        {
          Context.ClientPage.ClientResponse.Alert(DependenciesManager.ResourceManager.Localize("FORM_CANT_BE_INSERTED"));
          return;
        }
        SheerResponse.SetDialogValue(form.ID.ToString());
        return;
      }
    }
  }
}
