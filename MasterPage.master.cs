using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class MasterPage : System.Web.UI.MasterPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (isMobileBrowser())
        {
            NavigationMenu.Visible = false;
            NavigationTreeView.Visible = true;
        }

        if (Request.UserAgent.IndexOf("AppleWebKit") > 0)
        {
            Request.Browser.Adapters.Clear();
        }

        if (!IsPostBack)
        {
            AddEventHandlers();
            AddPageContent();
        }
    }
    /*02032010 - Fixed the display problem in Safari and Google Chrome
     http://www.google.com/support/forum/p/Chrome/thread?tid=7506cca26ec06af7&hl=en
     */
    protected override void AddedControl(Control control, int index)
    {
        // This is necessary because Safari and Chrome browsers don't display the Menu control correctly.
        // Add this to the code in your master page.
        if (Request.ServerVariables["http_user_agent"].IndexOf("Safari", StringComparison.CurrentCultureIgnoreCase) != -1)
            this.Page.ClientTarget = "uplevel";

        base.AddedControl(control, index);
    } 

    private void AddPageContent()
    {
        string pageName = HttpContext.Current.Request.Url.AbsolutePath.Substring(
                     HttpContext.Current.Request.Url.AbsolutePath.LastIndexOf("/") + 1);

        ContentPlaceHolder contentPlaceHolder = (ContentPlaceHolder)this.Page.Master.FindControl("ContentPlaceHolder1");
        Label label = new Label();
        label.Text = " <br /> Content for page: " + pageName;
        contentPlaceHolder.Controls.Add(label);
    }

    private void AddEventHandlers()
    {
        NavigationMenu.MenuItemDataBound += new MenuEventHandler(NavigationMenu_MenuItemDataBound);
        SiteMap.SiteMapResolve += new SiteMapResolveEventHandler(SiteMap_SiteMapResolve);
        SiteMapPath1.ItemCreated +=new SiteMapNodeItemEventHandler(SiteMapPath1_ItemCreated);
    }

    protected void SiteMapPath1_ItemCreated(object sender, SiteMapNodeItemEventArgs e)
    {
        if (e.Item.ItemType == SiteMapNodeItemType.Root)
        {
            SiteMapPath1.PathSeparator = " ";
        }
        else
        {
            SiteMapPath1.PathSeparator = " >> ";
        }
    }
  
    //http://msdn.microsoft.com/en-us/library/system.web.sitemap.sitemapresolve.aspx
    //solve > SiteMapNode is readonly, property Title cannot be modified. 

    SiteMapNode SiteMap_SiteMapResolve(object sender, SiteMapResolveEventArgs e)
    {
        if (SiteMap.CurrentNode != null)
        {
            SiteMapNode currentNode = SiteMap.CurrentNode.Clone(true);
            SiteMapNode tempNode = currentNode;
            tempNode = ReplaceNodeText(tempNode);

            return currentNode;
        }

        return null;
    }

    //remove <u></u> tag recursively
    internal SiteMapNode ReplaceNodeText(SiteMapNode smn)
    {
        //current node
        if (smn != null && smn.Title.Contains("<u>"))
        {
            smn.Title = smn.Title.Replace("<u>", "").Replace("</u>", "");
        }

        //parent nd
        if (smn.ParentNode != null)
        {
            if (smn.ParentNode.Title.Contains("<u>"))
            {
                SiteMapNode gpn = smn.ParentNode;
                smn.ParentNode.Title = smn.ParentNode.Title.Replace("<u>", "").Replace("</u>", "");
                smn = ReplaceNodeText(gpn);
            }
        }
        return smn;
    }

    void NavigationMenu_MenuItemDataBound(object sender, MenuEventArgs e)
    {
        SiteMapNode node = (SiteMapNode)e.Item.DataItem;
       
        //set the target of the navigation menu item (blank, self, etc...)
        if (node["target"] != null)
        {
            e.Item.Target = node["target"];
        }
        //create access key button
        if (node["accesskey"] != null)
        {
            CreateAccessKeyButton(node["accesskey"] as string, node.Url);
        }
    }

    //create access key button
    void CreateAccessKeyButton(string ak, string url)
    {
        HtmlButton inputBtn = new HtmlButton();
        inputBtn.Style.Add("width", "1px");
        inputBtn.Style.Add("height", "1px");
        inputBtn.Style.Add("position", "absolute");
        inputBtn.Style.Add("left", "-2555px");
        inputBtn.Style.Add("z-index", "-1");
        inputBtn.Attributes.Add("type", "button");
        inputBtn.Attributes.Add("value", "");
        inputBtn.Attributes.Add("accesskey", ak);
        inputBtn.Attributes.Add("onclick", "navigateTo('" + url + "');");

        AccessKeyPanel.Controls.Add(inputBtn);
    }

    /*
     * 02032010, Detecting a mobile browser
     * http://www.codeproject.com/KB/aspnet/mobiledetect.aspx
     Place this on a seperate class
     */
    public static readonly string[] mobiles =
          new[]
                {
                    "midp", "j2me", "avant", "docomo", 
                    "novarra", "palmos", "palmsource", 
                    "240x320", "opwv", "chtml",
                    "pda", "windows ce", "mmp/", 
                    "blackberry", "mib/", "symbian", 
                    "wireless", "nokia", "hand", "mobi",
                    "phone", "cdm", "up.b", "audio", 
                    "SIE-", "SEC-", "samsung", "HTC", 
                    "mot-", "mitsu", "sagem", "sony"
                    , "alcatel", "lg", "eric", "vx", 
                    "NEC", "philips", "mmm", "xx", 
                    "panasonic", "sharp", "wap", "sch",
                    "rover", "pocket", "benq", "java", 
                    "pg", "vox", "amoi", 
                    "bird", "compal", "kg", "voda",
                    "sany", "kdd", "dbt", "sendo", 
                    "sgh", "gradi", "jb", "dddi", 
                    "moto", "iphone"
                };

    public static bool isMobileBrowser()
    {
        //GETS THE CURRENT USER CONTEXT
        HttpContext context = HttpContext.Current;

        //FIRST TRY BUILT IN ASP.NT CHECK
        if (context.Request.Browser.IsMobileDevice)
        {
            return true;
        }
        //THEN TRY CHECKING FOR THE HTTP_X_WAP_PROFILE HEADER
        if (context.Request.ServerVariables["HTTP_X_WAP_PROFILE"] != null)
        {
            return true;
        }
        //THEN TRY CHECKING THAT HTTP_ACCEPT EXISTS AND CONTAINS WAP
        if (context.Request.ServerVariables["HTTP_ACCEPT"] != null &&
            context.Request.ServerVariables["HTTP_ACCEPT"].ToLower().Contains("wap"))
        {
            return true;
        }
        //AND FINALLY CHECK THE HTTP_USER_AGENT 
        //HEADER VARIABLE FOR ANY ONE OF THE FOLLOWING
        if (context.Request.ServerVariables["HTTP_USER_AGENT"] != null)
        {
            for (int i = 0; i < mobiles.Length; i++)
            {
                if (context.Request.ServerVariables["HTTP_USER_AGENT"].
                                                    ToLower().Contains(mobiles[i].ToLower()))
                {
                    return true;
                }
            }
        }

        return false;
    }

}
