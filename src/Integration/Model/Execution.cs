namespace Vertica.Integration.Model
{
	public enum Execution
	{
		/// <summary>
		/// Will not execute the step and abort the task execution.
		/// </summary>
		StepOut,

		/// <summary>
		/// Will not execute the step, but proceeed to next step.
		/// </summary>
		StepOver,

		/// <summary>
		/// Will execute the step.
		/// </summary>
		Execute,
	}
}