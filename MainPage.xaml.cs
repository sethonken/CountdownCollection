using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

using Xamarin.Forms;

namespace CountdownCollection {
    public partial class MainPage : ContentPage {
        public int lastDay;
        public int currentDay;
        public int utilPaddingHeight;

        DateTime currentTime;

        private System.Timers.Timer objTimer;

        public MainPage() {
            Debug.WriteLine("Starting main page");
            try {
                InitializeComponent();
                Debug.WriteLine("Components initialized");
                initializeEvents();
            } catch (System.Reflection.AmbiguousMatchException ex) {
                Debug.WriteLine("****\n****\n" + ex.Source + ex.Message + ex.StackTrace);
            }

            App.Current.MainPage = new NavigationPage();
            lastDay = -1;

            //start opening animation
            try {
                System.Threading.ThreadStart openingAnimationStart = animateOpening;
                System.Threading.Thread openingAnimationThread = new System.Threading.Thread(openingAnimationStart);
                openingAnimationThread.IsBackground = true;
                openingAnimationThread.Start();
            } catch (Exception ex) {
                Debug.WriteLine("****\n****\n" + ex.Source + ex.Message + ex.StackTrace);
            }

            //timer
            objTimer = new System.Timers.Timer();
            objTimer.Interval = 500;
            objTimer.Elapsed += ObjTimer_Elapsed;
            objTimer.Start();
        }

        public void ObjTimer_Elapsed(object sender, ElapsedEventArgs e) {
            refreshGrid();
            newDayRefresh();
        }

        public void newDayRefresh() {
            currentDay = DateTime.Now.Day;
            if (lastDay != currentDay) {
                FileHandler fileHandler = new FileHandler();

                //set current date text
                Device.BeginInvokeOnMainThread(() => {
                    currentDate.Text = "(" + DateTime.Now.Month + "/" + DateTime.Now.Day + "/" + DateTime.Now.Year + ")";
                });

                //new day, change any events that were today to next year
                for (int i = 0; i < GlobalVariables.myEvents.Count; i++) {
                    if (GlobalVariables.myEvents[i].DaysUntil < 0) {
                        if (GlobalVariables.myEvents[i].isOneTimeEvent()) {
                            GlobalVariables.myEvents.RemoveAt(i--);
                            continue;
                        }
                        GlobalVariables.resetEvent(GlobalVariables.myEvents[i]);
                    }
                    else {
                        break;
                    }
                }
                for (int i = 0; i < GlobalVariables.storedEvents.Count; i++) {
                    if (GlobalVariables.storedEvents[i].DaysUntil < 0) {
                        GlobalVariables.resetEvent(GlobalVariables.storedEvents[i]);
                    }
                    else {
                        break;
                    }
                }

                //check if a one time event has lapsed since last app open
                bool showElapsedEvents = false;
                for (int i = 0; i < GlobalVariables.myEvents.Count; i++) {
                    if (GlobalVariables.myEvents[i].isOneTimeEvent() && eventHasLapsed(GlobalVariables.myEvents[i])) {
                        GlobalVariables.elapsedEvents.Add(GlobalVariables.myEvents[i]);
                        GlobalVariables.myEvents.RemoveAt(i--);
                        showElapsedEvents = true;
                        continue;
                    }
                }

                //showElapsedEvents = false; //test
                if (showElapsedEvents) {
                    animateElapsedEvents();
                }

                fileHandler.updateMyEventsFile();
                refreshGrid();
                lastDay = currentDay;
                Device.BeginInvokeOnMainThread(() => {
                    populateGrid();
                });
            }
        }

        bool eventHasLapsed(Event oldEvent) {
            DateTime storedDate = new DateTime(GlobalVariables.lastRecordedYear, GlobalVariables.lastRecordedMonth, GlobalVariables.lastRecordedDay, 0, 0, 0);

            int daysUntilEvent = (oldEvent.getDate() - storedDate).Days;
            if (isLeapYear(storedDate)) {
                if (storedDate.Month < 3) {
                    daysUntilEvent--;
                }
            }
            if (isLeapYear(oldEvent.getDate())) {
                if (oldEvent.getDate().Month > 2) {
                    daysUntilEvent--;
                }
            }

            if (oldEvent.getDate().Month == 2 && oldEvent.getDate().Day == 29) {
                if (isLeapYear(storedDate) && storedDate.Month > 2) {
                    return false;
                }
            }

            if (daysUntilEvent >= 365) {
                return true;
            }

            return false;
        }

        bool isLeapYear(DateTime date) {
            if (date.Year % 4 != 0) {
                return false;
            }
            else if (date.Year % 100 != 0) {
                return true;
            }
            else if (date.Year % 400 != 0) {
                return false;
            }
            else {
                return true;
            }
        }

