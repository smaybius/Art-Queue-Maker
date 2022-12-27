using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Art_queue_maker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window
    {

        private static readonly Stack<string> undobuffer = new();
        private static string filename = "Untitled";
        private static bool saved = true;
        private static string ticker = "";
        public static string WindowTitle { get { return filename + ticker + " - Art Queue Maker"; } } //filename + ticker + " - Art Queue Maker"
        public MainWindow()
        {
            InitializeComponent();
            
            DataContext = this;
            saved = true;
        }

        private void NewMenu_Click(object sender, RoutedEventArgs e)
        {
            ClearList();
        }

        private void ClearList()
        {
            if (ConfirmSaved())
            {
                filename = "Untitled";
                Title = WindowTitle;
                while (LinkList.Items.Count > 0)
                {
                    LinkList.Items.Remove(0);
                }
                while (undobuffer.Count > 0)
                {
                    _ = undobuffer.Pop();
                }
            }
        }

        private void OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new()
            {
                // Set filter for file extension and default file extension 
                DefaultExt = ".txt",
                Filter = "Text files (*.txt)|*.txt"
            };


            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                if (ConfirmSaved())
                {
                    ClearList();
                    filename = dlg.FileName;
                    Title = WindowTitle;
                    string[] lines = File.ReadAllLines(filename);
                    foreach (var line in lines)
                        LinkList.Items.Add(line);
                }
            }
        }

        private void SaveMenu_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void SaveAsMenu_Click(object sender, RoutedEventArgs e)
        {
            SaveDialog();
        }

        private void SaveDialog()
        {
            SaveFileDialog dlg = new()
            {
                Filter = "Text file (*.txt)|*.txt"
            };
            if (dlg.ShowDialog() == true)
            {
                filename = dlg.FileName;
                SaveData();
            }
        }

        private void Save()
        {
            if (filename == "Untitled")
                SaveDialog();
            else
                SaveData();
        }

        private void SaveData()
        {
            //string savedata = JsonSerializer.Serialize(links);
            //Debug.WriteLine(savedata);
            // File.WriteAllText(filename, savedata);
            File.WriteAllText(filename, string.Empty);
            StreamWriter SaveFile = new(filename);
            foreach (var item in LinkList.Items)
            {
                SaveFile.WriteLine(item.ToString());
            }
            SaveFile.Close();
            saved = true;
            ticker = "";
            Title = WindowTitle;
        }

        private void InstructMenu_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("\"Pop to Clipboard\" will move the top item of the list to the entry box and copy it to the clipboard." +
                " When the archive.today mode is selected, " +
                "the pop button will open up a tab in your browser that saves the link to archive.today, instead of copying to the clipboard.\n" +
                "Buttons on the right:\n" +
                "+: Same thing as pressing enter on the entry box. Adds the contents of the entry box to the top of the list and clears the entry box\n" +
                "-: Removes the selected item from the list\n" +
                "^^: Moves the selected item to the top of the list\n" +
                "^: Moves the selected item up one space\n" +
                "v: Moves the selected item down one space\n" +
                "v v: Moves the selected item to the bottom of the list");
        }

        private bool ConfirmSaved()
        {
            Title = WindowTitle;
            if (saved) return true;
            if (ticker == "") return true;
            MessageBoxResult result = MessageBox.Show("You have unsaved changes. Do you want to save them before leaving them?", "Save changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);

            switch (result)
            {
                case MessageBoxResult.Cancel:
                    return false;
                case MessageBoxResult.Yes:
                    Save();
                    return true;
                case MessageBoxResult.No:
                    saved = true;
                    ticker = "";
                    Title = WindowTitle;
                    return true;
                default:
                    return false;
            }
        }

        private void AboutMenu_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("By AceOfSpadesProduc100\nLicensed under the GNU GPL v3 or later");
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            Undelete();
        }

        private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Fetch the real key.
            var key = e.Key == Key.System ? e.SystemKey : e.Key;

            if ((Keyboard.IsKeyDown(Key.LeftCtrl))
                && key == Key.Z)
            {
                Undelete();
            }

            if ((Keyboard.IsKeyDown(Key.LeftCtrl))
                && key == Key.S)
            {
                Save();
            }
        }

        private void Undelete()
        {
            if (undobuffer.Count == 0) return;
            LinkList.Items.Add(undobuffer.Pop());
            Unsave();

        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            undobuffer.Push((string)LinkList.SelectedValue);
            LinkList.Items.RemoveAt(LinkList.SelectedIndex);
            Unsave();
            if (LinkList.Items.Count == 0)
                DisableButtons();
        }

        private void Entry_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                AddItem();
                
            }

            var key = e.Key == Key.System ? e.SystemKey : e.Key;

            if ((Keyboard.IsKeyDown(Key.LeftCtrl))
                && key == Key.S)
            {
                Entry.IsReadOnly = true;
            }

            if ((Keyboard.IsKeyDown(Key.LeftCtrl))
                && key == Key.Z)
            {
                Entry.IsReadOnly = true;
            }
        }

        private void Entry_KeyUp(object sender, KeyEventArgs e)
        {
            var key = e.Key == Key.System ? e.SystemKey : e.Key;

            if ((Keyboard.IsKeyDown(Key.LeftCtrl))
                && key == Key.S)
            {
                Entry.IsReadOnly = false;
            }
            if ((Keyboard.IsKeyDown(Key.LeftCtrl))
                && key == Key.Z)
            {
                Entry.IsReadOnly = false;
            }
        }

        private void AddItem()
        {
            if (Entry.Text.Length > 0)
            {
                EnableButtons();
                Unsave();
                LinkList.Items.Add(Entry.Text);
                Entry.Clear();
            }
        }

        private void EnableButtons()
        {
            RemoveButton.IsEnabled = true;
            TopButton.IsEnabled = true;
            UpButton.IsEnabled = true;
            DownButton.IsEnabled = true;
            BottomButton.IsEnabled = true;
        }

        private void DisableButtons()
        {
            RemoveButton.IsEnabled = false;
            TopButton.IsEnabled = false;
            UpButton.IsEnabled = false;
            DownButton.IsEnabled = false;
            BottomButton.IsEnabled = false;
        }

        private void Unsave()
        {
            ticker = "*";
            saved = false;
            Title = WindowTitle;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddItem();
        }

        private void TopButton_Click(object sender, RoutedEventArgs e)
        {
            Unsave();
            var selectedIndex = LinkList.SelectedIndex;

            if (selectedIndex > 0)
            {
                var itemToMoveUp = LinkList.Items[selectedIndex];
                LinkList.Items.RemoveAt(selectedIndex);
                LinkList.Items.Insert(0, itemToMoveUp);
                LinkList.SelectedIndex = 0;
            }
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            Unsave();
            var selectedIndex = LinkList.SelectedIndex;

            if (selectedIndex > 0)
            {
                var itemToMoveUp = LinkList.Items[selectedIndex];
                LinkList.Items.RemoveAt(selectedIndex);
                LinkList.Items.Insert(selectedIndex - 1, itemToMoveUp);
                LinkList.SelectedIndex = selectedIndex - 1;
            }
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            Unsave();
            var selectedIndex = LinkList.SelectedIndex;

            if (selectedIndex + 1 < LinkList.Items.Count)
            {
                var itemToMoveDown = LinkList.Items[selectedIndex];
                LinkList.Items.RemoveAt(selectedIndex);
                LinkList.Items.Insert(selectedIndex + 1, itemToMoveDown);
                LinkList.SelectedIndex = selectedIndex + 1;
            }
        }

        private void BottomButton_Click(object sender, RoutedEventArgs e)
        {
            Unsave();
            var selectedIndex = LinkList.SelectedIndex;

            if (selectedIndex > 0)
            {
                var itemToMoveUp = LinkList.Items[selectedIndex];
                LinkList.Items.RemoveAt(selectedIndex);
                LinkList.Items.Insert(LinkList.Items.Count, itemToMoveUp);
                LinkList.SelectedIndex = LinkList.Items.Count;
            }
        }

        private void ClipboardRadio_Checked(object sender, RoutedEventArgs e)
        {
            PopButton.Content = "Pop to Clipboard";
        }

        private void ArchiveRadio_Checked(object sender, RoutedEventArgs e)
        {
            PopButton.Content = "Send to Archive.today";
        }

        private void PopButton_Click(object sender, RoutedEventArgs e)
        {
            Entry.Text = LinkList.Items[0].ToString();
            LinkList.Items.RemoveAt(0);
            if (ClipboardRadio.IsChecked == true)
                Clipboard.SetText(Entry.Text);
            else
                OpenUrl("https://archive.today/?run=1&url=" + HttpUtility.UrlEncode(Entry.Text));
            Unsave();
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        
    }
}
