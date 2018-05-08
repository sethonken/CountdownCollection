using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Windows.Storage;
using Xamarin.Forms;

namespace CountdownCollection {
    public class FileHandler {
        public FileHandler() {

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
                                case "Minute":
                                    minute = Int32.Parse(entryValue);
                                    break;
                                case "Second":
                                    second = Int32.Parse(entryValue);
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

            //events I still want to add
            ////chineseNewYear
            ////easter
            ////presidentsDay
            ////taxDay
            ////veteransDay
        }

        public void readMyEventsFile() {
            var filePath = pathToMyEventsFile();

            //values for an event
            string name, source;
            int month, day, hour, minute, second;
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
                                case "Second":
                                    second = Int32.Parse(entryValue);
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
                        //2016 used to prevent impossible date due to leap day, the year value will be reset anyhow
                        Event newEvent = new Event(name, new DateTime(2016, month, day, hour, minute, second), visible);
                        GlobalVariables.resetEvent(newEvent);
                        newEvent.setOneTimeEvent(oneTimeEvent);
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

        public string pathToMyEventsFile() {
            switch (Device.RuntimePlatform) {
                case Device.iOS:
                    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    return Path.Combine(documentsPath, "MyEvents.txt");
                case Device.UWP:
                    string root = ApplicationData.Current.LocalFolder.Path;
                    return root + @"\MyEvents.txt";
                default:
                    return "";
            }
        }

        public string pathToStoredEventsFile() {
            switch (Device.RuntimePlatform) {
                case Device.iOS:
                    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    return Path.Combine(documentsPath, "StoredEvents.txt");
                case Device.UWP:
                    string root = ApplicationData.Current.LocalFolder.Path;
                    return root + @"\StoredEvents.txt";
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

        public void updateMyEventsFile() {
            clearMyEventsFile();
            var filePath = pathToMyEventsFile();
            Debug.WriteLine(filePath);

            //write down current date
            System.IO.File.AppendAllText(filePath, "<LastDate>\n");
            System.IO.File.AppendAllText(filePath, "\t<Year=" + DateTime.Now.Year + ">\n");
            System.IO.File.AppendAllText(filePath, "\t<Month=" + DateTime.Now.Month + ">\n");
            System.IO.File.AppendAllText(filePath, "\t<Day=" + DateTime.Now.Day + ">\n");
            System.IO.File.AppendAllText(filePath, "</LastDate>\n");

            for (int i = 0; i < GlobalVariables.myEvents.Count; i++) {
                System.IO.File.AppendAllText(filePath, "<Event>\n");
                System.IO.File.AppendAllText(filePath, "\t<Name=" + GlobalVariables.myEvents[i].getName() + ">\n");
                System.IO.File.AppendAllText(filePath, "\t<Month=" + GlobalVariables.myEvents[i].getDate().Month + ">\n");
                System.IO.File.AppendAllText(filePath, "\t<Day=" + GlobalVariables.myEvents[i].getDate().Day + ">\n");
                System.IO.File.AppendAllText(filePath, "\t<Visible=" + GlobalVariables.myEvents[i].isVisible() + ">\n");
                System.IO.File.AppendAllText(filePath, "\t<OneTimeEvent=" + GlobalVariables.myEvents[i].isOneTimeEvent() + ">\n");
                System.IO.File.AppendAllText(filePath, "</Event>\n");
            }
        }

        public void updateStoredEventsFile() {
            clearStoredEventsFile();
            var filePath = pathToStoredEventsFile();

            for (int i = 0; i < GlobalVariables.storedEvents.Count; i++) {
                System.IO.File.AppendAllText(filePath, "<Event>\n");
                System.IO.File.AppendAllText(filePath, "\t<Name=" + GlobalVariables.storedEvents[i].getName() + ">\n");
                System.IO.File.AppendAllText(filePath, "\t<Month=" + GlobalVariables.storedEvents[i].getDate().Month + ">\n");
                System.IO.File.AppendAllText(filePath, "\t<Day=" + GlobalVariables.storedEvents[i].getDate().Day + ">\n");
                System.IO.File.AppendAllText(filePath, "\t<Visible=" + GlobalVariables.storedEvents[i].isVisible() + ">\n");
                System.IO.File.AppendAllText(filePath, "</Event>\n");
            }
        }

        public void clearMyEventsFile() {
            System.IO.File.Delete(pathToMyEventsFile());
        }

        public void clearStoredEventsFile() {
            System.IO.File.Delete(pathToStoredEventsFile());
        }

        public void appendEventToFile(Event newEvent) {
            var filePath = pathToMyEventsFile();

            System.IO.File.AppendAllText(filePath, "<Event>\n");
            System.IO.File.AppendAllText(filePath, "\t<Name=" + newEvent.getName() + ">\n");
            System.IO.File.AppendAllText(filePath, "\t<Month=" + newEvent.getDate().Month + ">\n");
            System.IO.File.AppendAllText(filePath, "\t<Day=" + newEvent.getDate().Day + ">\n");
            System.IO.File.AppendAllText(filePath, "\t<Visible=" + newEvent.isVisible() + ">\n");
            System.IO.File.AppendAllText(filePath, "\t<OneTimeEvent=" + newEvent.isOneTimeEvent() + ">\n");
            System.IO.File.AppendAllText(filePath, "</Event>\n");
        }
    }
}
