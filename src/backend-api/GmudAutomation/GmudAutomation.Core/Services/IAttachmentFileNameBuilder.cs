namespace GmudAutomation.Core.Services;

/// <summary>Gera o nome padronizado de anexo &lt;numero_us&gt;_SCRIPT_&lt;numero_task&gt;.ext (US 3.2, Task 206).</summary>
public interface IAttachmentFileNameBuilder
{
    string Build(int userStoryNumber, int taskNumber, string originalFileName);
}
