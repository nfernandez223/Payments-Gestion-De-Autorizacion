using Application.Interfaces;
using Domain.Entities;
using InfraEstructure.Persistence;

namespace InfraEstructure.Services
{
    public class ContextMgmt: IContextMgmt
    {
        private readonly AppDbContext _dbContext;

        public ContextMgmt(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void InsertSolicitud(Solicitud solicitud)
        {
            _dbContext.Add(solicitud);
            _dbContext.SaveChanges();
        }
    }
}
