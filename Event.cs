using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Xamarin.Forms;

namespace CountdownCollection {
    public class Event : INotifyPropertyChanged {
        string name;
        DateTime date;
        bool visible;
        bool oneTimeEvent;
        bool allDayEvent;

        //row components
        int botPad;
        int topPad;
        int row;
        Label eventLabel;
        Label dateLabel;
        Label countdown;
        BoxView backColor;
        BoxView yearFraction;
        BoxView backColor2;
        StackLayout stack;

        int daysUntil;
        int hoursUntil;
        int minutesUntil;
        int secondsUntil;

        string _countdown;

        public Event(string name, DateTime date) {
            init(name, date);
        }

        public Event(string name, DateTime date, bool visible) {
            init(name, date);
            this.visible = visible;
        }

        public void init(String name, DateTime date) {
            this.name = name;
            this.date = date;
            visible = true;
            oneTimeEvent = false;

            row = 0;
            eventLabel = new Label();
            dateLabel = new Label();
            countdown = new Label();
            backColor = new BoxView();
            yearFraction = new BoxView();
            backColor2 = new BoxView();
            stack = new StackLayout();
        }

        public void initEventLabel() {
            double value = Math.Min(1, ((getDate() - DateTime.Now).TotalHours) / 8760.0); //for color shade

            Device.BeginInvokeOnMainThread(() => {
                eventLabel.FontSize = GlobalVariables.eventFontSize;
                eventLabel.FontAttributes = FontAttributes.Bold;
                eventLabel.TextColor = Color.FromRgb(1.0, 1.0, (60.0 / 255.0) * (value));
            });
            eventLabel.Text = getName();
            eventLabel.HorizontalTextAlignment = TextAlignment.Center;
            eventLabel.LineBreakMode = LineBreakMode.TailTruncation;
            eventLabel.HorizontalOptions = LayoutOptions.CenterAndExpand;
            eventLabel.VerticalOptions = LayoutOptions.Start;
        }

        public void initDateLabel() {
            Device.BeginInvokeOnMainThread(() => {
                dateLabel.FontSize = GlobalVariables.dateFontSize;
                dateLabel.FontAttributes = FontAttributes.Bold;
            });
            dateLabel.Text = getDate().ToString("D");
            dateLabel.LineBreakMode = LineBreakMode.TailTruncation;
            dateLabel.HorizontalOptions = LayoutOptions.CenterAndExpand;
            dateLabel.TextColor = Color.White;
            dateLabel.VerticalOptions = LayoutOptions.End;
        }

        public void initCountdown() {
            Device.BeginInvokeOnMainThread(() => {
                countdown.FontSize = GlobalVariables.countdownFontSize;
                countdown.FontFamily = Device.RuntimePlatform == Device.iOS ? "alarm clock" : "Assets/Fonts/alarm clock.ttf#alarm clock";
            });
            countdown.SetBinding(Label.TextProperty, "Countdown");
            countdown.BindingContext = this;
            countdown.TextColor = Color.White;
            countdown.HorizontalOptions = LayoutOptions.End;
            countdown.VerticalOptions = LayoutOptions.Center;
            countdown.Margin = new Thickness(0, 0, 5, 0);
        }

        public void initBackground() {
            CountdownCollection.MainPage mainPage = (MainPage)App.Current.MainPage;
            double value = Math.Min(1, ((getDate() - DateTime.Now).TotalHours) / 8760.0); //for color shade

            backColor.Color = Color.FromRgb(0.0, 0.0, 220.0 / 255.0 - (220.0 / 255.0) * value);

            yearFraction.Color = Color.FromRgb(1.0, 1.0, (60.0 / 255.0) * (value));
            yearFraction.TranslationX = -(mainPage.Width) + (mainPage.Width) * fractionOfYear();
            yearFraction.HeightRequest = 2;
            yearFraction.VerticalOptions = LayoutOptions.End;

            //set yellow border when event day is today
            if (DateTime.Now.Day == getDate().Day && DateTime.Now.Month == getDate().Month && DateTime.Now.Year == getDate().Year) {
                backColor2.WidthRequest = GlobalVariables.grid2.Width * fractionOfYear();
                backColor2.Color = Color.FromRgb(1.0, 1.0, 0.0);
                backColor2.IsVisible = true;

                backColor.Margin = new Thickness(2, 2, 2, 2);
                botPad = 2;
                topPad = 0;
            }
            else {
                backColor2.IsVisible = false;
            }
        }

