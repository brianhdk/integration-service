namespace Vertica.Integration.Infrastructure.Configuration
{
	public interface IConfigurationRepository
	{
		Configuration[] GetAll();
		Configuration Get(string id);

		Configuration Save(Configuration configuration);

		void Delete(string id);
	}
}