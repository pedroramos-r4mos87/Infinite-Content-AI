using InfiniteContentAI.Data.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace InfiniteContentAI.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
public partial class ApplicationDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.1");
        new ProjectConfiguration().Configure(
            modelBuilder.Entity<global::InfiniteContentAI.Domain.Projects.Project>());
    }
}
