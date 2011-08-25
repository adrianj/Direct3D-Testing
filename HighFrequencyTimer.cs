using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;

namespace Direct3DLib
{
	public class HighFrequencyTimer : Component
	{
		private Stopwatch stopwatch = new Stopwatch();
		private const int nCounts = 5;
		private long [] count = new long[nCounts];
		private int currentIndex = 0;
		private double callsPerSecond = 0;
		private double refreshRate = 5;

		public void Restart()
		{
			stopwatch.Restart();
		}

		public void Stop()
		{
			stopwatch.Stop();
		}

		public TimeSpan Elapsed { get { return stopwatch.Elapsed; } }
		public double CallsPerSecond { get { return callsPerSecond; } }
		public double TimerRefreshRate { get { return refreshRate; } set { refreshRate = value; } }

		public double GetCallsPerSecond()
		{
			count[currentIndex]++;
			if (Elapsed.TotalSeconds >= (1/refreshRate))
			{
				callsPerSecond = CalculateCallsPerSecond();
				currentIndex = (currentIndex + 1) % nCounts;
				count[currentIndex] = 0;
				Restart();
			}
			return callsPerSecond;
		}

		private double CalculateCallsPerSecond()
		{
			long sum = 0;
			foreach (long l in count)
				sum += l;
			return (double)sum * refreshRate / (double)nCounts;
		}
	}
}
