USE [TechExchangeNew]
GO
/****** Object:  StoredProcedure [dbo].[AAUpdate]    Replace dev localhost URLs with production domain ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROC [dbo].[AAUpdate]

AS

	DECLARE @Old NVARCHAR(500)
	DECLARE @New NVARCHAR(500)
	SET @Old = 'https://localhost:7232/'
	SET @New = 'https://giaodichcongnghe.giaiphapmobifone.vn/'


	UPDATE dbo.ImagesAdver
	SET SRC = REPLACE(SRC, @Old, @New)

	UPDATE dbo.Store
	SET ImgLogo = REPLACE(ImgLogo, @Old, @New)

	UPDATE dbo.StoreImage
	SET ImageURL = REPLACE(ImageURL, @Old, @New)

	UPDATE dbo.NhaTuVan
	SET HinhDaiDien = REPLACE(HinhDaiDien, @Old, @New)

	UPDATE dbo.NhaCungUng
	SET HinhDaiDien = REPLACE(HinhDaiDien, @Old, @New),
	    Logo = REPLACE(Logo, @Old, @New),
	    VideoUrl = REPLACE(VideoUrl, @Old, @New)

	UPDATE dbo.Contents
	SET [Image] = REPLACE([Image], @Old, @New),
	    ImageBig = REPLACE(ImageBig, @Old, @New),
	    URL = REPLACE(URL, @Old, @New),
	    LinkFile = REPLACE(LinkFile, @Old, @New),
	    Contents = REPLACE(Contents, @Old, @New)

	UPDATE dbo.SanPhamCNTB
	SET [QuyTrinhHinhAnh] = REPLACE([QuyTrinhHinhAnh], @Old, @New)

GO
