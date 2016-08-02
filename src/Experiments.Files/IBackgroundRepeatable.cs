using System;

namespace Experiments.Files
{
	/// <summary>
	/// With a <see cref="IBackgroundRepeatable"/> we'll help out creating and starting the TPL task, you just need to implement what should happen in the Work(...) method - and return a wait-time (TimeSpan) for when we should execute your method again.
	/// We'll make sure to do the graceful shutdown, but if your code is running too long (more than 5 seconds (configurable)) - you'll be shutdown effectively by us.
	/// </summary>
	public interface IBackgroundRepeatable
	{
		TimeSpan Work(BackgroundRepeatedContext context);
	}
}