using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Windows.Storage;
using Windows.UI.Core;
using Xamarin.Forms;

namespace CountdownCollection {
    public class FileHandler {
        public bool saving;

        public FileHandler() {
            saving = false;
        }

        /*
         * Removes "<>" from file line entry
         */
        string convertEntry(string line) {
            string entry = line.Substring(line.IndexOf('<') + 1);
            entry = entry.Substring(0, entry.Length - 1);
            return entry;
        }

        /*
         * Returns entry type, found before "="
         */
        string getEntryType(string line) {
            return line.Substring(0, line.IndexOf('='));
        }

        /*
         * Returns entry value, found after "="
         */
        string getEntryValue(string line) {
            return line.Substring(line.IndexOf('=') + 1);
        }

        public void readStoredEventsFile() {
            var filePath = pathToStoredEventsFile();

            //values for an event
            string name, source;
            int month, day, hour, minute, second;
            bool visible;

            //read in stored events from text file
            string text, entryType, entryValue;
            if (!System.IO.File.Exists(filePath)) {
                resetStoredEventsFile();
            }
            string[] reader = System.IO.File.ReadAllLines(filePath);
            int i = 0;
            while (i < reader.Length) {
                //values for an event
                name = "";
                source = "";
                month = 1;
                day = 1;
                hour = 0;
                minute = 0;
                second = 0;
                visible = false;

                text = convertEntry(reader[i]);
                switch (text) {
                    case "Event":
                        i++;
                        text = convertEntry(reader[i]);
                        while (!text.Equals("/Event")) {
                            entryType = getEntryType(text);
                            entryValue = getEntryValue(text);
                            switch (entryType) {
                                case "Name":
                                    name = entryValue;
                                    break;
                                case "Source":
                                    source = entryValue;
                                    break;
                                case "Month":
                                    month = Int32.Parse(entryValue);
                                    break;
                                case "Day":
                                    day = Int32.Parse(entryValue);
                                    break;
                                case "Hour":
                                    hour = Int32.Parse(entryValue);
                                    break;
                                case "Visible":
                                    visible = bool.Parse(entryValue);
                                    break;
                            }
                            i++;
                            text = convertEntry(reader[i]);
                        }

                        //add the new event to list(s)
                        //2016 used to prevent impossible date due to leap day, the year value will be reset anyhow
                        Event newEvent = new Event(name, new DateTime(2016, month, day, hour, minute, second), visible);
                        GlobalVariables.resetEvent(newEvent);
                        GlobalVariables.storedEvents.Add(newEvent);

                        break;
                }
                i++;
            }
        }

        public void readMyEventsFile() {
            var filePath = pathToMyEventsFile();

            //values for an event
            string name, source;
            int year, month, day, hour, minute, second;
            bool visible;
            bool oneTimeEvent;

            //read in stored events from text file
            string text, entryType, entryValue;
            if (!System.IO.File.Exists(filePath)) {
                return;
            }
            string[] reader = System.IO.File.ReadAllLines(filePath);
            int i = 0;
            while (i < reader.Length) {
                //values for an event
                name = "";
                source = "";
                year = 2016; //2016 used to prevent impossible date due to leap day, the year value will be reset anyhow
                month = 1;
                day = 1;
                hour = 0;
                minute = 0;
                second = 0;
                visible = false;
                oneTimeEvent = false;

                text = convertEntry(reader[i]);
                switch (text) {
                    case "Event":
                        i++;
                        text = convertEntry(reader[i]);
                        while (!text.Equals("/Event")) {
                            entryType = getEntryType(text);
                            entryValue = getEntryValue(text);
                            switch (entryType) {
                                case "Name":
                                    name = entryValue;
                                    break;
                                case "Source":
                                    source = entryValue;
                                    break;
                                case "Year":
                                    year = Int32.Parse(entryValue);
                                    break;
                                case "Month":
                                    month = Int32.Parse(entryValue);
                                    break;
                                case "Day":
                                    day = Int32.Parse(entryValue);
                                    break;
                                case "Hour":
                                    hour = Int32.Parse(entryValue);
                                    break;
                                case "Minute":
                                    minute = Int32.Parse(entryValue);
                                    break;
                                case "Visible":
                                    visible = bool.Parse(entryValue);
                                    break;
                                case "OneTimeEvent":
                                    oneTimeEvent = bool.Parse(entryValue);
                                    break;
                            }
                            i++;
                            text = convertEntry(reader[i]);
                        }

                        //add the new event to list(s)
                        Event newEvent = new Event(name, new DateTime(year, month, day, hour, minute, second), visible);
                        newEvent.setOneTimeEvent(oneTimeEvent);
                        GlobalVariables.resetEvent(newEvent);
                        GlobalVariables.myEvents.Add(newEvent);

                        break;
                    case "LastDate":
                        i++;
                        text = convertEntry(reader[i]);
                        while (!text.Equals("/LastDate")) {
                            entryType = getEntryType(text);
                            entryValue = getEntryValue(text);
                            switch (entryType) {
                                case "Year":
                                    GlobalVariables.lastRecordedYear = Int32.Parse(entryValue);
                                    break;
                                case "Month":
                                    GlobalVariables.lastRecordedMonth = Int32.Parse(entryValue);
                                    break;
                                case "Day":
                                    GlobalVariables.lastRecordedDay = Int32.Parse(entryValue);
                                    break;
                            }
                            i++;
                            text = convertEntry(reader[i]);
                        }

                        break;
                }
                i++;
            }
        }

