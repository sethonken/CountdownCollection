using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CountdownCollection {
    public partial class AddNewEventPage : ContentPage {
        private CountdownCollection.MainPage mainPage;
        bool oneTimeEvent;

        public AddNewEventPage() {
            InitializeComponent();
            oneTimeEvent = false;
            this.mainPage = (MainPage)App.Current.MainPage;
            stack.Children[0].HeightRequest = mainPage.getUtilPaddingHeight();
        }

        public void addEvent(object sender, EventArgs e) {
            if (entryName.Text == null) {
                this.DisplayAlert("Error", "Event name cannot be empty", "Ok");
                return;
            }
            for (int i = 0; i < GlobalVariables.myEvents.Count; i++) {
                if (string.Compare(GlobalVariables.myEvents[i].getName(), entryName.Text, true) == 0) {
                    this.DisplayAlert("Error", "An event with that name already exists", "Ok");
                    return;
                }
            }
            for (int i = 0; i < GlobalVariables.storedEvents.Count; i++) {
                if (string.Compare(GlobalVariables.storedEvents[i].getName(), entryName.Text, true) == 0) {
                    this.DisplayAlert("Error", "An event with that name already exists", "Ok");
                    return;
                }
            }
            Event newEvent = new Event(entryName.Text, datePicked.Date);
            newEvent.setOneTimeEvent(oneTimeEvent);
            GlobalVariables.resetEvent(newEvent);
            GlobalVariables.myEvents.Add(newEvent);

            //save event to file
            try {
                FileHandler fileHandler = new FileHandler();
                fileHandler.appendEventToFile(newEvent);
            } catch (Exception ex) {
                Debug.WriteLine("Exception caught!");
                Debug.WriteLine(ex.Message);
            }
            mainPage.populateGrid();
            Navigation.PopModalAsync();
        }

        public void oneTimeSwitchChanged(object sender, ToggledEventArgs e) {
            oneTimeEvent = e.Value;
        }

        public void cancel(object sender, EventArgs e) {
            Navigation.PopModalAsync();
        }
    }
}