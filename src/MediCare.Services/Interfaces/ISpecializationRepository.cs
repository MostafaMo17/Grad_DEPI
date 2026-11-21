using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Interfaces
{
    public interface ISpecializationRepository
    {
        Task<bool> ExistsAsync(int specializationId);
    }
}
