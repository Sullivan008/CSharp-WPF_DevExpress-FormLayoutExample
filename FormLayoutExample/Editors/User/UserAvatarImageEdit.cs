using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.Editors;
using FormLayoutExample.ViewModels.User;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Media;

namespace FormLayoutExample.Editors.User
{
    public class UserAvatarImageEdit : ImageEdit
    {
        protected override void LoadCore()
        {
            if (Image == null)
            {
                return;
            }

            ImageSource image = LoadImage();

            if (image != null)
            {
                EditStrategy.SetImage(image);
            }
        }

        #region PRIVATE Helper Methods

        private ImageSource LoadImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = EditorLocalizer.GetString(EditorStringId.ImageEdit_OpenFileFilter),
            };

            if (openFileDialog.ShowDialog() != true)
                return null;

            using (Stream stream = openFileDialog.OpenFile())
            {
                if (!(stream is FileStream fs))
                    return null;

                ((UserViewModel)DataContext).UserAvatarImageExtension = Path.GetExtension(fs.Name);
                ((UserViewModel)DataContext).UserAvatarImageData = FileStreamToByteArray(fs);

                return ImageHelper.CreateImageFromStream(new MemoryStream(stream.GetDataFromStream()));
            }
        }

        private static byte[] FileStreamToByteArray(Stream fs)
        {
            int length = Convert.ToInt32(fs.Length);
            byte[] data = new byte[length];

            fs.Read(data, 0, length);

            return data;
        }

        #endregion
    }
}
