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
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Popups;
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
    public sealed partial class RegisterPage : Page
    {
        private const string dbName = "contacts-beta5.db";
        private byte[] imageBytes;

        public RegisterPage()
        {
            this.InitializeComponent();
        }

        private async void selectPhotoBtn_Click(object sender, RoutedEventArgs e)
        {
            // Clear previous returned file name, if it exists, between iterations of this scenario
            selectedPhotoBox.Text = "";

            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                // Application now has read/write access to the picked file
                selectedPhotoBox.Text = file.Name;

                var stream = await file.OpenReadAsync();

                using (var dataReader = new DataReader(stream))
                {
                    imageBytes = new byte[stream.Size];
                    await dataReader.LoadAsync((uint)stream.Size);
                    dataReader.ReadBytes(imageBytes);
                    // Convert byte[] to Base64 String
                    //string base64String = Convert.ToBase64String(bytes);
                }
            }
            else
            {
                selectedPhotoBox.Text = "Operation cancelled.";
            }
        }

        private async void submitRegistrationBtn_Click(object sender, RoutedEventArgs e)
        {
            bool isThereAlreadyContactWithTheSameName = await IsThereAContactWithTheSameName(usernameInput.Text);
            if (isThereAlreadyContactWithTheSameName == true)
            {
                MessageDialog message = new MessageDialog("Already there is an user with the same username. Try with another one");
                message.Commands.Add(new UICommand("Ok", async (command) =>
                { }));
                await message.ShowAsync();
            }
            else
            {
                if (firstNameInput.Text == "")
                {
                    showErrorMessage("\"First Name\"");
                }
                else if (lastNameInput.Text == "")
                {
                    showErrorMessage("\"Last Name\"");
                }
                else if (phoneInput.Text == "")
                {
                    showErrorMessage("\"Phone\"");
                }
                else if (usernameInput.Text == "")
                {
                    showErrorMessage("\"Usrname\"");
                }
                else if (passwordInput.Password == "")
                {
                    showErrorMessage("\"Password\"");
                }
                else
                {
                    MessageDialog message = new MessageDialog("Are you sure you want to submit your registration?");
                    message.Commands.Add(new UICommand("Yes", async (command) =>
                    {
                        Contact newOrder = GetDataFromUIFields();

                        await AddNewContactAsync(newOrder);

                        SendNotification("Successful registration", "/Images/save1.png");
                        this.Frame.Navigate(typeof(SignInPage));
                    }));
                    message.Commands.Add(new UICommand("No", (command) =>
                    {

                    }));
                    await message.ShowAsync();
                }
            }
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

        private async System.Threading.Tasks.Task AddNewContactAsync(Contact newContact)
        {
            bool dbExists = await CheckDbAsync(dbName);
            if (!dbExists)
            {
                await CreateDbAsync();
            }
            SQLiteAsyncConnection dbCon = new SQLiteAsyncConnection(dbName);
            await dbCon.InsertAsync(newContact);
        }

        private Contact GetDataFromUIFields()
        {
            Contact contact = new Contact();
            contact.FirstName = firstNameInput.Text;
            contact.LastName = lastNameInput.Text;
            contact.Phone = phoneInput.Text;
            if (skypeInput.Text != "")
            {
                contact.Skype = skypeInput.Text;
            }
            contact.Username = usernameInput.Text;
            contact.Password = passwordInput.Password;
            if (emailInput.Text != "")
            {
                contact.Email = emailInput.Text;
            }
            //contact.Friends = new string[100];
            if (imageBytes != null)
            {
                contact.Image = imageBytes;
            }
            return contact;
        }

        private async void cancelRegistrationBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog message = new MessageDialog("Are you sure you want to cancel your registration?");
            message.Commands.Add(new UICommand("Yes", (command) =>
            {
                this.Frame.Navigate(typeof(Pages.SignInPage));
            }));
            message.Commands.Add(new UICommand("No", (command) =>
            {

            }));
            await message.ShowAsync();
        }

        private async void showErrorMessage(string errorField)
        {
            MessageDialog message = new MessageDialog("The field " + errorField + " should be filled in before submitting!");
            message.Commands.Add(new UICommand("Ok", (command) =>
            {

            }));
            await message.ShowAsync();
        }

        public BitmapImage ImageFromBuffer(Byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes);
            BitmapImage image = new BitmapImage();
            image.SetSource(stream.AsRandomAccessStream());
            return image;
        }

        private async Task<bool> CheckDbAsync(string dbName)
        {
            bool dbExists = true;

            try
            {
                StorageFile sf = await ApplicationData.Current.LocalFolder.GetFileAsync(dbName);
            }
            catch (Exception)
            {
                dbExists = false;
            }
            return dbExists;
        }

        private async Task CreateDbAsync()
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(dbName);
            await conn.CreateTableAsync<Friend>();
            await conn.CreateTablesAsync<Contact, Message, FriendsRequest>();
        }

        private async Task<bool> IsThereAContactWithTheSameName(string username)
        {
            SQLiteAsyncConnection dbCon = new SQLiteAsyncConnection(dbName);
            var query = dbCon.Table<Contact>().Where(x => x.Username == username);
            Contact contact = await query.FirstOrDefaultAsync();
            return contact != null;
        }
    }
}
