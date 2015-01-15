namespace Vertica.Integration.Infrastructure.Configuration
{
	public interface IParametersProvider
	{
		Parameters Get();
		void Save(Parameters parameters);
	}
}