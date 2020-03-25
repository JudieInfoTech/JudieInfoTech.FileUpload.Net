USE [master]
GO


CREATE DATABASE [FileUploadDemo]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'FileUploadDemo', FILENAME = N'E:\softwares\MSSQL14.MSSQLSERVER\MSSQL\DATA\FileUploadDemo.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'FileUploadDemo_log', FILENAME = N'E:\softwares\MSSQL14.MSSQLSERVER\MSSQL\DATA\FileUploadDemo_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO


USE [FileUploadDemo] 
GO

DROP TABLE [dbo].[UploadedFiles]
GO


CREATE TABLE [dbo].[UploadedFiles](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ImageFilename] [nvarchar](50) NOT NULL,
	[Comments] [nvarchar](max) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[CreatedBy] [nvarchar](128) NULL,
	[ModifiedAt] [datetime2](7) NULL,
	[ModifiedBy] [nvarchar](128) NULL,
	[ImageFilePath] [nvarchar](250) NOT NULL,
	[RowVersion] [nvarchar](40) NULL,
 CONSTRAINT [PK_UploadedFiles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO







