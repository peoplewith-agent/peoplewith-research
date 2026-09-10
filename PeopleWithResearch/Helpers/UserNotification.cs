using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;
using Plugin.LocalNotification.Core.Models.AndroidOption;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PeopleWithResearch
{
    public class UserNotifications
    {
        public ObservableCollection<newuser> UsertoUpdate { get; set; } = new();
        public householdstudyrecord GroupDetails { get; set; } = new();

        public List<TQuestionnaire> QuestionnairesCompelted { get; set; } = new();
        public int NewDailyID { get; private set; }
        public int NewWeeklyID { get; private set; }

        private const string DailyTitle = "HOPPER Study - Action Required";
        private const string WeeklyTitle = "HOPPER Study - Reminder";
        private const string WeeklyDescription = "This is your weekly reminder to be on the look out for symptoms.";

        private readonly Random RanDomId = new();

        public async Task CancelAllNotifications()
        {
            try
            {
                LocalNotificationCenter.Current.ClearAll();
                LocalNotificationCenter.Current.CancelAll();
            }
            catch (Exception ex)
            {
            }
        }

        public async Task ScheduleWeeklyNotification(DateTime Passed, bool skip = false)
        {

            await CancelPending();

            NewWeeklyID = RanDomId.Next(100000, 1000001);
            Preferences.Set("weekly_notification_id", NewWeeklyID);

            DateTime triggerTime = GetNextWeeklyOccurrence(Passed);

            if (skip)
            {
                triggerTime = triggerTime.AddDays(7);
            }

            var notification = CreateNotification(
                NewWeeklyID,
                WeeklyTitle,
                WeeklyDescription,
                triggerTime,
                NotificationRepeat.Weekly
            );

            await LocalNotificationCenter.Current.Show(notification);
            await UpdateUserTime();
        }

        public async Task ScheduleDailyNotification(bool FromTForm = false)
        {
            try
            {
                await CancelPending(true);
                NewDailyID = RanDomId.Next(100000, 1000001);
                Preferences.Set("daily_notification_id", NewDailyID);

                string dayNum = await ReturnDayNum();
                DateTime triggerTime = GetNextOccurrence(ReturnTime());

                bool rolledOverToTomorrow = DateTime.Now > triggerTime;
                bool isUpcomingToday = !rolledOverToTomorrow;

                if (FromTForm && isUpcomingToday)
                {
                    triggerTime = triggerTime.AddDays(1);
                    rolledOverToTomorrow = true;
                }
                else
                {
                    //check tForm Completed
                    if (QuestionnairesCompelted != null)
                    {
                        var DateCheck = triggerTime.ToString("dd/MM/yyyy");
                        bool completedToday = QuestionnairesCompelted.Any(x =>
                        x.questionnaire_type == $"T{dayNum}" &&
                        x.date_completed != null &&
                        x.date_completed.StartsWith(DateCheck));

                        if (completedToday)
                        {
                            //Increment (Senario) filled out tForm and changed time of notification (Needs to check for completed) 
                            triggerTime = triggerTime.AddDays(1);
                            rolledOverToTomorrow = true;
                        }
                    }
                }
                                 
                if (rolledOverToTomorrow && dayNum != "x" && int.TryParse(dayNum, out int currentDay))
                {
                    dayNum = (currentDay + 1).ToString();
                }


                string description = $"Please tap here to complete the daily Symptom Questionnaire and Sampling.";
                //string description = $"Please tap here to complete the Day {dayNum} Symptom Questionnaire and Sampling.";
                var notification = CreateNotification(
                    NewDailyID,
                    DailyTitle,
                    description,
                    triggerTime,
                    NotificationRepeat.Daily
                );

                await LocalNotificationCenter.Current.Show(notification);
                await UpdateUserTime();
            }
            catch (Exception ex)
            {
            }
        }

        private async Task CancelPending(bool isDaily = false)
        {
            try
            {
                string key = isDaily ? "daily_notification_id" : "weekly_notification_id";
                int Pending = Preferences.Default.Get(key, 0);

                if (Pending != 0)
                {
                    LocalNotificationCenter.Current.Cancel(Pending);
                }
            }
            catch (Exception Ex)
            {

            }
        }

        private NotificationRequest CreateNotification(int id, string title, string description, DateTime notifyTime, NotificationRepeat repeatType)
        {
            return new NotificationRequest
            {
                NotificationId = id,
                Title = title,
                Description = description,
                BadgeNumber = 0,
                Sound = DeviceInfo.Platform == DevicePlatform.Android ? "pwjingo" : "pwjingo.aiff",
                Android = new AndroidOptions
                {
                    Priority = AndroidPriority.Max,
                    Ongoing = false,
                    ChannelId = "pwr_notifications",
                },
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = notifyTime,
                    RepeatType = repeatType,
                    NotifyRepeatInterval = null
                }
            };
        }

        private TimeSpan ReturnTime()
        {
            try
            {
                string selectedTime = Preferences.Get("notificationtime", "09:00");

                if (!TimeSpan.TryParseExact(selectedTime, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture, out TimeSpan parsedTime))
                {
                    return new TimeSpan(9, 0, 0);
                }
                return parsedTime;
            }
            catch
            {
                return new TimeSpan(9, 0, 0);
            }
        }

        private async Task<string> ReturnDayNum()
        {
            var householdGroupList = await APICalls.Instance.GetUserHouseholdInfo(Helpers.Settings.HouseholdGrouping);
            var householdGroup = householdGroupList?.FirstOrDefault();

            if (householdGroup == null) return "x";

            GroupDetails = householdGroup.studydetails ?? new householdstudyrecord();
            var activeEvent = GroupDetails.t_events?.FirstOrDefault(x => x.t_event_status == "active");

            if (activeEvent?.t1_start_date == null) return "x";

            var Userid = Helpers.Settings.UsersID;
            QuestionnairesCompelted = activeEvent.members.FirstOrDefault(x => x.user_id == Userid)?.questionnaires;

            if (!DateTime.TryParseExact(activeEvent.t1_start_date, "dd/MM/yyyy HH:mm",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime t1Start))
            {
                return "x";
            }

            return ((DateTime.Today - t1Start.Date).Days + 1).ToString();
        }


        public async Task UpdateUserTime(bool Cancel = false)
        {
            try
            {
                UsertoUpdate = await APICalls.Instance.Getuser();
                var user = UsertoUpdate?.FirstOrDefault();
                var notifDetails = user?.NotificationDetails?.FirstOrDefault();

                string notTime = Preferences.Default.Get("notificationtime", string.Empty);
                string targetTime = !string.IsNullOrEmpty(notTime) ? notTime : string.Empty;

                string currentDailyId = notifDetails?.DailyId ?? "0";
                string currentWeeklyId = notifDetails?.WeeklyId ?? "0";
                string DailyID = string.Empty;
                string WeeklyID = string.Empty;

                if (Cancel) 
                {
                    DailyID = "0";
                    WeeklyID = "0";
                    LocalNotificationCenter.Current.ClearAll();
                    LocalNotificationCenter.Current.CancelAll();
                }
                else
                {
                     DailyID = (NewDailyID != 0) ? NewDailyID.ToString() : currentDailyId;
                     WeeklyID = (NewWeeklyID != 0) ? NewWeeklyID.ToString() : currentWeeklyId;
                }

                var newData = new NotificationData()
                {
                    DailyTime = targetTime,
                    DailyId = DailyID,
                    WeeklyId = WeeklyID
                };

                string json = JsonSerializer.Serialize(newData);
                var changes = new Dictionary<string, object> { { "notificationtime", json } };

                await APICalls.Instance.UpdateUserData(Helpers.Settings.UsersID, changes);
            }
            catch (Exception ex)
            {

            }
        }

        private DateTime GetNextOccurrence(TimeSpan scheduledTime)
        {
            DateTime now = DateTime.Now;
            DateTime targetToday = DateTime.Today.Add(scheduledTime);
            return targetToday > now ? targetToday : targetToday.AddDays(1);
        }

        private DateTime GetNextWeeklyOccurrence(DateTime startDate)
        {
            DateTime now = DateTime.Now;
            DateTime nextOccurrence = startDate;

            while (nextOccurrence <= now)
            {
                nextOccurrence = nextOccurrence.AddDays(7);
            }

            return nextOccurrence;
        }
    }
    }