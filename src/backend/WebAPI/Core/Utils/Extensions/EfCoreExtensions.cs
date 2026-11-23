using Microsoft.EntityFrameworkCore;
using WebAPI.Core.Utils.Classes;
using WebAPI.Data.Configuration;

namespace WebAPI.Core.Utils.Extensions
{
    public static class EfCoreExtensions
    {
        public static void AddEntities(this ModelBuilder modelBuilder)
        {
            IEnumerable<Type> typesToRegister = EntityUtils.GetEntities();

            foreach (Type entityType in typesToRegister)
            {
                EntityMappingConfiguration entityMapping = new EntityMappingConfiguration(entityType);
                entityMapping.Map(modelBuilder);
            }
        }
    }
}
