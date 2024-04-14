using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using NPOI.POIFS.FileSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xzy.SK.Domain.Common.Model;
using Xzy.SK.Domain.Common.Options;
using Xzy.SK.Domain.Common.Utils;

namespace Xzy.SK.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MemoryStoreController : ControllerBase
    {
        private readonly Kernel _kernel;

        public MemoryStoreController(Kernel kernel)
        {
            _kernel = kernel;
        }

        /// <summary>
        /// 查询向量
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> MemoryStore(string text)
        {
            var handler = new OpenAIHttpClientHandler();
            //创建embedding实例
            var memoryWithCustomDb = new MemoryBuilder()
             .WithOpenAITextEmbeddingGeneration("text-embedding-ada-002", OpenAIOptions.Key, httpClient: new HttpClient(handler))
             .WithMemoryStore(new VolatileMemoryStore())
             .Build();

            //支持的vector-db
            //https://learn.microsoft.com/en-us/semantic-kernel/memories/vector-db

            var bilibiliFiles = BiliBiliData();
            var i = 0;
            foreach (var entry in bilibiliFiles)
            {
                await memoryWithCustomDb.SaveReferenceAsync(
                    collection: "BiliBili",
                    externalSourceName: "BiliBili",
                    externalId: entry.Key,
                    description: entry.Value,
                    text: entry.Value);

                Console.Write($" #{++i} 保存成功.");
            }

            var memories = memoryWithCustomDb.SearchAsync("BiliBili", text, limit: 2, minRelevanceScore: 0.5);

            string result = "";
            await foreach (MemoryQueryResult memory in memories)
            {
                result += $"Id:{memory.Metadata.Id},Description:{memory.Metadata.Description},Relevance：{memory.Relevance}\n";
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

        /// <summary>
        /// 第1部分：使用ISemanticTextMemory（textMemory）对象存储和检索内存。
        ///这是一种从代码角度存储内存的简单方法，无需使用内核。
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> TextMemory1()
        {
            IMemoryStore memoryStore = new VolatileMemoryStore();
            var handler = new OpenAIHttpClientHandler();
            var embeddingGenerator = new OpenAITextEmbeddingGenerationService("text-embedding-ada-002", OpenAIOptions.Key, httpClient: new HttpClient(handler));

            SemanticTextMemory textMemory = new(memoryStore, embeddingGenerator);

            await textMemory.SaveInformationAsync("Xzy", id: "info1", text: "我的名字是许泽宇", cancellationToken: default);
            await textMemory.SaveInformationAsync("Xzy", id: "info2", text: "我的职位是架构师", cancellationToken: default);
            await textMemory.SaveInformationAsync("Xzy", id: "info3", text: "我有13年工作经验", cancellationToken: default);
            await textMemory.SaveInformationAsync("Xzy", id: "info4", text: "我擅长.Net Core、微服务、云原生、AI", cancellationToken: default);

            var memoryPlugin = new TextMemoryPlugin(textMemory);
            var memoryFunctions = _kernel.ImportPluginFromObject(memoryPlugin);
            MemoryQueryResult? lookup = await textMemory.GetAsync("Xzy", "info1", cancellationToken: default);
            Console.WriteLine(" 'info1':" + lookup?.Metadata.Text ?? "ERROR: memory 没找到");

            return Ok();
        }

        /// <summary>
        ///第2部分：创建TextMemoryPlugin，通过内核存储和检索内存。
        ///这使得语义功能和人工智能（通过规划者）能够访问记忆
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> TextMemory2()
        {
            IMemoryStore memoryStore = new VolatileMemoryStore();
            //AzureOpenAITextEmbeddingGenerationService
            var handler = new OpenAIHttpClientHandler();
            var embeddingGenerator = new OpenAITextEmbeddingGenerationService("text-embedding-ada-002", OpenAIOptions.Key, httpClient: new HttpClient(handler));

            SemanticTextMemory textMemory = new(memoryStore, embeddingGenerator);
            var memoryPlugin = new TextMemoryPlugin(textMemory);
            var memoryFunctions = _kernel.ImportPluginFromObject(memoryPlugin);
            await _kernel.InvokeAsync(memoryFunctions["Save"], new()
            {
                [TextMemoryPlugin.CollectionParam] = "Xzy",
                [TextMemoryPlugin.KeyParam] = "info1",
                ["input"] = "我的名字是许泽宇"
            }, cancellationToken: default);
            var result = await _kernel.InvokeAsync(memoryFunctions["Retrieve"], new()
            {
                [TextMemoryPlugin.CollectionParam] = "Xzy",
                [TextMemoryPlugin.KeyParam] = "info1"
            }, cancellationToken: default);
            return Ok(result.GetValue<string>());
        }

        /// <summary>
        ///第三部分：用语义搜索回忆相似的想法
        ///使用AI嵌入基于意图而非特定密钥对内存进行模糊查找。
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> TextMemory3()
        {
            IMemoryStore memoryStore = new VolatileMemoryStore();
            var handler = new OpenAIHttpClientHandler();
            var embeddingGenerator = new OpenAITextEmbeddingGenerationService("text-embedding-ada-002", OpenAIOptions.Key, httpClient: new HttpClient(handler));

            SemanticTextMemory textMemory = new(memoryStore, embeddingGenerator);
            await textMemory.SaveInformationAsync("Xzy", id: "info1", text: "我的名字是许泽宇", cancellationToken: default);
            await textMemory.SaveInformationAsync("Xzy", id: "info2", text: "我的职位是架构师", cancellationToken: default);
            await textMemory.SaveInformationAsync("Xzy", id: "info3", text: "我有13年工作经验", cancellationToken: default);
            await textMemory.SaveInformationAsync("Xzy", id: "info4", text: "我擅长.Net Core、微服务、云原生、AI", cancellationToken: default);

            await foreach (var answer in textMemory.SearchAsync(
                     collection: "Xzy",
                     query: "我叫什么名字?",
                     limit: 2,
                     minRelevanceScore: 0.79,
                     withEmbeddings: true,
                     cancellationToken: default))
            {
                Console.WriteLine($"Answer: {answer.Metadata.Text} ");
            }
            return Ok();
        }

        private const string RecallFunctionDefinition = @"
回答问题时只考虑以下事实：
开始事实
关于我： {{Recall '我在哪里长大的？'}}
关于我： {{Recall '我现在住在哪里？'}}
结束事实

问题: {{$input}}

答案:
";

        /// <summary>
        ///TextMemoryPugin在语义函数中的回忆
        ///渲染提示模板时查找相关内存，然后将渲染的提示发送到
        ///用于回答自然语言查询的文本完成模型。
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> TextMemory4()
        {
            IMemoryStore memoryStore = new VolatileMemoryStore();
            var handler = new OpenAIHttpClientHandler();
            var embeddingGenerator = new OpenAITextEmbeddingGenerationService("text-embedding-ada-002", OpenAIOptions.Key, httpClient: new HttpClient(handler));

            SemanticTextMemory textMemory = new(memoryStore, embeddingGenerator);

            await textMemory.SaveInformationAsync("Xzy", id: "info1", text: "我的名字是许泽宇", cancellationToken: default);
            await textMemory.SaveInformationAsync("Xzy", id: "info2", text: "我的职位是架构师", cancellationToken: default);
            await textMemory.SaveInformationAsync("Xzy", id: "info3", text: "我有13年工作经验", cancellationToken: default);
            await textMemory.SaveInformationAsync("Xzy", id: "info4", text: "我擅长.Net Core、微服务、云原生、AI", cancellationToken: default);
            await textMemory.SaveInformationAsync("Xzy", id: "info5", text: "我住在武汉", cancellationToken: default);
            var memoryPlugin = new TextMemoryPlugin(textMemory);
            var memoryFunctions = _kernel.ImportPluginFromObject(memoryPlugin);
            var aboutMeOracle = _kernel.CreateFunctionFromPrompt(RecallFunctionDefinition,  new OpenAIPromptExecutionSettings() { MaxTokens = 100 });

            var result = await _kernel.InvokeAsync(aboutMeOracle, new KernelArguments()
            {
                [TextMemoryPlugin.CollectionParam] = "Xzy",
                [TextMemoryPlugin.RelevanceParam] = "0.79",
                [TextMemoryPlugin.LimitParam] = "2",
                [TextMemoryPlugin.InputParam] = "我住在哪里?"
            }, cancellationToken: default);
            return Ok(result.GetValue<string>());
        }
    }
}