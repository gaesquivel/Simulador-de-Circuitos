using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CircuitMVVMBase.MVVM
{

    public class RecentFile
    {
        public int Index { get; set; }
        public string FullPath { get; set; }
        public string FileName { get; set; }
        public RoutedCommand Command { get; set; }
    }

    public class RecentFilesViewModel
    {
        public string CurrentRecentFileName { get; set; }

        ObservableCollection<RecentFile> recentlist;
        public ObservableCollection<RecentFile> RecentFileList
        {
            get { return recentlist ?? (recentlist = new ObservableCollection<RecentFile>()); }
        }

        public RecentFilesViewModel()
        {
            CreateRecentFilesList();
            CurrentRecentFileName = "recentfiles.lst";
        }



        protected bool CreateRecentFilesList()
        {
            if (string.IsNullOrWhiteSpace(CurrentRecentFileName))
                return false;

            if (!File.Exists(CurrentRecentFileName))
            {
                StreamWriter writer =  File.CreateText(CurrentRecentFileName);
                writer.Close();
                return false;
            }
            TextReader reader = File.OpenText(CurrentRecentFileName);
            string txt = "";

            txt = reader.ReadToEnd();
            //do
            //{
            //   string str = reader.ReadLine();
            //} while (reader.);


            reader.Close();
            string[] arr = txt.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in arr)
            {
                RecentFileList.Add(new RecentFile() { Index = 1,
                                                    FileName = "file1.txt",
                                                    FullPath = "c:\\file1.txt"
                });

            }
            //RecentFileList = new ObservableCollection<RecentFile>();
            //RecentFileList.Add(new RecentFile() { Index = 1, FileName = "file1.txt", FullPath = "c:\\file1.txt" });
            //RecentFileList.Add(new RecentFile() { Index = 2, FileName = "file2.txt", FullPath = "d:\\file2.txt" });
            //return list;
            return true;
        }



    }
}
