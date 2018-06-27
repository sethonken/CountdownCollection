using Foundation;
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
        public int eventFontSize;
        public int dateFontSize;
        public int countdownFontSize;

        public bool fadingGrid;
        public bool populatingGrid;
        public bool animating;
        DateTime currentTime;

        ActivityIndicator newDayIndicator;

        public System.Timers.Timer objTimer;

        public MainPage() {
            InitializeComponent();
            initializeStoredEvents();
            initializeMyEvents();

            App.Current.MainPage = new NavigationPage();
            Content = animationStack;
            lastDay = -1;
            eventFontSize = 13;
            dateFontSize = 11;
            countdownFontSize = 23;
            fadingGrid = false;
            populatingGrid = true;
            animating = true;
            
            //initialize indicator for a new day refresh
            newDayIndicator = new ActivityIndicator();
            newDayIndicator.HorizontalOptions = LayoutOptions.FillAndExpand;
            newDayIndicator.VerticalOptions = LayoutOptions.Center;
            newDayIndicator.Color = Color.White;
            newDayIndicator.BackgroundColor = Color.Black;
            newDayIndicator.IsEnabled = true;
            newDayIndicator.IsRunning = true;

            //start opening animation
            System.Threading.ThreadStart openingAnimationStart = animateOpening;
            System.Threading.Thread openingAnimationThread = new System.Threading.Thread(openingAnimationStart);
            openingAnimationThread.IsBackground = true;
            openingAnimationThread.Start();

            //timer
            objTimer = new System.Timers.Timer();
            objTimer.Interval = 500;
            objTimer.Elapsed += ObjTimer_Elapsed;
            objTimer.Start();
        }

        public void ObjTimer_Elapsed(object sender, ElapsedEventArgs e) {
            refreshGrid();
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 59) {
                //fade grid back in
                System.Threading.ThreadStart t_Start = fadeGrid;
                System.Threading.Thread t = new System.Threading.Thread(t_Start);
                t.IsBackground = true;
                t.Start();
            }
            newDayRefresh();
        }

        public async void newDayRefresh() {
            bool setToGrid = false;
            bool populate = false;
            if (lastDay == -1) {
                setToGrid = true;
                populate = true;
            }
            currentDay = DateTime.Now.Day;
            if (lastDay != currentDay) {
                lastDay = currentDay;
                FileHandler fileHandler = new FileHandler();

                //set current date text
                Device.BeginInvokeOnMainThread(() => {
                    currentDate.Text = DateTime.Now.ToString("D");
                });

                //new day, change any events that were today to next year
                for (int i = 0; i < GlobalVariables.myEvents.Count; i++) {
                    if (GlobalVariables.myEvents[i].getDaysUntil() < 0) {
                        if (GlobalVariables.myEvents[i].isVisible()) {
                            populate = true;
                        }
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
                    if (GlobalVariables.storedEvents[i].getDaysUntil() < 0) {
                        if (GlobalVariables.storedEvents[i].isVisible()) {
                            populate = true;
                        }
                        GlobalVariables.resetEvent(GlobalVariables.storedEvents[i]);
                    }
                    else {
                        break;
                    }
                }

                //check if a one time event has lapsed since last app open
                for (int i = 0; i < GlobalVariables.myEvents.Count; i++) {
                    if (GlobalVariables.myEvents[i].isOneTimeEvent() && eventHasLapsed(GlobalVariables.myEvents[i])) {
                        GlobalVariables.myEvents.RemoveAt(i--);
                        continue;
                    }
                }

                //see if repopulating is necessary {
                if (!populate) {
                    for (int i = 0; i < GlobalVariables.myEvents.Count; i++) {
                        if (GlobalVariables.myEvents[i].getDate().DayOfYear < DateTime.Now.DayOfYear) {
                            continue;
                        }
                        if (GlobalVariables.myEvents[i].getDate().DayOfYear == DateTime.Now.DayOfYear && GlobalVariables.myEvents[i].getDate().Year == DateTime.Now.Year) {
                            if (GlobalVariables.myEvents[i].isVisible()) {
                                populate = true;
                                break;
                            }
                        }
                        else {
                            break;
                        }
                    }
                    for (int i = 0; i < GlobalVariables.storedEvents.Count; i++) {
                        if (GlobalVariables.storedEvents[i].getDate().DayOfYear < DateTime.Now.DayOfYear) {
                            continue;
                        }
                        if (GlobalVariables.storedEvents[i].getDate().DayOfYear == DateTime.Now.DayOfYear && GlobalVariables.storedEvents[i].getDate().Year == DateTime.Now.Year) {
                            if (GlobalVariables.storedEvents[i].isVisible()) {
                                populate = true;
                                break;
                            }
                        }
                        else {
                            break;
                        }
                    }
                }

                await Task.Run(() => {
                    try {
                        fileHandler.updateMyEventsFile();
                    } catch (Exception ex) {
                        Debug.WriteLine("Exception trying to update my events file:\n" + ex.Message);
                    }
                });
                refreshGrid();
                Device.BeginInvokeOnMainThread(() => {
                    if (populate) {
                        populateGrid();
                    }
                });

                while (populatingGrid) {
                    ;
                }
                //set page's content back to original stack
                if (setToGrid) {
                    while (animating) {
                        ;
                    }
                    Thread.Sleep(100);
                    Device.BeginInvokeOnMainThread(() => {
                        Content = animationStack;
                        animationStack.Children.Insert(0, mainStack);
                        Device.BeginInvokeOnMainThread(async () => {
                            animationStack.Children.RemoveAt(1);
                            uint fadeTime = 800;
                            Device.BeginInvokeOnMainThread(async () => {
                                await utilitiesPadding.FadeTo(1.0, fadeTime);
                            });
                            Device.BeginInvokeOnMainThread(async () => {
                                await Buttons.FadeTo(1.0, fadeTime);
                            });
                            await gridStack.FadeTo(1.0, fadeTime);
                        });
                    });
                }
                else {
                    Device.BeginInvokeOnMainThread(async () => {
                        await gridStack.FadeTo(1.0, 800);
                    });
                }
            }
        }

        public void displayActivityIndicator() {
            var utilPadding = new Label {
                BackgroundColor = Color.White,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                HeightRequest = getUtilPaddingHeight(),
                Opacity = 0
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

            Device.BeginInvokeOnMainThread(() => {
                Content = root;
            });
        }

        bool eventHasLapsed(Event oldEvent) {
            DateTime storedDate = new DateTime(GlobalVariables.lastRecordedYear, GlobalVariables.lastRecordedMonth, GlobalVariables.lastRecordedDay, 0, 0, 0);

            if ((oldEvent.getDate() - storedDate).Seconds < 0) {
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

        public void initializeStoredEvents() {
            GlobalVariables.storedEvents = new List<Event>();

            FileHandler fileHandler = new FileHandler();
            fileHandler.readStoredEventsFile();
        }

        public void initializeMyEvents() {
            GlobalVariables.myEvents = new List<Event>();

            FileHandler fileHandler = new FileHandler();
            fileHandler.readMyEventsFile();
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

        public void fadeGrid() {
            if (fadingGrid) {
                return;
            }
            fadingGrid = true;

            Device.BeginInvokeOnMainThread(async () => {
                await gridStack.FadeTo(0.0, 500);
                fadingGrid = false;
            });
        }

        public async void animateOpening() {
            //set utilities padding size based on device
            switch (Device.RuntimePlatform) {
                case Device.iOS:
                    unitLabel.Text = String.Format("{0,8}{1,11}{2,10}{3,8}", "Days", "Hrs", "Mins", "Secs");
                    utilPaddingHeight = 20;
                    //Debug.WriteLine("Device: " + NSProcessInfo.ProcessInfo.Environment["SIMULATOR_MODEL_IDENTIFIER"]);
                    //switch (NSProcessInfo.ProcessInfo.Environment["SIMULATOR_MODEL_IDENTIFIER"].ToString()) {
                    //    case "iPhone6,1":
                    //    case "iPhone8,4":
                    //        dateFontSize = 10;
                    //        countdownFontSize = 19;
                    //        eventFontSize = 11;
                    //        unitLabel.Text = String.Format("{0,7}{1,8}{2,7}{3,6}", "Days", "Hrs", "Mins", "Secs");
                    //        utilPaddingHeight = 20;
                    //        break;
                    //    case "iPhone10,3":
                    //        unitLabel.Text = String.Format("{0,8}{1,11}{2,10}{3,8}", "Days", "Hrs", "Mins", "Secs");
                    //        utilPaddingHeight = 40;
                    //        break;
                    //    default:
                    //        unitLabel.Text = String.Format("{0,8}{1,11}{2,10}{3,8}", "Days", "Hrs", "Mins", "Secs");
                    //        utilPaddingHeight = 20;
                    //        break;
                    //}
                    break;
                default:
                    unitLabel.Text = String.Format("{0,8}{1,10}{2,10}{3,10}", "Days", "Hrs", "Mins", "Secs");
                    utilPaddingHeight = 0;
                    break;
            }
            utilitiesPadding.HeightRequest = utilPaddingHeight;

            //fade out blue boxview
            Device.BeginInvokeOnMainThread(async () => {
                await animationStack.FadeTo(1.0, 1);
            });

            //create single cell grid and add background logo
            Grid logoGrid = new Grid {
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            Device.BeginInvokeOnMainThread(() => {
                this.BackgroundColor = Color.Black;
                animationStack.Children.Add(logoGrid);
            });

            tryagain:
            try {
                while (Bounds.Width <= 0) {
                    ;
                }
            } catch (Exception ex) {
                goto tryagain;
            }
            double imageHeight = (405.0 / 3524.0) * (Bounds.Width);
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
            while (Bounds == null || Bounds.Height <= 0) {
                ;
            }
            double logoGridTranslateLen = Bounds.Height / 2.16;
            Device.BeginInvokeOnMainThread(() => {
                logoGrid.TranslateTo(0.0, logoGridTranslateLen, 1);
                Device.BeginInvokeOnMainThread(async () => {
                    await LogoWithName.FadeTo(0.0, 800);
                    await LogoWithName.FadeTo(0.99, 600);
                    await LogoWithName.FadeTo(1.0, 500);
                });
            });

            //move logo and background to top together and briefly pause
            await Task.Run(() => {
                double len = 30.0; //segments in transition to top
                while (LogoWithName.Opacity != 1) {
                    ;
                }
                Device.BeginInvokeOnMainThread(async () => {
                    int i = 1;
                    while (i <= len) {
                        await logoBackground.FadeTo(i / len, 1);
                        Device.BeginInvokeOnMainThread(async () => {
                            if (i <= len) {
                                await logoGrid.TranslateTo(0.0, ((len - i) / len) * logoGridTranslateLen + (i / len) * (utilPaddingHeight) + (i / len) * 5, 1);
                                Device.BeginInvokeOnMainThread(() => {
                                    i++;
                                });
                            }
                        });
                    }
                });
                while (logoBackground.Opacity != 1) {
                    ;
                }
                Thread.Sleep(35);
                displayActivityIndicator();
                animating = false;
            });
        }

        public void print(string s) {
            Debug.WriteLine(s);
        }

        public void populateGrid() {
            populatingGrid = true;

            //clear grid
            grid2.Children.Clear();

            //sort events based on countdown
            GlobalVariables.myEvents.Sort((x, y) => x.getDate().CompareTo(y.getDate()));
            GlobalVariables.storedEvents.Sort((x, y) => x.getDate().CompareTo(y.getDate()));

            //add events to grid
            int row = 0;
            int myEventsIndex = 0;
            int storedEventsIndex = 0;
            Event currentEvent;

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
                //Debug.WriteLine("Added " + currentEvent.getName() + " to grid");
            }

            populatingGrid = false;
        }

        public void addEventToGrid(Event currentEvent, int row) {
            //Debug.WriteLine("Adding " + currentEvent.getName() + " to grid");

            DateTime date = currentEvent.getDate();
            double value = Math.Min(1, ((date - DateTime.Now).TotalHours) / 8760.0); //for color shade
            int botPad = 6;
            int topPad = 4;

            //components of row
            Label eventLabel = new Label();
            Label dateLabel = new Label();
            Label countdown = new Label();
            BoxView backColor = new BoxView();
            BoxView yearFraction = new BoxView();
            BoxView backColor2 = new BoxView();
            StackLayout stack = new StackLayout();

            eventLabel.FontSize = eventFontSize;
            eventLabel.Text = currentEvent.getName();
            eventLabel.HorizontalTextAlignment = TextAlignment.Center;
            eventLabel.LineBreakMode = LineBreakMode.TailTruncation;
            eventLabel.HorizontalOptions = LayoutOptions.CenterAndExpand;
            eventLabel.FontAttributes = FontAttributes.Bold;
            eventLabel.TextColor = Color.FromRgb(1.0, 1.0, (60.0 / 255.0) * (value));

            dateLabel.FontSize = dateFontSize;
            dateLabel.Text = currentEvent.getDate().ToString("D");
            dateLabel.LineBreakMode = LineBreakMode.TailTruncation;
            dateLabel.HorizontalOptions = LayoutOptions.CenterAndExpand;
            dateLabel.FontAttributes = FontAttributes.Bold;
            dateLabel.TextColor = Color.White;

            countdown.SetBinding(Label.TextProperty, "Countdown");
            countdown.BindingContext = currentEvent;
            countdown.FontSize = countdownFontSize;
            countdown.TextColor = Color.White;
            countdown.HorizontalOptions = LayoutOptions.End;
            countdown.VerticalOptions = LayoutOptions.Center;
            countdown.Margin = new Thickness(0, 0, 5, 0);
            countdown.FontFamily = Device.RuntimePlatform == Device.iOS ? "alarm clock" : "Assets/Fonts/alarm clock.ttf#alarm clock";

            backColor.Color = Color.FromRgb(0.0, 0.0, 220.0 / 255.0 - (220.0 / 255.0) * value);

            yearFraction.Color = Color.FromRgb(1.0, 1.0, (60.0 / 255.0) * (value));
            yearFraction.TranslationX = -(this.Width) + (this.Width) * fractionOfYear(currentEvent);
            yearFraction.HeightRequest = 2;
            yearFraction.VerticalOptions = LayoutOptions.End;

            //set yellow border when event day is today
            if (DateTime.Now.Day == currentEvent.getDate().Day && DateTime.Now.Month == currentEvent.getDate().Month && DateTime.Now.Year == currentEvent.getDate().Year) {
                backColor2.WidthRequest = grid2.Width * fractionOfYear(currentEvent);
                backColor2.Color = Color.FromRgb(1.0, 1.0, 0.0);
                grid2.Children.Add(backColor2, 0, 2, row, row + 1);
                backColor.Margin = new Thickness(2, 2, 2, 0);
                botPad = 0;
                topPad = 0;
            }

            //combine event and date labels into stacklayout
            stack.Padding = new Thickness(0, topPad, 0, botPad);
            stack.Spacing = 2;
            stack.VerticalOptions = LayoutOptions.Center;
            stack.Children.Add(eventLabel);
            stack.Children.Add(dateLabel);

            grid2.Children.Add(backColor, 0, 2, row, row + 1);
            grid2.Children.Add(yearFraction, 0, 2, row, row + 1);
            grid2.Children.Add(stack, 0, row);
            grid2.Children.Add(countdown, 1, row);

            //Debug.WriteLine(currentEvent.getName() + " added to grid");
        }

        public double fractionOfYear(Event currentEvent) {
            return (365.0 - currentEvent.getDaysUntil()) / 365.0;
        }
    }
}