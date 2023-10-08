﻿using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xzy.SK.Domain.Common.Map
{
    public static class MapperRegister
    {
        public static void AddMapper(this IServiceCollection services)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMissingTypeMaps = true;
                cfg.ValidateInlineMaps = false;
                cfg.AddProfile<AutoMapProfile>();
            });

            IMapper mapper = config.CreateMapper();

            //启动实体映射
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMissingTypeMaps = true;
                cfg.ValidateInlineMaps = false;
                cfg.AddProfile<AutoMapProfile>();
            });
        }
    }
}