        public void refreshBackground() {
            CountdownCollection.MainPage mainPage = (MainPage)App.Current.MainPage;
            double value = Math.Min(1, ((getDate() - DateTime.Now).TotalHours) / 8760.0); //for color shade

            Device.BeginInvokeOnMainThread(() => {
                eventLabel.TextColor = Color.FromRgb(1.0, 1.0, (60.0 / 255.0) * (value));
                dateLabel.Text = getDate().ToString("D");
                backColor.Color = Color.FromRgb(0.0, 0.0, 220.0 / 255.0 - (220.0 / 255.0) * value);
                yearFraction.Color = Color.FromRgb(1.0, 1.0, (60.0 / 255.0) * (value));
                yearFraction.TranslationX = -(mainPage.Width) + (mainPage.Width) * fractionOfYear();
            });

            //set yellow border when event day is today
            if (DateTime.Now.Day == getDate().Day && DateTime.Now.Month == getDate().Month && DateTime.Now.Year == getDate().Year) {
                Device.BeginInvokeOnMainThread(() => {
                    backColor2.WidthRequest = GlobalVariables.grid2.Width * fractionOfYear();
                    backColor2.Color = Color.FromRgb(1.0, 1.0, 0.0);
                    backColor2.IsVisible = true;
                    backColor.Margin = new Thickness(2, 2, 2, 2);
                });

                botPad = 2;
                topPad = 0;
            }
            else {
                Device.BeginInvokeOnMainThread(() => {
                    backColor2.IsVisible = false;
                    backColor.Margin = new Thickness(0, 0, 0, 0);
                });

                botPad = 8;
                topPad = 6;
            }

            Device.BeginInvokeOnMainThread(() => {
                stack.Padding = new Thickness(0, topPad, 0, botPad);
            });
        }

        public void addComponentsToGrid() {
            Grid.SetRow(backColor2, row);
            Grid.SetColumnSpan(backColor2, 2);
            GlobalVariables.grid2.Children.Add(backColor2);

            Grid.SetRow(backColor, row);
            Grid.SetColumnSpan(backColor, 2);
            GlobalVariables.grid2.Children.Add(backColor);

            Grid.SetRow(yearFraction, row);
            Grid.SetColumnSpan(yearFraction, 2);
            GlobalVariables.grid2.Children.Add(yearFraction);

            Grid.SetRow(stack, row);
            Grid.SetColumn(stack, 0);
            GlobalVariables.grid2.Children.Add(stack);

            Grid.SetRow(countdown, row);
            Grid.SetColumn(countdown, 1);
            GlobalVariables.grid2.Children.Add(countdown);
        }

        public void addEventToGrid() {
            botPad = 8;
            topPad = 6;

            initEventLabel();
            initDateLabel();
            initCountdown();
            initBackground();

            //combine event and date labels into stacklayout
            stack.Padding = new Thickness(0, topPad, 0, botPad);
            stack.Spacing = 1;
            stack.VerticalOptions = LayoutOptions.Center;
            stack.Children.Add(eventLabel);
            stack.Children.Add(dateLabel);

            addComponentsToGrid();
        }

        public double fractionOfYear() {
            if (getDaysUntil() < 0) {
                refreshTimeUntil(DateTime.Now);
            }
            return (365.0 - getDaysUntil()) / 365.0;
        }

        public void setRow(int row) {
            this.row = row;
        }

        public void refreshRow() {
            while (getDaysUntil() <= 0) {
                refreshTimeUntil(DateTime.Now);
            }

            int rowCount;

            var children = GlobalVariables.grid2.Children.ToList();

            for (rowCount = 0; rowCount < GlobalVariables.grid2.Children.Count / 5; rowCount++) {
                Label label = (Label)GlobalVariables.grid2.Children[GlobalVariables.grid2_realChildLocations[rowCount]];
                int day = Convert.ToInt32(label.Text.Substring(0, 4));
                int hour = Convert.ToInt32(label.Text.Substring(5, 2));
                int minute = Convert.ToInt32(label.Text.Substring(8, 2));

                bool insertHere = false;
                if (daysUntil < day) {
                    insertHere = true;
                }
                else if (daysUntil == day) {
                    if (hoursUntil < hour) {
                        insertHere = true;
                    }
                    else if (hoursUntil == hour) {
                        if (minutesUntil < minute) {
                            insertHere = true;
                        }
                    }
                }

                if (insertHere) {
                    break;
                }
            }

            //move all events at insert point down by 1 row
            Device.BeginInvokeOnMainThread(() => {
                foreach (var c in children.Where(c => Grid.GetRow(c) >= rowCount)) {
                    Grid.SetRow(c, Grid.GetRow(c) + 1);
                }
            });

            //move event to proper row
            Device.BeginInvokeOnMainThread(() => {
                foreach (var c in children.Where(c => Grid.GetRow(c) == 0)) {
                    Grid.SetRow(c, rowCount);
                }
            });

            //move all rows up by 1
            Device.BeginInvokeOnMainThread(() => {
                foreach (var child in children) {
                    Grid.SetRow(child, Grid.GetRow(child) - 1);
                }
            });

            //clean up
            GlobalVariables.grid2_realChildLocations.Insert(rowCount, GlobalVariables.grid2_realChildLocations[0]);
            GlobalVariables.grid2_realChildLocations.RemoveAt(0);
        }


        public void removeFromGrid() {
            var children = GlobalVariables.grid2.Children.ToList();
            Device.BeginInvokeOnMainThread(() => {
                foreach (var child in children.Where(child => Grid.GetRow(child) == 0)) {
                    GlobalVariables.grid2.Children.Remove(child);
                }
            });
            Device.BeginInvokeOnMainThread(() => {
                foreach (var child in children.Where(child => Grid.GetRow(child) > 0)) {
                    Grid.SetRow(child, Grid.GetRow(child) - 1);
                }
            });
        }

        public void DeleteButton_Clicked(object sender, EventArgs e) {
            GlobalVariables.eventToBeDeleted = getName();
        }

        public void VisibleSwitch_Toggled(object sender, ToggledEventArgs e) {
            visible = e.Value;
        }

        public void setOneTimeEvent(bool value) {
            oneTimeEvent = value;
        }

        public bool isOneTimeEvent() {
            return oneTimeEvent;
        }

        public void setAllDayEvent(bool allDayEvent) {
            this.allDayEvent = allDayEvent;
        }

        public bool isAllDayEvent() {
            return allDayEvent;
        }

        public void setVisible() {
            visible = true;
        }

        public void setInvisible() {
            visible = false;
        }

        public bool isVisible() {
            return visible;
        }

        public bool isNotVisible() {
            return !visible;
        }

        public string getName() {
            return name;
        }

        public void setName(string name) {
            this.name = name;
        }

        public DateTime getDate() {
            return date;
        }

        public void setDate(DateTime date) {
            this.date = date;
        }

        public int getDaysUntil() {
            return daysUntil;
        }

        public string countdownToString() {
            return String.Format("{0,4}{4,1}{1,2}{4,1}{2,2}{4,1}{3,2}", Math.Max(0, daysUntil), Math.Max(0, hoursUntil), Math.Max(0, minutesUntil), Math.Max(0, secondsUntil), "");
        }

        public void refreshTimeUntil(DateTime currentTime) {
            //set all times to 0 when event day is today (and time is past start time)
            if (currentTime.Day == date.Day && currentTime.Month == date.Month && currentTime.Year == date.Year) {
                if (currentTime.TimeOfDay > date.TimeOfDay) {
                    daysUntil = 0;
                    hoursUntil = 0;
                    minutesUntil = 0;
                    secondsUntil = 0;
                    Countdown = countdownToString();
                    return;
                }
                else if (String.Equals(name, "Daylight Saving Time Ends")) {
                    if (!isDaylightSavingTime(currentTime)) {
                        Debug.WriteLine("Is NOT Daylight Saving Time");
                        daysUntil = 0;
                        hoursUntil = 0;
                        minutesUntil = 0;
                        secondsUntil = 0;
                        Countdown = countdownToString();
                        return;
                    }
                    Debug.WriteLine("IS Daylight Saving Time");
                }
            }

            daysUntil = (int)(Math.Floor((date - currentTime).TotalDays));
            hoursUntil = (int)(Math.Floor((date - currentTime).TotalHours) % 24);
            minutesUntil = (int)(Math.Floor((date - currentTime).TotalMinutes) % 60);
            secondsUntil = (int)(Math.Floor((date - currentTime).TotalSeconds) % 60);

            Countdown = countdownToString();
        }

        public bool isDaylightSavingTime(DateTime currentTime) {
            if (Device.RuntimePlatform == Device.iOS) {
                DateTime hstTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "US/Hawaii");

                if (hstTime.Hour >= 20 || hstTime.Day != 3) {
                    return false;
                }
                return true;
            }
            else {
                DateTimeOffset nowDateTime = DateTimeOffset.Now;
                DateTimeOffset newDateTime = TimeZoneInfo.ConvertTime(
                    nowDateTime,
                    TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time"));

                if (newDateTime.Hour >= 20 || newDateTime.Day != 3) {
                    return false;
                }
                return true;
            }
        }

        public string Countdown {
            get {
                return _countdown;
            }
            set {
                _countdown = value;
                RaisePropertyChanged("Countdown");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string name) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}