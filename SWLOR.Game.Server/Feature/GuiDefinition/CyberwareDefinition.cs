using SWLOR.Game.Server.Feature.GuiDefinition.ViewModel;
using SWLOR.Game.Server.Service.GuiService;

namespace SWLOR.Game.Server.Feature.GuiDefinition
{
    /// <summary>
    /// The street-doc cyberclinic window. Modelled on the simple list-with-actions windows such as
    /// <see cref="EmotesDefinition"/>: a header showing Essence and nuyen, then one row per catalogue
    /// piece with an Install/Remove button. Opened by the cyberdoc conversation.
    /// </summary>
    public class CyberwareDefinition : IGuiWindowDefinition
    {
        private readonly GuiWindowBuilder<CyberwareViewModel> _builder = new();

        public GuiConstructedWindow BuildWindow()
        {
            _builder.CreateWindow(GuiWindowType.Cyberware)
                .SetInitialGeometry(0, 0, 480f, 420f)
                .SetTitle("Cyberclinic")
                .SetIsResizable(true)
                .SetIsCollapsible(true)

                .AddColumn(col =>
                {
                    col.AddRow(row =>
                    {
                        row.AddLabel()
                            .BindText(model => model.EssenceText)
                            .SetHeight(24f);
                    });
                    col.AddRow(row =>
                    {
                        row.AddLabel()
                            .BindText(model => model.NuyenText)
                            .SetHeight(24f);
                    });

                    col.AddRow(row =>
                    {
                        row.AddList(template =>
                        {
                            template.AddCell(cell =>
                            {
                                cell.AddLabel()
                                    .BindText(model => model.Names);
                            })
                                .SetWidth(150f);

                            template.AddCell(cell =>
                            {
                                cell.AddLabel()
                                    .BindText(model => model.Details);
                            })
                                .SetWidth(210f);

                            template.AddCell(cell =>
                            {
                                cell.AddButton()
                                    .BindText(model => model.ActionText)
                                    .BindOnClicked(model => model.OnClickCyberware());
                            })
                                .SetWidth(90f);
                        })
                            .BindRowCount(model => model.Names);
                    });
                });

            return _builder.Build();
        }
    }
}
