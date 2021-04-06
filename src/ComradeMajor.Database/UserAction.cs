using System;
using System.ComponentModel.DataAnnotations;

namespace ComradeMajor.Database
{
    public enum ActionType
    {
        kStarted,
        kFinished
    }

    public class UserAction
    {
        public ulong Id { get; set; }
        public ulong UserId { get; set; }
        [Required]
        public string Activity { get; set; }
        public ActionType ActionType { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public UserAction(ulong userId, string activity, ActionType actionType, DateTimeOffset timestamp)
        {
            UserId = userId;
            Activity = activity;
            ActionType = actionType;
            Timestamp = timestamp;
        }

        virtual public User? User { get; set; }
    }
}