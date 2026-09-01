using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xzy.SK.Domain.Common.Map
{
    public class AutoMapProfile : Profile
    {
        public AutoMapProfile()
        {
            //AutoMapper 11 起已移除 CreateMissingTypeMaps（动态映射），
            //使用 ToDTO/ToDTOList/MapTo 的类型对需要在这里显式 CreateMap

            //映射时忽略null值映射，适用于MapTo场景
            //CreateMap<BizCaseInfoEditDTO, PMP_BizCase_Main>().ForAllMembers(opt => opt.Condition((src, dest, sourceMember) => sourceMember != null));
        }
    }
}
