using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ImageTiler
{
	/*
	 * Reports progress as if it is part of a larger process.
	 */
	public interface PartialProgressReporter
	{
		/* The progress that was complete by other processes before this one. */
		double ProgressOffset { get; set; }
		/* The range of progress that this process is responsible for */
		double ProgressRange { get; set; }
		/* If progress reports are equally spaced, then use this to store incerement. */
		double ProgressIncrement { get; set; }
		/* Reports the progress made by this process (0 - 100) */
		void ReportProgress(BackgroundWorker progressReporter, double thisProcessProgress);
		/* Reports progress as an increment */
		void ReportIncrementalProgress(BackgroundWorker progressReporter);
	}

	public class SimpleProgressReporter : PartialProgressReporter
	{
		public double ProgressOffset { get; set; }
		public double ProgressRange { get; set; }
		public double ProgressIncrement { get; set; }

		double previouslyReportedPartialProgress;

		public SimpleProgressReporter()
		{
			ProgressOffset = 0;
			ProgressRange = 100;
			ProgressIncrement = 1;
		}

		public void ReportProgress(BackgroundWorker progressReporter, double thisProcessProgress)
		{
			double prog = (thisProcessProgress / 100 * ProgressRange) + ProgressOffset;
			previouslyReportedPartialProgress = thisProcessProgress;
			if(progressReporter.WorkerReportsProgress)
				progressReporter.ReportProgress((int)prog);
		}

		public void ReportIncrementalProgress(BackgroundWorker progressReporter)
		{
			ReportProgress(progressReporter, previouslyReportedPartialProgress + ProgressIncrement);
		}
	}
}
