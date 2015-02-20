using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Barista.Foundation.Specs
{
    [TestClass]
    public class NHibernateDataMapperSpecs
    {
        [TestMethod]
        public void When_i_have_valid_configuration_it_should_work()
        {
            var config = Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2008.ConnectionString())
        }
    }
}
