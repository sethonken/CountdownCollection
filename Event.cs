using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace CountdownCollection {
    public class Event : INotifyPropertyChanged {
        string name;
        DateTime date;
        bool visible;
        bool oneTimeEvent;
        bool allDayEvent;

        int daysUntil;
        int hoursUntil;
        int minutesUntil;
        int secondsUntil;

        string _countdown;

        public Event() {

        }

        public Event(string name, DateTime date) {
            this.name = name;
            this.date = date;
            visible = true;
            oneTimeEvent = false;
        }

        public Event(string name, DateTime date, bool visible) {
            this.name = name;
            this.date = date;
            this.visible = visible;
            oneTimeEvent = false;
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