        public void refreshGrid() {
            currentTime = DateTime.Now;
            for (int i = 0; i < GlobalVariables.myEvents.Count; i++) {
                GlobalVariables.myEvents[i].refreshTimeUntil(currentTime);
            }
            for (int i = 0; i < GlobalVariables.storedEvents.Count; i++) {
                GlobalVariables.storedEvents[i].refreshTimeUntil(currentTime);
            }
        }

        public void initializeEvents() {
            GlobalVariables.myEvents = new List<Event>();
            GlobalVariables.storedEvents = new List<Event>();
            GlobalVariables.elapsedEvents = new List<Event>();

            FileHandler fileHandler = new FileHandler();
            try {
                fileHandler.readStoredEventsFile();
                fileHandler.readMyEventsFile();
            } catch (Exception ex) {
                Debug.WriteLine("Exception Caught!\n" + ex.Message + ex.StackTrace);
            }
            //GlobalVariables.elapsedEvents.Add(GlobalVariables.storedEvents[0]); //test
            //GlobalVariables.elapsedEvents.Add(GlobalVariables.storedEvents[1]); //test
        }

        public void addNewEvent(object sender, EventArgs e) {
            Navigation.PushModalAsync(new AddNewEventPage());
        }

        public void manageEvents(object sender, EventArgs e) {
            Navigation.PushModalAsync(new ManageEventsPage());
        }

        public int getUtilPaddingHeight() {
            return utilPaddingHeight;
        }

