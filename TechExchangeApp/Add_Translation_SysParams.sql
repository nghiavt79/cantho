IF NOT EXISTS (SELECT 1 FROM SYS_PARAMETERS WHERE Name = 'TRANSLATION_PROVIDER')
BEGIN
    INSERT INTO SYS_PARAMETERS (Name, Val, Description, Domain, Activated, IsSystem, LanguageId, SiteId)
    VALUES ('TRANSLATION_PROVIDER', 'sonnet', N'Nhà cung cấp dịch vụ dịch thuật tự động (haiku, sonnet, google)', '', 1, 1, 1, 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM SYS_PARAMETERS WHERE Name = 'TRANSLATION_AUTO_PUBLISH')
BEGIN
    INSERT INTO SYS_PARAMETERS (Name, Val, Description, Domain, Activated, IsSystem, LanguageId, SiteId)
    VALUES ('TRANSLATION_AUTO_PUBLISH', '0', N'Tự động xuất bản bài dịch tiếng Anh (1 = Có, 0 = Không - Lưu nháp)', '', 1, 1, 1, 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM SYS_PARAMETERS WHERE Name = 'TRANSLATION_ANTHROPIC_API_KEY')
BEGIN
    INSERT INTO SYS_PARAMETERS (Name, Val, Description, Domain, Activated, IsSystem, LanguageId, SiteId)
    VALUES ('TRANSLATION_ANTHROPIC_API_KEY', '', N'API Key cho Anthropic (Claude Sonnet / Haiku)', '', 1, 1, 1, 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM SYS_PARAMETERS WHERE Name = 'TRANSLATION_GOOGLE_API_KEY')
BEGIN
    INSERT INTO SYS_PARAMETERS (Name, Val, Description, Domain, Activated, IsSystem, LanguageId, SiteId)
    VALUES ('TRANSLATION_GOOGLE_API_KEY', '', N'API Key cho Google Translate', '', 1, 1, 1, 1);
END
GO
