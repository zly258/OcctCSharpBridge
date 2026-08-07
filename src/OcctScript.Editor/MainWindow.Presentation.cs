using OcctScript.Domain;

namespace OcctScript.Editor;

public partial class MainWindow
{
    private void PopulateCommandCatalog()
    {
        var selectedType = (CommandCatalogCombo.SelectedItem as CommandCatalogItem)?.Type;
        var items = commandRegistry.GetAll()
            .Select(definition => new CommandCatalogItem(definition, ResourceText(definition.DisplayNameKey), ResourceText(definition.CategoryKey), ResourceText(definition.DescriptionKey)))
            .OrderBy(x => x.Definition.Order)
            .ToArray();
        CommandCatalogCombo.ItemsSource = items;
        CommandCatalogCombo.SelectedItem = items.FirstOrDefault(x => x.Type == selectedType) ?? items.FirstOrDefault();
        UpdateCatalogDescription();
    }

    private void RefreshDocumentBindings(Guid? preferredCommandId = null)
    {
        var commandId = preferredCommandId ?? selectedCommand?.Id;
        CommandList.ItemsSource = document.Commands.OrderBy(x => x.Order).ThenBy(x => x.Name, StringComparer.Ordinal).ToArray();
        ParameterGrid.ItemsSource = document.Parameters;
        selectedCommand = commandId.HasValue ? document.FindCommand(commandId.Value) : document.Commands.FirstOrDefault();
        CommandList.SelectedItem = selectedCommand;
        RefreshFieldRows();
        UpdateTitle();
        UpdateHistoryState();
    }

    private void RefreshFieldRows()
    {
        fieldRows.Clear();
        selectedCommand = CommandList.SelectedItem as ScriptCommand;
        if (selectedCommand is null)
        {
            OutputTopologyText.Text = string.Empty;
            SelectedCommandDescription.Text = string.Empty;
            return;
        }
        var definition = commandRegistry.GetRequired(selectedCommand.Type);
        OutputTopologyText.Text = definition.OutputTopology.ToString();
        SelectedCommandDescription.Text = ResourceText(definition.DescriptionKey);
        foreach (var fieldDefinition in definition.Fields)
        {
            if (!selectedCommand.Fields.TryGetValue(fieldDefinition.Name, out var value))
            {
                value = new CommandValue();
                selectedCommand.Fields[fieldDefinition.Name] = value;
            }
            fieldRows.Add(new CommandFieldRow
            {
                Definition = fieldDefinition,
                DisplayName = ResourceText(fieldDefinition.DisplayNameKey),
                ValueText = FormatFieldValue(value, fieldDefinition.FieldType),
                TypeText = ResourceText("FieldType." + fieldDefinition.FieldType),
                UnitText = fieldDefinition.UnitType,
                Hint = FieldHint(fieldDefinition)
            });
        }
    }

    private string FieldHint(CommandFieldDefinition field)
    {
        if (field.Name is "edgeIndices" or "faceIndices") return ResourceText("Ui.IndexHint");
        return field.FieldType switch
        {
            CommandFieldType.CommandReference or CommandFieldType.CommandReferenceList => ResourceText("Ui.ReferenceHint"),
            CommandFieldType.Point3 or CommandFieldType.Vector3 or CommandFieldType.PointList => ResourceText("Ui.PointHint"),
            _ => field.IsRequired ? ResourceText("Ui.Required") : string.Empty
        };
    }

    private string FormatFieldValue(CommandValue value, CommandFieldType fieldType) => fieldType switch
    {
        CommandFieldType.CommandReference => value.ReferenceId is Guid id ? CommandName(id) : string.Empty,
        CommandFieldType.CommandReferenceList => string.Join("; ", value.ReferenceIds.Select(CommandName)),
        CommandFieldType.Expression or CommandFieldType.Number or CommandFieldType.Integer => value.Expression,
        _ => value.Literal ?? string.Empty
    };

    private string CommandName(Guid id) => document.FindCommand(id)?.Name ?? id.ToString();
}
