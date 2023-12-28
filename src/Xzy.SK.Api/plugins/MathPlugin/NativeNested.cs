using Microsoft.SemanticKernel;
using RepoUtils;
using System.IO;
using System.Threading.Tasks;

namespace Xzy.SK.Api.plugins.MathPlugin
{
    public class NativeNested
    {
        private readonly Kernel _kernel;
        public NativeNested(Kernel kernel)
        {
            _kernel = kernel;
        }

        //通过自然语义先找到最大和最小的2个值，然后用最大值减去最小值得到结果返回
        [KernelFunction]
        public async Task<string> Test(string input)
        {
            var mathPlugin1 = _kernel.ImportPluginFromPromptDirectory(Path.Combine(RepoFiles.SamplePluginsPath(), "MathPlugin"));
            var mathPlugin2 = _kernel.ImportPluginFromObject(new MathSK(), "MathPlugin1");

            var maxmin = await _kernel.InvokeAsync( mathPlugin1["FindMaxMin"],new KernelArguments() { ["input"]=input } );

            var nums = maxmin.GetValue<string>().Split("-");

            var variables = new KernelArguments
            {
                ["num1"] = nums[0],
                ["num2"] = nums[1]
            };
            var result = await _kernel.InvokeAsync(mathPlugin2["Subtraction"], variables);
            return result.GetValue<string>();
        }
    }
}