        public string pathToStoredEventsFile() {
            return pathToStoredEventsFile(false);
        }

        public string pathToStoredEventsFile(bool temp) {
            switch (Device.RuntimePlatform) {
                case Device.iOS:
                    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    if (temp) {
                        return Path.Combine(documentsPath, "StoredEvents_temp.txt");
                    }
                    else {
                        return Path.Combine(documentsPath, "StoredEvents.txt");
                    }
                //case Device.UWP:
                //    string root = ApplicationData.Current.LocalFolder.Path;
                //    if (temp) {
                //        return root + @"\StoredEvents_temp.txt";
                //    }
                //    else {
                //        return root + @"\StoredEvents.txt";
                //    }
                default:
                    return "";
            }
        }

        public string pathToMyEventsFile() {
            return pathToMyEventsFile(false);
        }

        public string pathToMyEventsFile(bool temp) {
            switch (Device.RuntimePlatform) {
                case Device.iOS:
                    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    if (temp) {
                        return Path.Combine(documentsPath, "MyEvents_temp.txt");
                    }
                    else {
                        return Path.Combine(documentsPath, "MyEvents.txt");
                    }
                //case Device.UWP:
                //    string root = ApplicationData.Current.LocalFolder.Path;
                //    if (temp) {
                //        return root + @"\MyEvents_temp.txt";
                //    }
                //    else {
                //        return root + @"\MyEvents.txt";
                //    }
                default:
                    return "";
            }
        }

        /*
         * restores the visibility of stored events
         */
        public void resetStoredEventsFile() {
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MainPage)).Assembly;
            Stream stream = assembly.GetManifestResourceStream("CountdownCollection.EventsData.txt");

            var path = pathToStoredEventsFile();
            System.IO.File.Delete(path);

            using (var reader = new System.IO.StreamReader(stream)) {
                while (!reader.EndOfStream) {
                    System.IO.File.AppendAllText(path, reader.ReadLine() + "\n");
                }
            }

            stream.Close();
        }

        public void resetMyEventsFile() {
            System.IO.File.Delete(pathToMyEventsFile());
        }

        public void updateStoredEventsFile() {
            if (saving) {
                return;
            }
            saving = true;

            clearStoredEventsTempFile();
            var filePath = pathToStoredEventsFile(true);
            var realFilePath = pathToStoredEventsFile();

            for (int i = 0; i < GlobalVariables.storedEvents.Count; i++) {
                System.IO.File.AppendAllText(filePath, "<Event>\n");
                System.IO.File.AppendAllText(filePath, "\t<Name=" + GlobalVariables.storedEvents[i].getName() + ">\n");
                System.IO.File.AppendAllText(filePath, "\t<Month=" + GlobalVariables.storedEvents[i].getDate().Month + ">\n");
                System.IO.File.AppendAllText(filePath, "\t<Day=" + GlobalVariables.storedEvents[i].getDate().Day + ">\n");
                System.IO.File.AppendAllText(filePath, "\t<Hour=" + GlobalVariables.storedEvents[i].getDate().Hour + ">\n");
                System.IO.File.AppendAllText(filePath, "\t<Visible=" + GlobalVariables.storedEvents[i].isVisible() + ">\n");
                System.IO.File.AppendAllText(filePath, "</Event>\n");
            }

            System.IO.File.Copy(filePath, realFilePath, true);
            clearStoredEventsTempFile();

            saving = false;
        }

        public void updateMyEventsFile() {
            if (saving) {
                return;
            }
            saving = true;

            clearMyEventsTempFile();
            var filePath = pathToMyEventsFile(true);
            var realFilePath = pathToMyEventsFile();
            //Debug.WriteLine(realFilePath);

            //write down current date
            System.IO.File.AppendAllText(filePath, "<LastDate>\n");
            System.IO.File.AppendAllText(filePath, "\t<Year=" + DateTime.Now.Year + ">\n");
            System.IO.File.AppendAllText(filePath, "\t<Month=" + DateTime.Now.Month + ">\n");
            System.IO.File.AppendAllText(filePath, "\t<Day=" + DateTime.Now.Day + ">\n");
            System.IO.File.AppendAllText(filePath, "</LastDate>\n");

            for (int i = 0; i < GlobalVariables.myEvents.Count; i++) {
                System.IO.File.AppendAllText(filePath, "<Event>\n");
                System.IO.File.AppendAllText(filePath, "\t<Name=" + GlobalVariables.myEvents[i].getName() + ">\n");
                System.IO.File.AppendAllText(filePath, "\t<Year=" + GlobalVariables.myEvents[i].getDate().Year + ">\n");
                System.IO.File.AppendAllText(filePath, "\t<Month=" + GlobalVariables.myEvents[i].getDate().Month + ">\n");
                System.IO.File.AppendAllText(filePath, "\t<Day=" + GlobalVariables.myEvents[i].getDate().Day + ">\n");
                if (!GlobalVariables.myEvents[i].isAllDayEvent()) {
                    System.IO.File.AppendAllText(filePath, "\t<Hour=" + GlobalVariables.myEvents[i].getDate().Hour + ">\n");
                    System.IO.File.AppendAllText(filePath, "\t<Minute=" + GlobalVariables.myEvents[i].getDate().Minute + ">\n");
                }
                System.IO.File.AppendAllText(filePath, "\t<Visible=" + GlobalVariables.myEvents[i].isVisible() + ">\n");
                System.IO.File.AppendAllText(filePath, "\t<OneTimeEvent=" + GlobalVariables.myEvents[i].isOneTimeEvent() + ">\n");
                System.IO.File.AppendAllText(filePath, "</Event>\n");
            }

            System.IO.File.Copy(filePath, realFilePath, true);
            clearMyEventsTempFile();

            saving = false;
        }

        public void clearStoredEventsTempFile() {
            System.IO.File.Delete(pathToStoredEventsFile(true));
        }

        public void clearMyEventsTempFile() {
            System.IO.File.Delete(pathToMyEventsFile(true));
        }

        public void appendEventToFile(Event newEvent) {
            if (saving) {
                return;
            }
            saving = true;

            var filePath = pathToMyEventsFile();

            System.IO.File.AppendAllText(filePath, "<Event>\n");
            System.IO.File.AppendAllText(filePath, "\t<Name=" + newEvent.getName() + ">\n");
            System.IO.File.AppendAllText(filePath, "\t<Year=" + newEvent.getDate().Year + ">\n");
            System.IO.File.AppendAllText(filePath, "\t<Month=" + newEvent.getDate().Month + ">\n");
            System.IO.File.AppendAllText(filePath, "\t<Day=" + newEvent.getDate().Day + ">\n");
            if (!newEvent.isAllDayEvent()) {
                System.IO.File.AppendAllText(filePath, "\t<Hour=" + newEvent.getDate().Hour + ">\n");
                System.IO.File.AppendAllText(filePath, "\t<Minute=" + newEvent.getDate().Minute + ">\n");
            }
            System.IO.File.AppendAllText(filePath, "\t<Visible=" + newEvent.isVisible() + ">\n");
            System.IO.File.AppendAllText(filePath, "\t<OneTimeEvent=" + newEvent.isOneTimeEvent() + ">\n");
            System.IO.File.AppendAllText(filePath, "</Event>\n");

            saving = false;
        }
    }
}
