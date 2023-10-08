using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using NPOI.POIFS.FileSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xzy.SK.Domain.Common.Options;

namespace Xzy.SK.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MemoryStoreController : ControllerBase
    {
        public MemoryStoreController() {
        }

        /// <summary>
        /// 查询向量
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> MemoryStore(string text)
        {
            //创建embedding实例
            var kernelWithCustomDb = Kernel.Builder
             .WithAzureTextEmbeddingGenerationService("text-embedding-ada-002", OpenAIOptions.Endpoint, OpenAIOptions.Key)
             .WithMemoryStorage(new VolatileMemoryStore())
             .Build();

            //支持的vector-db
            //https://learn.microsoft.com/en-us/semantic-kernel/memories/vector-db

            var bilibiliFiles = BiliBiliData();
            var i = 0;
            foreach (var entry in bilibiliFiles)
            {
                await kernelWithCustomDb.Memory.SaveReferenceAsync(
                    collection: "BiliBili",
                    externalSourceName: "BiliBili",
                    externalId: entry.Key,
                    description: entry.Value,
                    text: entry.Value);

                Console.Write($" #{++i} 保存成功.");
            }


            var memories = kernelWithCustomDb.Memory.SearchAsync("BiliBili", text, limit: 2, minRelevanceScore: 0.5);

            string result = "" ;
            await foreach (MemoryQueryResult memory in memories)
            {
                result += $"Id:{memory.Metadata.Id},Description:{memory.Metadata.Description},Relevance：{memory.Relevance}\n" ;
            }

            return Ok(result);
        }


        private Dictionary<string, string> BiliBiliData()
        {
            return new Dictionary<string, string>
            {
                ["https://www.bilibili.com/video/BV1sr4y1f7zb/"]
                    = "SK 插件Plugins及VSCode调试工具",
                ["https://www.bilibili.com/video/BV1Hw411Y71S"]
                    = "SK 原生函数使用方法",
                ["https://www.bilibili.com/video/BV1zF411m7YA/"]
                    = "SK 嵌套函数使用方法",
                ["https://www.bilibili.com/video/BV1F841117Jc/"]
                    = "SK 原生函数及嵌套函数串联使用方法",
                ["https://www.bilibili.com/video/BV12j41187GX/"]
                    = "SK Plan流程编排",
                ["https://www.bilibili.com/video/BV1nm4y1V7dz/"]
                    = "SK 意图识别、json提取",
                ["https://www.bilibili.com/video/BV1Qj41147i6/"]
                    = "SK 依赖注入、Pipeline"
            };
        }
    }
}
