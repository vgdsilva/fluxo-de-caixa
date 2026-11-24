using Microsoft.EntityFrameworkCore;

namespace WebAPI.Data
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
