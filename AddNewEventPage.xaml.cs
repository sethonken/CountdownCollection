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
        bool allDayEvent;
        bool oneTimeEvent;
        bool changedFromZero;  //indicates if this is the first time a user changes the time in a new event page

        public AddNewEventPage() {
            InitializeComponent();
            allDayEvent = true;
            oneTimeEvent = false;
            changedFromZero = true;
            datePicked.MaximumDate = DateTime.Now.AddDays(9999);

            timePicked.PropertyChanged += TimePicked_PropertyChanged;
            this.mainPage = (MainPage)App.Current.MainPage;
            stack.Children[0].HeightRequest = mainPage.getUtilPaddingHeight();
        }

        /*
         *  
         * Events
         * 
         */
        private void TimePicked_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (changedFromZero && timePicked.Time.TotalSeconds != 0) {
                yearlySwitch.IsToggled = false;
                oneTimeEvent = true;
                changedFromZero = false;
            }
        }

        public async void addEvent(object sender, EventArgs e) {
            if (entryName.Text == null) {
                await this.DisplayAlert("Error", "Event name cannot be empty.", "Ok");
                return;
            }

            //remove white space on ends
            while (entryName.Text.Length > 0 && entryName.Text[0] == ' ') {
                entryName.Text = entryName.Text.Substring(1);
            }
            while (entryName.Text.Length > 0 && entryName.Text[entryName.Text.Length - 1] == ' ') {
                entryName.Text = entryName.Text.Substring(0, entryName.Text.Length - 1);
            }

            if (entryName.Text.Length == 0) {
                await this.DisplayAlert("Error", "Event name cannot be empty.", "Ok");
                return;
            }
            for (int i = 0; i < GlobalVariables.myEvents.Count; i++) {
                if (string.Compare(GlobalVariables.myEvents[i].getName(), entryName.Text, true) == 0) {
                    await this.DisplayAlert("Error", "An event with that name already exists.", "Ok");
                    return;
                }
            }
            for (int i = 0; i < GlobalVariables.storedEvents.Count; i++) {
                if (string.Compare(GlobalVariables.storedEvents[i].getName(), entryName.Text, true) == 0) {
                    await this.DisplayAlert("Error", "An event with that name already exists.", "Ok");
                    return;
                }
            }

            displayActivityIndicator();

            await Task.Run(() => {
                DateTime eventDate = new DateTime(datePicked.Date.Year, datePicked.Date.Month, datePicked.Date.Day, timePicked.Time.Hours, timePicked.Time.Minutes, 0);
                Event newEvent = new Event(entryName.Text, eventDate);
                newEvent.setAllDayEvent(allDayEvent);
                newEvent.setOneTimeEvent(oneTimeEvent);
                GlobalVariables.resetEvent(newEvent);
                GlobalVariables.myEvents.Add(newEvent);

                //save event to file
                FileHandler fileHandler = new FileHandler();
                fileHandler.appendEventToFile(newEvent);

                Device.BeginInvokeOnMainThread(() => {
                    mainPage.refreshGrid();
                    mainPage.populateGrid();
                    Navigation.PopModalAsync();
                });
            });
        }

        public void displayActivityIndicator() {
            var utilPadding = new Label {
                BackgroundColor = Color.White,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                HeightRequest = mainPage.getUtilPaddingHeight()
            };

            var logoBanner = new Image {
                Aspect = Aspect.AspectFit,
                Source = "LogoWithName.png",
                BackgroundColor = Color.FromHex("#0000dc"),
                Margin = new Thickness(0, 5, 0, 5)
            };

            var indicator = new ActivityIndicator() {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Color = Color.White,
                BackgroundColor = Color.Black,
                IsEnabled = true,
                IsRunning = true
            };

            var root = new StackLayout() {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Spacing = 0,
                BackgroundColor = Color.Black,
                Children = {
                    utilPadding,
                    logoBanner,
                    indicator
                }
            };

            Content = root;
        }

        public void allDaySwitchChanged(object sender, ToggledEventArgs e) {
            if (e.Value) {
                allDayEvent = true;
                Device.BeginInvokeOnMainThread(() => {
                    timePicked.ClearValue(TimePicker.TimeProperty);
                    startTimeStack.IsVisible = false;
                    });
            }
            else {
                allDayEvent = false;
                Device.BeginInvokeOnMainThread(() => {
                    startTimeStack.IsVisible = true;
                });
            }
        }

        public void yearlySwitchChanged(object sender, ToggledEventArgs e) {
            oneTimeEvent = !e.Value;
        }

        public void cancel(object sender, EventArgs e) {
            Navigation.PopModalAsync();
        }
    }
}