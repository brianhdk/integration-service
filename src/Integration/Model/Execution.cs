namespace Vertica.Integration.Model
{
	public enum Execution
	{
		/// <summary>
		/// Will not execute this Step and will abort the entire Task Execution.
		/// </summary>
		StepOut,

		/// <summary>
		/// Will not execute this Step but continue to the next Step in the chain.
		/// </summary>
		StepOver,

		/// <summary>
		/// Will execute this Step.
		/// </summary>
		Execute,
	}
}