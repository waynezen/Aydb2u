using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace SecretSanta.Storage
{
    public static class IsoStoreHelper
    {
        private static IsolatedStorageFile _isoStore;
        public static IsolatedStorageFile IsoStore
        {
            get { return _isoStore ?? (_isoStore = IsolatedStorageFile.GetUserStoreForApplication()); }
        }

        public static void SaveList<T>(string folderName, string dataName, List<T> dataList) where T : class
        {
            if (!IsoStore.DirectoryExists(folderName))
            {
                IsoStore.CreateDirectory(folderName);
            }

            string fileStreamName = string.Format("{0}\\{1}.dat", folderName, dataName);

            using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(fileStreamName, FileMode.Create, IsoStore))
            {
                DataContractSerializer dcs = new DataContractSerializer(typeof(ObservableCollection<T>));
                dcs.WriteObject(stream, ToObservableCollection<T>(dataList));
            }
        }

        public static List<T> LoadList<T>(string folderName, string dataName) where T : class
        {
            ObservableCollection<T> retval = new ObservableCollection<T>();

            string fileStreamName = string.Format("{0}\\{1}.dat", folderName, dataName);

            using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(fileStreamName, FileMode.OpenOrCreate, IsoStore))
            {
                if (stream.Length > 0)
                {
                    DataContractSerializer dcs = new DataContractSerializer(typeof(ObservableCollection<T>));
                    retval = dcs.ReadObject(stream) as ObservableCollection<T>;
                }
            }

            return new List<T>(retval);
        }

        private static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
        {
            var col = new ObservableCollection<T>();
            foreach (var cur in enumerable)
            {
                col.Add(cur);
            }
            return col;
        }

    }

}
