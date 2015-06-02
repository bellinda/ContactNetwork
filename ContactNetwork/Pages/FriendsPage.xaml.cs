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
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ContactNetwork.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FriendsPage : Page
    {
        private const string dbName = "contacts-beta5.db";
        private Contact contact;
        private Contact originalContact;
        private List<string> friends = new List<string>();
        private List<FriendsRequest> requests;

        public FriendsPage()
        {
            this.InitializeComponent();
        }

        private async Task<string> GetCOntactByUsernameAndPass(string username, string pass)
        {
            SQLiteAsyncConnection dbCon = new SQLiteAsyncConnection(dbName);
            var query = dbCon.Table<Contact>().Where(x => x.Username == username && x.Password == pass);
            List<Contact> contacts = await query.ToListAsync();
            Contact contact = contacts.FirstOrDefault();
            return contact.FirstName + " " + contact.LastName;
        }

        private async Task<Contact> GetCOntactByUsername(string username)
        {
            SQLiteAsyncConnection dbCon = new SQLiteAsyncConnection(dbName);
            var query = dbCon.Table<Contact>().Where(x => x.Username == username);
            List<Contact> contacts = await query.ToListAsync();
            Contact contact = contacts.FirstOrDefault();
            return contact;
        }

        private async Task<Contact> GetContactByName(string name)
        {
            string firstName = name.Split(' ')[0];
            string lastName = name.Split(' ')[1];
            SQLiteAsyncConnection dbCon = new SQLiteAsyncConnection(dbName);
            var query = dbCon.Table<Contact>().Where(x => x.FirstName == firstName && x.LastName == lastName);
            List<Contact> contacts = await query.ToListAsync();
            Contact contact = contacts.FirstOrDefault();
            return contact;
        }

        private async Task<byte[]> GetContactPicture(string username)
        {
            SQLiteAsyncConnection dbCon = new SQLiteAsyncConnection(dbName);
            var query = dbCon.Table<Contact>().Where(x => x.Username == username);
            List<Contact> contacts = await query.ToListAsync();
            Contact contact = contacts.FirstOrDefault();
            return contact.Image;
        }

        private async Task<List<string>> GetAllFriendsOfContact()
        {
            SQLiteAsyncConnection dbCon = new SQLiteAsyncConnection(dbName);
            string name = contact.FirstName + " " + contact.LastName;
            var query = dbCon.Table<FriendsRequest>().Where(x => x.FromName == name || x.ToName == name);
            requests = await query.ToListAsync();
            List<string> friends = new List<string>();
            foreach (FriendsRequest fr in requests)
            {
                if (fr.FromName != name)
                {
                    //Contact friend = await GetCOntactByUsername(fr.ToName);
                    friends.Add(fr.FromName);
                }
                if(fr.ToName != name)
                {
                    //Contact friend = await GetCOntactByUsername(fr.FromName);
                    friends.Add(fr.ToName);
                }
            }

            return friends;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            List<Contact> contacts = e.Parameter as List<Contact>;
            contact = contacts[1];
            originalContact = contacts[0];
            //contact = e.Parameter as Contact;

            nameField.Text = contact.FirstName + " " + contact.LastName;

            if (contact.Image != null)
            {
                byte[] imageBytes = contact.Image;
                BitmapImage image = await ByteArrayToBitmapImage(imageBytes);
                profileImg.Source = image;
            }
            else
            {
                profileImg.Source = new BitmapImage(new Uri("http://www.keralabjp.org/images/No_Image.jpg"));
            }
            

            username.Text = "Username: " + contact.Username;
            email.Text = "Email: " + contact.Email;
            phone.Text = "Phone: " + contact.Phone;
            skype.Text = "Skype: " + contact.Skype;

            RenderFriends();
        }

        private async void RenderFriends()
        {
            friends = await GetAllFriendsOfContact();
            List<ListViewItem> friendsInList = new List<ListViewItem>();
            foreach (string con in friends)
            {
                ListViewItem item = new ListViewItem();
                item.Content = con.Replace('\n', ' ');
                item.FontSize = 40;
                item.FontFamily = new FontFamily("Gabriola");
                //item.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
                friendsInList.Add(item);
            }
            friendsList.ItemsSource = friendsInList;
        }

        private async Task<BitmapImage> ByteArrayToBitmapImage(byte[] byteArray)
        {
            var bitmapImage = new BitmapImage();

            var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(byteArray.AsBuffer());
            stream.Seek(0);

            bitmapImage.SetSource(stream);
            return bitmapImage;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(BrowsePage), contact);
        }

        private async void friendsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            Contact c = await GetContactByName(e.ClickedItem.ToString());
            List<Contact> cons = new List<Contact>();
            cons.Add(contact);
            cons.Add(c);
            this.Frame.Navigate(typeof(FriendsPage), cons);
        }

        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MyProfilePage), originalContact);
        }
    }
}
