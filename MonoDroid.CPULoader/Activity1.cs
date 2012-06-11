/*
 * Copyright 2012 Tomasz Cielecki <tomasz@ostebaronen.dk>
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace MonoDroid.CPULoader
{
    [Activity(Label = "@string/ApplicationName", MainLauncher = true, Icon = "@drawable/icon")]
    public class Activity1 : Activity
    {
        private Button _startStopButton;
        private EditText _editTextPercentage;
        private EditText _editTextTime;
        private List<Thread> _threads;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            _startStopButton = FindViewById<Button>(Resource.Id.MyButton);
            _startStopButton.Click += StartStopButtonOnClick;

            _editTextPercentage = FindViewById<EditText>(Resource.Id.etPercent);
            _editTextTime = FindViewById<EditText>(Resource.Id.etTime);
        }

        void StartStopButtonOnClick(object sender, EventArgs e)
        {
            if (_startStopButton.Text == GetString(Resource.String.Begin))
            {
                if (null != _editTextPercentage && null != _editTextTime)
                {
                    UpdateButtonText(Resource.String.Stop);
                    ThreadPool.QueueUserWorkItem(state => Start(int.Parse(_editTextPercentage.Text), int.Parse(_editTextTime.Text)));
                }
            }
            else
            {
                Stop();
                UpdateButtonText(Resource.String.Begin);
            }
        }

        private void UpdateButtonText(int resId)
        {
            RunOnUiThread(() => _startStopButton.Text = GetString(resId));
        }

        /// <summary>
        /// Start loading the CPU
        /// </summary>
        /// <param name="percentage">Integer Percentage</param>
        /// <param name="seconds">How many seconds to load the CPU</param>
        private void Start(int percentage, int seconds)
        {
            _threads = new List<Thread>();

            for (int i = 0; i < System.Environment.ProcessorCount; i++)
            {
                Thread t = new Thread(new ParameterizedThreadStart(ConsumeCPU));
                t.Start(percentage);
                _threads.Add(t);
            }
            UpdateButtonText(Resource.String.Stop);
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            Stop();
        }

        /// <summary>
        /// Stop the execution of the CPU load
        /// </summary>
        private void Stop()
        {
            if (_threads == null) throw new ArgumentNullException("_threads");
            foreach (var t in _threads)
            {
                t.Abort();
            }
            _threads.Clear();
            UpdateButtonText(Resource.String.Begin);
        }

        /// <summary>
        /// Simulate CPU load
        /// Borowed from: http://stackoverflow.com/questions/2514544/simulate-steady-cpu-load-and-spikes
        /// </summary>
        /// <param name="percentage">int percentage to load the CPU</param>
        private static void ConsumeCPU(object percentage)
        {
            int percent = -1;
            if (percentage is int)
                percent = (int)percentage;
            if (percent < 0 || percent > 100)
                throw new ArgumentOutOfRangeException("percentage");
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (true)
            {
                // Make the loop go on for "percentage" milliseconds then sleep the 
                // remaining percentage milliseconds. So 40% utilization means work 40ms and sleep 60ms
                if (watch.ElapsedMilliseconds > percent)
                {
                    Thread.Sleep(100 - percent);
                    watch.Reset();
                    watch.Start();
                }
            }
        }
    }
}

