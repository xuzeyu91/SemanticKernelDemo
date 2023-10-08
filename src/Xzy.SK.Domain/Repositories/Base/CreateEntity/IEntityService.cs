using System;
using System.Collections.Generic;
using System.Text;

namespace Xzy.SK.Domain.Repositories
{
    public interface IEntityService
    {
        bool CreateEntity(string entityName, string filePath);
    }
}
