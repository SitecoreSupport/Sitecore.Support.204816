using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Form.Core.Configuration;
using Sitecore.Pipelines.GetRenderingDatasource;
using Sitecore.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Support.Form.Core.Pipeline.InsertRenderings.Processors
{
  public class GetDialogUrl
  {
    public void Process(GetRenderingDatasourceArgs args)
    {
      Assert.IsNotNull(args, "args");
      if (args.ContextItemPath != null && (args.RenderingItem.ID == IDs.FormInterpreterID || args.RenderingItem.ID == IDs.FormMvcInterpreterID))
      {
        Assert.IsNotNull(args.ContentDatabase, "args.ContentDatabase");
        Assert.IsNotNull(args.ContextItemPath, "args.ContextItemPath");
        Database contentDatabase = args.ContentDatabase;
        Item item = contentDatabase.GetItem(args.ContextItemPath, Context.Language);
        Assert.IsNotNull(item, "currentItem");
        object value = Context.ClientData.GetValue(StaticSettings.PrefixId + StaticSettings.PlaceholderKeyId);
        string value2 = (value != null) ? value.ToString() : string.Empty;
        string designMode = StaticSettings.DesignMode;
        if (item.Visualization !=null)
        {
          UrlString urlString = new UrlString(UIUtil.GetUri("control:Forms.InsertFormWizard"));
          urlString.Add("id", item.ID.ToString());
          urlString.Add("db", item.Database.Name);
          urlString.Add("la", item.Language.Name);
          urlString.Add("vs", item.Version.Number.ToString());
          urlString.Add("pe", "1");
          if (!string.IsNullOrEmpty(value2))
          {
            urlString.Add("placeholder", value2);
          }
          if (!string.IsNullOrEmpty(designMode))
          {
            urlString.Add("mode", designMode);
          }
          args.DialogUrl = urlString.ToString();
          if (string.IsNullOrEmpty(args.CurrentDatasource))
          {
            string text = args.RenderingItem["data source"];
            if (!string.IsNullOrEmpty(text))
            {
              args.CurrentDatasource = text;
            }
          }
          args.AbortPipeline();
        }
      }
    }
  }
}