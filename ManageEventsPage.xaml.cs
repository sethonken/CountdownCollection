using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CountdownCollection {
    public partial class ManageEventsPage : ContentPage {
        private CountdownCollection.MainPage mainPage;

        bool storedEventsSelected;
        bool myEventsSelected;
        Grid myEventsGrid;

        public ManageEventsPage() {
            InitializeComponent();
            this.mainPage = (MainPage)App.Current.MainPage;

            stack.Children[0].HeightRequest = mainPage.getUtilPaddingHeight();

            storedEventsSelected = true;
            myEventsSelected = false;

            //initialize myEventsGrid
            myEventsGrid = new Grid();
            myEventsGrid.HorizontalOptions = LayoutOptions.FillAndExpand;
            myEventsGrid.ColumnDefinitions = storedEventsGrid.ColumnDefinitions;

            populateStoredEventsGrid();
            populateMyEventsGrid();
        }

        public void done(object sender, EventArgs e) {
            FileHandler fileHandler = new FileHandler();
            fileHandler.updateStoredEventsFile();
            fileHandler.updateMyEventsFile();
            mainPage.populateGrid();
            Navigation.PopModalAsync();
        }

        public void changeToStoredEvents(object sender, EventArgs e) {
            myEventsButton.BackgroundColor = Color.DarkGray;
            storedEventsButton.BackgroundColor = Color.LightSteelBlue;

            if (myEventsSelected) {
                myEventsSelected = false;
            }
            if (!storedEventsSelected) {
                storedEventsSelected = true;
                scrollView.Content = storedEventsGrid;
            }
        }

        public void changeToMyEvents(object sender, EventArgs e) {
            storedEventsButton.BackgroundColor = Color.DarkGray;
            myEventsButton.BackgroundColor = Color.LightSteelBlue;

            if (storedEventsSelected) {
                storedEventsSelected = false;
            }
            if (!myEventsSelected) {
                myEventsSelected = true;
                scrollView.Content = myEventsGrid;
            }
        }

        public void populateStoredEventsGrid() {
            //sort events based on name
            GlobalVariables.storedEvents.Sort((x, y) => x.getName().CompareTo(y.getName()));

            //add events to grid
            int row = 0;
            int storedEventsIndex = 0;
            Event currentEvent;
            while (storedEventsIndex < GlobalVariables.storedEvents.Count()) {
                currentEvent = null;
                currentEvent = GlobalVariables.storedEvents[storedEventsIndex++];

                StackLayout stack = new StackLayout();
                stack.HorizontalOptions = Xamarin.Forms.LayoutOptions.Start;
                stack.VerticalOptions = Xamarin.Forms.LayoutOptions.CenterAndExpand;

                Label eventLabel = new Label();
                eventLabel.Text = currentEvent.getName();
                eventLabel.HorizontalTextAlignment = TextAlignment.Center;
                eventLabel.HorizontalOptions = Xamarin.Forms.LayoutOptions.Start;
                eventLabel.VerticalOptions = Xamarin.Forms.LayoutOptions.EndAndExpand;
                eventLabel.FontSize = 16;
                eventLabel.TextColor = Color.Black;

                Label dateLabel = new Label();
                dateLabel.Text = "(" + currentEvent.getDate().Month + "/" + currentEvent.getDate().Day + "/" + currentEvent.getDate().Year + ")";
                dateLabel.HorizontalOptions = Xamarin.Forms.LayoutOptions.Start;
                dateLabel.VerticalOptions = Xamarin.Forms.LayoutOptions.StartAndExpand;
                dateLabel.FontSize = 13;
                dateLabel.TextColor = Xamarin.Forms.Color.Black;

                stack.Children.Add(eventLabel);
                stack.Children.Add(dateLabel);

                Xamarin.Forms.Switch visibleSwitch = new Xamarin.Forms.Switch();
                visibleSwitch.HorizontalOptions = Xamarin.Forms.LayoutOptions.EndAndExpand;
                visibleSwitch.VerticalOptions = Xamarin.Forms.LayoutOptions.Center;
                visibleSwitch.Toggled += currentEvent.VisibleSwitch_Toggled;
                visibleSwitch.IsToggled = currentEvent.isVisible();

                //add grid's back color
                BoxView backColor = new BoxView();
                backColor.HorizontalOptions = Xamarin.Forms.LayoutOptions.FillAndExpand;
                backColor.Color = Color.LightSteelBlue;

                storedEventsGrid.Children.Add(backColor, 0, 3, row, row + 1);
                storedEventsGrid.Children.Add(stack, 0, 2, row, row + 1);
                storedEventsGrid.Children.Add(visibleSwitch, 2, row);

                row++;
            }
        }

        public void populateMyEventsGrid() {
            myEventsGrid.Children.Clear();

            //sort events based on name
            GlobalVariables.myEvents.Sort((x, y) => x.getName().CompareTo(y.getName()));

            //add events to grid
            int row = 0;
            int myEventsIndex = 0;
            Event currentEvent;
            while (myEventsIndex < GlobalVariables.myEvents.Count()) {
                currentEvent = null;
                currentEvent = GlobalVariables.myEvents[myEventsIndex++];

                Button deleteButton = new Button();
                deleteButton.VerticalOptions = LayoutOptions.Center;
                deleteButton.HorizontalOptions = LayoutOptions.Center;
                deleteButton.Opacity = 0.4;
                deleteButton.HeightRequest = 30;

                deleteButton.Clicked += currentEvent.DeleteButton_Clicked;
                deleteButton.Clicked += DeleteButton_Clicked;

                Image deleteButtonImage = new Image();
                deleteButtonImage.Source = "DeleteButton.png";
                deleteButtonImage.Aspect = Aspect.AspectFill;
                deleteButtonImage.HorizontalOptions = LayoutOptions.Center;
                deleteButtonImage.VerticalOptions = LayoutOptions.Center;
                deleteButtonImage.HeightRequest = 30;

                StackLayout stack = new StackLayout();
                stack.HorizontalOptions = LayoutOptions.FillAndExpand;
                stack.VerticalOptions = LayoutOptions.FillAndExpand;

                Label eventLabel = new Label();
                eventLabel.Text = currentEvent.getName();
                eventLabel.HorizontalTextAlignment = TextAlignment.Center;
                eventLabel.HorizontalOptions = Xamarin.Forms.LayoutOptions.Start;
                eventLabel.VerticalOptions = Xamarin.Forms.LayoutOptions.EndAndExpand;
                eventLabel.FontSize = 16;
                eventLabel.TextColor = Color.Black;

                Label dateLabel = new Label();
                dateLabel.Text = "(" + currentEvent.getDate().Month + "/" + currentEvent.getDate().Day + "/" + currentEvent.getDate().Year + ")";
                dateLabel.HorizontalOptions = Xamarin.Forms.LayoutOptions.Start;
                dateLabel.VerticalOptions = Xamarin.Forms.LayoutOptions.StartAndExpand;
                dateLabel.FontSize = 13;
                dateLabel.TextColor = Xamarin.Forms.Color.Black;

                stack.Children.Add(eventLabel);
                stack.Children.Add(dateLabel);

                Xamarin.Forms.Switch visibleSwitch = new Xamarin.Forms.Switch();
                visibleSwitch.HorizontalOptions = Xamarin.Forms.LayoutOptions.EndAndExpand;
                visibleSwitch.VerticalOptions = Xamarin.Forms.LayoutOptions.Center;
                visibleSwitch.Toggled += currentEvent.VisibleSwitch_Toggled;
                visibleSwitch.IsToggled = currentEvent.isVisible();

                //add grid's back color
                BoxView backColor = new BoxView();
                backColor.HorizontalOptions = Xamarin.Forms.LayoutOptions.FillAndExpand;
                backColor.Color = Color.LightSteelBlue;

                myEventsGrid.Children.Add(backColor, 0, 3, row, row + 1);
                myEventsGrid.Children.Add(deleteButtonImage, 0, row);
                myEventsGrid.Children.Add(deleteButton, 0, row);
                myEventsGrid.Children.Add(stack, 1, row);
                myEventsGrid.Children.Add(visibleSwitch, 2, row);

                row++;
            }
        }

        public async void DeleteButton_Clicked(object sender, EventArgs e) {
            bool x = await DisplayAlert("Are you sure?", "\"" + GlobalVariables.eventToBeDeleted + "\" will be deleted.", "Yes", "No");
            if (x) {
                for (int i = 0; i < GlobalVariables.myEvents.Count; i++) {
                    if (GlobalVariables.myEvents[i].getName().Equals(GlobalVariables.eventToBeDeleted)) {
                        GlobalVariables.myEvents.RemoveAt(i);
                        populateMyEventsGrid();
                        return;
                    }
                }
            }
        }
    }
}
