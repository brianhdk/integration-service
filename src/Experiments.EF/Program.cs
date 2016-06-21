using System.IO;
using Experiments.EF.DAL;
using Experiments.EF.Models;
using Vertica.Integration;

namespace Experiments.EF
{
	class Program
	{
		static void Main(string[] args)
		{
			using (IApplicationContext context = ApplicationContext.Create(application => application
				.Database(database => database.DisableIntegrationDb())))
			{
				var writer = context.Resolve<TextWriter>();

				using (var db = new SchoolContext())
				{
					foreach (Student student in db.Students)
						writer.WriteLine(student.ID);
				}
			}
		}
	}
}
