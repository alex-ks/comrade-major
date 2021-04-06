using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ComradeMajor.Database;

namespace ComradeMajor.App
{
    public struct StatEntry
    {
        public string Activity { get; set; }
        public TimeSpan Sum { get; set; }
    }

    public class StatsCalculator
    {
        public async Task<List<StatEntry>> GetTop(ulong userId,
                                                  ComradeMajorDbContext context,
                                                  DateTimeOffset start,
                                                  DateTimeOffset end)
        {
            var actions = from action in context.UserActions!.AsQueryable()
                          where action.UserId == userId &&
                              start <= action.Timestamp &&
                              action.Timestamp < end
                          orderby action.Timestamp
                          select action;
            var activities = from action in actions.AsEnumerable()
                             where action.ActionType == ActionType.kFinished
                             let previous = actions.Where(a => a.Timestamp < action.Timestamp).LastOrDefault()
                             where previous != null &&
                                   previous.ActionType == ActionType.kStarted &&
                                   previous.Activity == action.Activity
                             select new { start = previous, finish = action };
            var groups = from activity in activities
                         group activity by activity.start.Activity;
            var durations = from g in groups
                            let sum = g.Aggregate(TimeSpan.FromTicks(0),
                                                  (res, act) => res +
                                                                (act.finish.Timestamp - act.start.Timestamp))
                            orderby sum descending
                            select new StatEntry { Activity = g.Key, Sum = sum };
            return await durations.ToAsyncEnumerable().ToListAsync();
        }
    }
}