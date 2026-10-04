using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Data
{
    public class DbContext_TuiXach : DbContext
    {
        public DbContext_TuiXach(DbContextOptions<DbContext_TuiXach> options): base(options)
        {
        }

    }
}
