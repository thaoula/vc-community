update [Case] 
set 
AgentId = (select top 1 [AccountId] FROM [PlatformAccount]),
AgentName = (select top 1 UserName FROM [PlatformAccount]),
CaseTemplateId = (select top 1 CaseTemplateId from [CaseTemplate])