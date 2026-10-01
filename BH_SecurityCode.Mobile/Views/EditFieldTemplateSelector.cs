using BH_SecurityCode.Mobile.ViewModels;

namespace BH_SecurityCode.Mobile.Views
{
    /// <summary>입력 칸 종류(<see cref="EditKind"/>)에 맞는 템플릿을 고른다.</summary>
    public sealed class EditFieldTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? TextTemplate { get; set; }
        public DataTemplate? MultilineTemplate { get; set; }
        public DataTemplate? ChoiceTemplate { get; set; }
        public DataTemplate? DateTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var field = item as EditFieldViewModel;
            var template = field?.Kind switch
            {
                EditKind.Multiline => MultilineTemplate,
                EditKind.Choice => ChoiceTemplate,
                EditKind.Date => DateTemplate,
                _ => TextTemplate,
            };
            return template ?? TextTemplate!;
        }
    }
}
