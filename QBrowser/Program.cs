using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;

public class SimpleWebBrowser : Form
{
    private TextBox urlTextBox;
    private TabControl tabControl;
    
    public SimpleWebBrowser()
{
    Cef.Initialize(new CefSettings());
    var settings = new CefSettings
    {
        // Set the UserAgent to a Chrome UserAgent string
        UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.0.3 Safari/605.1.15"
    };

    tabControl = new TabControl { Dock = DockStyle.Fill };
    tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
    tabControl.DrawItem += TabControl_DrawItem;
    tabControl.MouseDown += TabControl_MouseDown;
    tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
    tabControl.Appearance = TabAppearance.Normal;
    tabControl.Selecting += TabControl_Selecting;

    urlTextBox = new TextBox { Dock = DockStyle.Top };
    urlTextBox.KeyPress += UrlTextBox_KeyPress;

    Controls.Add(tabControl);
    Controls.Add(urlTextBox); 
    
    AddTab("https://www.google.com");
    AddNewTabButton();
}
   private void UrlTextBox_KeyPress(object sender, KeyPressEventArgs e)
{
    if (e.KeyChar == (char)Keys.Enter)
    {
        string url = urlTextBox.Text;

        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
        {
            if (url.Contains("."))
            {
                url = "https://" + url;
            }
            else
            {
                url = "https://www.google.com/search?q=" + url;
            }
        }

        if (tabControl.SelectedTab.Controls.Count > 0 && tabControl.SelectedTab.Controls[0] is ChromiumWebBrowser browser)
        {
            browser.Load(url);
        }
    }
}

private void TabControl_Selecting(object sender, TabControlCancelEventArgs e)
{
    if (e.TabPage.Text == "+")
    {
        e.Cancel = true;
        AddTab("https://www.google.com");
    }
}

    private void TabControl_DrawItem(Object sender, DrawItemEventArgs e)
{
    var tabControl = (TabControl)sender;
    if (e.Index < tabControl.TabPages.Count)
    {
        var tabPage = tabControl.TabPages[e.Index];
        e.Graphics.DrawString(tabPage.Text, e.Font, Brushes.Black, e.Bounds);
    }
}

    private void TabControl_MouseDown(object sender, MouseEventArgs e)
    {
        for (var i = 0; i < tabControl.TabPages.Count; i++)
        {
            var tabRect = tabControl.GetTabRect(i);
            tabRect.Inflate(-2, -2);
            var closeImage = new Rectangle(tabRect.Right - 15, tabRect.Top, 15, tabRect.Height);

            if (closeImage.Contains(e.Location))
            {
                tabControl.TabPages.RemoveAt(i);
                break;
            }
        }
    }

    private void TabControl_SelectedIndexChanged(Object sender, EventArgs e)
{
    var tabControl = (TabControl)sender;
    if (tabControl.SelectedTab.Controls.Count > 0 && tabControl.SelectedTab.Controls[0] is ChromiumWebBrowser browser)
    {
        urlTextBox.Text = browser.Address;
    }
}

private void AddTab(string url)
{
    var browser = new ChromiumWebBrowser(url);
    browser.TitleChanged += Browser_TitleChanged;

    var tabPage = new TabPage();
    tabPage.Controls.Add(browser);
    browser.Dock = DockStyle.Fill;

    if (tabControl.TabCount == 0)
    {
        tabControl.TabPages.Add(tabPage);
    }
    else
    {
        tabControl.TabPages.Insert(tabControl.TabCount - 1, tabPage);
    }

    tabControl.SelectedTab = tabPage;
}

    private void AddNewTabButton()
{
    var newTabButtonPage = new TabPage("+");
    tabControl.TabPages.Add(newTabButtonPage);
}

    private void Browser_TitleChanged(object sender, TitleChangedEventArgs e)
    {
        this.Invoke((Action)delegate
        {
            var browser = (ChromiumWebBrowser)sender;
            var tabPage = (TabPage)browser.Parent;
            tabPage.Text = e.Title;
        });
    }

    [STAThread]
    static void Main()
    {
        try
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SimpleWebBrowser());
        }
        catch (Exception ex)
        {
            File.WriteAllText("error.log", ex.ToString());
        }
    }
}