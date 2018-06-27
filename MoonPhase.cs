using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

/*
 * Taken from:
 * https://gist.github.com/adrianstevens/776530e198734b34a9c8a43aaf880041
 */

namespace CountdownCollection {
    public class MoonPhase {
        public DateTime GetNextNewMoon(DateTime currentDay) {
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MainPage)).Assembly;
            Stream stream = assembly.GetManifestResourceStream("CountdownCollection.MoonTimes.txt");

            using (var reader = new System.IO.StreamReader(stream)) {
                while (!reader.EndOfStream) {
                    string line = reader.ReadLine();
                    if (line.Contains("Year")) {
                        reader.ReadLine();
                        line = reader.ReadLine();
                        if (line.Contains(DateTime.UtcNow.Year.ToString())) {
                            while (!line.Contains(DateTime.UtcNow.ToString("MMM"))) {
                                line = reader.ReadLine();
                            }

                            int newMoonIndex = 8;
                            int year = DateTime.UtcNow.Year;
                            int month = DateTime.UtcNow.Month;
                            int day = DateTime.UtcNow.Day;
                            int hour = 0;
                            int minute = 0;
                            while (true) {
                                //find if time is after current time
                                while (line.Length <= newMoonIndex || line[newMoonIndex] == ' ') {
                                    line = reader.ReadLine();
                                    if (line.Contains("Year")) {
                                        year++;
                                    }
                                }

                                if (line.Substring(newMoonIndex, 3).Equals(DateTime.UtcNow.ToString("MMM"))) {
                                    day = Convert.ToInt32(line.Substring(newMoonIndex + 4, 2));
                                    if (day >= DateTime.UtcNow.Day) {
                                        hour = Convert.ToInt32(line.Substring(newMoonIndex + 8, 2));
                                        minute = Convert.ToInt32(line.Substring(newMoonIndex + 11, 2));
                                        DateTime dt = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(year, month, day, hour, minute, 0), TimeZoneInfo.Local);
                                        if (dt < DateTime.Now) {
                                            line = reader.ReadLine();
                                            continue;
                                        }
                                        stream.Close();
                                        return dt;
                                    }
                                }
                                else {
                                    if (!line.Contains(DateTime.UtcNow.ToString("MMM"))) {
                                        month++;
                                        if (month > 12) {
                                            month -= 12;
                                        }
                                        day = Convert.ToInt32(line.Substring(newMoonIndex + 4, 2));
                                        hour = Convert.ToInt32(line.Substring(newMoonIndex + 8, 2));
                                        minute = Convert.ToInt32(line.Substring(newMoonIndex + 11, 2));
                                        DateTime dt = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(year, month, day, hour, minute, 0), TimeZoneInfo.Local);
                                        if (dt < DateTime.Now) {
                                            line = reader.ReadLine();
                                            continue;
                                        }
                                        stream.Close();
                                        return dt;
                                    }
                                }
                                line = reader.ReadLine();
                            }
                        }
                    }
                }
            }
            return TimeZoneInfo.ConvertTimeFromUtc(new DateTime(2018, 1, 17, 2, 17, 0), TimeZoneInfo.Local);
        }

        public DateTime GetNextFullMoon(DateTime currentDay) {
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MainPage)).Assembly;
            Stream stream = assembly.GetManifestResourceStream("CountdownCollection.MoonTimes.txt");

            using (var reader = new System.IO.StreamReader(stream)) {
                while (!reader.EndOfStream) {
                    string line = reader.ReadLine();
                    if (line.Contains("Year")) {
                        reader.ReadLine();
                        line = reader.ReadLine();
                        if (line.Contains(DateTime.UtcNow.Year.ToString())) {
                            while (!line.Contains(DateTime.UtcNow.ToString("MMM"))) {
                                line = reader.ReadLine();
                            }

                            int newMoonIndex = 44;
                            int year = DateTime.UtcNow.Year;
                            int month = DateTime.UtcNow.Month;
                            int day = DateTime.UtcNow.Day;
                            int hour = 0;
                            int minute = 0;
                            while (true) {
                                //find if time is after current time
                                while (line.Length <= newMoonIndex || line[newMoonIndex] == ' ') {
                                    line = reader.ReadLine();
                                    if (line.Contains("Year")) {
                                        year++;
                                    }
                                }

                                if (line.Substring(newMoonIndex, 3).Equals(DateTime.UtcNow.ToString("MMM"))) {
                                    day = Convert.ToInt32(line.Substring(newMoonIndex + 4, 2));
                                    if (day >= DateTime.UtcNow.Day) {
                                        hour = Convert.ToInt32(line.Substring(newMoonIndex + 8, 2));
                                        minute = Convert.ToInt32(line.Substring(newMoonIndex + 11, 2));
                                        DateTime dt = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(year, month, day, hour, minute, 0), TimeZoneInfo.Local);
                                        if (dt < DateTime.Now) {
                                            line = reader.ReadLine();
                                            continue;
                                        }
                                        stream.Close();
                                        return dt;
                                    }
                                }
                                else {
                                    if (!line.Contains(DateTime.UtcNow.ToString("MMM"))) {
                                        month++;
                                        if (month > 12) {
                                            month -= 12;
                                        }
                                        day = Convert.ToInt32(line.Substring(newMoonIndex + 4, 2));
                                        hour = Convert.ToInt32(line.Substring(newMoonIndex + 8, 2));
                                        minute = Convert.ToInt32(line.Substring(newMoonIndex + 11, 2));
                                        DateTime dt = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(year, month, day, hour, minute, 0), TimeZoneInfo.Local);
                                        if (dt < DateTime.Now) {
                                            line = reader.ReadLine();
                                            continue;
                                        }
                                        stream.Close();
                                        return dt;
                                    }
                                }
                                line = reader.ReadLine();
                            }
                        }
                    }
                }
            }
            return TimeZoneInfo.ConvertTimeFromUtc(new DateTime(2018, 1, 2, 2, 24, 0), TimeZoneInfo.Local);
        }
    }
}
