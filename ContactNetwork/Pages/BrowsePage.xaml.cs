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
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Popups;
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
    public sealed partial class BrowsePage : Page
    {
        private Contact contact;
        private List<Contact> notFriends;
        private const string dbName = "contacts-beta5.db";

        public BrowsePage()
        {
            this.InitializeComponent();

        }

        private async void RenderPeople()
        {
            List<FriendsRequest> requests = await GetAllEntriesFriends();
            List<string> friends = GetAllContactsFromFriendsRequests(requests);
            notFriends = await GetAllContactsThatAreNotFriends(friends);


            List<GridViewItem> itemsList = new List<GridViewItem>();

            for (int i = 0; i < notFriends.Count; i++ )
            {
                GridViewItem item = new GridViewItem();
                item.FontFamily = new Windows.UI.Xaml.Media.FontFamily("Gabriola");
                item.FontSize = 40;
                item.Content = notFriends[i].FirstName + "\n" + notFriends[i].LastName; 
                item.Width = 200;
                item.Height = 200;

                string color = GetBgrColorForCurrentIndex(i);

                byte R = Convert.ToByte(color.Substring(1, 2), 16);
                byte G = Convert.ToByte(color.Substring(3, 2), 16);
                byte B = Convert.ToByte(color.Substring(5, 2), 16);
                item.Background = new SolidColorBrush(Color.FromArgb(255, (byte)R, (byte)G, (byte)B));
                if (color == "#f29a10" || color == "#5ea225")
                {
                    item.Foreground = new SolidColorBrush(Colors.Black);
                }

                item.BorderBrush = new SolidColorBrush(Colors.Black);
                item.BorderThickness = new Thickness(2);

                itemsList.Add(item);
            }
            peopleGrid.ItemsSource = itemsList;
        }

        private async Task<List<Contact>> GetAllContactsThatAreNotFriends(List<string> friends)
        {
            SQLiteAsyncConnection dbCon = new SQLiteAsyncConnection(dbName);
            var query = dbCon.Table<Contact>();
            List<Contact> requests = await query.ToListAsync();
            List<Contact> allNeeded = new List<Contact>();
            foreach (var req in requests)
            {
                if (!friends.Contains(req.FirstName + " " + req.LastName))
                {
                    allNeeded.Add(req);
                }
            }
            return allNeeded;
        }

        private string GetBgrColorForCurrentIndex(int index)
        {
            string color = "";
            if (index % 7 == 0)
            {
                color = "#1f561f";   //dark green
            }
            else if (index % 7 == 1)
            {
                color = "#468dea";  //hell blue
            }
            else if (index % 7 == 2)
            {
                color = "#f29a10";   //yellow
            }
            else if (index % 7 == 3)
            {
                color = "#312d2d";  //gray
            }
            else if (index % 7 == 4)
            {
                color = "#5ea225";  //green
            }
            else if (index % 7 == 5)
            {
                color = "#b22020";  //red
            }
            else if (index % 7 == 6)
            {
                color = "#d44b27";  //orange
            }
            return color;
        }

        private List<string> GetAllContactsFromFriendsRequests(List<FriendsRequest> requests)
        {
            List<string> notFriends = new List<string>();
            foreach (var request in requests)
            {
                if (!notFriends.Contains(request.FromName))
                {
                    notFriends.Add(request.FromName.Replace('\n', ' '));
                }
                if (!notFriends.Contains(request.ToName))
                {
                    notFriends.Add(request.ToName.Replace('\n', ' '));
                }
            }
            return notFriends;
        }

        private async Task<List<FriendsRequest>> GetAllEntriesFriends()
        {
            SQLiteAsyncConnection dbCon = new SQLiteAsyncConnection(dbName);
            String name = contact.FirstName + " " + contact.LastName;
            var query = dbCon.Table<FriendsRequest>().Where(x => x.FromName == name || x.ToName == name);
            List<FriendsRequest> requests = await query.ToListAsync();
            //query = dbCon.Table<FriendsRequest>().Where(x => x.ToName == name);
            //List<FriendsRequest> requests2 = await query.ToListAsync();
            //foreach (var req in requests2)
            //{
            //    requests.Add(req);
            //}
            return requests;
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MyProfilePage), contact);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            contact = e.Parameter as Contact;

            RenderPeople();
        }

        private async void peopleGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageDialog message = new MessageDialog("Do you really want to add this user to your contacts network?");
            message.Commands.Add(new UICommand("Yes", async (command) =>
            {
                string selectedName = e.ClickedItem.ToString();
                FriendsRequest request = new FriendsRequest(contact.FirstName + " " + contact.LastName, selectedName);
                AddNewFriendsRequestToTheDb(request);
                //Contact newOrder = await GetUserByName(selectedName);

                SendNotification("The user is added to your friends contacts", "/Images/add.png");
                this.Frame.Navigate(typeof(MyProfilePage), contact);
            }));
            message.Commands.Add(new UICommand("No", (command) =>
            {

            }));
            await message.ShowAsync();
        }

        private async Task<Contact> GetUserByName(string selectedName)
        {
            SQLiteAsyncConnection dbCon = new SQLiteAsyncConnection(dbName);
            var query = dbCon.Table<Contact>().Where(x => (x.FirstName + " " + x.LastName) == selectedName);
            List<Contact> contacts = await query.ToListAsync();
            Contact selContact = contacts.FirstOrDefault();
            return selContact;
        }

        private async void AddNewFriendsRequestToTheDb(FriendsRequest request)
        {
            SQLiteAsyncConnection dbCon = new SQLiteAsyncConnection(dbName);
            await dbCon.InsertAsync(request);
        }

        private void SendNotification(string mainMessage, string imageSrc)
        {
            var notificationXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);
            string toastXmlString = "<toast>"
                               + "<visual version='1'>"
                               + "<binding template='toastImageAndText01'>"
                               + "<image id='1' src='" + imageSrc + "' alt='image placeholder'/>"
                               + "<text id='1'>" + mainMessage + "</text>"
                               + "</binding>"
                               + "</visual>"
                               + "</toast>";
            notificationXml.LoadXml(toastXmlString);         //var toeastElement = notificationXml.GetElementsByTagName("text");
            //toeastElement[0].AppendChild(notificationXml.CreateTextNode("This is Notification Message"));
            var toastNotification = new ToastNotification(notificationXml);
            ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
        }
    }
}
