﻿using System;
using System.ServiceProcess;

namespace TcpEventServer
{
    partial class WinService : ServiceBase
    {
        public WinService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {

            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message,
                    System.Diagnostics.EventLogEntryType.Information);
            }
        }

        protected override void OnStop()
        {

        }
    }
}
