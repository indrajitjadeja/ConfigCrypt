using System;
using System.Linq;
using System.Windows;
using System.Configuration;
using System.Diagnostics;
using System.ComponentModel;
namespace ConfigCrypt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool flgEncrypt = true;
        Configuration config = null;
        ConnectionStringsSection conSection = null;

        public MainWindow()
        {
            InitializeComponent();
            textFile.IsReadOnly = true;
            btnBrowse.IsEnabled = true;
            //Code to give right to current user for RSAConfigurationProvider
            const int ERROR_CANCELLED = 1223;
            string domain = Environment.UserDomainName;
            string currUser = Environment.UserName;
            //MessageBox.Show("Domain: " + domain + Environment.NewLine + "Current User: " + currUser,domain +@"\"+ currUser);
            try
            {
                new Process()
                    {
                        StartInfo = new ProcessStartInfo(@"C:\Windows\Microsoft.NET\Framework\v2.0.50727\aspnet_regiis.exe", @"-pa NetFrameworkConfigurationKey " + domain + @"\" + currUser)
                        {
                            Verb = "runas",
                            WindowStyle = ProcessWindowStyle.Hidden,
                            UseShellExecute = true,
                            RedirectStandardOutput = false,
                            RedirectStandardError = false
                        }
                    }.Start();
            }
            catch (Win32Exception ex)
            {
                if (ex.NativeErrorCode == ERROR_CANCELLED)
                {
                    MessageBoxResult choice = MessageBox.Show("Application must be run as Administrator. \n Do you wish to relaunch?", "ConfigCrypt", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (choice == MessageBoxResult.Yes)
                        new MainWindow();
                    else
                        Application.Current.Shutdown();
                }
            }
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.DefaultExt = ".config";
                dlg.Filter = "Config Files (.config)|*.config";
                dlg.Multiselect = false;
                dlg.AddExtension = false;
                Nullable<bool> result = dlg.ShowDialog();
                if (result == true)
                {
                    flgEncrypt = checkCrypt(dlg.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Got an Error in Encrypting/Decrypting config File," + Environment.NewLine + "Please contact to Dev team." + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                textFile.Text = string.Empty;
                btnEncrypt.IsEnabled = false;
            }
        }

        private bool checkCrypt(string filename)
        {
            textFile.Text = filename;
            try
            {
                ExeConfigurationFileMap conFile = new ExeConfigurationFileMap();
                conFile.ExeConfigFilename = filename;
                config = ConfigurationManager.OpenMappedExeConfiguration(conFile, ConfigurationUserLevel.None);
                conSection = config.GetSection("connectionStrings") as ConnectionStringsSection;
                if (conSection.SectionInformation.IsProtected)
                {
                    btnEncrypt.Content = "Decrypt";
                    btnEncrypt.IsEnabled = true;
                    return false;
                }
                else
                {
                    btnEncrypt.Content = "Encrypt";
                    btnEncrypt.IsEnabled = true;
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Config File may have get Encrypted or Decrypted on other machine." + Environment.NewLine + "Please use this utility on that machine." + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                textFile.Text = string.Empty;
                btnEncrypt.IsEnabled = false;
                return false;
            }
        }
        public static string psw = string.Empty;
        private void btnEncrypt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Login dlgLogin = new Login(flgEncrypt);
                dlgLogin.Owner = this;
                dlgLogin.ShowDialog();

                //psw = Microsoft.VisualBasic.Interaction.InputBox("Enter Password:", "Password Required");
                //if (psw.ToLower().Equals("indrajit") && !string.IsNullOrEmpty(psw))
                if (dlgLogin.DialogResult ?? false)
                {
                    if (flgEncrypt)
                    {
                        conSection.SectionInformation.ProtectSection("RsaProtectedConfigurationProvider");
                        //DpapiProtectedConfigurationProvider RsaProtectedConfigurationProvider DataProviderConfigurationSection
                    }
                    else
                    {
                        conSection.SectionInformation.UnprotectSection();
                    }
                    conSection.SectionInformation.ForceSave = true;
                    config.Save();
                    ConfigurationManager.RefreshSection("connectionStrings");
                    string successMsg = (flgEncrypt) ? "Encrypted sccessfully!!" : "Decrypted sccessfully!!";
                    MessageBox.Show(successMsg, "Success..!!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    Process.Start("notepad.exe", config.FilePath);
                    textFile.Text = string.Empty;
                    btnEncrypt.IsEnabled = false;
                }
                else
                {
                    MessageBox.Show("Password Incorrect.!", "Password", MessageBoxButton.OK, MessageBoxImage.Error);
                    textFile.Text = string.Empty;
                    btnEncrypt.IsEnabled = false;
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show("Got an Error in Encrypting/Decrypting config File," + Environment.NewLine + "Please contact to Dev team." + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                textFile.Text = string.Empty;
                btnEncrypt.IsEnabled = false;
            }
        }


        private void textFile_PreviewDragOver(object sender, DragEventArgs e)
        {
            try
            {
                e.Handled = true;
                var filenames = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (filenames == null) return;
                var fileName = filenames.FirstOrDefault();
                if (fileName == null) return;
                if (System.IO.Path.IsPathRooted(fileName) && System.IO.Path.GetExtension(fileName).ToLower().Equals(".config"))
                {
                    textFile.Text = fileName;
                    flgEncrypt = checkCrypt(fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Got an Error in Encrypting/Decrypting config File," + Environment.NewLine + "Please contact to Dev team." + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                textFile.Text = string.Empty;
                btnEncrypt.IsEnabled = false;
            }
        }

        private void textFile_PreviewDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        private void textFile_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == System.Windows.Input.Key.V)
                {
                    try
                    {
                        if (System.IO.Path.IsPathRooted(Clipboard.GetText()) && System.IO.Path.GetExtension(Clipboard.GetText()).ToLower().Equals(".config"))
                            flgEncrypt = checkCrypt(Clipboard.GetText());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Path of only cofnig file is allowed to paste." + Environment.NewLine + "Error:" + ex.Message, "Success..!!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Got an Error in Encrypting/Decrypting config File," + Environment.NewLine + "Please contact to Dev team." + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                textFile.Text = string.Empty;
                btnEncrypt.IsEnabled = false;
            }
        }
    }
}
