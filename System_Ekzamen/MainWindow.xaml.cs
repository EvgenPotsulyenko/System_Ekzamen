using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace System_Ekzamen
{
    public partial class MainWindow : Window
    {
        bool del = true; //отмена действия
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) // проверка текста
        {            
            Regex regex = new Regex(@"\w*фикус\w*", RegexOptions.IgnoreCase); // запрещенное слово
            string result = regex.Replace(textbox1.Text, "*******"); // замена на звезды
            textbox2.Text = result;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) // проверка файла
        {
            string path = @"D:\Copies"; // путь к исправленым файлам 
            FileInfo namefile = new FileInfo(textpath.Text); // путь к файлу
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists) // создание пути
            {
                dirInfo.Create();
            }
            string fileText = File.ReadAllText(textpath.Text); // чтение содержимого файла
            Regex regex = new Regex(@"\w*фикус\w*", RegexOptions.IgnoreCase);
            string result = regex.Replace(fileText, "*******");
            textfile.Text = result;
            if (Regex.IsMatch(fileText, @"\w*фикус\w*", RegexOptions.IgnoreCase))
            {
                File.AppendAllText(path + "\\" + "copy_" + namefile.Name, result); // запись измененого текста
            }          

        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            listbox1.Items.Clear(); // очистка найденных файлов
            await Task.Run(() => SafeEnumerateFiles()); // поиск файлов
            progres.Maximum = listbox1.Items.Count; // максимальное значение загрузки
            await Task.Run(() => search()); // поиск запрещенных слов
        }
        async public void search()
        {
            for (int i = 0; i < listbox1.Items.Count; i++) // проверка всех путей
            {
                if (del == false) // отмена действия
                {
                    break;
                }
                Dispatcher.Invoke(new ThreadStart(delegate { progres.Value++; })); 
                string path2 = @"D:\Copies";
                FileInfo namefile = new FileInfo(Convert.ToString(listbox1.Items[i]));
                DirectoryInfo dirInfo = new DirectoryInfo(path2);
                if (!dirInfo.Exists) // создание пути
                {
                    dirInfo.Create();
                }
                string fileText = File.ReadAllText(Convert.ToString(listbox1.Items[i])); // считывание содержимого файла
                Regex regex = new Regex(@"\w*фикус\w*", RegexOptions.IgnoreCase);
                string result = regex.Replace(fileText, "*******"); // проверка и замена
                if (Regex.IsMatch(fileText, @"\w*фикус\w*", RegexOptions.IgnoreCase)) //  запись файла с замененными запрещенными словами
                {
                    File.AppendAllText(path2 + "\\" + "copy_" + namefile.Name, result);
                    FileInfo fileInfo = new FileInfo(@"D:\Copies\otchet.txt"); // создание отчёта
                    if (!fileInfo.Exists)
                    {
                        using (FileStream fs2 = fileInfo.Open(FileMode.OpenOrCreate,FileAccess.ReadWrite, FileShare.None)) { };
                    }
                    File.AppendAllText(fileInfo.FullName,"Путь к файлу:" + namefile.DirectoryName + "\n"); 
                    File.AppendAllText(fileInfo.FullName, "Размер файла:" + namefile.Length + "\n");
                }
            }
            MessageBox.Show("Работа успешно выполнена");
        }

        async public void SafeEnumerateFiles()
        {
            foreach (var drive in DriveInfo.GetDrives()) // поиск дисков
            {
                string searchPattern = "*.txt*";
                SearchOption searchOption = SearchOption.AllDirectories;
                var dirs = new Stack<string>();
                dirs.Push(drive.Name);
                while (dirs.Count > 0) // проход по дискам
                {
                    if (del == false)
                    {                       
                        break;
                        
                    }
                    string currentDirPath = dirs.Pop();
                    if (searchOption == SearchOption.AllDirectories)
                    {
                        try
                        {
                            string[] subDirs = Directory.GetDirectories(currentDirPath);
                            foreach (string subDirPath in subDirs)
                            {
                                dirs.Push(subDirPath);                               
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            continue;
                        }
                        catch (DirectoryNotFoundException)
                        {
                            continue;
                        }   
                        catch
                        {
                            break;
                        }
                    }
                    string[] files = null;
                    try
                    {
                        files = Directory.GetFiles(currentDirPath, searchPattern);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        continue;
                    }
                    catch (DirectoryNotFoundException)
                    {
                        continue;
                    }
                    foreach (string filePath in files)
                    {
                        Dispatcher.Invoke(new ThreadStart(delegate { listbox1.Items.Add(filePath); })); // запись пути
                    }
                }
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            del = false; // отмена действия
        }
    }
}
