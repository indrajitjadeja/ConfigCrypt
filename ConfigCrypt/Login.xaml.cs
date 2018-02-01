using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ConfigCrypt
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private bool _isEncrypt;
        public Login(bool isEncrypt)
        {
            InitializeComponent();
            txtPassword.Focus();
            _isEncrypt = isEncrypt;
        }
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if ((_isEncrypt && txtPassword.Password.Equals("ecw@hMiS@!@#")) || (!_isEncrypt && txtPassword.Password.Equals("ecw@hMiS@!@#Dev")))
            {                
                this.DialogResult = true;
            }
            else
            {
                txtPassword.Password = string.Empty;
                btnLogin.IsEnabled = false;
                this.DialogResult = false;
            }
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            btnLogin.IsEnabled = (txtPassword.Password.Length > 0) ? true : false;
        }
    }
}
