using AutoMapper;
using System;
using System.Collections.Generic;

namespace Xzy.SK.Domain.Common.Map
{
    public static class MapperExtend
    {
        private static IMapper? _mapper;

        /// <summary>
        /// 注入全局映射器实例，由 services.AddMapper() 在启动时调用
        /// </summary>
        internal static void UseMapper(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        private static IMapper Mapper =>
            _mapper ?? throw new InvalidOperationException("AutoMapper 未初始化，请先在 Startup 中调用 services.AddMapper()。");

        /// <summary>
        /// Entity集合转DTO集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static List<T> ToDTOList<T>(this object value)
        {
            if (value == null)
                return new List<T>();

            return Mapper.Map<List<T>>(value);
        }
        /// <summary>
        /// Entity转DTO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ToDTO<T>(this object value)
        {
            if (value == null)
                return default(T);

            return Mapper.Map<T>(value);
        }

        /// <summary>
        /// 给已有对象map,适合update场景，如需过滤空值需要在AutoMapProfile 设置
        /// 注意：AutoMapper 11 起移除了 CreateMissingTypeMaps（动态映射），
        /// 未显式 CreateMap 的类型对会在运行时抛出 AutoMapperMappingException
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static T MapTo<T>(this object self, T result)
        {
            if (self == null)
                return default(T);
            return (T)Mapper.Map(self, result, self.GetType(), typeof(T));
        }
    }
}
