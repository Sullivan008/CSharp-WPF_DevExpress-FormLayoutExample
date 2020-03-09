using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using FormLayoutExample.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Image = System.Drawing.Image;

namespace FormLayoutExample.ViewModels.User
{
    [POCOViewModel]
    public class UserViewModel : ViewModelBase, IDataErrorInfo
    {
        private const string DefaultCountry = "HU";

        #region PROPERTIES Getters/ Setters

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Nationality { get; set; }

        public IEnumerable<string> Countries
        {
            get
            {
                IEnumerable<string> countries = CountriesInitialize();

                Nationality = countries.FirstOrDefault(x => x.Equals(DefaultCountry));

                return countries;
            }
        }

        public DateTime BirthDate { get; set; } = DateTime.Now;

        public string PersonalPhoneNumber { get; set; }

        public string WorkPhoneNumber { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string PasswordConfirm { get; set; }

        public ImageSource UserAvatarImage { get; set; }

        public string UserAvatarImageExtension { get; set; }

        public byte[] UserAvatarImageData { get; set; }

        #endregion

        #region COMMANDS 

        private DelegateCommand _saveCommand;
        public DelegateCommand GetUserDataCommand =>
            _saveCommand ?? (_saveCommand = new DelegateCommand(GetUserData, CanGetUserData));

        #endregion

        #region ERROR HANDLERS

        public string Error =>
            this["UserName"] != null || this["Password"] != null ||
            this["PasswordConfirm"] != null ? "Correct values" : null;

        public string this[string columnName]
        {
            get
            {
                const int passwordMinLength = 5;

                switch (columnName)
                {
                    case "Username" when string.IsNullOrEmpty(Username):
                        return "Please, enter your Username";
                    case "Password" when string.IsNullOrEmpty(Password):
                        return "Please, enter your Password";
                    case "PasswordConfirm" when string.IsNullOrEmpty(PasswordConfirm):
                        return "Please, enter your Password again";
                    case "Password":
                        if (Password.Length < passwordMinLength)
                        {
                            return $"Password must be at least {passwordMinLength} characters long";
                        }

                        if (!Equals(Password, PasswordConfirm) && PasswordConfirm != null)
                        {
                            return "The two passwords do not match";
                        }

                        return null;
                    case "PasswordConfirm":
                        if (PasswordConfirm.Length < passwordMinLength)
                        {
                            return $"Password must be at least {passwordMinLength} characters long";
                        }

                        return !Equals(Password, PasswordConfirm) ? "The two passwords do not match" : null;
                    default:
                        return string.Empty;
                }
            }
        }

        #endregion

        #region COMMAND Helper Methods

        public void GetUserData()
        {
            try
            {
                SaveUserInputData();
            }
            catch (IOException ex)
            {
                new NotificationDialog("Error", $"The INPUT data save failed! Please try again!\n\nException Message: {ex.Message}")
                    .ShowMessageBoxByMessageType(MessageBoxImage.Error);
            }
        }

        public bool CanGetUserData()
        {
            IDataErrorInfo errorInfo = this;

            string errorMessage = errorInfo[GetPropertyName(() => Username)] +
                                  errorInfo[GetPropertyName(() => Password)] +
                                  errorInfo[GetPropertyName(() => PasswordConfirm)];


            return string.IsNullOrEmpty(errorMessage);
        }

        #endregion

        #region PRIVATE Helper Methods

        private static List<string> CountriesInitialize()
        {
            List<string> result = new List<string>();

            string[] countries = File
                .ReadAllText($"{Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\"))}/App_Data/countries.txt", Encoding.UTF8)
                .Split(new[] { "\r\n" }, StringSplitOptions.None);

            result.AddRange(countries);

            return result;
        }

        private void SaveUserInputData()
        {
            Guid userGuid = Guid.NewGuid();

            string applicationFolder = AppDomain.CurrentDomain.BaseDirectory;
            string targetFolder = $@"{applicationFolder}\Users\{userGuid}";

            CheckTargetFolder(targetFolder);

            SaveUserTextData(targetFolder);

            SaveUserAvatar(targetFolder);
        }

        private void CheckTargetFolder(string targetFolder)
        {
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }
        }

        private void SaveUserTextData(string targetFolder)
        {
            using (StreamWriter sw = new StreamWriter($@"{targetFolder}\userData.txt"))
            {
                sw.WriteLine("Personal details\n");

                sw.WriteLine($"\tFirst name: { (string.IsNullOrEmpty(FirstName) ? "Unknown" : FirstName) }");
                sw.WriteLine($"\tLast name: { (string.IsNullOrEmpty(LastName) ? "Unknown" : LastName) }");
                sw.WriteLine($"\tNationality: { Nationality }");
                sw.WriteLine($"\tBirth date: { BirthDate:yyyy-MM-dd}");

                sw.WriteLine("\nContact details\n");

                sw.WriteLine($"\tPersonal phone number: { (string.IsNullOrEmpty(PersonalPhoneNumber) ? "Unknown" : PersonalPhoneNumber) }");
                sw.WriteLine($"\tWork phone number: { (string.IsNullOrEmpty(WorkPhoneNumber) ? "Unknown" : WorkPhoneNumber) }");

                sw.WriteLine("\nLogin details\n");

                sw.WriteLine($"\tUser name: { Username }");
                sw.WriteLine($"\tPassword: { Password }");
                sw.Write($"\tPassword confirmation: { PasswordConfirm }");

                new NotificationDialog("Notification",
                        $"The INPUT data has been saved to the path below {((FileStream)sw.BaseStream).Name}")
                    .ShowMessageBoxByMessageType(MessageBoxImage.Information);
            }
        }

        private void SaveUserAvatar(string targetFolder)
        {
            using (MemoryStream ms = new MemoryStream(UserAvatarImageData))
            {
                Guid fileName = Guid.NewGuid();

                Image image = Image.FromStream(ms);
                ImageFormat imageFormat = SetImageFormatByExtension(UserAvatarImageExtension);

                if (imageFormat == null)
                {
                    new NotificationDialog("Error", "Selected image not supported! Please choose another image!")
                        .ShowMessageBoxByMessageType(MessageBoxImage.Error);
                }
                else
                {
                    image.Save($@"{targetFolder}\{fileName}.{UserAvatarImageExtension}", imageFormat);

                    new NotificationDialog("Notification",
                            $@"The INPUT image has been saved to the path below {targetFolder}\{fileName}{UserAvatarImageExtension}")
                        .ShowMessageBoxByMessageType(MessageBoxImage.Information);
                }

            }
        }

        private ImageFormat SetImageFormatByExtension(string userAvatarExtension)
        {
            switch (userAvatarExtension.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    return ImageFormat.Jpeg;
                case ".png":
                    return ImageFormat.Png;
                case ".bmp":
                    return ImageFormat.Bmp;
                case ".gif":
                    return ImageFormat.Gif;
                case ".tiff":
                    return ImageFormat.Tiff;
                default:
                    return null;
            }
        }

        #endregion
    }
}