        public async void animateOpening() {
            //set utilities padding size based on device
            switch (Device.RuntimePlatform) {
                case Device.iOS:
                    utilPaddingHeight = 20;
                    break;
                default:
                    utilPaddingHeight = 0;
                    break;
            }
            utilitiesPadding.HeightRequest = utilPaddingHeight;

            //fade out blue boxview and move CC logo to left side and shrink
            Content = animationStack;
            await animationStack.FadeTo(1.0, 1000);

            //create single cell grid and add background logo
            Grid logoGrid = new Grid {
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            logoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(153, GridUnitType.Star) });
            logoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(728, GridUnitType.Star) });
            Device.BeginInvokeOnMainThread(() => {
                this.BackgroundColor = Color.Black;
                animationStack.Children.Add(logoGrid);
            });
            double imageHeight = (405.0 / 3524.0) * (this.Width);
            BoxView logoBackground = new BoxView() {
                BackgroundColor = Color.FromRgb(0.0, 0.0, 220.0 / 255.0),
                Opacity = 0.0,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                HeightRequest = imageHeight
            };
            Device.BeginInvokeOnMainThread(() => {
                logoGrid.Children.Add(logoBackground, 0, 2, 0, 1);
            });

            //add logo image to grid and fade in
            LogoWithName.Opacity = 0.0;
            Device.BeginInvokeOnMainThread(() => {
                logoGrid.Children.Add(LogoWithName, 0, 2, 0, 1);
            });
            double logoGridTranslateLen = Bounds.Height / 2.16;
            await logoGrid.TranslateTo(0.0, logoGridTranslateLen, 1);
            await LogoWithName.FadeTo(1.0, 1600);

            //move logo and background to top together and briefly pause
            double len = 35.0;
            double logoGridTranslated = (-this.Height + LogoWithName.Height) / 2.0;
            for (int i = 1; i <= len; i++) {
                Device.BeginInvokeOnMainThread(() => {
                    logoBackground.Opacity = i / len;
                });
                await logoGrid.TranslateTo(0.0, ((len - i) / len) * logoGridTranslateLen + (i / len) * utilPaddingHeight + 5, 1);
            }
            await logoGrid.FadeTo(1.0, 1);

            //set page's content back to original stack
            Device.BeginInvokeOnMainThread(() => {
                animationStack.Children.Insert(0, mainStack);
            });
            Device.BeginInvokeOnMainThread(async () => {
                animationStack.Children.RemoveAt(1);
                Device.BeginInvokeOnMainThread(async () => {
                    await utilitiesPadding.FadeTo(1.0, 1000);
                });
                await gridStack.FadeTo(1.0, 1000);
            });
        }

        public void animateElapsedEvents() {
            Device.BeginInvokeOnMainThread(async () => {
                //define new stack layout for animation
                StackLayout animationStack = new StackLayout();
                animationStack.BackgroundColor = Color.Gold;
                animationStack.Opacity = 0.0;

                //set page's content to animation
                Content = animationStack;

                //fade in stack layout
                await animationStack.FadeTo(1.0, 1500);

                for (int i = 0; i < GlobalVariables.elapsedEvents.Count; i++) {
                    //create elapsed event label
                    Label eventLabel = new Label();
                    eventLabel.Text = GlobalVariables.elapsedEvents[i].getName();
                    eventLabel.FontSize = 40;
                    eventLabel.TextColor = Color.White;
                    eventLabel.FontAttributes = FontAttributes.Bold;
                    eventLabel.HorizontalTextAlignment = TextAlignment.Center;
                    eventLabel.HorizontalOptions = LayoutOptions.FillAndExpand;
                    eventLabel.VerticalOptions = LayoutOptions.StartAndExpand;

                    //create elapsed event date label
                    Label dateLabel = new Label();
                    dateLabel.Text = "(" + GlobalVariables.elapsedEvents[i].getDate().Month +
                                     "/" + GlobalVariables.elapsedEvents[i].getDate().Day +
                                     "/" + GlobalVariables.elapsedEvents[i].getDate().Year + ")";
                    dateLabel.FontSize = 20;
                    dateLabel.TextColor = Color.White;
                    dateLabel.FontAttributes = FontAttributes.Bold;
                    dateLabel.HorizontalTextAlignment = TextAlignment.Center;
                    dateLabel.HorizontalOptions = LayoutOptions.FillAndExpand;
                    dateLabel.VerticalOptions = LayoutOptions.EndAndExpand;

                    //add event name and date to event stack 
                    StackLayout eventStack = new StackLayout();
                    eventStack.BackgroundColor = Color.Blue;
                    //eventStack.Opacity = 0;
                    eventStack.Children.Add(eventLabel);
                    eventStack.Children.Add(dateLabel);
                    await eventStack.TranslateTo(0, this.Height / 2.0, 1);

                    //add event stack to animation
                    animationStack.Children.Add(eventStack);

                    //move event stack from center page to top 1/8, fading in
                    double y = this.Height / 2.0;
                    while (y > this.Height / 8.0) {
                        eventStack.Opacity = (this.Height / 2.0 - y) / (this.Height / 2.0 - this.Height / 8.0);
                        await eventStack.TranslateTo(0, y, 1);
                        y -= 20;
                    }

                    //pause
                    await eventStack.FadeTo(1.0, 500);

                    //fade out event stack
                    await eventStack.FadeTo(0.0, 500);

                    //remove event stack from animation stack
                    animationStack.Children.Clear();

                    Debug.WriteLine("Animated " + eventLabel.Text);
                }

                //set page's content back to original stack
                Content = mainStack;
            });
        }

        public void populateGrid() {
            //clear grid
            grid2.Children.Clear();

            //sort events based on countdown
            Thread t_sortEvents = new Thread(() => {
                Thread.CurrentThread.IsBackground = true;
                GlobalVariables.myEvents.Sort((x, y) => x.getDate().CompareTo(y.getDate()));
                GlobalVariables.storedEvents.Sort((x, y) => x.getDate().CompareTo(y.getDate()));
            });
            t_sortEvents.Start();

            //add events to grid
            int row = 0;
            int myEventsIndex = 0;
            int storedEventsIndex = 0;
            Event currentEvent;

            t_sortEvents.Join();

            while (myEventsIndex < GlobalVariables.myEvents.Count || storedEventsIndex < GlobalVariables.storedEvents.Count) {
                currentEvent = null;
                if (myEventsIndex < GlobalVariables.myEvents.Count) {
                    if (storedEventsIndex < GlobalVariables.storedEvents.Count) {
                        if (GlobalVariables.myEvents[myEventsIndex].getDate() < GlobalVariables.storedEvents[storedEventsIndex].getDate()) {
                            currentEvent = GlobalVariables.myEvents[myEventsIndex++];
                        }
                        else {
                            currentEvent = GlobalVariables.storedEvents[storedEventsIndex++];
                        }
                    }
                    else {
                        currentEvent = GlobalVariables.myEvents[myEventsIndex++];
                    }
                }
                else {
                    currentEvent = GlobalVariables.storedEvents[storedEventsIndex++];
                }
                if (currentEvent.isNotVisible()) {
                    continue;
                }
                addEventToGrid(currentEvent, row++);
            }
        }

        public void addEventToGrid(Event currentEvent, int row) {
            Debug.WriteLine("Adding " + currentEvent.getName() + " to grid at " + row);
            DateTime date = currentEvent.getDate();
            double value = Math.Min(1, ((date - DateTime.Now).TotalHours) / 8760.0); //for color shade
            int valueLabelSize = 27;

            Label eventLabel = new Label();
            eventLabel.FontSize = 12;
            eventLabel.Text = currentEvent.getName();
            eventLabel.HorizontalTextAlignment = TextAlignment.Center;
            eventLabel.HorizontalOptions = LayoutOptions.CenterAndExpand;
            eventLabel.FontAttributes = FontAttributes.Bold;
            eventLabel.TextColor = Color.FromRgb(1.0, 1.0, (60.0 / 255.0) * (value));

            Label dateLabel = new Label();
            dateLabel.FontSize = 10;
            dateLabel.Text = "(" + currentEvent.getDate().Month + "/" + currentEvent.getDate().Day + "/" + currentEvent.getDate().Year + ")";
            dateLabel.HorizontalOptions = LayoutOptions.CenterAndExpand;
            dateLabel.FontAttributes = FontAttributes.Bold;
            dateLabel.TextColor = Color.White;

            Label numDays = new Label();
            numDays.FontSize = valueLabelSize;
            numDays.SetBinding(Label.TextProperty, "DaysUntil");
            numDays.BindingContext = currentEvent;
            numDays.TextColor = Color.White;
            numDays.HorizontalOptions = LayoutOptions.CenterAndExpand;
            numDays.FontFamily = Device.RuntimePlatform == Device.iOS ? "alarm clock" : "Assets/Fonts/alarm clock.ttf#alarm clock";

            Label numHours = new Label();
            numHours.FontSize = valueLabelSize;
            numHours.SetBinding(Label.TextProperty, "HoursUntil");
            numHours.BindingContext = currentEvent;
            numHours.TextColor = Color.White;
            numHours.HorizontalOptions = LayoutOptions.CenterAndExpand;
            numHours.FontFamily = Device.RuntimePlatform == Device.iOS ? "alarm clock" : "Assets/Fonts/alarm clock.ttf#alarm clock";

            Label numMinutes = new Label();
            numMinutes.FontSize = valueLabelSize;
            numMinutes.SetBinding(Label.TextProperty, "MinutesUntil");
            numMinutes.BindingContext = currentEvent;
            numMinutes.TextColor = Color.White;
            numMinutes.HorizontalOptions = LayoutOptions.CenterAndExpand;
            numMinutes.FontFamily = Device.RuntimePlatform == Device.iOS ? "alarm clock" : "Assets/Fonts/alarm clock.ttf#alarm clock";

            Label numSeconds = new Label();
            numSeconds.FontSize = valueLabelSize;
            numSeconds.SetBinding(Label.TextProperty, "SecondsUntil");
            numSeconds.BindingContext = currentEvent;
            numSeconds.TextColor = Color.White;
            numSeconds.HorizontalOptions = LayoutOptions.CenterAndExpand;
            numSeconds.FontFamily = Device.RuntimePlatform == Device.iOS ? "alarm clock" : "Assets/Fonts/alarm clock.ttf#alarm clock";

            //add grid's back color
            BoxView backColor = new BoxView();
            backColor.Color = Color.FromRgb(0.0, 0.0, 220.0 / 255.0 - (220.0 / 255.0) * value);

            BoxView yearFraction = new BoxView();
            yearFraction.Color = Color.FromRgb(1.0, 1.0, (60.0 / 255.0) * (value));
            yearFraction.TranslationX = -(this.Width) + (this.Width) * fractionOfYear(currentEvent);
            yearFraction.HeightRequest = 3;
            yearFraction.VerticalOptions = LayoutOptions.End;

            //set yellow border when event day is today
            if (DateTime.Now.Day == currentEvent.getDate().Day && DateTime.Now.Month == currentEvent.getDate().Month) {
                BoxView backColor2 = new BoxView();
                backColor2.WidthRequest = grid2.Width * fractionOfYear(currentEvent);
                backColor2.Color = Color.FromRgb(1.0, 1.0, 0.0);
                grid2.Children.Add(backColor2, 0, 5, row, row + 1);
                backColor.Margin = 3;
            }

            //combine event and date labels into stacklayout
            StackLayout stack = new StackLayout();
            stack.Padding = new Thickness(0, 0, 0, 3);
            stack.Children.Add(eventLabel);
            stack.Children.Add(dateLabel);

            grid2.Children.Add(backColor, 0, 5, row, row + 1);
            grid2.Children.Add(yearFraction, 0, 5, row, row + 1);
            grid2.Children.Add(stack, 0, row);
            grid2.Children.Add(numDays, 1, row);
            grid2.Children.Add(numHours, 2, row);
            grid2.Children.Add(numMinutes, 3, row);
            grid2.Children.Add(numSeconds, 4, row);
        }

        public double fractionOfYear(Event currentEvent) {
            return (365.0 - currentEvent.DaysUntil) / 365.0;
        }
    }
}