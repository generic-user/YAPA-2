﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YAPA.Shared.Contracts;

namespace YAPA.Shared.Common
{

    public class Dashboard : IPlugin
    {
        private readonly IPomodoroRepository _itemRepository;
        private readonly PomodoroEngineSettings _engineSettings;
        private readonly IPomodoroEngine _engine;

        public Dashboard(IPomodoroEngine engine, IPomodoroRepository itemRepository, PomodoroEngineSettings engineSettings)
        {
            _itemRepository = itemRepository;
            _engineSettings = engineSettings;
            _engine = engine;

            _engine.OnPomodoroCompleted += _engine_OnPomodoroCompleted;
        }

        public IEnumerable<PomodoroEntity> GetPomodoros()
        {
            //last 4 full months + current
            var date = DateTime.Now.Date.AddMonths(-4);
            var fromDate = new DateTime(date.Year, date.Month, 01, 0, 0, 0, 0, DateTimeKind.Utc);

            var totalDays = (int)(DateTime.Now.Date - fromDate).TotalDays;
            var emptyPomodoros = Enumerable.Range(0, totalDays + 1).Select(x => new PomodoroEntity { Count = 0, DateTime = fromDate.AddDays(x) }).ToList();
            var capturedPomodoros = _itemRepository.Pomodoros.Where(x => x.DateTime >= fromDate).ToList();

            var joinedPomodoros = capturedPomodoros.Union(emptyPomodoros)
                .GroupBy(c => c.DateTime.Date, c => new { Count = c.Count, WorkTime = c.DurationMin },
                    (time, ints) => new PomodoroEntity { DateTime = time, Count = ints.Sum(x => x.Count), DurationMin = ints.Sum(x => x.WorkTime) });

            return joinedPomodoros.OrderBy(x => x.DateTime.Date);
        }

        public int CompletedToday()
        {
            var today = DateTime.Now.Date;
            return _itemRepository.Pomodoros.Where(x => x.DateTime == today).Select(a => a.Count).DefaultIfEmpty(0).Sum();
        }

        private void _engine_OnPomodoroCompleted()
        {
            Task.Run(() =>
            {
                _itemRepository.Add(new PomodoroEntity
                {
                    Count = 1,
                    DateTime = DateTime.UtcNow.Date,
                    DurationMin = _engine.WorkTime / 60,
                    ProfileName = _engineSettings.ActiveProfile
                });
            });
        }
    }

    public class DashboardSettings : IPluginSettings
    {
        private readonly ISettingsForComponent _settings;

        public DashboardSettings(ISettings settings)
        {
            _settings = settings.GetSettingsForComponent(nameof(Dashboard));
        }

        public void DeferChanges()
        {
            _settings.DeferChanges();
        }
    }
}
