using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CountdownCollection {
    public partial class ManageEventsPage : ContentPage {
        private CountdownCollection.MainPage mainPage;

        bool storedEventsSelected;
        bool myEventsSelected;
        Grid myEventsGrid;

        ActivityIndicator selectingIndicator;

        public ManageEventsPage() {
            InitializeComponent();
            this.mainPage = (MainPage)App.Current.MainPage;

            stack.Children[0].HeightRequest = mainPage.getUtilPaddingHeight();

            storedEventsSelected = true;
            myEventsSelected = false;

            //initialize myEventsGrid
            myEventsGrid = new Grid();
            myEventsGrid.HorizontalOptions = LayoutOptions.FillAndExpand;
            myEventsGrid.VerticalOptions = LayoutOptions.Start;
            myEventsGrid.ColumnDefinitions = storedEventsGrid.ColumnDefinitions;

            //initialize indicator for selecting mass switches
            selectingIndicator = new ActivityIndicator();
            selectingIndicator.HorizontalOptions = LayoutOptions.FillAndExpand;
            selectingIndicator.VerticalOptions = LayoutOptions.Center;
            selectingIndicator.Color = Color.White;
            selectingIndicator.BackgroundColor = Color.Black;
            selectingIndicator.IsEnabled = true;
            selectingIndicator.IsRunning = true;

            populateGrids();
        }

        public void populateGrids() {
            populateStoredEventsGrid();
            populateMyEventsGrid();
        }

        public async void done(object sender, EventArgs e) {
            displayActivityIndicator();

            await Task.Run(() => {
                FileHandler fileHandler = new FileHandler();
                fileHandler.updateStoredEventsFile();
                fileHandler.updateMyEventsFile();

                Device.BeginInvokeOnMainThread(() => {
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

        public void displaySelectingIndicator() {
            Device.BeginInvokeOnMainThread(() => {
                scrollView.Content = selectingIndicator;
            });
        }

        public async void selectAll(object sender, EventArgs e) {
            if (myEventsSelected && myEventsGrid.Children.Count == 0) {
                return;
            }

            Device.BeginInvokeOnMainThread(() => {
                displaySelectingIndicator();
            });

            await Task.Run(() => {
                Thread.Sleep(400);
                if (storedEventsSelected) {
                    for (int i = 0; i < GlobalVariables.storedEvents.Count(); i++) {
                        GlobalVariables.storedEvents[i].setVisible();
                    }
                    Device.BeginInvokeOnMainThread(() => {
                        populateStoredEventsGrid();
                        scrollView.Content = storedEventsGrid;
                    });
                }
                else if (myEventsSelected) {
                    for (int i = 0; i < GlobalVariables.myEvents.Count(); i++) {
                        GlobalVariables.myEvents[i].setVisible();
                    }
                    Device.BeginInvokeOnMainThread(() => {
                        populateMyEventsGrid();
                        scrollView.Content = myEventsGrid;
                    });
                }
            });
        }

        public async void unselectAll(object sender, EventArgs e) {
            if (myEventsSelected && myEventsGrid.Children.Count == 0) {
                return;
            }

            Device.BeginInvokeOnMainThread(() => {
                displaySelectingIndicator();
            });

            await Task.Run(() => {
                Thread.Sleep(400);
                if (storedEventsSelected) {
                    for (int i = 0; i < GlobalVariables.storedEvents.Count(); i++) {
                        GlobalVariables.storedEvents[i].setInvisible();
                    }
                    Device.BeginInvokeOnMainThread(() => {
                        populateStoredEventsGrid();
                        scrollView.Content = storedEventsGrid;
                    });
                }
                else if (myEventsSelected) {
                    for (int i = 0; i < GlobalVariables.myEvents.Count(); i++) {
                        GlobalVariables.myEvents[i].setInvisible();
                    }
                    Device.BeginInvokeOnMainThread(() => {
                        populateMyEventsGrid();
                        scrollView.Content = myEventsGrid;
                    });
                }
            });
        }

        public async void restoreDefaults(object sender, EventArgs e) {
            Device.BeginInvokeOnMainThread(() => {
                displaySelectingIndicator();
            });

            await Task.Run(() => {
                Thread.Sleep(400);
                FileHandler fileHandler = new FileHandler();
                fileHandler.resetStoredEventsFile();
                mainPage.initializeStoredEvents();
                Device.BeginInvokeOnMainThread(() => {
                    populateStoredEventsGrid();
                    scrollView.Content = storedEventsGrid;
                });
            });
        }

        public async void deleteAll(object sender, EventArgs e) {
            if (myEventsGrid.Children.Count == 0) {
                return;
            }
            bool x = await DisplayAlert("Are you sure?", "All of your custom events will be deleted.", "Delete", "Cancel");
            if (x) {
                FileHandler fileHandler = new FileHandler();
                fileHandler.resetMyEventsFile();
                mainPage.initializeMyEvents();
                populateMyEventsGrid();
            }
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
                deleteAllButton.IsVisible = false;
                restoreDefaultsButton.IsVisible = true;
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
                restoreDefaultsButton.IsVisible = false;
                deleteAllButton.IsVisible = true;
            }
        }

        public async void DeleteButton_Clicked(object sender, EventArgs e) {
            bool x = await DisplayAlert("Are you sure?", "\"" + GlobalVariables.eventToBeDeleted + "\" will be deleted.", "Delete", "Cancel");
            if (x) {
                Device.BeginInvokeOnMainThread(() => {
                    displaySelectingIndicator();
                });

                await Task.Run(() => {
                    Thread.Sleep(400);
                    for (int i = 0; i < GlobalVariables.myEvents.Count; i++) {
                        if (GlobalVariables.myEvents[i].getName().Equals(GlobalVariables.eventToBeDeleted)) {
                            GlobalVariables.myEvents.RemoveAt(i);
                            Device.BeginInvokeOnMainThread(() => {
                                populateMyEventsGrid();
                                scrollView.Content = myEventsGrid;
                            });

                            //remove event from saved file
                            FileHandler fileHandler = new FileHandler();
                            fileHandler.updateMyEventsFile();

                            return;
                        }
                    }
                });
            }
        }

        public void populateStoredEventsGrid() {
            storedEventsGrid.Children.Clear();

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
                dateLabel.Text = currentEvent.getDate().ToString("MMMM d, yyyy");
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

            GlobalVariables.storedEvents.Sort((x, y) => x.getName().CompareTo(y.getName()));

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
                eventLabel.HorizontalTextAlignment = TextAlignment.Start;
                eventLabel.HorizontalOptions = Xamarin.Forms.LayoutOptions.Start;
                eventLabel.VerticalOptions = Xamarin.Forms.LayoutOptions.EndAndExpand;
                eventLabel.FontSize = 16;
                eventLabel.TextColor = Color.Black;

                Label dateLabel = new Label();
                dateLabel.Text = currentEvent.getDate().ToString("MMMM d, yyyy");
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
    }
}