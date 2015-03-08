using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Windows.UI.Notifications;
using AzureServices.Authentication;
using AzureServices.VirtualMachines;
using Data.Models;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using MS.WindowsAPICodePack.Internal;

namespace DesktopVmClient
{
  public partial class MainWindow : Window
  {
    private const String AppId = "Azure Management Desktop Client";
    private readonly ViewModel _dataContext;
    private readonly NotifyIcon _notifyIcon;

    public MainWindow()
    {
      InitializeComponent();
      // TODO: Get these values from persistence layer
      _dataContext = new ViewModel("", "", "", "", Auth.GetAuthorizationHeader());
      DataContext = _dataContext;
      TryCreateShortcut();
      var components = new Container();
      _notifyIcon = new NotifyIcon(components) {Text = "", Visible = true};
      GetVm();
      _notifyIcon.Visible = true;
      _notifyIcon.Icon = DrawOffIcon();
      BuildPopup();
    }

    public Icon DrawOffIcon()
    {
      var bit = new Bitmap(16, 16);
      var g = Graphics.FromImage(bit);

      Brush br = new SolidBrush(Color.Red);

      g.DrawString("Off", new Font("Arial", 5), br, 0, 0);
      var hicon = bit.GetHicon();
      var ico = System.Drawing.Icon.FromHandle(hicon);

      return ico;
    }

    public Icon DrawOnIcon()
    {
      var bit = new Bitmap(16, 16);
      var g = Graphics.FromImage(bit);

      Brush br = new SolidBrush(Color.Chartreuse);

      g.DrawString("On", new Font("Arial", 5), br, 0, 0);
      var hicon = bit.GetHicon();
      var ico = System.Drawing.Icon.FromHandle(hicon);

      return ico;
    }

    // In order to display toasts, a desktop application must have a shortcut on the Start menu.
    // Also, an AppUserModelID must be set on that shortcut.
    // The shortcut should be created as part of the installer. The following code shows how to create
    // a shortcut and assign an AppUserModelID using Windows APIs. You must download and include the 
    // Windows API Code Pack for Microsoft .NET Framework for this code to function
    //
    // Included in this project is a wxs file that be used with the WiX toolkit
    // to make an installer that creates the necessary shortcut. One or the other should be used.
    private bool TryCreateShortcut()
    {
      var shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                         "\\Microsoft\\Windows\\Start Menu\\Programs\\Azure Management Desktop Client.lnk";
      if (!File.Exists(shortcutPath))
      {
        InstallShortcut(shortcutPath);
        return true;
      }
      return false;
    }

    private void InstallShortcut(String shortcutPath)
    {
      // Find the path to the current executable
      var exePath = Process.GetCurrentProcess().MainModule.FileName;
      var newShortcut = (IShellLinkW) new CShellLink();

      // Create a shortcut to the exe
      ErrorHelper.VerifySucceeded(newShortcut.SetPath(exePath));
      ErrorHelper.VerifySucceeded(newShortcut.SetArguments(""));

      // Open the shortcut property store, set the AppUserModelId property
      var newShortcutProperties = (IPropertyStore) newShortcut;

      using (var appId = new PropVariant(AppId))
      {
        ErrorHelper.VerifySucceeded(newShortcutProperties.SetValue(SystemProperties.System.AppUserModel.ID, appId));
        ErrorHelper.VerifySucceeded(newShortcutProperties.Commit());
      }

      // Commit the shortcut to disk
      var newShortcutSave = (IPersistFile) newShortcut;

      ErrorHelper.VerifySucceeded(newShortcutSave.Save(shortcutPath, true));
    }

    // Create and show the toast.
    // See the "Toasts" sample for more detail on what can be done with toasts
    private void CreateAndShowToast(string message)
    {
      // Get a toast XML template
      var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText01);

      // Fill in the text elements
      var stringElements = toastXml.GetElementsByTagName("text");
      stringElements[0].AppendChild(toastXml.CreateTextNode(message));

      // Specify the absolute path to an image
      var imagePath = "file:///" + Path.GetFullPath("toastImageAndText.png");
      var imageElements = toastXml.GetElementsByTagName("image");
      imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

      // Create the toast and attach event listeners
      var toast = new ToastNotification(toastXml);

      // Show the toast. Be sure to specify the AppUserModelId on your application's shortcut!
      ToastNotificationManager.CreateToastNotifier(AppId).Show(toast);
    }

    /// <summary>
    ///   Function builds the right click menu for the icon in the systray
    /// </summary>
    public void BuildPopup()
    {
      //build the popup menu
      var mnuItms = new MenuItem[3];

      mnuItms[0] = new MenuItem {Text = "Exit"};
      mnuItms[0].Click += ExitSelect;
      mnuItms[0].DefaultItem = true;
      mnuItms[1] = new MenuItem {Text = "Start Azure VM"};
      mnuItms[1].Click += StartVm;
      mnuItms[1].DefaultItem = false;
      mnuItms[2] = new MenuItem {Text = "Stop Azure VM"};
      mnuItms[2].Click += StopVm;
      mnuItms[2].DefaultItem = false;

      var notifyiconMnu = new ContextMenu(mnuItms);
      _notifyIcon.ContextMenu = notifyiconMnu;
    }

    /// <summary>
    ///   Is called when "Exit" is selected on the right click menu
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void ExitSelect(object sender, EventArgs e)
    {
      //hide the tray icon
      _notifyIcon.Visible = false;
      //close up
      Close();
    }

    /// <summary>
    ///   Is called when "Exit" is selected on the right click menu
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public async void StartVm(object sender, EventArgs e)
    {
      if (_dataContext.HasValues)
      {
        CreateAndShowToast("Starting VM...");
        var startResult = await Management.StartRole(_dataContext);
        _notifyIcon.Icon = DrawOnIcon();
      }
    }

    /// <summary>
    ///   Is called when "Exit" is selected on the right click menu
    /// </summary>
    public async void GetVm()
    {
      if (_dataContext.HasValues)
      {
        CreateAndShowToast("Getting VM info...");
        var getResult = await Management.GetRole(_dataContext);
        //_notifyIcon.Icon = DrawOnIcon();
        if (getResult)
        {
          _notifyIcon.Icon = DrawOnIcon();
        }
        else
        {
          _notifyIcon.Icon = DrawOffIcon();
        }
      }
    }

    /// <summary>
    ///   Is called when "Exit" is selected on the right click menu
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public async void StopVm(object sender, EventArgs e)
    {
      if (_dataContext.HasValues)
      {
        CreateAndShowToast("Stopping VM...");
        var stopResult = await Management.ShutdownRole(_dataContext);
        _notifyIcon.Icon = DrawOffIcon();
      }
    }

    /// <summary>
    ///   Is called when "Exit" is selected on the right click menu
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void Configure(object sender, EventArgs e)
    {
      _dataContext.IsConfigMode = !_dataContext.IsConfigMode;
      ConfigStack.Visibility = _dataContext.IsConfigMode ? Visibility.Visible : Visibility.Collapsed;
    }

    private void MainWindow_OnClosing(object sender, CancelEventArgs e)
    {
      //hide the tray icon
      _notifyIcon.Visible = false;
    }
  }
}