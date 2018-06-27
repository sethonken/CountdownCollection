using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Xamarin.Forms;

namespace CountdownCollection {
    public partial class App : Application {
        public App() {
            InitializeComponent();
            MainPage = new CountdownCollection.MainPage();
        }

        protected override void OnStart() {
            // Handle when your app starts
        }

        protected override void OnSleep() {
            // Handle when your app sleeps
            CountdownCollection.MainPage mainPage = (MainPage)App.Current.MainPage;
            mainPage.objTimer.Stop();
        }

        protected override void OnResume() {
            // Handle when your app resumes
            CountdownCollection.MainPage mainPage = (MainPage)App.Current.MainPage;
            mainPage.objTimer.Start();
        }
    }

    public static class GlobalVariables {
        public static List<Event> myEvents; //events that the user has defined
        public static List<Event> storedEvents; //common events that have been programmed in

        public static string eventToBeDeleted = "";
        public static int lastRecordedYear = 0;
        public static int lastRecordedMonth = 0;
        public static int lastRecordedDay = 0;

        /*
         * Gives event correct date based on year
         */
        public static void resetEvent(Event newEvent) {
            int year, day;
            int sundayCount, mondayCount;
            MoonPhase moonPhase = new MoonPhase();

            switch (newEvent.getName()) {
                case "Chinese New Year":
                    year = DateTime.Now.Year;

                    while (true) {
                        ChineseLunisolarCalendar chinese = new ChineseLunisolarCalendar();
                        GregorianCalendar gregorian = new GregorianCalendar();
                        DateTime chineseNewYear = chinese.ToDateTime(year, 1, 1, 0, 0, 0, 0);
                        DateTime cny = new DateTime(gregorian.GetYear(chineseNewYear), gregorian.GetMonth(chineseNewYear), gregorian.GetDayOfMonth(chineseNewYear));
                        if (cny > DateTime.Now) {
                            newEvent.setDate(new DateTime(cny.Year, cny.Month, cny.Day));
                            break;
                        }
                        else {
                            year++;
                        }
                    }
                    break;
                case "Columbus Day":
                    year = DateTime.Now.Year;
                    day = 1;
                    mondayCount = 0;
                    while (true) {
                        if (new DateTime(year, newEvent.getDate().Month, day).DayOfWeek == DayOfWeek.Monday) {
                            mondayCount++;
                            if (mondayCount == 2) {
                                break;
                            }
                        }
                        day++;
                    }
                    if (DateTime.Now.Month == newEvent.getDate().Month && DateTime.Now.Day > day || DateTime.Now.Month > newEvent.getDate().Month) {
                        year = DateTime.Now.Year + 1;
                        day = 1;
                        mondayCount = 0;
                        while (true) {
                            if (new DateTime(year, newEvent.getDate().Month, day).DayOfWeek == DayOfWeek.Monday) {
                                mondayCount++;
                                if (mondayCount == 2) {
                                    break;
                                }
                            }
                            day++;
                        }
                        newEvent.setDate(new DateTime(DateTime.Now.Year + 1, newEvent.getDate().Month, day, 0, 0, 0));
                    }
                    else {
                        newEvent.setDate(new DateTime(DateTime.Now.Year, newEvent.getDate().Month, day, 0, 0, 0));
                    }
                    break;
                case "Daylight Saving Time Ends":
                    year = DateTime.Now.Year;
                    day = 1;
                    while (true) {
                        if (new DateTime(year, newEvent.getDate().Month, day).DayOfWeek == DayOfWeek.Sunday) {
                            break;
                        }
                        day++;
                    }
                    if (DateTime.Now.Month == newEvent.getDate().Month && DateTime.Now.Day > day || DateTime.Now.Month > newEvent.getDate().Month) {
                        year = DateTime.Now.Year + 1;
                        day = 1;
                        while (true) {
                            if (new DateTime(year, newEvent.getDate().Month, day).DayOfWeek == DayOfWeek.Sunday) {
                                break;
                            }
                            day++;
                        }
                        newEvent.setDate(new DateTime(year, newEvent.getDate().Month, day, newEvent.getDate().Hour, 0, 0));
                    }
                    else {
                        newEvent.setDate(new DateTime(year, newEvent.getDate().Month, day, newEvent.getDate().Hour, 0, 0));
                    }
                    break;
                case "Daylight Saving Time Starts":
                    year = DateTime.Now.Year;
                    day = 1;
                    sundayCount = 0;
                    while (true) {
                        if (new DateTime(year, newEvent.getDate().Month, day).DayOfWeek == DayOfWeek.Sunday) {
                            sundayCount++;
                            if (sundayCount == 2) {
                                break;
                            }
                        }
                        day++;
                    }
                    if (DateTime.Now.Month == newEvent.getDate().Month && DateTime.Now.Day > day || DateTime.Now.Month > newEvent.getDate().Month) {
                        year = DateTime.Now.Year + 1;
                        day = 1;
                        sundayCount = 0;
                        while (true) {
                            if (new DateTime(year, newEvent.getDate().Month, day).DayOfWeek == DayOfWeek.Sunday) {
                                sundayCount++;
                                if (sundayCount == 2) {
                                    break;
                                }
                            }
                            day++;
                        }
                        newEvent.setDate(new DateTime(year, newEvent.getDate().Month, day, newEvent.getDate().Hour, 0, 0));
                    }
                    else {
                        newEvent.setDate(new DateTime(year, newEvent.getDate().Month, day, newEvent.getDate().Hour, 0, 0));
                    }
                    break;
                case "Easter":
                    year = DateTime.Now.Year;

                    while (true) {
                        int a = year % 19;
                        int b = year / 100;
                        int c = year % 100;
                        int d = b / 4;
                        int e = b % 4;
                        int f = (b + 8) / 25;
                        int g = (b - f + 1) / 3;
                        int h = (19 * a + b - d - g + 15) % 30;
                        int i = c / 4;
                        int k = c % 4;
                        int l = (32 + 2 * e + 2 * i - h - k) % 7;
                        int m = (a + 11 * h + 22 * l) / 451;
                        int n = (h + l - 7 * m + 114) / 31;
                        int p = (h + l - 7 * m + 114) % 31;

                        DateTime easter = new DateTime(year, n, p + 1);
                        if (easter > DateTime.Now) {
                            newEvent.setDate(new DateTime(easter.Year, easter.Month, easter.Day, 0, 0, 0));
                            break;
                        }
                        else {
                            year++;
                        }
                    }
                    break;
                case "Father's Day":
                    year = DateTime.Now.Year;
                    day = 1;
                    sundayCount = 0;
                    while (true) {
                        if (new DateTime(year, newEvent.getDate().Month, day).DayOfWeek == DayOfWeek.Sunday) {
                            sundayCount++;
                            if (sundayCount == 3) {
                                break;
                            }
                        }
                        day++;
                    }
                    if (DateTime.Now.Month == newEvent.getDate().Month && DateTime.Now.Day > day || DateTime.Now.Month > newEvent.getDate().Month) {
                        year = DateTime.Now.Year + 1;
                        day = 1;
                        sundayCount = 0;
                        while (true) {
                            if (new DateTime(year, newEvent.getDate().Month, day).DayOfWeek == DayOfWeek.Sunday) {
                                sundayCount++;
                                if (sundayCount == 3) {
                                    break;
                                }
                            }
                            day++;
                        }
                        newEvent.setDate(new DateTime(DateTime.Now.Year + 1, newEvent.getDate().Month, day, 0, 0, 0));
                    }
                    else {
                        newEvent.setDate(new DateTime(DateTime.Now.Year, newEvent.getDate().Month, day, 0, 0, 0));
                    }
                    break;
                case "Friday the 13th":
                    DateTime temp = DateTime.Now;

                    while (temp.DayOfWeek != DayOfWeek.Friday) {
                        temp = temp.AddDays(1);
                    }

                    while (temp.Day != 13) {
                        temp = temp.AddDays(7);
                    }

                    temp = new DateTime(temp.Year, temp.Month, temp.Day, 0, 0, 0);

                    newEvent.setDate(temp);
                    break;
                case "Full Moon":
                    newEvent.setDate(moonPhase.GetNextFullMoon(DateTime.Today));
                    break;
                case "Labor Day":
                    year = DateTime.Now.Year;
                    day = 1;
                    while (true) {
                        if (new DateTime(year, newEvent.getDate().Month, day).DayOfWeek == DayOfWeek.Monday) {
                            break;
                        }
                        day++;
                    }
                    if (DateTime.Now.Month == newEvent.getDate().Month && DateTime.Now.Day > day || DateTime.Now.Month > newEvent.getDate().Month) {
                        year = DateTime.Now.Year + 1;
                        day = 1;
                        while (true) {
                            if (new DateTime(year, newEvent.getDate().Month, day).DayOfWeek == DayOfWeek.Monday) {
                                break;
                            }
                            day++;
                        }
                        newEvent.setDate(new DateTime(DateTime.Now.Year + 1, newEvent.getDate().Month, day, 0, 0, 0));
                    }
                    else {
                        newEvent.setDate(new DateTime(DateTime.Now.Year, newEvent.getDate().Month, day, 0, 0, 0));
                    }
                    break;
                case "Leap Day":
                    year = DateTime.Now.Year;
                    if (DateTime.Now.Month > 2) {
                        year++;
                    }
                    while (true) {
                        if (year % 4 != 0) {
                            year++;
                            continue;
                        }
                        else if (year % 100 != 0) {
                            break;
                        }
                        else if (year % 400 != 0) {
                            year++;
                            continue;
                        }
                        else {
                            break;
                        }
                    }
                    newEvent.setDate(new DateTime(year, newEvent.getDate().Month, 29, newEvent.getDate().Hour, 0, 0));
                    break;
                case "Mardi Gras":
                    year = DateTime.Now.Year;

                    while (true) {
                        int a = year % 19;
                        int b = year / 100;
                        int c = year % 100;
                        int d = b / 4;
                        int e = b % 4;
                        int f = (b + 8) / 25;
                        int g = (b - f + 1) / 3;
                        int h = (19 * a + b - d - g + 15) % 30;
                        int i = c / 4;
                        int k = c % 4;
                        int l = (32 + 2 * e + 2 * i - h - k) % 7;
                        int m = (a + 11 * h + 22 * l) / 451;
                        int n = (h + l - 7 * m + 114) / 31;
                        int p = (h + l - 7 * m + 114) % 31;

                        DateTime easter = new DateTime(year, n, p + 1);
                        if (easter.AddDays(-47) > DateTime.Now) {
                            newEvent.setDate((new DateTime(easter.Year, easter.Month, easter.Day, 0, 0, 0)).AddDays(-47));
                            break;
                        }
                        else {
                            year++;
                        }
                    }
                    break;
                case "Martin Luther King Jr. Day":
                    year = DateTime.Now.Year;
                    day = 1;
                    mondayCount = 0;
                    while (true) {
                        if (new DateTime(year, newEvent.getDate().Month, day).DayOfWeek == DayOfWeek.Monday) {
                            mondayCount++;
                            if (mondayCount == 3) {
                                break;
                            }
                        }
                        day++;
                    }
                    if (DateTime.Now.Month == newEvent.getDate().Month && DateTime.Now.Day > day || DateTime.Now.Month > newEvent.getDate().Month) {
                        year = DateTime.Now.Year + 1;
                        day = 1;
                        mondayCount = 0;
                        while (true) {
                            if (new DateTime(year, newEvent.getDate().Month, day).DayOfWeek == DayOfWeek.Monday) {
                                mondayCount++;
                                if (mondayCount == 3) {
                                    break;
                                }
                            }
                            day++;
                        }
                        newEvent.setDate(new DateTime(DateTime.Now.Year + 1, newEvent.getDate().Month, day, 0, 0, 0));
                    }
                    else {
                        newEvent.setDate(new DateTime(DateTime.Now.Year, newEvent.getDate().Month, day, 0, 0, 0));
                    }
                    break;
                case "Memorial Day":
                    year = DateTime.Now.Year;
                    day = 31;
                    while (true) {
                        if (new DateTime(year, newEvent.getDate().Month, day).DayOfWeek == DayOfWeek.Monday) {
                            break;
                        }
                        day--;
                    }
                    if (DateTime.Now.Month == newEvent.getDate().Month && DateTime.Now.Day > day || DateTime.Now.Month > newEvent.getDate().Month) {
                        year = DateTime.Now.Year + 1;
                        day = 31;
                        while (true) {
                            if (new DateTime(year, newEvent.getDate().Month, day).DayOfWeek == DayOfWeek.Monday) {
                                break;
                            }
                            day--;
                        }
                        newEvent.setDate(new DateTime(DateTime.Now.Year + 1, newEvent.getDate().Month, day, 0, 0, 0));
                    }
                    else {
                        newEvent.setDate(new DateTime(DateTime.Now.Year, newEvent.getDate().Month, day, 0, 0, 0));
                    }
                    break;
                case "Mother's Day":
                    year = DateTime.Now.Year;
                    day = 1;
                    sundayCount = 0;
                    while (true) {
                        if (new DateTime(year, newEvent.getDate().Month, day).DayOfWeek == DayOfWeek.Sunday) {
                            sundayCount++;
                            if (sundayCount == 2) {
                                break;
                            }
                        }
                        day++;
                    }
                    if (DateTime.Now.Month == newEvent.getDate().Month && DateTime.Now.Day > day || DateTime.Now.Month > newEvent.getDate().Month) {
                        year = DateTime.Now.Year + 1;
                        day = 1;
                        sundayCount = 0;
                        while (true) {
                            if (new DateTime(year, newEvent.getDate().Month, day).DayOfWeek == DayOfWeek.Sunday) {
                                sundayCount++;
                                if (sundayCount == 2) {
                                    break;
                                }
                            }
                            day++;
                        }
                        newEvent.setDate(new DateTime(DateTime.Now.Year + 1, newEvent.getDate().Month, day, 0, 0, 0));
                    }
                    else {
                        newEvent.setDate(new DateTime(DateTime.Now.Year, newEvent.getDate().Month, day, 0, 0, 0));
                    }
                    break;
                case "New Moon":
                    newEvent.setDate(moonPhase.GetNextNewMoon(DateTime.Today));
                    break;
                case "Thanksgiving":
                    year = DateTime.Now.Year;
                    day = 1;
                    int thursdayCount = 0;
                    while (true) {
                        if (new DateTime(year, 11, day).DayOfWeek == DayOfWeek.Thursday) {
                            thursdayCount++;
                            if (thursdayCount == 4) {
                                break;
                            }
                        }
                        day++;
                    }
                    if (DateTime.Now.Month == 11 && DateTime.Now.Day > day || DateTime.Now.Month > 11) {
                        year = DateTime.Now.Year + 1;
                        day = 1;
                        thursdayCount = 0;
                        while (true) {
                            if (new DateTime(year, 11, day).DayOfWeek == DayOfWeek.Thursday) {
                                thursdayCount++;
                                if (thursdayCount == 4) {
                                    break;
                                }
                            }
                            day++;
                        }
                        newEvent.setDate(new DateTime(DateTime.Now.Year + 1, newEvent.getDate().Month, day, 0, 0, 0));
                    }
                    else {
                        newEvent.setDate(new DateTime(DateTime.Now.Year, newEvent.getDate().Month, day, 0, 0, 0));
                    }
                    break;
                default:
                    //special case for user event on Feb 29
                    if (newEvent.getDate().Day == 29 && newEvent.getDate().Month == 2 || newEvent.isOneTimeEvent()) {
                        newEvent.setDate(new DateTime(newEvent.getDate().Year, newEvent.getDate().Month, newEvent.getDate().Day, newEvent.getDate().Hour, newEvent.getDate().Minute, newEvent.getDate().Second));
                    }
                    else if (DateTime.Now.Month == newEvent.getDate().Month && DateTime.Now.Day > newEvent.getDate().Day || DateTime.Now.Month > newEvent.getDate().Month) {
                        newEvent.setDate(new DateTime(DateTime.Now.Year + 1, newEvent.getDate().Month, newEvent.getDate().Day, newEvent.getDate().Hour, newEvent.getDate().Minute, newEvent.getDate().Second));
                    }
                    else {
                        newEvent.setDate(new DateTime(DateTime.Now.Year, newEvent.getDate().Month, newEvent.getDate().Day, newEvent.getDate().Hour, newEvent.getDate().Minute, newEvent.getDate().Second));
                    }
                    break;
            }
        }
    }
}
