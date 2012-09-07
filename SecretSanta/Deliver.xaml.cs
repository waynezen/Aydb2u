using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using SecretSanta.CustomClasses;
using ZXing;
using System.ComponentModel;
using SecretSanta.Storage;

namespace SecretSanta
{
    public partial class Deliver : PhoneApplicationPage
    {
        private Imaging imager;
        private LocalResource _localResx = null;
        private Delivery _selectedDelivery;

        public Deliver()
        {
            InitializeComponent();

            _localResx = LocalResource.GetInstance;
            imager = new Imaging(BarcodeImage, imaging_RunWorkerCompleted);
        }

        private void DeliverButton_Click(object sender, RoutedEventArgs e)
        {
            imager.Show();
        }

        private void DisplayResult(Result result)
        {
            if (result != null)
            {
                // Load Delivery
                _selectedDelivery = new DeliveryData().GetDeliveriesLocal().SingleOrDefault(d => d.Id == result.Text);

                Address.Text = _selectedDelivery.Address.ToString();

                DeliveryStatus.ItemsSource = GetStatusValues();
                DeliveryStatus.SelectedItem = GetStatusValues().Single ( s => s.Key == _selectedDelivery.Status);

                Note.ItemsSource = GetNoteValues();
                SecondaryNote.ItemsSource = GetSecondaryNoteValues();

                DeliveryImaging.Visibility = System.Windows.Visibility.Collapsed;
                DeliveryUpdate.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                Message.Text = "No barcode found.";
            }
        }

        private void imaging_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // processing the result of the background scanning
            if (e.Cancelled)
            {
                Message.Text = "Cancelled.";
            }
            else if (e.Error != null)
            {
                Message.Text = e.Error.Message;
            }
            else
            {
                var result = (Result)e.Result;
                DisplayResult(result);
            }
        }

        private List<KeyValuePair<int, string>> GetStatusValues()
        {
            var statuses = new List<KeyValuePair<int, string>>();
            statuses.Add(new KeyValuePair<int, string>(0, "Undelivered"));
            statuses.Add(new KeyValuePair<int, string>(1, "Delivered"));
            return statuses;
        }

        private List<KeyValuePair<int, string>> GetNoteValues()
        {
            var notes = new List<KeyValuePair<int, string>>();
            notes.Add(new KeyValuePair<int, string>(0, "Not home"));
            notes.Add(new KeyValuePair<int, string>(1, "Moved"));
            notes.Add(new KeyValuePair<int, string>(2, "Incorrect address/client doesn’t live there"));
            notes.Add(new KeyValuePair<int, string>(3, "Can’t find address/address doesn’t exist"));
            notes.Add(new KeyValuePair<int, string>(4, "Require buzzer number"));
            notes.Add(new KeyValuePair<int, string>(5, "Require apartment number"));
            notes.Add(new KeyValuePair<int, string>(6, "Client is out of town"));
            notes.Add(new KeyValuePair<int, string>(7, "Incorrect gift - gender or age"));
            notes.Add(new KeyValuePair<int, string>(8, "Gift refused"));
            return notes;
        }

        private List<KeyValuePair<int, string>> GetSecondaryNoteValues()
        {
            var secondaryNotes = new List<KeyValuePair<int, string>>();
            secondaryNotes.Add(new KeyValuePair<int, string>(0, " "));
            secondaryNotes.Add(new KeyValuePair<int, string>(1, "Not-Home Form left"));
            secondaryNotes.Add(new KeyValuePair<int, string>(2, "No place to leave Not-Home Form"));
            secondaryNotes.Add(new KeyValuePair<int, string>(3, "Client received gifts at new address"));
            secondaryNotes.Add(new KeyValuePair<int, string>(4, "Address verified/corrected – will  deliver on Sunday"));
            return secondaryNotes;
        }

        private void DeliveryStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Get the data object that represents the current selected item
            KeyValuePair<int, string> data = (KeyValuePair<int, string>)((sender as ListPicker).SelectedItem);

            if (data.Value == "Undelivered")
            {
                lblNote.Visibility = System.Windows.Visibility.Visible;
                Note.Visibility = System.Windows.Visibility.Visible;
                lblSecondaryNote.Visibility = System.Windows.Visibility.Visible;
                SecondaryNote.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                lblNote.Visibility = System.Windows.Visibility.Collapsed;
                Note.Visibility = System.Windows.Visibility.Collapsed;
                lblSecondaryNote.Visibility = System.Windows.Visibility.Collapsed;
                SecondaryNote.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void ApplicationBarIconButtonSave_Click(object sender, EventArgs e)
        {
            var selectedStatus = (KeyValuePair<int, string>)DeliveryStatus.SelectedItem;
            _selectedDelivery.Status = ((KeyValuePair<int, string>)DeliveryStatus.SelectedItem).Key;
            _selectedDelivery.Comment = Comment.Text;

            if (selectedStatus.Value == "Undelivered")
            {
                _selectedDelivery.Note = ((KeyValuePair<int, string>)Note.SelectedItem).Value;
                _selectedDelivery.SecondaryNote = ((KeyValuePair<int, string>)SecondaryNote.SelectedItem).Value;
            }

            new DeliveryData().UpdateDeliveryToLocalAndWeb(_selectedDelivery);
            Deployment.Current.Dispatcher.BeginInvoke(() => { NavigationService.Navigate(new Uri("/Deliveries.xaml", UriKind.Relative)); });
        }

    }
} 