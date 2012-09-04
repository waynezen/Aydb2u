
using System.Windows.Controls;
using System.ComponentModel;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using ZXing;
using ZXing.Common;
using System;
using System.Windows.Media;
using Microsoft.Devices;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
namespace SecretSanta.CustomClasses
{
    public class Imaging 
    {
        private PhotoChooserTask imager;
        private Image barcodeImage;
        private BackgroundWorker imageWorker;

        public Imaging(Image barcodeImageControl, RunWorkerCompletedEventHandler imagingCompletedEvent)
        {

            bool isDebug = false;
#if DEBUG
            isDebug = true;
#endif
            barcodeImage = barcodeImageControl;

            // prepare Photo Chooser Task for the open button
            // Create New Task, Then Set the Complete Event Handler
            imager = new PhotoChooserTask();

            if (!isDebug)
            {
                imager.ShowCamera = true;
            }

            imager.Completed += photoChooser_Complete;

            // prepare the backround worker thread for the image processing
            imageWorker = new BackgroundWorker();
            imageWorker.DoWork += imageWorker_DoWork;
            imageWorker.RunWorkerCompleted += imagingCompletedEvent;
        }

        private void photoChooser_Complete(object sender, Microsoft.Phone.Tasks.PhotoResult e)
        {
            // setting the image in the display and start scanning in the background
            if (e.TaskResult == TaskResult.OK)
            {
                BitmapImage bmp = new BitmapImage();
                bmp.SetSource(e.ChosenPhoto);

                barcodeImage.Dispatcher.BeginInvoke(() => { barcodeImage.Source = bmp; });

                imageWorker.RunWorkerAsync(new WriteableBitmap(bmp));
            }
        }

        public void Show()
        {
            imager.Show();
        }

        private static void imageWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // scanning for a barcode
            var wbmp = (WriteableBitmap)e.Argument;

            var luminiance = new RGBLuminanceSource(wbmp, wbmp.PixelWidth, wbmp.PixelHeight);
            var binarizer = new HybridBinarizer(luminiance);
            var binBitmap = new BinaryBitmap(binarizer);
            var reader = new MultiFormatReader();
            e.Result = reader.decode(binBitmap);
        }

        private void RunWorkerAsync(WriteableBitmap bmp)
        {
            imageWorker.RunWorkerAsync(new WriteableBitmap(bmp));
        }

    }
}
