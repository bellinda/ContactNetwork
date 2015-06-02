using ContactNetwork.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ContactNetwork.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SignInPage : Page
    {
        private const string dbName = "contacts-beta5.db";
        private Contact contact;

        public SignInPage()
        {
            this.InitializeComponent();
            //GetAllEntries();
        }

        private void registerBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Pages.RegisterPage));
        }

        private async void signInBtn_Click(object sender, RoutedEventArgs e)
        {
            bool areUsernameAndPassRight = await AreUsernameAndPassRight(usernameInput.Text, passInput.Password);
            if (areUsernameAndPassRight)
            {
                this.Frame.Navigate(typeof(Pages.MyProfilePage), contact);
            }
            else
            {
                wrongInput.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        private async Task<bool> AreUsernameAndPassRight(string username, string pass)
        {
            SQLiteAsyncConnection dbCon = new SQLiteAsyncConnection(dbName);
            var query = dbCon.Table<Contact>().Where(x => x.Username == username && x.Password == pass);
            List<Contact> contacts = await query.ToListAsync();
            contact = contacts.FirstOrDefault();
            if (contact == null)
            {
                return false;
            }
            return true;
        }

        private async void GetAllEntries()
        {
            SQLiteAsyncConnection dbCon = new SQLiteAsyncConnection(dbName);
            var query = dbCon.Table<Contact>();
            List<Contact> contacts = await query.ToListAsync();
        }
    }
}
