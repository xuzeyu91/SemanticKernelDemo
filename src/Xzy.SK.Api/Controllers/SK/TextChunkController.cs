using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Text;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace Xzy.SK.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TextChunkController : ControllerBase
    {
        private readonly Kernel _kernel;

        public TextChunkController(Kernel kernel)
        {
            _kernel = kernel;
        }

        /// <summary>
        /// 文本分块
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Test()
        {
            string msg = System.IO.File.ReadAllText("诛仙.txt");


            ///将纯文本拆分成行。
            ///<param name=“text”>要拆分的文本</param>
            ///<param name=“maxTokensPerLine”>每行的最大令牌数</param>
            ///<param name=“tokenCounter”>对字符串中的令牌进行计数的函数。如果未提供，将使用默认计数器</param>
            ///＜return＞行列表</return>
            var lines = TextChunker.SplitPlainTextLines(msg, 40);

            ///将纯文本拆分为段落。
            ///<param name=“lines”>文本行</param>
            ///<param name=“maxTokensPerParagraph”>每个段落的最大令牌数</param>
            ///<param name=“overlapTokens”>段落之间重叠的令牌数</param>
            ///<param name=“chunkHeader”>要在每个单独的区块前加上前缀的文本</param>
            ///<param name=“tokenCounter”>对字符串中的令牌进行计数的函数。如果未提供，将使用默认计数器</param>
            ///＜return＞段落列表</return>
            var paragraphs = TextChunker.SplitPlainTextParagraphs(lines, 4000);

            return Ok();
        }
    }
}