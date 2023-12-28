using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xzy.SK.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FunCoreDemoController : ControllerBase
    {
        private readonly Kernel _kernel;

        public FunCoreDemoController(Kernel kernel)
        {
            _kernel = kernel;
        }

        private const string ChatTranscript = @"约翰：你好，你好吗？
简：我很好，谢谢。你好吗
约翰：我做得很好，写了一些示例代码。
简：太棒了！我也在写一些示例代码。
约翰：你在写什么？
简：我正在写一个聊天机器人。
约翰：太酷了。我也在写一个聊天机器人。
简：你是用什么语言写的？
约翰：我是用C#写的。
简：我是用Python写的。
约翰：太酷了。我需要学习Python。
简：我需要学习C#。
约翰：我可以试试你的聊天机器人吗？
简：当然，这是链接。
约翰：谢谢！
简：不客气。
简：看看我的聊天机器人写的这首诗：
简：玫瑰是红色的
简：小提琴是蓝色的
简：我正在写一个聊天机器人
简：你呢？
约翰：太酷了。让我看看我的是否也会写诗。
约翰：这是我的聊天机器人写的一首诗：
约翰：宇宙的奇异性是个谜。
约翰：宇宙是个谜。
约翰：宇宙是个谜。
约翰：宇宙是个谜。
约翰：看来我需要改进我的，哦，好吧。
简：你可能想试着用另一种型号。
简：我用的是GPT-3型号。
约翰：我用的是GPT-2型号。这是有道理的。
约翰：这是模型更新后的一首新诗。
约翰：宇宙是个谜。
约翰：宇宙是个谜。
约翰：宇宙是个谜。
约翰：哎呀，它真的卡住了，不是吗？你能帮我调试代码吗？
简：当然，出什么问题了？
约翰：我不确定。我认为这是代码中的一个错误。
简：我去看看。
简：我想我发现问题了。
简：看起来你没有给模型传递正确的参数。
约翰：谢谢你的帮助！
简：我现在正在写一个机器人来总结对话。我想确保它在谈话时间长的时候起作用。
约翰：所以你需要一直和我说话才能产生长时间的对话？
简：是的，没错。
约翰：好的，我继续说。我们应该谈什么？
简：我不知道，你想谈什么？
约翰：我不知道，CoPilot为我们做了大部分谈话，这很好。但有时肯定会卡住。
简：我同意，很高兴CoPilot为我们做了大部分的谈话。
简：但有时候肯定会卡住。
约翰：你知道需要多长时间吗？
简：我想最大长度是1024个代币。大约为1024*4=4096个字符。
约翰：角色太多了。
简：是的。
约翰：我不知道还能说多久。
简：我想我们快到了。让我检查一下。
简：我有个坏消息，我们只走了一半。
约翰：哦，不，我不确定我能不能坚持下去。我累了。
简：我也累了。
约翰：也许有一大块文本可以用来进行长时间的对话。
简：这是个好主意。让我看看能不能找到一个。也许是Lorem Ipsum？
约翰：是的，这是个好主意。
简：我找到了一个Lorem Ipsum生成器。
Jane：这是一个4096个字符的Lorem Ipsum文本：
简：Lorem ipsum悲哀坐amet，con
简：Lorem ipsum悲哀坐amet，consectetur adipiscing elit。Sed euismod，nunc sit amet aliquam
简：Lorem ipsum悲哀坐amet，consectetur adipiscing elit。Sed euismod，nunc sit amet aliquam
简：亲爱的，这只是在重复stuf。
约翰：我想我们完了。
简：但我们不是！我们还需要1500个字符。
约翰：哦，卡南达，我们的家乡。
简：你所有的儿子都有真正的爱国者之爱。
约翰：我们看到你的心在发光。
简：真正的北方坚强而自由。
约翰：加拿大啊，我们从四面八方为你站岗。
简：上帝保佑我们的土地光荣自由。
约翰：加拿大啊，我们为你站岗。
简：哦，加拿大，我们为你站岗。
简：很有趣，谢谢。让我现在检查一下。
简：我想我们还需要600个字。
约翰：哦，你能看见吗？
简：黎明前。
约翰：我们多么自豪地欢呼啊。
简：在黄昏的最后一刻。
约翰：宽阔的条纹和明亮的星星。
简：通过危险的战斗。
约翰：哦，我们看的城墙。
简：我们真是太殷勤了。
约翰：还有火箭的红光。
简：炸弹在空中爆炸。
约翰：彻夜提供证据。
简：我们的国旗还在那儿。
约翰：哦，说那条星条旗还没有飘扬。
简：哦，自由之地。
约翰：还有勇敢者的家。
简：你是西雅图海怪队的球迷吗？
约翰：是的，我喜欢。我喜欢去看比赛。
简：我也是西雅图海怪队的球迷。谁是你最喜欢的球员？
约翰：我喜欢看所有的球员，但我想我最喜欢的是马蒂·贝尼尔斯。
简：是的，他是个伟大的球员。我也喜欢看他。我也喜欢看贾登·施瓦茨。
约翰：亚当·拉尔森是另一个好的。那只大猫！
简：我们成功了！它足够长了。非常感谢。
约翰：不客气。我很高兴我们能帮忙。再见
简：再见！";

        /// <summary>
        /// 会话总结，标签提取
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ConversationSummary()
        {
            KernelPlugin conversationSummaryPlugin = _kernel.ImportPluginFromType<ConversationSummaryPlugin>();

            FunctionResult summary = await _kernel.InvokeAsync(
                 conversationSummaryPlugin["SummarizeConversation"],new KernelArguments() { ["input"]= ChatTranscript });

            Console.WriteLine("SummarizeConversation:");
            Console.WriteLine(summary.GetValue<string>());

            summary = await _kernel.InvokeAsync(
                 conversationSummaryPlugin["SummarizeConversation"], new KernelArguments() { ["input"] = ChatTranscript });

            Console.WriteLine("GetConversationActionItems:");
            Console.WriteLine(summary.GetValue<string>());

            summary = await _kernel.InvokeAsync(
                 conversationSummaryPlugin["SummarizeConversation"], new KernelArguments() { ["input"] = ChatTranscript });

            Console.WriteLine("GetConversationTopics:");
            Console.WriteLine(summary.GetValue<string>());
            return Ok();
        }
    }
}