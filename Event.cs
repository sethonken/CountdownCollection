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

        int _daysUntil;
        int _hoursUntil;
        int _minutesUntil;
        int _secondsUntil;

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

        public void refreshTimeUntil(DateTime currentTime) {
            //set all times to 0 when event day is today
            if (currentTime.Day == date.Day && currentTime.Month == date.Month) {
                _daysUntil = 0;
                _hoursUntil = 0;
                _minutesUntil = 0;
                _secondsUntil = 0;

                DaysUntil = 0;
                HoursUntil = 0;
                MinutesUntil = 0;
                SecondsUntil = 0;
                return;
            }

            _daysUntil = (int)(Math.Floor((date - currentTime).TotalDays));
            _hoursUntil = (int)(Math.Floor((date - currentTime).TotalHours) % 24);
            _minutesUntil = (int)(Math.Floor((date - currentTime).TotalMinutes) % 60);
            _secondsUntil = (int)(Math.Floor((date - currentTime).TotalSeconds) % 60);

            DaysUntil = (int)(Math.Floor((date - currentTime).TotalDays));
            HoursUntil = (int)(Math.Floor((date - currentTime).TotalHours) % 24);
            MinutesUntil = (int)(Math.Floor((date - currentTime).TotalMinutes) % 60);
            SecondsUntil = (int)(Math.Floor((date - currentTime).TotalSeconds) % 60);
        }

        public int DaysUntil {
            get {
                return _daysUntil;
            }
            set {
                _daysUntil = value;
                RaisePropertyChanged("DaysUntil");
            }
        }

        public int HoursUntil {
            get {
                return _hoursUntil;
            }
            set {
                _hoursUntil = value;
                RaisePropertyChanged("HoursUntil");
            }
        }

        public int MinutesUntil {
            get {
                return _minutesUntil;
            }
            set {
                _minutesUntil = value;
                RaisePropertyChanged("MinutesUntil");
            }
        }

        public int SecondsUntil {
            get {
                return _secondsUntil;
            }
            set {
                _secondsUntil = value;
                RaisePropertyChanged("SecondsUntil");
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