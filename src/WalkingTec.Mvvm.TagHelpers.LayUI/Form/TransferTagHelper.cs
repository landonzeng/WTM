﻿using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using WalkingTec.Mvvm.Core;

namespace WalkingTec.Mvvm.TagHelpers.LayUI
{
    /// <summary>
    /// 穿梭框
    /// </summary>
    [HtmlTargetElement("wt:transfer", Attributes = REQUIRED_ATTR_NAMES, TagStructure = TagStructure.WithoutEndTag)]
    public class TransferTagHelper : BaseFieldTag
    {
        private const string REQUIRED_ATTR_NAMES = "field,items";

        private const string _idPrefix = "_transfer";
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };

        /// <summary>
        /// 左侧穿梭框上方标题
        /// </summary>
        public string LeftTitle { get; set; }

        /// <summary>
        /// 右侧穿梭框上方标题
        /// </summary>
        public string RightTitle { get; set; }

        /// <summary>
        /// 左侧穿梭框数据源
        /// </summary>
        public ModelExpression Items { get; set; }

        /// <summary>
        /// 启用搜索
        /// 默认 false
        /// </summary>
        public bool EnableSearch { get; set; }

        /// <summary>
        /// 没有数据时的文案
        /// </summary>
        public string NonePlaceholder { get; set; } = "无数据";

        /// <summary>
        /// 搜索无匹配数据时的文案
        /// </summary>
        public string SearchNonePlaceholder { get; set; } = "无匹配数据";

        /// <summary>
        /// 数值改变的回调
        /// 在滑块数值被改变时触发。该回调非常重要，可动态获得滑块当前的数值。你可以将得到的数值，赋值给隐藏域，或者进行一些其它操作。
        /// param0: 得到当前被穿梭的数据
        /// param1: 如果数据来自左边，index 为 0，否则为 1
        /// param2: 当前 transfer 实例
        /// </summary>
        public string ChangeFunc { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.Add("id", $"{_idPrefix}{Id}");

            #region 添加下拉数据 并 设置默认选中

            var modeltype = Field.Metadata.ModelType;
            var listItems = Items?.Model as List<ComboSelectListItem>;

            var data = listItems.Select(x => new
            {
                x.Value,
                Title = x.Text,
                x.Disabled,
                Checked = x.Selected
            }).ToArray();

            #endregion

            // 赋默认值
            if (string.IsNullOrEmpty(DefaultValue) && Field.Model != null)
            {
                var selectVal = new List<string>();
                if (modeltype.IsArray || (modeltype.IsGenericType && typeof(List<>).IsAssignableFrom(modeltype.GetGenericTypeDefinition())))
                {
                    foreach (var item in Field.Model as dynamic)
                    {
                        selectVal.Add($"'{item.ToString().ToLower()}'");
                    }
                }
                else
                {
                    selectVal.Add(Field.Model.ToString().ToLower());
                }
                DefaultValue = $"[{string.Join(",", selectVal)}]";
            }

            var title = $"['{(string.IsNullOrEmpty(LeftTitle) ? "待选择" : LeftTitle)}','{(string.IsNullOrEmpty(RightTitle) ? "选择的" : RightTitle)}']";

            var content = $@"
<script>
;!function(){{
    var $ = layui.$;
    var transfer = layui.transfer;
    var name = '{Field.Name}';
    var container = $('#{_idPrefix}{Id}');
    function defaultFunc(data,index,transferIns) {{
        var selectVals = transfer.getData('{Id}');
        // remove old values
        var inputs = $('#{_idPrefix}{Id} input[name='+name+']')
        if(inputs!=null && inputs.length>0){{
            for (var i = 0; i < inputs.length; i++) {{
                inputs[i].remove();
            }}
        }}
        // add new values
        for (var i = 0; i < selectVals.length; i++) {{
            container.append('<input type=""hidden"" name=""'+name+'"" value=""'+selectVals[i].value+'""/>');
        }}
    }}
    var defaultVal = {DefaultValue};
    var transferIns = transfer.render({{
        elem: '#{_idPrefix}{Id}'
        ,title:{title}
        ,data:{JsonConvert.SerializeObject(data, _jsonSerializerSettings)}
        {(string.IsNullOrEmpty(DefaultValue) ? string.Empty : $",value:defaultVal")}
        ,id:'{Id}'
        ,text:{{none:'{NonePlaceholder}',searchNone:'{SearchNonePlaceholder}'}}
        {(!EnableSearch ? string.Empty : ",showSearch:true")}
        {(!Width.HasValue ? string.Empty : $",width:{Width}")}
        {(!Height.HasValue ? string.Empty : $",height:{Height}")}
        ,onchange: function(data,index){{defaultFunc(data,index,transferIns);
        {(string.IsNullOrEmpty(ChangeFunc) ? string.Empty : $"{ChangeFunc}(data, index,transferIns);")}
        }}
    }});
    // init default value
    if(defaultVal!=null && defaultVal.length>0){{
        for (var i = 0; i < defaultVal.length; i++) {{
            container.append('<input type=""hidden"" name=""'+name+'"" value=""'+defaultVal[i]+'""/>');
        }}
    }}
}}();
</script>
";
            output.PostElement.AppendHtml(content);
            base.Process(context, output);
        }
    }
}